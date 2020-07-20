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
    private InputManager input;

    private float mouseAngle;
    private Vector2 unitMousePosition;

    private float launchRamp;
    private float defaultFixedDeltaTime;

    [SerializeField]
    private int maxJumps;
    private int jumps = 1;

    private void Start()
    {
        ToBouncy();
        launchRamp = 0.2f;
        defaultFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void Update()
    {
        //if(bounce) rb.sharedMaterial = bouncy;
        //else rb.sharedMaterial = sticky;

        //if(Input.GetKeyDown(KeyCode.Space))
        //{
        //    rb.gravityScale = 1;
        //    bounce = true;
        //    rb.velocity += Vector2.up * 5 + Vector2.right;
        //}

        if(input.GrabOS && mode == Mode.Sticky)
        {
            ToBouncy();
        }
        actionTransform.rotation = Quaternion.Euler(0, 0, mouseAngle * Mathf.Rad2Deg);

        if(input.Action && !input.ShootMode && !input.BlockMode && jumps < maxJumps)
        {
            launchRamp += Time.deltaTime;
            launchRamp = Mathf.Clamp01(launchRamp);
            Time.timeScale = 1 - launchRamp + 0.1f;
            Time.fixedDeltaTime = (1 - launchRamp + 0.1f) * defaultFixedDeltaTime;
            //ToBouncy();
            //rb.velocity = unitMousePosition * 5f + rb.velocity * Mathf.Clamp01(Vector2.Dot(rb.velocity.normalized, unitMousePosition));
        }
        else if(input.ActionOffOS && !input.ShootMode && !input.BlockMode)
        {
            if(jumps < maxJumps)
            {
                ToBouncy();
                rb.velocity = unitMousePosition * 10f * launchRamp + rb.velocity * Mathf.Clamp01(Vector2.Dot(rb.velocity.normalized, unitMousePosition));
                Time.timeScale = 1;
                Time.fixedDeltaTime = defaultFixedDeltaTime;
                jumps++;
            }

            launchRamp = 0.2f;
        }
    }

    private void ToBouncy()
    {
        mode = Mode.Bouncy;
        rb.gravityScale = 1;
        rb.sharedMaterial = bouncy;
    }
    private void ToSticky()
    {
        mode = Mode.Sticky;
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;
        rb.sharedMaterial = sticky;
    }

    private void FixedUpdate()
    {
        mouseAngle = Mathf.Atan2(input.MousePosition.y - rb.position.y, input.MousePosition.x - rb.position.x);
        unitMousePosition = Vector2.right * Mathf.Cos(mouseAngle) + Vector2.up * Mathf.Sin(mouseAngle);


    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        jumps = 0;
        if(collision.transform.tag == "Ground" && input.Grab)
        {
            ToSticky();
            Vector2 normal = collision.GetContact(0).normal;
            float rotation = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, rotation - 90);
        }
    }
}
