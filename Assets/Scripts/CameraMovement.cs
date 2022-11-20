using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] public float speed = 0.06f;
    [SerializeField] public float zoomSpeed = 10.0f;
    [SerializeField] public float rotateSpeed;
    
    [SerializeField] public float maxHeight = 40f;
    [SerializeField] public float minHeight = 4f;
    
    void Update()
    {
        var t = transform;
        float horizontalSpeed = speed * Input.GetAxis("Horizontal");
        float verticalSpeed = speed * Input.GetAxis("Vertical");
        float scrollSpeed = -zoomSpeed * Input.GetAxis("Mouse ScrollWheel");

        Vector3 verticalMove = new Vector3(0, scrollSpeed, 0);
        Vector3 lateralMove = horizontalSpeed * t.right;
        Vector3 forwardMove = t.forward;
        forwardMove.y = 0;
        forwardMove.Normalize();
        forwardMove *= verticalSpeed;

        Vector3 move = verticalMove + lateralMove + forwardMove;

        t.position += move;
    }
}
