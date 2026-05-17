using EngineeringSimulationLab.Simulations.Gravity;
using EngineeringSimulationLab.UI;
using UnityEngine;

namespace EngineeringSimulationLab.Core
{
    public sealed class SimulationDemoRunner : MonoBehaviour
    {
        [SerializeField] private Camera simulationCamera;

        private ISimulationDemo activeDemo;
        private GravitySimulationController gravityDemo;
        private DemoLauncherUI launcherUI;
        private SpaceCameraController spaceCameraController;
        private bool isPaused;
        private double timeScaleSecondsPerSecond = GravitySimulationController.DefaultTimeScaleSecondsPerSecond;

        public ISimulationDemo ActiveDemo => activeDemo;
        public double TimeScaleSecondsPerSecond => timeScaleSecondsPerSecond;
        public float TimeScale => (float)(timeScaleSecondsPerSecond / GravitySimulationController.DefaultTimeScaleSecondsPerSecond);
        public bool IsPaused => isPaused;
        public Camera SimulationCamera => simulationCamera;

        private void Awake()
        {
            EnsureCamera();
            CreateGravityDemo();
            CreateLauncherUI();
            SelectGravityDemo();
        }

        private void Update()
        {
            if (activeDemo == null || isPaused)
            {
                return;
            }

            activeDemo.TickSimulation(Time.deltaTime);
            launcherUI?.RefreshReadouts();
        }

        public void SelectGravityDemo()
        {
            activeDemo = gravityDemo;
            activeDemo.Initialize();
            SetPaused(false);
            launcherUI?.SetActiveDemo(activeDemo.DemoName);
            launcherUI?.RefreshReadouts();
        }

        public void ResetActiveDemo()
        {
            activeDemo?.ResetSimulation();
            SetPaused(false);
            launcherUI?.RefreshReadouts();
        }

        public void TogglePaused()
        {
            SetPaused(!isPaused);
        }

        public void SetPaused(bool paused)
        {
            isPaused = paused;
            activeDemo?.SetPaused(paused);
            launcherUI?.SetPaused(paused);
        }

        public void SetTimeScale(float value)
        {
            float daysPerSecond = Mathf.Clamp(value, 0.1f, 365f);
            SetTimeScaleSecondsPerSecond(daysPerSecond * 86_400d);
        }

        public void SetTimeScaleSecondsPerSecond(double secondsPerSecond)
        {
            timeScaleSecondsPerSecond = System.Math.Max(1d, secondsPerSecond);
            gravityDemo?.SetTimeScaleSecondsPerSecond(timeScaleSecondsPerSecond);
            launcherUI?.SetTimeScaleLabel(timeScaleSecondsPerSecond);
        }

        public void StepActiveDemo()
        {
            gravityDemo?.StepOneTick();
            launcherUI?.RefreshReadouts();
        }

        public GravitySimulationController GetGravityDemo()
        {
            return gravityDemo;
        }

        private void EnsureCamera()
        {
            if (simulationCamera == null)
            {
                simulationCamera = Camera.main;
            }

            if (simulationCamera == null)
            {
                GameObject cameraObject = new GameObject("Simulation Camera");
                simulationCamera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
            }

            simulationCamera.orthographic = true;
            simulationCamera.orthographicSize = 92f;
            simulationCamera.transform.position = new Vector3(0f, 0f, -10f);
            simulationCamera.backgroundColor = new Color(0.025f, 0.035f, 0.055f);
            simulationCamera.clearFlags = CameraClearFlags.SolidColor;

            spaceCameraController = simulationCamera.GetComponent<SpaceCameraController>();
            if (spaceCameraController == null)
            {
                spaceCameraController = simulationCamera.gameObject.AddComponent<SpaceCameraController>();
            }

            spaceCameraController.Initialize(simulationCamera);
        }

        private void CreateGravityDemo()
        {
            GameObject demoObject = new GameObject("GravityPlanetsDemo");
            gravityDemo = demoObject.AddComponent<GravitySimulationController>();
            gravityDemo.transform.SetParent(transform);
        }

        private void CreateLauncherUI()
        {
            GameObject uiObject = new GameObject("Demo Launcher UI");
            uiObject.transform.SetParent(transform);
            launcherUI = uiObject.AddComponent<DemoLauncherUI>();
            launcherUI.Initialize(this);
        }
    }
}
