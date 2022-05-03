using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    #region Events

    public delegate void StartTouch(Vector2 position, float time);
    public event StartTouch OnStartTouch;

    public delegate void EndTouch(Vector2 position, float time);
    public event EndTouch OnEndTouch;

    #endregion

    public static InputManager  instance;

    #region Inspector Variables

    public float                minimumSwipeDistance    = .2f;
    public float                maximumSwipeTime        = 1f;

    #endregion

    private TouchControls       touchControls;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        touchControls = new TouchControls();
    }

    private void OnEnable()
    {
        touchControls.Enable();
    }

    private void OnDisable()
    {
        touchControls.Disable();
    }

    private void Start()
    {
        touchControls.Touch.PrimaryContact.started  += context => StartTouchPrimary(context);
        touchControls.Touch.PrimaryContact.canceled += context => EndTouchPrimary(context);
    }

    private void StartTouchPrimary(InputAction.CallbackContext context)
    {
        if (OnStartTouch != null)
            OnStartTouch(Helpful.ScreenToWorld(Camera.main, touchControls.Touch.PrimaryPosition.ReadValue<Vector2>())
                , (float)context.startTime);
    }

    private void EndTouchPrimary(InputAction.CallbackContext context)
    {
        if (OnEndTouch != null)
            OnEndTouch(Helpful.ScreenToWorld(Camera.main, touchControls.Touch.PrimaryPosition.ReadValue<Vector2>())
                , (float)context.time);

    }

    public Vector2 PrimaryPosition()
    {
        return Helpful.ScreenToWorld(Camera.main, touchControls.Touch.PrimaryPosition.ReadValue<Vector2>());
    }
}
