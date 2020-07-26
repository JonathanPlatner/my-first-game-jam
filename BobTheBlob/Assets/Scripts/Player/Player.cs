using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer mainSprite;
    [SerializeField]
    private SpriteRenderer whiteSprite;
    enum Mode { Bouncy, Sticky }
    private Mode mode;
    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private PhysicsMaterial2D bouncy;
    [SerializeField]
    private PhysicsMaterial2D sticky;
    [SerializeField]
    private Transform actionTransform;
    [SerializeField]
    private Transform arrowTransform;
    [SerializeField]
    private Transform shieldTransform;
    [SerializeField]
    private Transform cannonTransform;

    private float actionAngle;

    [SerializeField]
    private InputManager im;

    private float mouseAngle;
    private Vector2 unitMousePosition;

    private float launchRamp;
    private float defaultFixedDeltaTime;

    [SerializeField]
    private int maxJumps;
    private int jumps = 1;
    [SerializeField]
    private float launchPowerScale = 5;
    [SerializeField]
    private float maxLaunchPower = 5;
    private bool dragging;

    [SerializeField]
    private float accel = 10f;
    [SerializeField]
    private float bounceControl = 0.1f;
    [SerializeField]
    private float maxVelocity = 5f;
    private Vector2 velocity;

    [SerializeField]
    private Animator anim;


    private Vector2 dragPosition;
    private Camera playerCam;

    private bool stickyToggle;
    private bool delayInput;

    private Vector2 normal = Vector2.zero;
    private Vector2 moveVector;

    [SerializeField]
    private GameObject bullet;

    private int health = 100;

    private bool invincible = false;
    private float rateOfFire = 3;
    private float timeSinceLastShot = 0;


    public Vector2 Velocity { get { return rb.velocity; } }

    private void Start()
    {
        ToBouncy();
        launchRamp = 0.2f;
        defaultFixedDeltaTime = Time.fixedDeltaTime;
        playerCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    private void Update()
    {
        if(im.Test.Down())
        {
            Debug.Log("Down");
        }

        if(im.Grab.Up() && mode == Mode.Sticky)
        {
            ToBouncy();
        }

        if (timeSinceLastShot < 1 / rateOfFire)
        {
            timeSinceLastShot += Time.deltaTime;
        }
        actionTransform.rotation = Quaternion.Euler(0, 0, mouseAngle * Mathf.Rad2Deg);
        if(im.Cannon.Active())
        {
            cannonTransform.gameObject.SetActive(true);
            shieldTransform.gameObject.SetActive(false);
            arrowTransform.gameObject.SetActive(false);
        }
        else if(im.Shield.Active())
        {
            cannonTransform.gameObject.SetActive(false);
            shieldTransform.gameObject.SetActive(true);
            arrowTransform.gameObject.SetActive(false);
        }
        else
        {
            cannonTransform.gameObject.SetActive(false);
            shieldTransform.gameObject.SetActive(false);
            arrowTransform.gameObject.SetActive(true);
        }

        if(im.Action.Down() && im.Cannon.Active())
        {
            Shoot();
        }
        if(im.Action.Down() && !im.Cannon.Active() && !im.Shield.Active() && jumps < maxJumps && !dragging)
        {
            StartDrag();
        }
        if((im.Action.Up() || !im.Action.Active()) && dragging)
        {
            EndDrag();
        }
        if(im.Cancel.Down() && dragging)
        {
            CancelDrag();
        }
        moveVector = new Vector2(im.Lateral.Value(), im.Longitudinal.Value());

        if(mode == Mode.Sticky)
        {
            RaycastHit2D hit = Physics2D.Raycast(rb.position, -normal, 0.4f);
            Debug.DrawRay(rb.position, -normal * 0.4f);
            if(hit.collider != null)
            {
                if(hit.collider.tag == "Ground")
                {
                    //Debug.Log("hit");
                }
                else
                {
                    //Debug.Log("no hit");
                    ToBouncy();
                }

            }
            else
            {
                //Debug.Log("no hit");
                ToBouncy();
            }

        }

        if(Dead())
        {
            Destroy(gameObject);
        }
    }
    public void Bounce(Vector2 velocity)
    {
        rb.velocity = velocity;
        TakeDamage(-5, false);
    }
    private void ToBouncy()
    {
        mode = Mode.Bouncy;
        rb.gravityScale = 1;
        rb.sharedMaterial = bouncy;
        rb.velocity = velocity;
        anim.SetBool("Bouncy", true);
        anim.SetBool("Sticky", false);
        anim.SetBool("QuickSticky", false);

    }
    private void ToSticky(bool fast)
    {
        mode = Mode.Sticky;
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;
        rb.sharedMaterial = sticky;
        if(fast)
        {
            anim.SetBool("Sticky", true);
        }
        else
        {
            anim.SetBool("Sticky", true);
        }
        anim.SetBool("Bouncy", false);

    }

    private void FixedUpdate()
    {
        mouseAngle = Mathf.Atan2(im.Mouse.World(playerCam).y - rb.position.y, im.Mouse.World(playerCam).x - rb.position.x);
        unitMousePosition = Vector2.right * Mathf.Cos(mouseAngle) + Vector2.up * Mathf.Sin(mouseAngle);
        if(mode == Mode.Bouncy)
        {
            rb.AddForce(bounceControl * im.Lateral.Value() * Vector2.right);
        }

        if(mode == Mode.Sticky)
        {
            Vector2 tangent = new Vector2(-normal.y, normal.x);
            Vector2 velocity = Vector2.Dot(moveVector, tangent) * tangent;
            rb.MovePosition(rb.position + velocity * Time.deltaTime * 1.8f);
        }
    }

    private void StartDrag()
    {
        dragPosition = im.Mouse.World(playerCam);
        Time.timeScale = 0.2f;
        Time.fixedDeltaTime = defaultFixedDeltaTime * 0.2f;
        dragging = true;
    }

    private void EndDrag()
    {
        Vector2 dragVector = im.Mouse.World(playerCam) - dragPosition;
        dragging = false;
        Time.timeScale = 1;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
        if(dragVector.magnitude > 0.1f)
        {

            jumps++;
            ToBouncy();


            rb.velocity = Vector2.ClampMagnitude(dragVector, maxLaunchPower) * launchPowerScale + rb.velocity * Mathf.Clamp01(Vector2.Dot(rb.velocity.normalized, dragVector));
        }

    }
    private void Shoot()
    {
        if(health > 5)
        {
            if(timeSinceLastShot >= 1 / rateOfFire)
            {
                GameObject go_bullet = Instantiate(bullet, cannonTransform.position, Quaternion.identity);
                go_bullet.GetComponent<Rigidbody2D>().AddForce(cannonTransform.up * 20);
                TakeDamage(5, false);
                timeSinceLastShot = 0;
            }
        }
    }
    private void CancelDrag()
    {
        dragging = false;
        Time.timeScale = 1;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        jumps = 0;
        if(collision.transform.tag == "Ground")
        {
            if(im.Grab.Active())
            {
                ToSticky(true);
                normal = collision.GetContact(0).normal;
                float rotation = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, rotation - 90);
            }
        }
        else if (collision.transform.tag == "EnemyBullet")
        {
            TakeDamage(25, true);
        }
       
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        jumps = 0;
        if(collision.transform.tag == "Ground" && im.Grab.Active())
        {
            if(Vector2.Dot(rb.velocity, normal) < 0)
            {
                ToSticky(false);
                normal = collision.GetContact(0).normal;
                float rotation = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, rotation - 90);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.transform.tag == "Ground")
        {
        }
    }
    
    private void TakeDamage(int damage, bool flash)
    {
        if (flash)
        {
            StartCoroutine(Flash());
        }
        health -= damage;
        if(health > 100)
        {
            health = 100;
        }
    }

    private bool Dead()
    {
        return health <= 0;
    }

    IEnumerator Flash()
    {
        int flashCount = 4;
        float flashInterval = 0.125f;
        bool flashState = false;
        invincible = true;
        for(int i = 0; i < flashCount; i++)
        {
            if(flashState)
            {
                mainSprite.enabled = true;
                whiteSprite.enabled = false;
            }
            else
            {
                mainSprite.enabled = false;
                whiteSprite.enabled = true;
            }
            flashState = !flashState;
            yield return new WaitForSeconds(flashInterval);
        }
        mainSprite.enabled = true;
        whiteSprite.enabled = false;
        invincible = false;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform.tag == "Blade")
        {
            if(!invincible)
            {
                TakeDamage(20, true);
            }
        }
    }
}
