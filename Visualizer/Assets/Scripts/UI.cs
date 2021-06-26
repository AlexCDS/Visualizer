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
    
    public Camera renderCam;
    public MeshController meshController;

    private Vector2 assetScroll = Vector2.zero;
    private Rect hiddenAssetsRect;
    private Rect shownAssetsRect;
    private InputController.Layer assetsLayer;

    private RenderTexture renderTexture;
    private List<ObjectData> loadedObjects = new List<ObjectData>();
    private List<SliderControl> sliders = new List<SliderControl>();


    private bool showAssets = false;

    int Width => Screen.width / 3 - margins * 4 / 3;

    public static void RequestSliderControl(System.Action<float> action, string name, float min = 0f, float max = 1f, float initial = 0.5f)
    {
        Instance?.sliders.Add(new SliderControl { name = name, min = min, max = max, value = initial, action = action });
    }

    void Start()
    {
        int x = Screen.width;
        if (Instance == null)
            Instance = this;
        else if(Instance != this)
            Destroy(gameObject);
        
        renderTexture = new RenderTexture(Width, Width, 24);
        renderCam.forceIntoRenderTexture = true;
        renderCam.targetTexture = renderTexture;

        UnityEngine.AddressableAssets.Addressables.LoadAssetsAsync<GameObject>("mesh", null).Completed += LoadAssets;

        hiddenAssetsRect = new Rect(0, margins, Screen.width, margins);
        shownAssetsRect = new Rect(0, Screen.height / 3 * 2, x, Screen.height * 0.33f);
        assetsLayer = new InputController.Layer(hiddenAssetsRect);
        InputController.AddLayer(assetsLayer);
    }

    void OnGUI()
    {
        Sliders();
        ChooseAssets();
    }

    void ChooseAssets()
    {
        int x = Screen.width;
        int y = Screen.height;

        if(GUI.Button(new Rect(0, !showAssets ? Screen.height - margins : Screen.height / 3 * 2 - margins * 2, Screen.width, margins), ""))
        {
            showAssets = !showAssets;
            assetsLayer.rect = showAssets ? shownAssetsRect : hiddenAssetsRect; 
        }

        if (!showAssets)
            return;

        assetScroll = GUI.BeginScrollView(shownAssetsRect, assetScroll, new Rect(0, 0, x - margins * 2, margins + ((Width + margins) * (loadedObjects.Count / 3 + 1))), false, true);
        for (int i = 0; i < loadedObjects.Count; i++)
        {
            int currentCollumn = i % 3;
            int currentRow = i / 3;
            Rect buttonRect = new Rect(
                margins / 2 + margins * currentCollumn + Width * currentCollumn,
                (margins * currentRow) + (currentRow * Width), 
                Width, 
                Width);

            if(GUI.Button(buttonRect, loadedObjects[i].render))
            {
                SwapMesh(i);
            }
        }

        GUI.EndScrollView();
    }

    private void SwapMesh(int id)
    {
        for (int i = 0; i < meshController.transform.childCount; i++)
        {
            Destroy(meshController.transform.GetChild(i).gameObject);
        }

        Instantiate(loadedObjects[id].go, meshController.transform);
    }

    private void LoadAssets(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<IList<GameObject>> obj)
    {
        foreach (var go in obj.Result)
        {
            var target = Instantiate(go, Vector3.zero, Quaternion.identity);
            MeshFilter meshFilter = target.GetComponent<MeshFilter>();
            Bounds b = GetBounds(target);

            FitToBounds(renderCam, b);
            target.layer = 6;
            renderCam.Render();
            loadedObjects.Add(new ObjectData { go = go, render = ReadTexture(renderTexture) });
            target.SetActive(false);
            Destroy(target);
        }
    }

    Bounds GetBounds(GameObject go)
    {
        Bounds bounds = new Bounds();
        foreach(var meshFilter in go.GetComponentsInChildren<MeshFilter>())
        {
            bounds.Encapsulate(meshFilter.mesh.bounds);
        }

        bounds.Expand(0.1f);
        return bounds;
    }

    void FitToBounds(Camera camera, Bounds bounds)
    {
        float fov = camera.fieldOfView;
        Vector3 extents = bounds.extents;
        float max = extents.x;
        if (extents.y > max)
            max = extents.y;
        if (extents.z > max)
            max = extents.z;
        
        float dist = max / Mathf.Tan(fov);
        camera.gameObject.transform.position = -camera.gameObject.transform.forward * dist;
    }

    private Texture2D ReadTexture(RenderTexture renderTexture)
    {
        var swap = RenderTexture.active;
        RenderTexture.active = renderTexture;
        Texture2D texture = new Texture2D(Width, Width);
        texture.ReadPixels(new Rect(0, 0, Width, Width), 0, 0);
        texture.Apply();

        RenderTexture.active = swap;
        return texture;
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

    struct ObjectData
    {
        public GameObject go;
        public Texture2D render;
    }
}
