using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace EngineeringSimulationLab.Simulations.Gravity
{
    public sealed class BodySelectionController : MonoBehaviour
    {
        private GravitySimulationController simulation;
        private Camera targetCamera;
        private System.Action onSelectionChanged;
        private bool chooseSpawnPointMode;

        public bool ChooseSpawnPointMode => chooseSpawnPointMode;

        public void Initialize(GravitySimulationController gravitySimulation, Camera camera, System.Action selectionChanged)
        {
            simulation = gravitySimulation;
            targetCamera = camera;
            onSelectionChanged = selectionChanged;
        }

        public void BeginChooseSpawnPoint()
        {
            chooseSpawnPointMode = true;
        }

        private void Update()
        {
            if (simulation == null || targetCamera == null || !LeftMousePressedThisFrame())
            {
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Vector3 mouse = MousePosition();
            Vector3 world = targetCamera.ScreenToWorldPoint(mouse);
            Vector2 world2 = new Vector2(world.x, world.y);

            if (chooseSpawnPointMode)
            {
                simulation.SetPendingSpawnWorldPosition(world2);
                chooseSpawnPointMode = false;
                onSelectionChanged?.Invoke();
                return;
            }

            GravityBody body = simulation.GetBodyAtWorldPosition(world2, Mathf.Max(0.16f, targetCamera.orthographicSize * 0.012f));
            if (body != null)
            {
                simulation.SelectBody(body);
                onSelectionChanged?.Invoke();
            }
        }

        private static Vector3 MousePosition()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null ? (Vector3)Mouse.current.position.ReadValue() : Vector3.zero;
#else
            return Input.mousePosition;
#endif
        }

        private static bool LeftMousePressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }
    }
}
