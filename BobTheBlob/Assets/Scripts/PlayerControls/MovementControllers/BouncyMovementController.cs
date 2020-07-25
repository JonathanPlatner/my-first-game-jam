using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncyMovementController : MovementController
{
    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // lateral movement input
        float inputX = Input.GetAxis("Horizontal");
        LateralMove(inputX * player.LATERAL_ACCELERATION * Time.fixedDeltaTime);

    }

    public void Update()
    {
        // jump input
        if(player.isGrounded && Input.GetKeyDown(KeyCode.Space) && jumpCharges > 0)
        {
            Jump();
        }
    }
}
