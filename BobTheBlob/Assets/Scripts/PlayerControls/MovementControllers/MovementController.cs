using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class MovementController: MonoBehaviour{
    public Rigidbody2D rb;
    public PlayerController player;

    public virtual void OnEnterState()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<PlayerController>();
    }

    public virtual void OnExitState()
    {

    }

    public virtual void FixedUpdate()
    {
        UpdateJumpState();
        UpdateLateralMovement();
    }

    public enum JumpState { Stand, Jump, Hang, Fall }
    public JumpState jumpState = JumpState.Stand;
    float hangStartTime;
    protected int jumpCharges = 1;

    protected virtual void UpdateJumpState()
    {
        bool inputJump = Input.GetAxis("Jump") > 0f;
        switch (jumpState)
        {
            case JumpState.Stand:
                if(!player.isGrounded)
                {
                    jumpState = JumpState.Fall;
                }
                break;
            case JumpState.Jump:
                rb.velocity += Vector2.up * player.JUMP_GRAVITY * Time.fixedDeltaTime;
                if(rb.velocity.y <= 0)
                {
                    // transition to hang state
                    jumpState = JumpState.Hang;
                    hangStartTime = Time.time;
                }
                if(!inputJump)
                {
                    // transition to fall state
                    rb.velocity = Vector2.right * rb.velocity.x;
                    jumpState = JumpState.Fall;
                }
                break;
            case JumpState.Hang:
                rb.velocity += Vector2.up * player.HANG_GRAVITY * Time.fixedDeltaTime;
                if(!inputJump || Time.time - hangStartTime >= player.HANG_TIME)
                {
                    // transition to fall state
                    jumpState = JumpState.Fall;
                }
                break;
            case JumpState.Fall:
                rb.velocity += Vector2.up * player.FALL_GRAVITY * Time.fixedDeltaTime;
                if(player.isGrounded)
                {
                    jumpState = JumpState.Stand;
                }
                break;
        }

        if(rb.velocity.y < -player.MAX_FALL_SPEED)
        {
            rb.velocity = new Vector2(rb.velocity.x, -player.MAX_FALL_SPEED);
        }
    }
    public virtual void Jump()
    {
        rb.velocity += (Vector2)transform.up * player.JUMP_SPEED;
        jumpState = JumpState.Jump;
        jumpCharges--;

        player.ForceUngrounded();
    }

    public int launchCharges = 1;
    public void Launch(Vector2 dir)
    {
        Debug.DrawRay(transform.position, dir,Color.white,5f);
        rb.velocity += dir.normalized * Mathf.Min(player.MAX_LAUNCH_SPEED, dir.magnitude);

        player.ForceUngrounded();
    }

    void UpdateLateralMovement()
    {
        if(Mathf.Abs(Input.GetAxis("Horizontal")) > 0.001f || !player.isGrounded) { return; }

        float lateralSpeed = Vector2.Dot(transform.right, rb.velocity);
        float verticalSpeed = Vector2.Dot(transform.up, rb.velocity);
        if(lateralSpeed > player.LATERAL_MAX_SPEED)
        {
            rb.velocity = transform.up * verticalSpeed + transform.right * player.LATERAL_MAX_SPEED;
        }

        // apply drag
        int sign = lateralSpeed > 0f ? -1 : 1;
        float dx = Mathf.Min(Mathf.Abs(lateralSpeed), player.LATERAL_DRAG * Time.fixedDeltaTime);
        rb.velocity += (Vector2)transform.right * sign * dx;
    }

    public void LateralMove(float vx)
    {
        if(Mathf.Abs(Vector2.Dot(transform.right, rb.velocity)) < player.LATERAL_MAX_SPEED)
        {
            float dot = Vector2.Dot(Vector2.right, transform.right);
            int sign = 1;
            if(Mathf.Abs(dot) < 0.0001f)
            {
                sign = Vector2.Dot(Vector2.up, transform.right) > 0f ? 1 : -1;
            } else
            {
                sign = dot > 0f ? 1 : -1;
            }
            rb.velocity += sign * (Vector2)transform.right * vx;
        }
    }

    public virtual void OnGroundedEnter() {
        jumpCharges = 1;
        launchCharges = player.MAX_LAUNCH_CHARGES;

        // slow lateral movement
        rb.velocity = transform.up * Vector2.Dot(transform.up, rb.velocity) * 0.5f;
    }
    public virtual void OnGroundedExit() { }

    public virtual void OnDragStart(DragInfo drag)
    {
        Time.timeScale = 0.5f; // placeholder, TODO: create central time manager so multiple entities can interact with time while managing conflicts 
    }
    public virtual void OnDragEnd(DragInfo drag)
    {
        // end time slow
        Time.timeScale = 1f; // placeholder, TODO: create central time manager so multiple entities can interact with time while managing conflicts 
        if(launchCharges > 0)
        {
            Launch(drag.start - drag.end);
            launchCharges--;
        }
        Debug.Log("drag end");
    }
}
