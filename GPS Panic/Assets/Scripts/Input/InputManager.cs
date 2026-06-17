using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using System;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace GPSPanic.Input
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        public event Action OnSwipeLeft;
        public event Action OnSwipeRight;
        public event Action<Vector2> OnDrag;
        public event Action<Vector2> OnDragDelta;
        public event Action OnTouchReleased;

        [SerializeField] private float swipeThreshold = 50f;
        [SerializeField] private float dragThreshold = 10f;

        private Vector2 startTouchPosition;
        private Vector2 lastTouchPosition;
        private bool isDragging = false;
        private bool isMouseActive = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
            Touch.onFingerDown += OnFingerDown;
            Touch.onFingerMove += OnFingerMove;
            Touch.onFingerUp += OnFingerUp;
        }

        private void OnDisable()
        {
            Touch.onFingerDown -= OnFingerDown;
            Touch.onFingerMove -= OnFingerMove;
            Touch.onFingerUp -= OnFingerUp;
            EnhancedTouchSupport.Disable();
        }

        private void Update()
        {
            HandleMouseInput();
            HandleKeyboardInput();
        }

        private void HandleMouseInput()
        {
            if (Pointer.current == null) return;

            // Mouse Down
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                startTouchPosition = Mouse.current.position.ReadValue();
                lastTouchPosition = startTouchPosition;
                isDragging = false;
                isMouseActive = true;
            }

            // Mouse Move
            if (isMouseActive && Mouse.current.leftButton.isPressed)
            {
                Vector2 currentPosition = Mouse.current.position.ReadValue();
                Vector2 deltaFromStart = currentPosition - startTouchPosition;
                Vector2 deltaFromLast = currentPosition - lastTouchPosition;

                if (deltaFromStart.magnitude > dragThreshold)
                {
                    isDragging = true;
                    OnDrag?.Invoke(currentPosition);
                    OnDragDelta?.Invoke(deltaFromLast);
                }

                lastTouchPosition = currentPosition;
            }

            // Mouse Up
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                Vector2 endTouchPosition = Mouse.current.position.ReadValue();
                Vector2 delta = endTouchPosition - startTouchPosition;

                if (!isDragging)
                {
                    if (Mathf.Abs(delta.x) > swipeThreshold)
                    {
                        if (delta.x < 0) OnSwipeLeft?.Invoke();
                        else OnSwipeRight?.Invoke();
                    }
                }

                isDragging = false;
                isMouseActive = false;
                OnTouchReleased?.Invoke();
            }
        }

        private void HandleKeyboardInput()
        {
            if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
            {
                OnSwipeLeft?.Invoke();
            }
            if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                OnSwipeRight?.Invoke();
            }
        }

        private void OnFingerDown(Finger finger)
        {
            if (finger.index == 0)
            {
                startTouchPosition = finger.currentTouch.startScreenPosition;
                lastTouchPosition = startTouchPosition;
                isDragging = false;
            }
        }

        private void OnFingerMove(Finger finger)
        {
            if (finger.index == 0)
            {
                Vector2 currentPosition = finger.currentTouch.screenPosition;
                Vector2 deltaFromStart = currentPosition - startTouchPosition;
                Vector2 deltaFromLast = currentPosition - lastTouchPosition;

                if (deltaFromStart.magnitude > dragThreshold)
                {
                    isDragging = true;
                    OnDrag?.Invoke(currentPosition);
                    OnDragDelta?.Invoke(deltaFromLast);
                }

                lastTouchPosition = currentPosition;
            }
        }

        private void OnFingerUp(Finger finger)
        {
            if (finger.index == 0)
            {
                Vector2 endTouchPosition = finger.currentTouch.screenPosition;
                Vector2 delta = endTouchPosition - startTouchPosition;

                if (!isDragging)
                {
                    if (Mathf.Abs(delta.x) > swipeThreshold)
                    {
                        if (delta.x < 0) OnSwipeLeft?.Invoke();
                        else OnSwipeRight?.Invoke();
                    }
                }
                
                isDragging = false;
                OnTouchReleased?.Invoke();
            }
        }
    }
}
