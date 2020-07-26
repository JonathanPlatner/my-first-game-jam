using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Charger : Enemy
{
    private new string name = "Charger";
    public override string Name { get { return name; } }
    public override int MaxHealth { get { return 10; } }

    private Transform target;
    public override Transform Target { get { return target; } }

    enum Facing { Left, Right }
    private Facing facing;
    enum State { Idle, Patrolling, Attacking, }
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
    private float dragFactor = 1;

    [Header("Detection")]
    [SerializeField]
    private float detectionRange = 5f;
    [SerializeField]
    private float detectionAngle = 20;
    private float detectionSlope; // Going to convert the angle into a slope, so I don't have to work with inverse tangents (cheaper computationally)
    private float detectionXValue;

    [Header("Attacking")]
    [SerializeField]
    private Transform blade;
    [SerializeField]
    private SpriteRenderer bladeSprite;
    [SerializeField]
    private float swingSpeed = 10;
    [SerializeField]
    private float swingMax = 100;

    [SerializeField]
    private AudioSource hitSource;

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

        facing = (Facing)UnityEngine.Random.Range(0, 2);
        blade.rotation = Quaternion.Euler(0, 0, 90);
    }

    private void Update()
    {

        Idle();
        Patrol();
        Attack();
        if(justTransitioned)
        {
            ResetTimers();
            justTransitioned = false;
        }
        if(Dead())
        {
            Destroy(gameObject);
        }
        //Debug.Log(state);
        //Swing();

    }
    private void FixedUpdate()
    {
        rb.AddForce(Vector2.right * moveForce);
        rb.AddForce(-rb.velocity.x * Vector2.right * dragFactor);
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


            PrepareSwing();
            // Transition Conditions
            stateTimers[(int)state] += Time.deltaTime;
            if(stateTimers[(int)state] >= stateActionDurations[(int)state])
            {
                state = State.Patrolling;
                justTransitioned = true;
                return;
            }
            if(Detect())
            {
                //Debug.Log("Detected");
                state = State.Attacking;
                justTransitioned = true;
                SetTargetAngle(bladeAngle, facing == Facing.Left ? -20 : 20, 0.2f);
                return;
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



            //RaycastHit2D hit = Physics2D.Raycast(rb.position + (facing == Facing.Left ? Vector2.left : Vector2.right), Vector2.down * 0.6f, 1f);
            //Debug.DrawRay(rb.position + (facing == Facing.Left ? Vector2.left : Vector2.right), Vector2.down * 0.6f);
            if(Detect())
            {
                //Debug.Log("Detected");
                moveForce = 0;
                state = State.Attacking;
                justTransitioned = true;
                SetTargetAngle(bladeAngle, facing == Facing.Left ? -20 : 20, 0.2f);
                return;
            }
            if(LedgeDetect())
            {
                //if(hit.collider.tag != "Ground")
                //{
                state = State.Idle;
                justTransitioned = true;
                facing = (Facing)(1 - (int)facing);
                return;
                //}
            }
            PrepareSwing();
            //else if(hit.collider == null)
            //{
            //    state = State.Idle;
            //    justTransitioned = true;
            //    facing = (Facing)(1 - (int)facing);
            //    return;
            //}


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

    private void Attack()
    {
        if(state == State.Attacking)
        {
            moveForce = maxStateForces[(int)state];
            if(facing == Facing.Left) moveForce *= -1;


            if(target.position.x > rb.position.x)
            {
                facing = Facing.Right;
            }
            else
            {
                facing = Facing.Left;
            }
            moveForce = maxStateForces[(int)state];
            if(facing == Facing.Left) moveForce *= -1;

            if(LedgeDetect())
            {
                if(Mathf.Abs(rb.velocity.x) > 0.1f)
                {
                    moveForce = -rb.velocity.x * 2 * dragFactor;
                }
                else
                {
                    moveForce = 0;
                }
                PrepareSwing();
            }
            else
            {
                Swing();
            }
            if(LoseDetect())
            {
                state = State.Idle;
                justTransitioned = true;
                return;
            }
        }
    }

    private bool Detect()
    {
        if(target != null)
        {
            if((rb.position - (Vector2)target.position).sqrMagnitude <= detectionRange * detectionRange)
            {
                float x = facing == Facing.Left ? rb.position.x - target.position.x : target.position.x - rb.position.x;
                float y = target.position.y - rb.position.y;
                if(y < x * detectionSlope && y > 0)
                {
                    RaycastHit2D hit = Physics2D.Raycast(rb.position, (Vector2)target.position - rb.position);
                    if(hit.collider.tag == "Player")
                    {
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
        if(hit.transform != null)
        {
            if(hit.transform.tag != "Player")
            {
                return true;
            }
        }
        return false;
    }

    private float interpolator;
    private float swingDuration = 1;
    private float bladeAngle;
    private float startAngle;
    private float endAngle;
    private bool swing;
    private void Swing()
    {

        interpolator += Time.deltaTime / swingDuration;
        bladeAngle = startAngle * (1 - interpolator) + endAngle * interpolator;
        if(interpolator >= 1)
        {
            interpolator = 1;
            bladeAngle = startAngle * (1 - interpolator) + endAngle * interpolator;
            swing = !swing;
            if(swing)
            {
                SetTargetAngle(bladeAngle, facing == Facing.Left ? 110 : -110, 0.2f);
            }
            else
            {
                SetTargetAngle(bladeAngle, facing == Facing.Left ? -20 : 20, 0.2f);
            }

        }

        blade.rotation = Quaternion.Euler(0, 0, bladeAngle + 90);
    }

    private void PrepareSwing()
    {
        interpolator += Time.deltaTime / swingDuration;
        bladeAngle = startAngle * (1 - interpolator) + endAngle * interpolator;
        if(interpolator >= 1)
        {
            interpolator = 1;
            bladeAngle = startAngle * (1 - interpolator) + endAngle * interpolator;

            SetTargetAngle(bladeAngle, facing == Facing.Left ? -20 : 20, 1f);
        }

        blade.rotation = Quaternion.Euler(0, 0, bladeAngle + 90);
    }
    private void SetTargetAngle(float start, float end, float duration)
    {
        interpolator = 0;
        startAngle = start; endAngle = end; swingDuration = duration;
    }

    private bool LedgeDetect()
    {
        RaycastHit2D hit = Physics2D.Raycast(rb.position + (facing == Facing.Left ? Vector2.left : Vector2.right), Vector2.down, 0.3f);
        //Debug.DrawRay(rb.position + (facing == Facing.Left ? Vector2.left : Vector2.right), Vector2.down * 0.3f);
        if(hit.collider != null)
        {
            if(hit.collider.tag != "Ground")
            {
                return true;
            }
        }
        else if(hit.collider == null)
        {
            return true;
        }
        return false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.tag == "Player")
        {
            Player player = collision.transform.GetComponent<Player>();
            Vector2 toPlayer = (Vector2)player.transform.position - rb.position;
            float velocityComponentTowardEnemy = Vector2.Dot(player.Velocity - rb.velocity, toPlayer) / toPlayer.magnitude;
            if (velocityComponentTowardEnemy > 2)
            {

                player.Bounce(Vector2.up * velocityComponentTowardEnemy + player.Velocity.x * Vector2.right);
                TakeDamage((int)(velocityComponentTowardEnemy / 2));
                hitSource.Play();
            }
        }
        else if (collision.collider.tag == "PlayerBullet")
        {
            TakeDamage(2);
            hitSource.Play();
        }
    }
}
