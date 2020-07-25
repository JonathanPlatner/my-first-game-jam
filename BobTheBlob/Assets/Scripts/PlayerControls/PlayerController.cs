using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;

    //grounded config
    public float GROUNDED_EPSILON = 0.01f;
    private float ungroundTime = 0f;
    private float GROUND_COOLDOWN;

    // horizontal movement config
    public float LATERAL_ACCELERATION;
    public float LATERAL_MAX_SPEED;
    public float LATERAL_STOP_TIME = 0.2f;
    public float LATERAL_DRAG { get; private set; }

    // jumping config
    public float JUMP_PEAK_HEIGHT;
    public float JUMP_TIME_TO_PEAK;
    public float HANG_TIME = 0.8f;
    public float HANG_GRAVITY = 0.1f;
    public float FALL_TIME = 0.5f;
    public float JUMP_SPEED { get; private set; }
    public float JUMP_GRAVITY { get; private set; }
    public float FALL_GRAVITY { get; private set; }

    //launch config
    public int MAX_LAUNCH_CHARGES = 1;
    public float MAX_LAUNCH_SPEED = 30f;

    // shoot config
    public float SHOOT_COOLDOWN;
    public float SHOOT_SPEED;
    public Transform PROJECTILE_PREFAB;


    public enum PlayerState { Bouncy, Sticky, Cannon, Shield }
    PlayerState currentState;
    MovementController movementController;
    public bool isGrounded;

    void ChangeState(PlayerState nextState)
    {
        // remove the current movement controller
        if(movementController!= null)
        {
            movementController.OnExitState();
            Destroy((Component) movementController);
        }

        switch (nextState) {
            case PlayerState.Bouncy:
                movementController = gameObject.AddComponent<BouncyMovementController>();
                break;
            case PlayerState.Sticky:
                movementController = gameObject.AddComponent<StickyMovementController>();
                break;
            case PlayerState.Cannon:
                movementController = gameObject.AddComponent<CannonMovementController>();
                break;
        }

        movementController.OnEnterState();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ChangeState(PlayerState.Bouncy);
        isGrounded = false;
        GROUND_COOLDOWN = Time.fixedDeltaTime * 1.5f;

        //initialize horizontal movement variables
        LATERAL_DRAG = LATERAL_MAX_SPEED / LATERAL_STOP_TIME;

        //initialize jump variables
        JUMP_SPEED = 2f * JUMP_PEAK_HEIGHT / JUMP_TIME_TO_PEAK;
        JUMP_GRAVITY = -JUMP_SPEED / JUMP_TIME_TO_PEAK;
        FALL_GRAVITY = -2f * JUMP_PEAK_HEIGHT / Mathf.Pow(FALL_TIME, 2f);
    }

    private void Update()
    {
        // set sprite x direction
        if(Math.Abs(Vector2.Dot(rb.velocity, transform.right)) >= 0.001f)
        {
            GetComponent<SpriteRenderer>().flipX = Vector2.Dot(rb.velocity, transform.right) < 0;
        }

        // mode switching
        if(Input.GetKeyDown(KeyCode.E))
        {
            if(currentState != PlayerState.Sticky)
            {
                ChangeState(PlayerState.Sticky);
            } else
            {
                ChangeState(PlayerState.Bouncy);
            }
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            if(currentState != PlayerState.Cannon)
            {
                ChangeState(PlayerState.Cannon);
            } else
            {
                ChangeState(PlayerState.Bouncy);
            }
        }
    }

    private void FixedUpdate()
    {
        CheckGroundedState();
        CheckDragState();
        CheckGroundedState();
    }

    private void LateUpdate()
    {
        CheckGroundedState();
    }

    Collider2D[] groundCollider = new Collider2D[2];
    public LayerMask groundLayer;
    // grounded state update
    void CheckGroundedState()
    {
        if(!isGrounded && (Time.time - ungroundTime) <= GROUND_COOLDOWN) { return; }
        bool groundCheck = false;
        /*
        float hitboxHeight = 0f;//transform.lossyScale.y * 0.5f; //placeholder until shape of hitbox is decided on, can do vertex comparison for variable hitbox mesh
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.down);
        Debug.DrawRay(transform.position, Vector2.down);

        foreach(RaycastHit2D hit in hits)
        {
            if(hit.collider.gameObject == this.gameObject)
            {
                continue;
            }
            if(hit.distance <= GROUNDED_EPSILON + hitboxHeight)
            {
                groundCheck = true;
            }
        }
        */

        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        /*
        ContactPoint2D[] contacts = new ContactPoint2D[20];
        collider.GetContacts(contacts);
        foreach (ContactPoint2D c in contacts)
        {
            Debug.DrawRay(c.point, c.normal, Color.red);
            if(Math.Abs(c.separation) < GROUNDED_EPSILON && Vector2.Dot(c.normal, transform.up) >= 0.9f)
            {
                groundCheck = true;
                Debug.Log(transform.position.y + " . height");
                Debug.Log("separation: " + c.separation);
                break;
            }
        }
        */

        Vector2 p1 = collider.bounds.center - transform.right * (collider.bounds.extents.x - GROUNDED_EPSILON) - transform.up * collider.bounds.extents.y;
        Vector2 p2 = collider.bounds.center + transform.right * (collider.bounds.extents.x - GROUNDED_EPSILON) - transform.up * (collider.bounds.extents.y + GROUNDED_EPSILON);
        Debug.DrawLine(p1, p2, Color.red);

        groundCheck = Physics2D.OverlapArea(p1, p2, groundLayer);

        // change in grounded state
        if(isGrounded != groundCheck)
        {
            if(groundCheck)
            {
                movementController.OnGroundedEnter();
            } else
            {
                movementController.OnGroundedExit();
            }
            isGrounded = groundCheck;
        }
    }

    public void ForceUngrounded()
    {
        isGrounded = false;
        ungroundTime = Time.time;
    }

    bool dragging = false;
    DragInfo currentDragInfo;
    void CheckDragState()
    {
        // drag started
        if (Input.GetMouseButton(0) && !dragging)
        {
            dragging = true;
            currentDragInfo = new DragInfo(Input.mousePosition);
            movementController.OnDragStart(currentDragInfo);
        }

        // drag ended
        if(!Input.GetMouseButton(0) && dragging)
        {
            dragging = false;
            currentDragInfo.end = Input.mousePosition;
            movementController.OnDragEnd(currentDragInfo);
        }
    }
}

public struct DragInfo
{
    public DragInfo(Vector3 startPosition)
    {
        start = startPosition;
        end = startPosition;
    }
    public Vector3 start;
    public Vector3 end;
}
