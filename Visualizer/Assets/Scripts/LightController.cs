using UnityEngine;

public class LightController : MonoBehaviour
{
    public float minIntensity = 0f;
    public float maxIntensity = 10f;
    public string lightName = "Light";

    private Light directionalLight;

    void Start()
    {
        directionalLight = GetComponent<Light>();
        UI.RequestSliderControl(OnSliderValueChanged, $"{lightName} intensity", "Lighting", initial: (directionalLight.intensity - minIntensity) / maxIntensity);
    }

    public void OnSliderValueChanged(float t)
    {
        directionalLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
    }
}