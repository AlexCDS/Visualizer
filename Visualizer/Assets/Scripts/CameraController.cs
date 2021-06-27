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
    public float rotSpeedYaw = 360f;
    public float rotSpeedPitch = 135f;

    public float velocityDampening = 1f;

    public AnimationCurve movementIntensity;

    float pitch = 0;
    float yaw = 0;
    private Vector3 velocity;
    private Vector3 position;

    private void Start()
    {
        position = gameObject.transform.position;
    }
    
    void Update()
    {
        if (InputController.TouchCount == 2)
        {
            Vector3 rot = new Vector3(movementIntensity.Evaluate(Mathf.Abs(InputController.Delta.x) / Screen.width) * Mathf.Sign(InputController.Delta.x) * rotSpeedYaw,
                movementIntensity.Evaluate(Mathf.Abs(InputController.Delta.y) / Screen.height) * Mathf.Sign(InputController.Delta.y) * rotSpeedPitch);

            pitch = Mathf.Clamp(pitch + rot.y, -45, 90);
            yaw += rot.x;

            rotationAnchor.transform.rotation = Quaternion.Euler(pitch, yaw, 0);
            position = gameObject.transform.position;
        }
        else if(InputController.TouchCount == 1)
        {
            velocity = rotationAnchor.transform.rotation * (movementIntensity.Evaluate(InputController.Delta.magnitude / Screen.width) * InputController.Delta) * speed;
        }

        if(velocity.magnitude > 0)
        {
            Vector3 boundaries = Camera.main.WorldToScreenPoint(Vector3.zero, Camera.MonoOrStereoscopicEye.Mono);

            float rightDot = Vector3.Dot(velocity.normalized, rotationAnchor.transform.right);
            float upDot = Vector3.Dot(velocity.normalized, rotationAnchor.transform.up);


            if (boundaries.x > Screen.width && Mathf.Sign(rightDot) == -1 || boundaries.x < 0 && Mathf.Sign(rightDot) == 1)
                velocity.x = 0;

            if (boundaries.y > Screen.height && Mathf.Sign(upDot) == -1 || boundaries.y < 0 && Mathf.Sign(velocity.y) == 1)
                velocity.y = 0;

            position += velocity * Time.deltaTime;
            gameObject.transform.position = position;

            if (velocity.magnitude < velocityDampening * Time.deltaTime)
                velocity = Vector3.zero;
            else
                velocity -= velocity * velocityDampening * Time.deltaTime;
        }

        if (InputController.ScrollDelta.magnitude > 0)
        {
            Vector3 move = new Vector3(0,0, zoomSpeed * InputController.ScrollDelta.y);
            
            if ((gameObject.transform.localPosition + move).z > -1f || (gameObject.transform.localPosition + move).z < -20f)
                return;

            gameObject.transform.localPosition += move;
            position = gameObject.transform.position;
        }
    }
}
