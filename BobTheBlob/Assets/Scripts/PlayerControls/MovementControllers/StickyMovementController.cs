using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyMovementController : MovementController {

    BoxCollider2D box;

    void ResetRotation()
    {
        transform.position = box.bounds.center + Vector3.down * box.bounds.extents.y;
        transform.rotation = Quaternion.identity;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();
        box = GetComponent<BoxCollider2D>();
    }

    public override void OnExitState()
    {
        base.OnExitState();
        ResetRotation();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        // lateral movement input
        float inputX = Input.GetAxis("Horizontal");
        LateralMove(inputX * player.LATERAL_ACCELERATION * Time.fixedDeltaTime);


        // reset rotation if not sticking to anything
        if(!player.isGrounded && transform.rotation != Quaternion.identity)
        {
            ResetRotation();
        }
    }

    public void Update()
    {
        // jump input
        if(player.isGrounded && Input.GetKeyDown(KeyCode.Space) && jumpCharges > 0)
        {
            Jump();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //if(collision.gameObject.layer.Equals(player.groundLayer))
        //{
        Debug.Log("collision, grounded: " + player.isGrounded);
            if (!player.isGrounded)
            {
                // stick to surface
                transform.rotation = Quaternion.LookRotation(Vector3.forward, collision.contacts[0].normal);
                transform.position = collision.contacts[0].point;
            Debug.Log(collision.contacts[0].normal);
            Debug.DrawRay(collision.contacts[0].point, collision.contacts[0].normal, Color.blue);
            //jumpState = JumpState.Stand;
            }
            //Debug.Break();
        //}
    }

    protected override void UpdateJumpState()
    {
        if(!player.isGrounded)
        {
            base.UpdateJumpState();
        }
    }

    public override void OnGroundedExit()
    {
        base.OnGroundedExit();
        transform.rotation = Quaternion.identity;
    }

}