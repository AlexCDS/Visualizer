using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshController : MonoBehaviour
{
    public float minScale;
    public float maxScale;

    private float yaw;
    private float pitch;
    private float roll;

    void Start()
    {
        UI.RequestSliderControl(OnSliderValueChanged, "Scale", "Mesh", initial: (1 - minScale) / maxScale);
        UI.RequestSliderControl(OnYawChanged, "Yaw", "Mesh", 0, 359, 0);
        UI.RequestSliderControl(OnPitchChanged, "Pitch", "Mesh", 0, 359, 0);
        UI.RequestSliderControl(OnRollChanged, "Roll", "Mesh", 0, 359, 0);
    }

    private void OnSliderValueChanged(float ratio)
    {
        float scale = Mathf.Lerp(minScale, maxScale, ratio);
        gameObject.transform.localScale = new Vector3(scale, scale, scale);
    }

    private void OnYawChanged(float angle)
    {
        yaw = angle;
        Rotate();
    }

    private void OnPitchChanged(float angle)
    {
        pitch = angle;
        Rotate();
    }

    private void OnRollChanged(float angle)
    {
        roll = angle;
        Rotate();
    }

    private void Rotate()
    {
        gameObject.transform.rotation = Quaternion.Euler(yaw, pitch, roll);
    }
}
