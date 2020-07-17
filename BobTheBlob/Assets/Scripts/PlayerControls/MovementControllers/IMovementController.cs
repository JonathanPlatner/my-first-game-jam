using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MovementController: MonoBehaviour{
    public Rigidbody2D rb;
    public PlayerController player;

    public void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<PlayerController>();
    }

    public virtual void EnterState(MovementController prev) { }

    public virtual void ExitState(MovementController next) { }

    public virtual void OnGroundedEnter() {
        player.launchCharges = player.MAX_LAUNCH_CHARGES;
    }
    public virtual void OnGroundedExit() { }

    public virtual void OnDragStart(DragInfo drag) { }
    public virtual void OnDragEnd(DragInfo drag) { }

    public void Launch(Vector2 direction)
    {
        rb.velocity += direction.normalized * player.ACCELERATION_RATE;
    }
}
