using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - target.position;
    }

    void Update()
    {
        transform.position = new Vector3(target.position.x, transform.position.y, transform.position.z);
        
    }
}
