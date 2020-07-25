using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed;
    private float timeToLive = 1f;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, timeToLive);
    }
}
