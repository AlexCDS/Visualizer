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

    void Update()
    {
        if (InputController.Touches == 2)
        {
            Vector3 rot = InputController.Delta * rotSpeed * Time.deltaTime;
            xRot += rot.y;
            yRot += rot.x;

            rotationAnchor.transform.rotation = Quaternion.Euler(xRot, yRot, 0);
        }
        else if(InputController.Touches == 1)
        {
            gameObject.transform.position = gameObject.transform.position + gameObject.transform.rotation * InputController.Delta * Time.deltaTime;
        }

        if (InputController.ScrollDelta.magnitude > 0)
        {
            gameObject.transform.position += gameObject.transform.forward * zoomSpeed * InputController.ScrollDelta.magnitude * Mathf.Sign(Input.mouseScrollDelta.y) * Time.deltaTime;
        }
    }
}
