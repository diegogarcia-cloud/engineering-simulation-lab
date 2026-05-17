using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace EngineeringSimulationLab.Simulations.Gravity
{
    public sealed class SpaceCameraController : MonoBehaviour
    {
        [SerializeField] private float minZoom = 2f;
        [SerializeField] private float maxZoom = 260f;
        [SerializeField] private float zoomSensitivity = 1.275f;

        private Camera targetCamera;
        private Vector3 previousMousePosition;
        private bool dragging;

        public void Initialize(Camera camera)
        {
            targetCamera = camera;
            if (targetCamera != null)
            {
                targetCamera.orthographic = true;
                targetCamera.orthographicSize = 92f;
            }
        }

        private void Update()
        {
            if (targetCamera == null)
            {
                return;
            }

            HandlePan();
            HandleZoom();
        }

        private void HandlePan()
        {
            Vector3 mouse = MousePosition();
            if (MiddleMousePressedThisFrame())
            {
                dragging = true;
                previousMousePosition = mouse;
            }

            if (MiddleMouseReleasedThisFrame())
            {
                dragging = false;
            }

            if (!dragging || !MiddleMouseHeld())
            {
                return;
            }

            Vector3 before = targetCamera.ScreenToWorldPoint(previousMousePosition);
            Vector3 after = targetCamera.ScreenToWorldPoint(mouse);
            Vector3 delta = before - after;
            targetCamera.transform.position += new Vector3(delta.x, delta.y, 0f);
            previousMousePosition = mouse;
        }

        private void HandleZoom()
        {
            float scroll = ScrollDelta();
            if (Mathf.Abs(scroll) < 0.001f)
            {
                return;
            }

            float factor = 1f - scroll * zoomSensitivity;
            targetCamera.orthographicSize = Mathf.Clamp(targetCamera.orthographicSize * factor, minZoom, maxZoom);
        }

        private static Vector3 MousePosition()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null ? (Vector3)Mouse.current.position.ReadValue() : Vector3.zero;
#else
            return Input.mousePosition;
#endif
        }

        private static float ScrollDelta()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null ? Mouse.current.scroll.ReadValue().y * 0.01f : 0f;
#else
            return Input.mouseScrollDelta.y;
#endif
        }

        private static bool MiddleMousePressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.middleButton.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(2);
#endif
        }

        private static bool MiddleMouseReleasedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.middleButton.wasReleasedThisFrame;
#else
            return Input.GetMouseButtonUp(2);
#endif
        }

        private static bool MiddleMouseHeld()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.middleButton.isPressed;
#else
            return Input.GetMouseButton(2);
#endif
        }
    }
}
