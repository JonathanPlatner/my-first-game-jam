using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
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

    private float actionAngle;
    //[SerializeField]
    //private bool bounce;

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
        
        //if(im.Grab.Down() && mode == Mode.Sticky)
        //{
        //    ToBouncy();
        //}
        actionTransform.rotation = Quaternion.Euler(0, 0, mouseAngle * Mathf.Rad2Deg);

        if(im.Action.Down() && !im.Cannon.Active() && !im.Shield.Active() && jumps < maxJumps && !dragging)
        {
            StartDrag();
        }
        if(im.Action.Up() && dragging)
        {
            EndDrag();
        }
        if(im.Cancel.Down() && dragging)
        {
            CancelDrag();
        }
    }

    private void ToBouncy()
    {
        mode = Mode.Bouncy;
        rb.gravityScale = 1;
        rb.sharedMaterial = bouncy;
        anim.SetBool("Bouncy", true);
        anim.SetBool("Sticky", false);
        anim.SetBool("QuickSticky",false);
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
        Time.timeScale = 1;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
        jumps++;
        ToBouncy();
        dragging = false;
        Vector2 dragVector = im.Mouse.World(playerCam) - dragPosition;
        rb.velocity = Vector2.ClampMagnitude(dragVector, maxLaunchPower) * launchPowerScale + rb.velocity * Mathf.Clamp01(Vector2.Dot(rb.velocity.normalized, dragVector));
        
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
        if(collision.transform.tag == "Ground" && im.Grab.Active())
        {
            ToSticky(true);
            Vector2 normal = collision.GetContact(0).normal;
            float rotation = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, rotation - 90);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        jumps = 0;
        if(collision.transform.tag == "Ground" && im.Grab.Down() && mode != Mode.Sticky)
        {
            ToSticky(false);
            Vector2 normal = collision.GetContact(0).normal;
            float rotation = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, rotation - 90);
        }
        //if(collision.transform.tag == "Ground" && im.Grab.Down() && mode == Mode.Sticky)
        //{
        //    ToBouncy();
        //    //Vector2 normal = collision.GetContact(0).normal;
        //    //float rotation = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg;
        //    //transform.rotation = Quaternion.Euler(0, 0, rotation - 90);
        //}
    }
}
