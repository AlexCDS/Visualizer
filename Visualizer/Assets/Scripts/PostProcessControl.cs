using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class PostProcessControl : MonoBehaviour
{
    public float floatParamMax = 10f;
    
    void Start()
    {
        Volume v = GetComponent<Volume>();

        foreach(var comp in v.profile.components)
        {
            for (int i = 0; i < comp.parameters.Count; i++)
            {
                if (comp.parameters[i].overrideState)
                    RequestSlider(comp.parameters[i], $"{comp.name.Substring(0,5)}...{i + 1}");
            }
        }
    }

    private void RequestSlider(VolumeParameter parameter, string sliderName)
    {
        switch(parameter)
        {
            case MinFloatParameter mfp:
                UI.RequestSliderControl((f) => { mfp.Override(f); }, $"{sliderName}", "Post Process", mfp.min, floatParamMax, mfp.value);
                break;
            case ClampedFloatParameter cfp:
                UI.RequestSliderControl((f) => { cfp.Override(f); }, $"{sliderName}", "Post Process", cfp.min, cfp.max, cfp.value);
                break;
            case FloatParameter fp:
                UI.RequestSliderControl((f) => { fp.Override(f); }, $"{sliderName}", "Post Process", -floatParamMax, floatParamMax, fp.value);
                break;
            default:
                Debug.LogWarning($"Cannot create slider for unsuported Post Process parameter type: {parameter.GetType()}");
                break;
        }
    }
}
