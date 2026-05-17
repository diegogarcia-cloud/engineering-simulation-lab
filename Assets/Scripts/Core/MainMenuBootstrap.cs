using UnityEngine;

namespace EngineeringSimulationLab.Core
{
    public sealed class MainMenuBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRunnerAfterSceneLoad()
        {
            if (FindFirstObjectByType<SimulationDemoRunner>() != null)
            {
                return;
            }

            GameObject runnerObject = new GameObject("Simulation Demo Runner");
            runnerObject.AddComponent<SimulationDemoRunner>();
        }

        private void Awake()
        {
            if (FindFirstObjectByType<SimulationDemoRunner>() != null)
            {
                return;
            }

            GameObject runnerObject = new GameObject("Simulation Demo Runner");
            runnerObject.AddComponent<SimulationDemoRunner>();
        }
    }
}
