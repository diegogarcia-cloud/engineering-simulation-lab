using System.Reflection;
using EngineeringSimulationLab.Core;
using EngineeringSimulationLab.Simulations.Common;
using EngineeringSimulationLab.Simulations.Gravity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EngineeringSimulationLab.EditorTools
{
    public static class SimulationLabSmokeTest
    {
        public static void Run()
        {
            Debug.Log("SimulationLabSmokeTest: starting.");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject runnerObject = new GameObject("Smoke Test Runner");
            SimulationDemoRunner runner = runnerObject.AddComponent<SimulationDemoRunner>();
            InvokeAwake(runner);

            if (runner.ActiveDemo == null)
            {
                throw new System.InvalidOperationException("Runner did not select a default demo.");
            }

            if (runner.GetGravityDemo().Bodies.Count != 9)
            {
                throw new System.InvalidOperationException("Gravity demo did not create the full solar system preset.");
            }

            if (System.Math.Abs(GravityPhysics.G - 6.67430e-11d) > 1e-20d)
            {
                throw new System.InvalidOperationException("Gravity simulation is not using the real SI gravitational constant.");
            }

            if (runner.GetGravityDemo().Sun == null || runner.GetGravityDemo().Sun.PositionMeters.Magnitude > 1e-6d)
            {
                throw new System.InvalidOperationException("The Sun is not fixed at the origin.");
            }

            Vector2d startPosition = runner.GetGravityDemo().FindBodyByName("Earth").PositionMeters;
            runner.GetGravityDemo().StepOneTick();
            Vector2d endPosition = runner.GetGravityDemo().FindBodyByName("Earth").PositionMeters;
            if ((endPosition - startPosition).Magnitude <= 1d)
            {
                throw new System.InvalidOperationException("Gravity simulation did not advance body positions.");
            }

            runner.SetPaused(true);
            if (!runner.IsPaused)
            {
                throw new System.InvalidOperationException("Pause state was not applied.");
            }

            runner.GetGravityDemo().AddCustomBody("Smoke Probe", 1e12d, 0.12d, new Vector2d(UnitFormatter.AstronomicalUnitMeters * 1.2d, 0d), Vector2d.Zero, Color.white, true);
            if (runner.GetGravityDemo().Bodies.Count != 10)
            {
                throw new System.InvalidOperationException("Add body control did not add a body.");
            }

            runner.ResetActiveDemo();
            if (runner.GetGravityDemo().Bodies.Count != 9 || runner.IsPaused)
            {
                throw new System.InvalidOperationException("Reset did not restore the running preset.");
            }

            Object.DestroyImmediate(runnerObject);
            Debug.Log("SimulationLabSmokeTest: passed.");
            EditorApplication.Exit(0);
        }

        private static void InvokeAwake(SimulationDemoRunner runner)
        {
            MethodInfo awake = typeof(SimulationDemoRunner).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            if (awake == null)
            {
                throw new System.MissingMethodException(nameof(SimulationDemoRunner), "Awake");
            }

            awake.Invoke(runner, null);
        }
    }
}
