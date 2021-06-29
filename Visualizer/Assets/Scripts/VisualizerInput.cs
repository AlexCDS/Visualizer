using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VisualizerInput : MonoBehaviour
{
    #region Static
    public static VisualizerInput Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<VisualizerInput>();
            }

            return instance;
        }
        private set => instance = value;
    }
    private static VisualizerInput instance;

    public static void AddLayer(Layer layer)
    {
        Instance.layers.Add(layer);
    }

    public static void RemoveLayer(Layer layer)
    {
        Instance.layers.Remove(layer);
    }

    public static float DoubleTapDelay => Instance.doubleTapDelay;
    public static int TouchCount => SystemInfo.deviceType == DeviceType.Handheld? Input.touchCount : Instance.touches;
    public static Vector2 Delta => Instance.delta;
    public static Vector2 InputPosition => Instance.inputPosition;
    public static Vector2 ScrollDelta => Instance.scrollDelta;
    #endregion

    public float doubleTapDelay = 0.2f;
    public float dotToZoom = 0.5f;

    private int touches = 0;
    private Vector2 delta;
    private Vector2 inputPosition;
    private Vector2 scrollDelta;

    private int screenDiag;

    public static event Action<bool> Touch;

    private List<Layer> layers = new List<Layer>();

    private Vector2 MousePosition
    {
        get
        {
            Vector2 position = Input.mousePosition;
            position.y = Screen.height - position.y;
            return position;
        }
    }
    private bool touching = false;
    private Vector2 previousTouchPosition;

    private void Start()
    {
        Application.targetFrameRate = 60;
        screenDiag = (int)Mathf.Sqrt(Screen.height * Screen.height + Screen.width * Screen.width);
    }

    public void Update()
    {
#if UNITY_IOS || UNITY_ANDROID
        HandeldInput();
#else
        PcInput();
#endif
    }

    private void PcInput()
    {
        if (!touching)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                if (layers.Any(l => l.TryBlockTouch(MousePosition)))
                    return;

                UpdatePosition(true);
                touching = true;

                if (Input.GetMouseButtonDown(1))
                    touches = 2;
                else
                    touches = 1;

                Touch?.Invoke(true);
            }
        }
        else
        {
            if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
            {
                touching = false;
                Touch?.Invoke(false);
                touches = 0;
                ResetPosition();
                return;
            }

            UpdatePosition();
        }

        if(!layers.Any(l => l.Contains(MousePosition)))
            scrollDelta = Input.mouseScrollDelta * 0.1f;
    }

    private void HandeldInput()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            var t = Input.GetTouch(i);
        }

        if (Input.touchCount == 0)
        {
            if(inputPosition != Vector2.zero)
                ResetPosition();

            return;
        }

        var touch = Input.GetTouch(0);
        if (layers.Any(l => l.TryBlockTouch(touch.position.WithY(Screen.height - touch.position.y))))
        {
            return;
        }

        inputPosition = touch.position;
        delta = touch.deltaPosition;

        if (TouchCount <= 1)
        {
            return;
        }

        touch = Input.GetTouch(1);
        if (touch.deltaPosition.magnitude > 0.001f && delta.magnitude > 0.001f)
        {
            float dot1 = Vector2.Dot(delta.normalized, (touch.position - inputPosition).normalized);
            float dot2 = Vector2.Dot(touch.deltaPosition.normalized, (inputPosition - touch.position).normalized);

            if (Mathf.Abs(dot1) >= dotToZoom && Mathf.Abs(dot2) >= dotToZoom && Mathf.Sign(dot1) == Mathf.Sign(dot2))
            {
                scrollDelta = new Vector2(0, (delta.magnitude + touch.deltaPosition.magnitude) / screenDiag * Mathf.Sign(dot1));
                delta = Vector2.zero;
            }
            else
            {
                delta *= Vector2.Dot(delta.normalized, touch.deltaPosition.normalized) + 1 * 0.5f;
            }
        }
        else
            scrollDelta = Vector2.zero;
    }

    private void ResetPosition()
    {
        delta = previousTouchPosition = inputPosition = Vector2.zero;
    }

    private void UpdatePosition(bool reset = false)
    {
        if(reset)
            previousTouchPosition = MousePosition;

        inputPosition = MousePosition;
        delta = previousTouchPosition - inputPosition;
        previousTouchPosition = inputPosition;
    }

    public class Layer
    {
        public bool DoubleTap => SystemInfo.deviceType == DeviceType.Handheld ? Input.touchCount > 0 && Input.GetTouch(0).tapCount > 1 : Time.time - lastRecordedTap <= DoubleTapDelay;
        public float lastRecordedTap;
        public Rect rect;
        public bool blocker = true;

        public event EventHandler<float> Interacted;
        
        public bool TryBlockTouch(Vector2 position)
        {
            if (rect.Contains(position))
            {
                Interacted?.Invoke(this, Time.time);
                lastRecordedTap = Time.time;

                if (blocker)
                    return true;
            }
            return false;
        }

        public bool Contains(Vector2 position)
        {
            return blocker && rect.Contains(position);
        }
        
        public Layer(Rect rect)
        {
            this.rect = rect;
        }
    }
}

static class Extension
{
    public static Vector2 WithX(this Vector2 v, float x)
    {
        v.x = x;
        return v;
    }

    public static Vector2 WithY(this Vector2 v, float y)
    {
        v.y = y;
        return v;
    }
}
