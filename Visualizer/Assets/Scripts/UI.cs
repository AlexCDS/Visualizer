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
    
    public int margins = 50;
    public int spaceBetweenControls = 50;
    public int sliderHeight = 40;
    public int sliderWidth = 60;
    public float lightIntensity = 0.5f;
    
    public Camera renderCam;
    public MeshController meshController;

    private Vector2 assetScroll = Vector2.zero;
    private Rect hiddenSliderRect;
    private Rect hiddenAssetsRect;
    private Rect shownAssetsRect;
    private InputController.Layer assetsLayer;
    private InputController.Layer sliderLayer;
    private Texture2D white; 

    private RenderTexture renderTexture;
    private List<GameObjectData> loadedGameobjects = new List<GameObjectData>();
    private List<MaterialData> loadedMaterials = new List<MaterialData>();
    private List<TextureData> loadedTextures = new List<TextureData>();
    private List<Texture2D> textures = new List<Texture2D>();

    private Dictionary<string, SliderGroup> sliders = new Dictionary<string, SliderGroup>();

    private int showAssets = 0;
    
    private bool showSliders = false;

    int AssetButtonsWidth => (int)(shownAssetsRect.width / 3) - margins - spaceBetweenControls * 2;

    public static void RequestSliderControl(System.Action<float> action, string name, string category, float min = 0f, float max = 1f, float initial = 0.5f)
    {
        if (!Instance.sliders.ContainsKey(category))
            Instance.sliders[category] = new SliderGroup();

        Instance.sliders[category].Add(new SliderControl { name = name, min = min, max = max, value = initial, action = action });
    }

    void Start()
    {
        int x = Screen.width;
        if (Instance == null)
            Instance = this;
        else if(Instance != this)
            Destroy(gameObject);
        
        UnityEngine.AddressableAssets.Addressables.LoadAssetsAsync<GameObject>("mesh", null).Completed += LoadGameobjects;
        UnityEngine.AddressableAssets.Addressables.LoadAssetsAsync<Material>("materials", null).Completed += LoadMaterials;
        UnityEngine.AddressableAssets.Addressables.LoadAssetsAsync<Texture2D>("textures", null).Completed += LoadTextures;

        hiddenSliderRect = new Rect(x - margins, 0, margins, Screen.height);

        hiddenAssetsRect = new Rect(0, margins, Screen.width, margins);
        shownAssetsRect = new Rect(0, Screen.height / 3 * 2 - margins, x - margins, Screen.height * 0.33f);
        
        white = new Texture2D(1, 1);
        white.SetPixel(0, 0, Color.white);
        
        renderTexture = new RenderTexture(AssetButtonsWidth, AssetButtonsWidth, 24);
        renderCam.forceIntoRenderTexture = true;
        renderCam.targetTexture = renderTexture;

        assetsLayer = new InputController.Layer(hiddenAssetsRect);
        sliderLayer = new InputController.Layer(hiddenSliderRect);
        InputController.AddLayer(assetsLayer);
        InputController.AddLayer(sliderLayer);
    }

    void OnGUI()
    {
        DrawSliders();
        DrawAssetsMenu();
    }

    void DrawAssetsMenu()
    {
        int x = Screen.width;
        int y = Screen.height;

        if(GUI.Button(new Rect(0, Screen.height - margins * 2, Screen.width / 3 - margins / 3, margins * 2), "Models"))
        {
            textures = loadedGameobjects.Select(l => l.render).ToList();
            showAssets = showAssets == 1? 0 : 1;
            assetsLayer.rect = showAssets > 0 ? shownAssetsRect : hiddenAssetsRect;
        }
        if (GUI.Button(new Rect(Screen.width / 3 - margins / 3, Screen.height - margins * 2, Screen.width / 3 - margins / 3, margins * 2), "Materials"))
        {
            textures = loadedMaterials.Select(l => l.render).ToList();
            showAssets = showAssets == 2 ? 0 : 2;
            assetsLayer.rect = showAssets > 0 ? shownAssetsRect : hiddenAssetsRect;
        }
        if (GUI.Button(new Rect(Screen.width / 3 * 2 - (margins / 3 * 2), Screen.height - margins * 2, Screen.width / 3 - margins / 3, margins * 2), "Textures"))
        {
            textures = loadedTextures.Select(l => l.texture).ToList();
            showAssets = showAssets == 3 ? 0 : 3;
            assetsLayer.rect = showAssets > 0 ? shownAssetsRect : hiddenAssetsRect;
        }

        if (showAssets == 0)
            return;
        
        assetScroll = GUI.BeginScrollView(shownAssetsRect, assetScroll, new Rect(0, 0, shownAssetsRect.width - margins * 2, margins + ((AssetButtonsWidth + margins) * (textures.Count / 3 + 1))), false, true);
        for (int i = 0; i < textures.Count; i++)
        {
            int currentCollumn = i % 3;
            int currentRow = i / 3;
            Rect buttonRect = new Rect(
                margins + spaceBetweenControls * currentCollumn + AssetButtonsWidth * currentCollumn,
                (spaceBetweenControls * currentRow) + (currentRow * AssetButtonsWidth), 
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
            var target = Instantiate(go, Vector3.zero, Quaternion.identity);
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
        Texture2D texture = new Texture2D(AssetButtonsWidth, AssetButtonsWidth);
        texture.ReadPixels(new Rect(0, 0, AssetButtonsWidth, AssetButtonsWidth), 0, 0);
        texture.Apply();

        RenderTexture.active = swap;
        return texture;
    }

    void DrawSliders()
    {
        GUIStyle style = new GUIStyle();

        int x = Screen.width;
        int y = Screen.height;

        if(GUI.Button(new Rect(x - margins, 0, margins, y), ""))
        {
            showSliders = !showSliders;
        }

        if (!showSliders)
            return;
        
        int currentY = margins;
        int currentX = x / 3 * 2;
        int sliderWidth = x / 3 - margins - spaceBetweenControls;

        foreach ((string category, SliderGroup sliderGroup) in sliders.Select(s => (s.Key, s.Value)))
        {
            int guiGroupHeight = sliderGroup.Count * (sliderHeight + spaceBetweenControls);

            if (GUI.Button(new Rect(x - margins - sliderWidth, currentY, sliderWidth, margins * 2), category))
            {
                sliderGroup.enabled = !sliderGroup.enabled;
            }
            currentY += margins * 2;
            
            if(!sliderGroup.enabled)
            {
                currentY += margins + spaceBetweenControls;
                continue;
            }

            GUI.BeginGroup(new Rect(currentX, currentY, x / 3, guiGroupHeight), "");

            for (int i = 0; i < sliderGroup.Count; i++)
            {
                Vector2 labelSize = style.CalcSize(new GUIContent(sliderGroup[i].name + " "));
                float controlY = (labelSize.y + spaceBetweenControls) * i;

                GUI.DrawTexture(new Rect(labelSize.x, controlY + 10, sliderWidth - labelSize.x, 2), white);
                GUI.Label(new Rect(0, controlY, labelSize.x, sliderWidth), sliderGroup[i].name + " ");
                float newValue = GUI.HorizontalSlider(new Rect(labelSize.x, controlY + 5, sliderWidth - labelSize.x, sliderHeight), sliderGroup[i].value, sliderGroup[i].min, sliderGroup[i].max);

                if (!Mathf.Approximately(newValue, sliderGroup[i].value))
                {
                    sliderGroup[i].action?.Invoke(newValue);
                    sliderGroup[i].SetValue(newValue);
                }
            }
            currentY += guiGroupHeight + margins;
            GUI.EndGroup();
        }

        sliderLayer.rect = new Rect(currentX, 0, x / 3, currentY);
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
