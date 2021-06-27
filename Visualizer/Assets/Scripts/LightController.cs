using UnityEngine;

public class LightController : MonoBehaviour
{
    public float minIntensity = 0f;
    public float maxIntensity = 10f;

    public GameObject rotateTarget;

    private Light directionalLight;

    private float yaw;
    private float pitch;

    void Start()
    {
        Vector3 eulerRot = gameObject.transform.rotation.eulerAngles;
        
        directionalLight = GetComponent<Light>();
        UI.RequestSliderControl(OnIntensityChanged, $"Intensity", "Lighting", initial: (directionalLight.intensity - minIntensity) / maxIntensity);
        UI.RequestSliderControl(OnYawChanged, $"Yaw", "Lighting", 0, 359, yaw = 66f);
        UI.RequestSliderControl(OnPitchChanged, $"Pitch", "Lighting", 0, 180, pitch = 33f);
    }

    public void OnIntensityChanged(float ratio)
    {
        directionalLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, ratio);
    }

    public void OnYawChanged(float angle)
    {
        yaw = angle;
        (rotateTarget ?? gameObject).transform.rotation = Quaternion.Euler(pitch, yaw, 0);
    }

    public void OnPitchChanged(float angle)
    {
        pitch = angle;
        (rotateTarget ?? gameObject).transform.rotation = Quaternion.Euler(pitch, yaw, 0);
    }
}