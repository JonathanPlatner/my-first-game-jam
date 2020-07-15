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

    public enum PlayerState {
        Speed
    }
    PlayerState currentState;
    IMovementController iMovementController;

    void ChangeState(PlayerState nextState)
    {
        // remove the current movement controller
        if(iMovementController != null)
        {
            Destroy((Component) iMovementController);
        }

        switch (nextState) {
            case PlayerState.Speed:
                iMovementController = gameObject.AddComponent<MobilityMovementController>();
                break;
        }

        iMovementController.Initialize();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ChangeState(PlayerState.Speed);
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
    }

    // grounded state update
    public bool isGrounded { get; private set; }

    void CheckGroundedState()
    {
        float hitboxHeight = 0f;//transform.lossyScale.y * 0.5f; //placeholder until shape of hitbox is decided on, can do vertex comparison for variable hitbox mesh
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.down);
        Debug.DrawRay(transform.position, Vector2.down);

        isGrounded = false;
        foreach(RaycastHit2D hit in hits)
        {
            if(hit.collider.gameObject == this.gameObject)
            {
                continue;
            }
            if(hit.distance <= GROUNDED_EPSILON + hitboxHeight)
            {
                isGrounded = true;
            }
        }


    }

}
