using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UI : MonoBehaviour
{
    class SliderControl
    {
        public float min;
        public float max;
        public float value;
        public string name;
        public System.Action<float> action;

        public void SetValue(float newValue) => value = newValue;
    }

    public static UI Instance
    {
        get
        {
            if(instance == null)
            {
                instance = FindObjectOfType<UI>();
            }

            return instance;
        }
        private set => instance = value;
    }
    private static UI instance;
    
    public int margins = 50;
    public int spaceBetweenControls = 50;
    public int sliderHeight = 40;
    public int sliderWidth = 60;
    public float lightIntensity = 0.5f;

    public static void RequestSliderControl(System.Action<float> action, string name, float min = 0f, float max = 1f, float initial = 0.5f)
    {
        Instance?.sliders.Add(new SliderControl { name = name, min = min, max = max, value = initial, action = action });
    }

    void Sliders()
    {
        int x = Screen.width;
        int y = Screen.height;

        for (int i = 0; i < sliders.Count; i++)
        {
            float controlX = x - sliderWidth - margins;
            float controlY = margins + (sliderHeight + spaceBetweenControls) * i;

            GUI.Label(new Rect(controlX - sliders[i].name.Length * 7 + 20, controlY, sliders[i].name.Length * 7, sliderHeight), sliders[i].name);
            float newValue = GUI.HorizontalSlider(new Rect(controlX, controlY, sliderWidth, sliderHeight), sliders[i].value, sliders[i].min, sliders[i].max);
            if (!Mathf.Approximately(newValue, sliders[i].value))
            {
                sliders[i].action?.Invoke(newValue);
                sliders[i].SetValue(newValue);
            }
        }
    }
}
