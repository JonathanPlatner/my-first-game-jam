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
    private bool canChangeDirection;
    Direction movingDirection;

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
    float desiredAttackRange;

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
        
        // Create arrays to hold one timer per state type
        maxStateForces = new float[Enum.GetNames(typeof(State)).Length];
        enemySprite = gameObject.GetComponent<SpriteRenderer>();

        for (int i = 0; i < Mathf.Min(maxStateForces.Length, maxForces.Length); i++)
        {
            maxStateForces[i] = maxForces[i];
        }

        detectionSlope = Mathf.Tan(detectionAngle * Mathf.Deg2Rad);

        remainingCooldownTime = cooldownTime;
        canChangeDirection = true;
        movingDirection = rb.velocity.x < 0 ? Direction.Left : Direction.Right;
        desiredAttackRange = detectionRange + 1f;

        ChangeState(State.Patrolling);
        ChangeDirectionTo(Direction.Left);
        //ChangeDirectionTo(Direction.Right);
    }

    private void Update()
    {
        movingDirection = rb.velocity.x < 0 ? Direction.Left : Direction.Right;
        switch (state)
        {
            case State.Patrolling:
                Patrol();
                break;
            case State.Attacking:
                Attack();
                break;
        }

        if (LedgeDetect() && canChangeDirection)
        {
            // Don't allow the enemy to change directions right after they attempted to change directions.
            canChangeDirection = false;
            Invoke("EnableDirectionChange", 3f);
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

    private void ChangeState(State newState)
    {
        state = newState;
        moveForce = maxStateForces[(int)state];
    }

    private void FixedUpdate()
    {
        rb.AddForce(Vector2.right * moveForce);
        rb.AddForce(-rb.velocity.x * Vector2.right * dragFactor);
    }

    private void Patrol()
    {
        if (Detect(detectionRange * detectionRange))
        {
            ChangeState(State.Attacking);
            return;
        }
    }

    private void Attack()
    {
        FacePlayer();
        float distToPlayer = rb.position.x - target.position.x;
        if ((distToPlayer*distToPlayer) < (desiredAttackRange*desiredAttackRange))
        {
            Direction relativePlayerDir = GetPlayerDirection();
            if (relativePlayerDir == Direction.Left)
            {
                moveForce = Math.Abs(moveForce);
            }
            else
            {
                // Player is on the right, so move to the left to reach desiredAttackRange.
                if (moveForce > 0)
                    moveForce *= -1;
            }
        }

        if (!Detect(2 * (detectionRange * detectionRange)))
        {
            // Player out of range
            ChangeState(State.Patrolling);
            return;            
        }

        if (remainingCooldownTime <= 0)
        {
            // Shoot 
            remainingCooldownTime = cooldownTime;
            GameObject bullet = Instantiate(projectile, shotPoint.position, Quaternion.identity);
            bullet.GetComponent<Rigidbody2D>().AddForce(shotPoint.up * 450);
        }
    }

    private void FacePlayer()
    {
        Direction relativePlayerDir = GetPlayerDirection();
        ChangeDirectionTo(relativePlayerDir);
    }

    private bool Detect(float rangeSqr)
    {
        if (target != null)
        {
            if ((rb.position - (Vector2)target.position).sqrMagnitude <= rangeSqr)
            {
                float x = facing == Direction.Left ? rb.position.x - target.position.x : target.position.x - rb.position.x;
                float y = target.position.y - rb.position.y;
                if (y < Math.Abs(x * detectionSlope) && y > 0)
                {
                    RaycastHit2D hit = Physics2D.Raycast(rb.position, (Vector2)target.position - rb.position);
                    if (hit == null)
                        return false;
                    if (hit.collider.tag == "Player")
                    {
                        Debug.DrawLine(rb.position, hit.point, Color.red);
                        return true;
                    }
                    else
                    {
                        Debug.DrawLine(rb.position, hit.point, Color.blue);
                    }

                }
                else
                {
                    Debug.DrawLine(rb.position, target.position, Color.green);
                }

            }
        }
        return false;
    }

    private void ChangeDirectionTo(Direction newDirection)
    {
        if (movingDirection != newDirection)
        {
            moveForce *= -1;
        }
        if (newDirection != facing)
        {
            // Flip shotpoint
            Vector3 newPos = shotPoint.transform.localPosition;
            Quaternion newRot = shotPoint.transform.localRotation;
            newPos.x *= -1;
            newRot.z *= -1;
            shotPoint.transform.localPosition = newPos;
            shotPoint.transform.localRotation = newRot;
        }
        facing = newDirection;
        enemySprite.flipX = newDirection == Direction.Right;
    }

    private bool LedgeDetect()
    {
        RaycastHit2D hit = Physics2D.Raycast(rb.position + (movingDirection == Direction.Left ? Vector2.left : Vector2.right), Vector2.down, 0.5f);
        Debug.DrawRay(rb.position + (movingDirection == Direction.Left ? Vector2.left : Vector2.right), Vector2.down * 0.5f);
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

    private void EnableDirectionChange()
    {
        canChangeDirection = true;
    }

    private Direction GetPlayerDirection()
    {
        return rb.position.x < target.position.x ? Direction.Right : Direction.Left;
    }
}