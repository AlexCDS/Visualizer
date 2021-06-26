using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    #region Static
    public static InputController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<InputController>();
            }

            return instance;
        }
        private set => instance = value;
    }
    private static InputController instance;

    public static void AddLayer(Layer layer)
    {
        Instance.layers.Add(layer);
    }

    public static void RemoveLayer(Layer layer)
    {
        Instance.layers.Remove(layer);
    }

    public static int Touches => Instance.touches;
    public static Vector2 Delta => Instance.delta;
    public static Vector2 InputPosition => Instance.inputPosition;
    public static Vector2 ScrollDelta => Instance.scrollDelta;
    #endregion

    private int touches = 0;
    private Vector2 delta;
    private Vector2 inputPosition;
    private Vector2 scrollDelta;

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
    
    public void Update()
    {
        bool inLayer = layers.Any(l => l.Contains(MousePosition));

        if (!touching)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                if (inLayer)
                {
                    return;
                }

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
                touches = 0;
                Touch?.Invoke(false);
                ResetPosition();
                return;
            }

            UpdatePosition();
        }

        if (!inLayer)
        {
            scrollDelta = Input.mouseScrollDelta;
        }
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
        public Rect rect;

        public bool Contains(Vector2 position)
        {
            return rect.Contains(position);
        }

        public Layer(Rect rect)
        {
            this.rect = rect;
        }
    }
}
