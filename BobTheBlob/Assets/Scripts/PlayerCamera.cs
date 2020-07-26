using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField]
    private Transform lowerLeft;
    [SerializeField]
    private Transform upperRight;

    [SerializeField]
    private Transform target;
    [SerializeField]
    private float snap = 0.05f;

    private Camera cam;

    private float minX;
    private float minY;
    private float maxX;
    private float maxY;

    private void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }
    private void FixedUpdate()
    {
        if(target != null)
        {
            Vector3 targetCameraPosition = transform.position * (1 - snap) + target.position * snap;
            targetCameraPosition.z = transform.position.z;

            transform.position = UpdateCameraBounds(targetCameraPosition);
        }

    }
    private Vector3 UpdateCameraBounds(Vector3 desiredPosition)
    {
        float cameraWidth = cam.aspect * cam.orthographicSize;
        float cameraHeight = cam.orthographicSize;

        minX = lowerLeft.position.x + cameraWidth;
        minY = lowerLeft.position.y + cameraHeight;

        maxX = upperRight.position.x - cameraWidth;
        maxY = upperRight.position.y - cameraHeight;

        Vector3 actualPosition = desiredPosition;
        if(desiredPosition.x < minX) actualPosition.x = minX;
        if(desiredPosition.y < minY) actualPosition.y = minY;
        if(desiredPosition.x > maxX) actualPosition.x = maxX;
        if(desiredPosition.y > maxY) actualPosition.y = maxY;

        return actualPosition;
    }
}
