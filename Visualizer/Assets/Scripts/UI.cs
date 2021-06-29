using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UI : MonoBehaviour
{
    class SliderGroup
    {
        public List<SliderControl> sliders = new List<SliderControl>();
        public bool enabled = false;

        public void Add(SliderControl slider) => sliders.Add(slider);
        public void Remove(SliderControl slider) => sliders.Remove(slider);
        public int Count => sliders.Count;
        public SliderControl this[int i]
        {
            get { return sliders[i]; }
            set { sliders[i] = value; }
        }
    }

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

    [Range(0, 0.05f)]
    public float margins = 0.025f;
    private int Margins => UIRatioToScreenSize(margins, Orientation.width);
    [Range(0, 0.05f)]
    public float spaceBetweenControls = 0.025f;
    private int SpaceBetweenControls => UIRatioToScreenSize(spaceBetweenControls, Orientation.width);
    [Range(0, 0.05f)]
    public float sliderHeight = 0.025f;
    private int SliderHeight => UIRatioToScreenSize(sliderHeight, Orientation.height);
    [Range(0, 0.05f)]
    public float buttonsHeight = 0.025f;
    private int ButtonsHeight => UIRatioToScreenSize(buttonsHeight, Orientation.height);
    [Range(0, 0.5f)]
    public float sideMenuWidth = 0.35f;
    private int SideMenuWitdth => UIRatioToScreenSize(sideMenuWidth, Orientation.width);

    [Range(0, 0.5f)]
    public float bottomTapHeight = 0.1f; 
    public int BottomTapHeight => UIRatioToScreenSize(bottomTapHeight, Orientation.height); 
    [Range(0, 0.5f)]
    public float sideTapWidth = 0.1f; 
    public int SideTapWidth => UIRatioToScreenSize(sideTapWidth, Orientation.width); 
    [Range(0, 1f)]
    public float assetsMenuHeight = 0.35f; 
    private int AssetsMenuHeight => UIRatioToScreenSize(assetsMenuHeight, Orientation.height);

    [Range(1, 10)]
    public int AssetsPerRow = 3;

    [Range(0,100)]
    public int fontSize = 48;

    public Camera renderCam;
    public MeshController meshController;

    private int Width => Screen.width;
    private int Height => Screen.height;

    public GUIStyle labelStyle;
    public GUIStyle buttonStyle;
    public GUIStyle sliderStyle;
    public GUIStyle thumbStyle;

    private Vector2 assetScroll = Vector2.zero;
    private Rect sliderRect;
    private Rect assetsRect;
    private VisualizerInput.Layer assetsLayer;
    private VisualizerInput.Layer sliderLayer;
    private VisualizerInput.Layer screenLayer;
    private Texture2D white; 

    private RenderTexture renderTexture;
    private List<GameObjectData> loadedGameobjects = new List<GameObjectData>();
    private List<MaterialData> loadedMaterials = new List<MaterialData>();
    private List<TextureData> loadedTextures = new List<TextureData>();
    private List<Texture2D> textures = new List<Texture2D>();

    private Dictionary<string, SliderGroup> sliders = new Dictionary<string, SliderGroup>();

    private int showAssets = 0;
    private bool showSliders = false;

    int AssetButtonsWidth => (int)(assetsRect.width * (1f / (float)AssetsPerRow) - Margins - SpaceBetweenControls * (AssetsPerRow - 1));

    public static void RequestSliderControl(System.Action<float> action, string name, string category, float min = 0f, float max = 1f, float initial = 0.5f)
    {
        if (!Instance.sliders.ContainsKey(category))
            Instance.sliders[category] = new SliderGroup();

        Instance.sliders[category].Add(new SliderControl { name = name, min = min, max = max, value = initial, action = action });
    }

    void Start()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        white = new Texture2D(1, 1);
        white.SetPixel(0, 0, Color.white);

        sliderRect = new Rect(Width - SideTapWidth, 0, SideTapWidth, Screen.height);
        assetsRect = new Rect(0, Height - AssetsMenuHeight, Width, AssetsMenuHeight);

        renderTexture = new RenderTexture(AssetButtonsWidth, AssetButtonsWidth, 24);
        renderCam.forceIntoRenderTexture = true;
        renderCam.targetTexture = renderTexture;

        assetsLayer = new VisualizerInput.Layer(assetsRect) { blocker = false };
        sliderLayer = new VisualizerInput.Layer(sliderRect) { blocker = false };
        whole = new VisualizerInput.Layer(new Rect(0, 0, Width, Height)) { blocker = false };

        VisualizerInput.AddLayer(whole);
        VisualizerInput.AddLayer(assetsLayer);
        VisualizerInput.AddLayer(sliderLayer);

        assetsLayer.Interacted += OnAssetsLayerInteract;
        sliderLayer.Interacted += OnSliderLayerInteract;
        screenLayer.Interacted += OnGeneralInteract;

        UnityEngine.AddressableAssets.Addressables.LoadAssetsAsync<GameObject>("mesh", null).Completed += LoadGameobjects;
        UnityEngine.AddressableAssets.Addressables.LoadAssetsAsync<Material>("materials", null).Completed += LoadMaterials;
        UnityEngine.AddressableAssets.Addressables.LoadAssetsAsync<Texture2D>("textures", null).Completed += LoadTextures;
    }

    private void OnGeneralInteract(object sender, float e)
    {
        VisualizerInput.Layer layer = (VisualizerInput.Layer)sender;
        if (!layer.DoubleTap)
            return;

        HideSliders();
        HideAssets();
    }

    private void OnSliderLayerInteract(object sender, float time)
    {
        VisualizerInput.Layer layer = (VisualizerInput.Layer)sender;
        if (!layer.DoubleTap)
            return;

        if (!showSliders)
            ShowSliders();
        else
            HideSliders();

        HideAssets();
    }

    private void OnAssetsLayerInteract(object sender, float time)
    {
        VisualizerInput.Layer layer = (VisualizerInput.Layer)sender;
        if (!layer.DoubleTap || showAssets != 0)
            return;

        HideSliders();
        ShowAssets();
    }

    private void HideAssets()
    {
        assetsLayer.blocker = false;
        showAssets = 0;
    }

    private void ShowAssets()
    {
        textures = loadedGameobjects.Select(l => l.render).ToList();
        assetsLayer.blocker = true;
        showAssets = 1;
    }

    private void HideSliders()
    {
        sliderLayer.blocker = false;
        showSliders = false;
    }

    private void ShowSliders()
    {
        sliderLayer.blocker = true;
        showSliders = true;
    }

    private void OnDestroy()
    {
        assetsLayer.Interacted -= OnAssetsLayerInteract;
        sliderLayer.Interacted -= OnSliderLayerInteract;
        screenLayer.Interacted -= OnGeneralInteract;
    }
    
    void OnGUI()
    {
        InitializeGUI();
        DrawSliders();
        DrawAssetsMenu();
    }

    private void OnValidate()
    {
        initialized = false;
    }

    private bool initialized = false;
    private void InitializeGUI()
    {
        if (initialized)
            return;

        initialized = true;
        GUI.skin.label.fontSize = SliderHeight - 4;
        GUI.skin.label.alignment = TextAnchor.UpperLeft;
        GUI.skin.button.fontSize = ButtonsHeight - 4;
        GUI.skin.horizontalSliderThumb.stretchHeight = true;
        GUI.skin.horizontalSliderThumb.fixedHeight = 0;
        GUI.skin.horizontalSliderThumb.fixedWidth = SliderHeight;
        GUI.skin.horizontalSlider.fixedHeight = 0;
        
        GUI.skin.horizontalSliderThumb.alignment = TextAnchor.MiddleLeft;
        GUI.skin.verticalScrollbar.fixedWidth = ButtonsHeight;
        GUI.skin.verticalScrollbarThumb.fixedWidth = ButtonsHeight;
        
        labelStyle = GUI.skin.label;
        buttonStyle = GUI.skin.button;
        sliderStyle = GUI.skin.horizontalSlider;
        thumbStyle = GUI.skin.horizontalSliderThumb;

        sliderRect = new Rect(Width - SideTapWidth, 0, SideTapWidth, Screen.height);
        assetsRect = new Rect(0, Screen.height - AssetsMenuHeight - ButtonsHeight - SpaceBetweenControls, Width, AssetsMenuHeight - ButtonsHeight - SpaceBetweenControls);
    }

    void DrawSliders()
    {
        if (!showSliders)
            return;

        int currentY = Margins;
        int currentX = Width - SideMenuWitdth;
        
        foreach ((string category, SliderGroup sliderGroup) in sliders.Select(s => (s.Key, s.Value)))
        {
            int guiGroupHeight = sliderGroup.Count * (SliderHeight + SpaceBetweenControls);
            
            if (GUI.Button(new Rect(Width - Margins - SideMenuWitdth, currentY, SideMenuWitdth, ButtonsHeight), category))
            {
                sliderGroup.enabled = !sliderGroup.enabled;
            }
            currentY += ButtonsHeight;

            if (!sliderGroup.enabled)
            {
                currentY += Margins + SpaceBetweenControls;
                continue;
            }

            GUI.BeginGroup(new Rect(currentX, currentY, SideMenuWitdth, guiGroupHeight), "");

            for (int i = 0; i < sliderGroup.Count; i++)
            {
                Vector2 labelSize = GUI.skin.label.CalcSize(new GUIContent(sliderGroup[i].name + " "));
                float controlY = (SliderHeight + SpaceBetweenControls) * i;

                GUI.DrawTexture(new Rect(labelSize.x, controlY + labelSize.y * .5f, SideMenuWitdth - labelSize.x - Margins, 2), white);
                GUI.Label(new Rect(0, controlY, labelSize.x, labelSize.y), sliderGroup[i].name + " ");
                float newValue = GUI.HorizontalSlider(new Rect(labelSize.x, controlY, SideMenuWitdth - labelSize.x - Margins, SliderHeight), sliderGroup[i].value, sliderGroup[i].min, sliderGroup[i].max);

                if (!Mathf.Approximately(newValue, sliderGroup[i].value))
                {
                    sliderGroup[i].action?.Invoke(newValue);
                    sliderGroup[i].SetValue(newValue);
                }
            }
            currentY += guiGroupHeight + Margins;
            GUI.EndGroup();
        }
    }

    void DrawAssetsMenu()
    {
        if (showAssets == 0)
            return;

        if(GUI.Button(new Rect(0, Height - ButtonsHeight, Width * .33f - Margins * .33f, ButtonsHeight), "Models"))
        {
            textures = loadedGameobjects.Select(l => l.render).ToList();
            showAssets = 1;
        }
        if (GUI.Button(new Rect(Width * .33f - Margins * .33f, Height - ButtonsHeight, Width * .33f - Margins * .33f, ButtonsHeight), "Materials"))
        {
            textures = loadedMaterials.Select(l => l.render).ToList();
            showAssets = 2;
        }
        if (GUI.Button(new Rect(Width * .66f - Margins * .66f, Height - ButtonsHeight, Width * .33f - Margins * .33f, ButtonsHeight), "Textures"))
        {
            textures = loadedTextures.Select(l => l.texture).ToList();
            showAssets = 3;
        }

        assetScroll = GUI.BeginScrollView(assetsRect, assetScroll, new Rect(0, 0, assetsRect.width - ButtonsHeight - 1, Margins + ((AssetButtonsWidth + Margins) * (textures.Count / AssetsPerRow + 1))), false, true);
        for (int i = 0; i < textures.Count; i++)
        {
            int currentCollumn = i % AssetsPerRow;
            int currentRow = i / AssetsPerRow;
            Rect buttonRect = new Rect(
                Margins + SpaceBetweenControls * currentCollumn + AssetButtonsWidth * currentCollumn,
                (SpaceBetweenControls * currentRow) + (currentRow * AssetButtonsWidth), 
                AssetButtonsWidth, 
                AssetButtonsWidth);

            if(GUI.Button(buttonRect, textures[i]))
            {
                Swap(i);
            }
        }

        GUI.EndScrollView();
    }

    private void Swap(int id)
    {
        switch(showAssets)
        {
            case 1:
                SwapMesh(id);
                break;
            case 2:
                SwapMaterial(id);
                break;
            case 3:
                SwapTexture(id);
                break;
        }
    }

    private void SwapMesh(int id)
    {
        for (int i = 0; i < meshController.transform.childCount; i++)
        {
            Destroy(meshController.transform.GetChild(i).gameObject);
        }

        Instantiate(loadedGameobjects[id].go, meshController.transform);
    }

    private void SwapMaterial(int id)
    {
        foreach(var meshRenderer in meshController.GetComponentsInChildren<MeshRenderer>())
        {
            meshRenderer.material = loadedMaterials[id].material;
        }
    }
    
    private void SwapTexture(int id)
    {
        foreach(var meshRenderer in meshController.GetComponentsInChildren<MeshRenderer>())
        {
            meshRenderer.material.mainTexture = loadedTextures[id].texture;
        }
    }

    private void LoadGameobjects(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<IList<GameObject>> obj)
    {
        foreach (var go in obj.Result)
        {
            var target = Instantiate(go);
            MeshFilter meshFilter = target.GetComponent<MeshFilter>();
            Bounds b = GetBounds(target);

            FitToBounds(renderCam, b);
            target.layer = 6;
            renderCam.Render();
            loadedGameobjects.Add(new GameObjectData { go = go, render = ReadTexture(renderTexture) });
            target.SetActive(false);
            Destroy(target);
        }
    }

    private void LoadMaterials(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<IList<Material>> obj)
    {
        GameObject target = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        target.transform.position = new Vector3(0, 0.5f, 0);
        target.layer = 6;
        FitToBounds(renderCam, new Bounds(Vector3.zero, Vector3.one + Vector3.up * 0.5f));

        foreach (var mat in obj.Result)
        {
            MeshFilter meshFilter = target.GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = target.GetComponent<MeshRenderer>();
            meshRenderer.material = mat;

            renderCam.Render();

            loadedMaterials.Add(new MaterialData { material = mat, render = ReadTexture(renderTexture) });
        }

        target.SetActive(false);
        Destroy(target);
    }

    private void LoadTextures(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<IList<Texture2D>> obj)
    {
        foreach (var tex in obj.Result)
        {
            loadedTextures.Add(new TextureData { texture = tex });
        }
    }

    Bounds GetBounds(GameObject go)
    {
        Bounds bounds = new Bounds();
        Vector3 center = Vector3.zero;
        int count = 0;
        foreach (var meshFilter in go.GetComponentsInChildren<MeshFilter>())
        {
            bounds.Encapsulate(meshFilter.mesh.bounds);
            center += meshFilter.mesh.bounds.center;
            count++;
        }
        center /= count;

        bounds.center = center;
        return bounds;
    }

    void FitToBounds(Camera camera, Bounds bounds)
    {
        float fov = camera.fieldOfView;
        Vector3 extents = bounds.extents;
        float max = extents.x + bounds.center.x;
        if (extents.y + bounds.center.y > max)
            max = extents.y;
        if (extents.z + bounds.center.z > max)
            max = extents.z;
        
        float dist = max / Mathf.Tan(fov);
        camera.gameObject.transform.position = -camera.gameObject.transform.forward * dist + Vector3.up;
    }

    private Texture2D ReadTexture(RenderTexture renderTexture)
    {
        var swap = RenderTexture.active;
        RenderTexture.active = renderTexture;
        Texture2D texture = new Texture2D(AssetButtonsWidth, AssetButtonsWidth);
        texture.ReadPixels(new Rect(0, 0, AssetButtonsWidth, AssetButtonsWidth), 0, 0);
        texture.Apply();

        RenderTexture.active = swap;
        return texture;
    }
    
    enum Orientation { width, height };
    private int UIRatioToScreenSize(float pct, Orientation orientation)
    {
        return (int)((orientation == Orientation.height? Screen.height : Screen.width) * pct);
    }

    struct GameObjectData
    {
        public GameObject go;
        public Texture2D render;
    }

    struct MaterialData
    {
        public Material material;
        public Texture2D render;
    }

    struct TextureData
    {
        public Texture2D texture;
    }
}
