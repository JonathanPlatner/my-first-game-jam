using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class CannonMovementController : MovementController 
{
    float lastShootTime = 0f;
    Transform projectileContainer;
    const String CONTAINER_NAME = "ProjectileContainer";

    public override void OnEnterState()
    {
        base.OnEnterState();
        try
        {
            projectileContainer = GameObject.Find(CONTAINER_NAME).transform;
        } catch(NullReferenceException e)
        {
            projectileContainer = new GameObject(CONTAINER_NAME).transform;
        }
    }

    public override void OnDragStart(DragInfo drag){}
    public override void OnDragEnd(DragInfo drag)
    {
        if(Time.time - lastShootTime >= player.SHOOT_COOLDOWN)
        {
            Shoot((Camera.main.ScreenToWorldPoint(drag.end) - transform.position).normalized);
        }
    }

    void Shoot(Vector2 dir)
    {
        GameObject projectile = Instantiate(player.PROJECTILE_PREFAB, transform.position + (Vector3) dir, Quaternion.identity, projectileContainer).gameObject;
        projectile.GetComponent<Rigidbody2D>().velocity = dir * player.SHOOT_SPEED;
        lastShootTime = Time.time;
    }
}
