using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallBlock : MonoBehaviour
{
    public float dropDelay = 0.3f;
    public float dropSpeed = 5f;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            Invoke("StartDrop", dropDelay);
        }
    }

    void StartDrop()
    {
        GetComponent<Rigidbody2D>().velocity = Vector2.down * dropSpeed;
        Invoke("Deactivate", 10f);
    }

    void Deactivate()
    {
        gameObject.SetActive(false);
    }

}
