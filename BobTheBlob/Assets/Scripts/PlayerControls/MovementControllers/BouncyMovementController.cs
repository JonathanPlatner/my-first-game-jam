using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncyMovementController : MovementController
{
    public void FixedUpdate()
    {
        UpdateHorizontalMovement();
        UpdateJumpMovement();
    }

    void UpdateHorizontalMovement()
    {
        // horizontal acceleration
        float inputX = Input.GetAxis("Horizontal");
        if(rb.velocity.magnitude < player.SPEED_MAX)
        {
            rb.AddForce(Vector2.right * inputX * player.ACCELERATION_RATE);
        }
    }

    void UpdateJumpMovement()
    {
        // jump mechanics
        if(player.isGrounded)
        {
            float inputJump = Input.GetAxis("Jump");
            rb.velocity = rb.velocity + (Vector2) transform.up * inputJump * player.JUMP_SPEED;
        }
    }

    public override void OnDragStart(DragInfo drag)
    {
        // start time slow while aiming launch
        Time.timeScale = 0.5f; // placeholder, TODO: create central time manager so multiple entities can interact with time while managing conflicts 
    }

    public override void OnDragEnd(DragInfo drag)
    {
        // end time slow
        Time.timeScale = 1f; // placeholder, TODO: create central time manager so multiple entities can interact with time while managing conflicts 
        if(player.launchCharges > 0)
        {
            Launch(drag.start - drag.end);
            player.launchCharges--;
        }
        Debug.Log("drag end");
    }
}
