using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public interface IMovementController{
    void Initialize();
}

public class MobilityMovementController : MonoBehaviour, IMovementController
{
    Rigidbody2D rb;
    PlayerController player;
    public void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<PlayerController>();
    }

    public void FixedUpdate()
    {
        UpdateHorizontalMovement();
        UpdateJumpMovement();
        UpdateLauchMovement();
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

    bool dragging = false;
    Vector3 dragStartPosition;
    void UpdateLauchMovement()
    {
        // drag started
        if (Input.GetMouseButton(1) && !dragging)
        {
            dragStartPosition = Input.mousePosition;
            dragging = true;
        }

        // drag ended
        if(!Input.GetMouseButton(1) && dragging)
        {
            Launch((Vector2) (dragStartPosition - Input.mousePosition));
            dragging = false;
        }
    }

    void Launch(Vector2 direction)
    {
        rb.velocity = rb.velocity + direction.normalized * player.ACCELERATION_RATE;
    }



}
