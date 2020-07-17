using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;

    public float ACCELERATION_RATE;
    public float SPEED_MAX;
    public float GROUNDED_EPSILON = 0.01f;
    public float JUMP_SPEED;
    public float HORIZONTAL_DRAG = 1f;
    public int MAX_LAUNCH_CHARGES = 1;

    public int launchCharges = 1;

    public enum PlayerState {
        Bouncy
    }
    PlayerState currentState;
    MovementController movementController;
    public bool isGrounded { get; private set; }

    void ChangeState(PlayerState nextState)
    {
        // remove the current movement controller
        if(movementController!= null)
        {
            Destroy((Component) movementController);
        }

        switch (nextState) {
            case PlayerState.Bouncy:
                movementController= gameObject.AddComponent<BouncyMovementController>();
                break;
        }

        movementController.Initialize();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ChangeState(PlayerState.Bouncy);
        isGrounded = false;
    }

    private void Update()
    {
        // set sprite x direction
        if(rb.velocity.x != 0)
        {
            GetComponent<SpriteRenderer>().flipX = rb.velocity.x < 0;
        }
    }

    private void FixedUpdate()
    {
        CheckGroundedState();
        CheckDragState();
    }

    // grounded state update
    void CheckGroundedState()
    {
        float hitboxHeight = 0f;//transform.lossyScale.y * 0.5f; //placeholder until shape of hitbox is decided on, can do vertex comparison for variable hitbox mesh
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.down);
        Debug.DrawRay(transform.position, Vector2.down);

        bool groundCheck = false;
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
            Time.timeScale = 0.5f; // placeholder, TODO: create central time manager so multiple entities can interact with time while managing conflicts 
        }

        // drag ended
        if(!Input.GetMouseButton(0) && dragging)
        {
            dragging = false;
            currentDragInfo.end = Input.mousePosition;
            movementController.OnDragEnd(currentDragInfo);
            Time.timeScale = 1f; // placeholder, TODO: create central time manager so multiple entities can interact with time while managing conflicts 
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
