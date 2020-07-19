using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Ranger : Enemy
{
    private new string name = "Ranger";
    public override string Name { get { return name; } }

    private Transform target;
    public override Transform Target { get { return target; } }

    enum Direction { Left, Right }
    private Direction facing;
    enum State { Idle, Patrolling, Attacking }

    private State state;

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

    [Header("Attacking")]
    [SerializeField]
    private float cooldownTime = 0.25f;
    private float remainingCooldownTime;
    public GameObject projectile;
    public Transform shotPoint;
    private float detectionSlope;

    private void Start()
    {
        try
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
        catch (NullReferenceException e)
        {
            Debug.LogWarning(e.Message);
        }
        state = State.Patrolling;

        // Create arrays to hold one timer per state type
        maxStateForces = new float[Enum.GetNames(typeof(State)).Length];
        enemySprite = gameObject.GetComponent<SpriteRenderer>();

        for (int i = 0; i < Mathf.Min(maxStateForces.Length, maxForces.Length); i++)
        {
            maxStateForces[i] = maxForces[i];
        }

        detectionSlope = Mathf.Tan(detectionAngle * Mathf.Deg2Rad);

        remainingCooldownTime = cooldownTime;

        ChangeDirectionTo(Direction.Left);
    }

    private void Update()
    {
        switch (state)
        {
            case State.Patrolling:
                Patrol();
                break;
            case State.Attacking:
                Attack();
                break;
        }

        if (LedgeDetect())
        {
            switch (state)
            {
                case State.Patrolling:
                    Direction newDirection = facing == Direction.Right || rb.velocity.x > 0 ? Direction.Left : Direction.Right;
                    ChangeDirectionTo(newDirection);
                    break;
                case State.Attacking:
                    moveForce = 0;
                    break;
            }
        }

        remainingCooldownTime -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        rb.AddForce(Vector2.right * moveForce);
        rb.AddForce(-rb.velocity.x * Vector2.right * dragFactor);
    }

    private void Patrol()
    {
        moveForce = maxStateForces[(int)state];
        if (facing == Direction.Left) moveForce *= -1;

        if (Detect())
        {
            state = State.Attacking;
            return;
        }
    }

    private void Attack()
    {
        //float distToPlayerSqr = (rb.position - (Vector2)target.position).sqrMagnitude;
        moveForce = maxStateForces[(int)state];
        if (remainingCooldownTime <= 0)
        {
            Instantiate(projectile, shotPoint.position, Quaternion.identity);
            remainingCooldownTime = cooldownTime;
        }
    }

    private bool Detect()
    {
        if (target != null)
        {
            if ((rb.position - (Vector2)target.position).sqrMagnitude <= detectionRange * detectionRange)
            {
                float x = facing == Direction.Left ? rb.position.x - target.position.x : target.position.x - rb.position.x;
                float y = target.position.y - rb.position.y;
                if (y < x * detectionSlope && y > 0)
                {
                    RaycastHit2D hit = Physics2D.Raycast(rb.position, (Vector2)target.position - rb.position);
                    if (hit.collider.tag == "Player")
                    {
                        Debug.DrawLine(rb.position, hit.point, Color.red);
                        return true;
                    }
                    else
                    {
                        Debug.DrawLine(rb.position, hit.point, Color.red);
                    }

                }
                else
                {
                    Debug.DrawLine(rb.position, target.position, Color.red);
                }

            }      
        }
        return false;
    }

    private void ChangeDirectionTo(Direction newDirection)
    {
        facing = newDirection;
        moveForce *= -1;
        enemySprite.flipX = newDirection == Direction.Right;
    }

    private bool LedgeDetect()
    {
        Direction movingDirection = rb.velocity.x < 0 ? Direction.Left : Direction.Right;
        RaycastHit2D hit = Physics2D.Raycast(rb.position + (movingDirection == Direction.Left ? Vector2.left : Vector2.right), Vector2.down, 0.5f);
        //Debug.DrawRay(rb.position + (movingDirection == Direction.Left ? Vector2.left : Vector2.right), Vector2.down * 0.5f);
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