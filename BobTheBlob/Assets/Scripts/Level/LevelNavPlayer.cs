using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelNavPlayer : MonoBehaviour
{
    private Vector3[] levelPositions;
    public int index;
    [SerializeField]
    private Rigidbody2D rb;
    private int maxIndex = 0;


    private Vector2 input;
    private void Start()
    {
        Transform levelParent = GameObject.Find("Levels").transform;
        levelPositions = new Vector3[levelParent.childCount];
        for(int i = 0; i < levelPositions.Length; i++)
        {
            levelPositions[i] = levelParent.GetChild(i).position;
        }
        index = PersistentData.LevelIndex;
        maxIndex = PersistentData.maxLevel;
        rb.MovePosition(levelPositions[index]);
    }

    private void Update()
    {
        input = Vector2.zero;
        if(Input.GetKeyDown(KeyCode.W))
        {
            input.y++;
        }
        if(Input.GetKeyDown(KeyCode.S))
        {
            input.y--;
        }
        if(Input.GetKeyDown(KeyCode.D))
        {
            input.x++;
        }
        if(Input.GetKeyDown(KeyCode.A))
        {
            input.x--;
        }

        Vector2 forward = Vector2.zero;
        Vector2 backward = Vector2.zero;

        if(index > 0)
        {
            backward = levelPositions[index - 1] - levelPositions[index];
        }
        if(index < levelPositions.Length - 1)
        {
            forward = levelPositions[index + 1] - levelPositions[index];
        }

        if(Vector2.Dot(forward, input) > 0)
        {
            if(index < maxIndex)
            {
                index++;
            }
        }
        else if(Vector2.Dot(backward, input) > 0)
        {
            index--;
        }
        PersistentData.LevelIndex = index;
    }

    private void FixedUpdate()
    {
        rb.MovePosition(levelPositions[index]);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(collision.tag == "Level")
            {
                collision.gameObject.GetComponent<LevelTransition>().Load();
            }
        }
    }

}
