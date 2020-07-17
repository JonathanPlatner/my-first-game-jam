using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Ranger : Enemy
{
    private new string name = "Ranger";
    public override string Name { get { return name; } }

    private Transform target;
    public override Transform Target { get { return target; } }

    enum Facing { Left, Right }
    private Facing facing;
    enum State { Idle, Patrolling, Attacking }

    private State state;
    private bool justTransitioned;

    //private float[] stateTimers;
    //private float[] stateActionDurations;

    [Header("Motion")]
    [SerializeField]
    private Rigidbody2D rb;
    private float[] maxStateForces;
    [SerializeField]
    private float[] maxForces;
    private float moveForce;
    [SerializeField]
    private float dragFactor = 1;
    private SpriteRenderer enemySprite;

    [Header("Detection")]
    [SerializeField]
    private float detectionRange = 5f;
    [SerializeField]
    private float detectionAngle = 20;
    //private float detectionSlope;
    //private float detectionXValue;

    private void Start()
    {
        try
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
        catch(NullReferenceException e)
        {
            Debug.LogWarning(e.Message);
        }
        state = State.Patrolling;

        // Create arrays to hold one timer per state type
        //stateTimers = new float[Enum.GetNames(typeof(State)).Length];
        //stateActionDurations = new float[Enum.GetNames(typeof(State)).Length];
        maxStateForces = new float[Enum.GetNames(typeof(State)).Length];
        for (int i=0; i<Mathf.Min(maxStateForces.Length, maxForces.Length); i++)
        {
            maxStateForces[i] = maxForces[i];
        }
        justTransitioned = true;

        //detectionSlope = Mathf.Tan(detectionAngle * Mathf.Deg2Rad);
        //detectionXValue = Mathf.Cos(detectionAngle * Mathf.Deg2Rad) * detectionRange;

        enemySprite = gameObject.GetComponent<SpriteRenderer>();
        //facing = (Facing)UnityEngine.Random.Range(0, 2);
        facing = Facing.Right;
    }

    private void Update()
    {
        Patrol();
        enemySprite.flipX = facing == Facing.Right;
        // Attack();
        // if (justTransitioned)
        // {
        //     ResetTimers();
        //     justTransitioned = false;
        // }
    }

    private void FixedUpdate()
    {
        rb.AddForce(Vector2.right * moveForce);
        rb.AddForce(-rb.velocity.x * Vector2.right * dragFactor);
    }

    private void Patrol()
    {
        if (state == State.Patrolling)
        {
            moveForce = maxStateForces[(int)state];
            if (facing == Facing.Left) moveForce *= -1;

            if (LedgeDetect())
            {
                facing = facing == Facing.Left ? Facing.Right : Facing.Left;
            }
        }
    }

    private bool LedgeDetect()
    {
        RaycastHit2D hit = Physics2D.Raycast(rb.position + (facing == Facing.Left ? Vector2.left : Vector2.right), Vector2.down, 0.3f);
        Debug.DrawRay(rb.position + (facing == Facing.Left ? Vector2.left : Vector2.right), Vector2.down * 0.3f);
        if (hit.collider != null)
        {
            if (hit.collider.tag != "Ground")
            {
                return true;
            }
        }
        else if (hit.collider == null)
        {
            return true;
        }
        return false;
    }
}