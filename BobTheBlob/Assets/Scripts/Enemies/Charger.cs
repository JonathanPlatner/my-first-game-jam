using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Charger : Enemy
{
    private new string name = "Charger";
    public override string Name { get { return name; } }

    private Transform target;
    public override Transform Target { get { return target; } }

    enum Facing { Left, Right }
    private Facing facing;
    enum State { Idle, Patrolling, Preparing, Charging, Waiting }
    // State explanation
    // Idle - standing around doing nothing. Will switch to patrolling after a random amount of time.
    // Patrolling - moving in a straight line. Will switch to idle at the edge of a platform or after a random amount of time.
    // Preparing - Has detected the player, preparing to charge. If it has previously hit the player, it will back up to a minimum distance and charge again.
    // Charging - charging at the player. Will return to preparing after it hits or passes by the player
    // Waiting - Player is detected, but out of reach. Will wait at a set horizontal distance from the player. Will immediately transition to charging after the player is accessible again
    private State state;
    private bool justTransitioned;

    private float[] stateTimers;
    private float[] stateActionDurations;

    [Header("Motion")]
    [SerializeField]
    private Rigidbody2D rb;
    private float[] maxStateForces;
    [SerializeField]
    private float[] maxForces;
    private float moveForce;
    [SerializeField]
    private float dragFactor;

    [SerializeField]
    private float detectionRange = 5f;
    [SerializeField]
    private float detectionAngle = 40f;
    private float detectionSlope; // Going to convert the angle into a slope, so I don't have to work with inverse tangents (cheaper computationally)
    private float detectionXValue;

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
        state = State.Idle;

        /* Create a float array of timers and durations for each state the enemy can be in (this is often very useful for AI). This code will also grab the current size of the state enum,
           so the float arrays will always have the correct size. */
        stateTimers = new float[Enum.GetNames(typeof(State)).Length];
        stateActionDurations = new float[Enum.GetNames(typeof(State)).Length];
        maxStateForces = new float[Enum.GetNames(typeof(State)).Length];
        for(int i = 0; i < Mathf.Min(maxStateForces.Length, maxForces.Length); i++)
        {
            maxStateForces[i] = maxForces[i];
        }
        justTransitioned = true;

        detectionSlope = Mathf.Tan(detectionAngle * Mathf.Deg2Rad);
        detectionXValue = Mathf.Cos(detectionAngle * Mathf.Deg2Rad) * detectionRange;
    }

    private void Update()
    {

        Idle();
        Patrol();
        Prepare();
        if(justTransitioned)
        {
            ResetTimers();
            justTransitioned = false;
        }

        //Debug.Log(state);

    }
    private void FixedUpdate()
    {
        rb.AddForce(Vector2.right * moveForce);
        //Debug.Log(moveForce);
        // Drag
        rb.AddForce(-rb.velocity.x * Vector2.right * dragFactor);
        //Debug.Log(rb.velocity.x);
    }

    private void ResetTimers()
    {
        for(int i = 0; i < stateTimers.Length; i++)
        {
            if((int)state != i)
            {
                stateTimers[i] = 0;
            }
        }
    }

    private void Idle()
    {
        if(state == State.Idle)
        {
            if(justTransitioned)
            {
                stateActionDurations[(int)state] = UnityEngine.Random.Range(4f, 6f);
            }

            if(Mathf.Abs(rb.velocity.x) > 0.1f)
            {
                moveForce = -rb.velocity.x * 2 * dragFactor;
            }
            else
            {
                moveForce = 0;
            }

           

            // Transition Conditions
            stateTimers[(int)state] += Time.deltaTime;
            if(stateTimers[(int)state] >= stateActionDurations[(int)state])
            {
                state = State.Patrolling;
                justTransitioned = true;
            }
            if(Detect())
            {
                //Debug.Log("Detected");
                state = State.Preparing;
                justTransitioned = true;
            }
        }
    }
    private void Patrol()
    {
        if(state == State.Patrolling)
        {
            if(justTransitioned)
            {
                stateActionDurations[(int)state] = UnityEngine.Random.Range(4f, 6f);
            }

            // Actions
            // Move in a direction (determined by facing). Stop and switch directions if a wall or ledge is encountered.
            moveForce = maxStateForces[(int)state];
            if(facing == Facing.Left) moveForce *= -1;


            RaycastHit2D hit = Physics2D.Raycast(rb.position + (facing == Facing.Left ? Vector2.left : Vector2.right), Vector2.down * 0.6f, 1f);
            //Debug.DrawRay(rb.position + (facing == Facing.Left ? Vector2.left : Vector2.right), Vector2.down * 0.6f);
            if(Detect())
            {
                //Debug.Log("Detected");
                moveForce = 0;
                state = State.Preparing;
                justTransitioned = true;
                return;
            }
            if(hit.collider != null)
            {
                if(hit.collider.tag != "Ground")
                {
                    state = State.Idle;
                    justTransitioned = true;
                    facing = (Facing)(1 - (int)facing);
                    return;
                }
            }
            else if(hit.collider == null)
            {
                state = State.Idle;
                justTransitioned = true;
                facing = (Facing)(1 - (int)facing);
                return;
            }


            // Transition Conditions
            stateTimers[(int)state] += Time.deltaTime;
            if(stateTimers[(int)state] >= stateActionDurations[(int)state])
            {
                state = State.Idle;
                justTransitioned = true;
                return;
            }
        }
    }

    private void Prepare()
    {
        if (state == State.Preparing)
        {
            if(LoseDetect())
            {
                //Debug.Log("Lost");
                state = State.Idle;
                justTransitioned = true;
            }
        }
    }

    private bool Detect()
    {
        //Debug.DrawLine(rb.position, rb.position +
        //    (facing == Facing.Left ? Vector2.left : Vector2.right) * detectionXValue +
        //    Vector2.up * detectionSlope * detectionXValue);

        //Debug.DrawLine(rb.position, rb.position +
        //   (facing == Facing.Left ? Vector2.left : Vector2.right) * detectionXValue +
        //   Vector2.down * detectionSlope * detectionXValue);

        if(target != null)
        {
            if((rb.position - (Vector2)target.position).sqrMagnitude <= detectionRange * detectionRange)
            {
                float x = facing == Facing.Left ? rb.position.x - target.position.x : target.position.x - rb.position.x;
                float y = target.position.y - rb.position.y;
                if(Mathf.Abs(y) < x * detectionSlope)
                {
                    RaycastHit2D hit = Physics2D.Raycast(rb.position, (Vector2)target.position - rb.position);
                    //Debug.Log(hit.transform.tag);
                    //Debug.DrawRay(rb.position, (Vector2)target.position - rb.position);
                    if (hit.collider.tag == "Player")
                    {
                        //Debug.Log("Detected");
                        //Debug.DrawLine(rb.position, hit.point, Color.green);
                        return true;
                    }
                    else
                    {
                        //Debug.DrawLine(rb.position, hit.point, Color.red);
                    }
                    
                }
                else
                {
                    //Debug.DrawLine(rb.position, target.position, Color.red);
                }

            }

        }
        return false;
    }

    private bool LoseDetect()
    {
        RaycastHit2D hit = Physics2D.Raycast(rb.position, (Vector2)target.position - rb.position);
        //Debug.DrawRay(rb.position, (Vector2)target.position - rb.position);
        if (hit.transform.tag != "Player")
        {
            return true;
        }
        return false;
    }
}
