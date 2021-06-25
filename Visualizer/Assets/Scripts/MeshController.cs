using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshController : MonoBehaviour
{
    public float minScale;
    public float maxScale;
    
    void Start()
    {
        UI.RequestSliderControl(OnSliderValueChanged, "Mesh scale", initial: (1 - minScale) / maxScale);
    }

    private void OnSliderValueChanged(float t)
    {
        float scale = Mathf.Lerp(minScale, maxScale, t);
        gameObject.transform.localScale = new Vector3(scale, scale, scale);
    }
}
