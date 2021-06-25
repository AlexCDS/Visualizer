using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public GameObject rotationAnchor;

    public float minDiffLenght = 0.35f;
    public float maxDiffLenght = 1f;
    public float speed = 1f;
    public float zoomSpeed = 1f;
    public float rotSpeed = 1f;

    public AnimationCurve movementIntensity;

    float xRot = 0;
    float yRot = 0;

    Vector2 previousTouchPosition;
    Vector3 dir;
    
    void Update()
    {
        InputActionMap map = new InputActionMap("Camera Controller");

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            previousTouchPosition = Input.mousePosition;
        }
        
        if (Input.GetMouseButton(1))
        {
            Vector3 rot = dir * rotSpeed * Time.deltaTime;
            xRot += rot.y;
            yRot += rot.x;

            rotationAnchor.transform.rotation = Quaternion.Euler(xRot, yRot, 0);
        }
        else if (Input.GetMouseButton(0))
        {
            gameObject.transform.position = gameObject.transform.position + gameObject.transform.rotation * dir * Time.deltaTime;
        }
        else
            dir = Vector3.zero;

        if (Input.mouseScrollDelta.magnitude > 0)
        {
            gameObject.transform.position += gameObject.transform.forward * zoomSpeed * Input.mouseScrollDelta.magnitude * Mathf.Sign(Input.mouseScrollDelta.y) * Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            Vector2 diff = Input.mousePosition.ToVector2() - previousTouchPosition;
            if (diff.magnitude > minDiffLenght)
            {
                float t = (diff.magnitude - minDiffLenght) / (maxDiffLenght - minDiffLenght);
                float s = movementIntensity.Evaluate(t) * speed;

                diff = diff = diff.normalized * s;
                dir = new Vector3(diff.x, diff.y);
            }
            else
                dir = Vector3.zero;

            previousTouchPosition = Input.mousePosition;
        }
    }
}

public static class Extension
{
    public static Vector2 ToVector2(this Vector3 v) => new Vector2(v.x, v.y);
}
