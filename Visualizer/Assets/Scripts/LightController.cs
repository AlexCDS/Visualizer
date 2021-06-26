using UnityEngine;

public class LightController : MonoBehaviour
{
    public float minIntensity = 0f;
    public float maxIntensity = 10f;
    public string lightName = "Light";

    public GameObject rotateTarget;

    private Light directionalLight;

    private float yaw;
    private float pitch;

    void Start()
    {
        Vector3 eulerRot = gameObject.transform.rotation.eulerAngles;
        
        directionalLight = GetComponent<Light>();
        UI.RequestSliderControl(OnIntensityChanged, $"{lightName} intensity", "Lighting", initial: (directionalLight.intensity - minIntensity) / maxIntensity);
        UI.RequestSliderControl(OnYawChanged, $"{lightName} yaw", "Lighting", 0, 359, 0);
        UI.RequestSliderControl(OnPitchChanged, $"{lightName} pitch", "Lighting", 0, 180, 45);
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