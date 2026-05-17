using System.Reflection;
using EngineeringSimulationLab.Core;
using EngineeringSimulationLab.Simulations.Common;
using EngineeringSimulationLab.Simulations.Gravity;
using NUnit.Framework;
using UnityEngine;

namespace EngineeringSimulationLab.Tests
{
    public sealed class SimulationLabEditModeTests
    {
        [Test]
        public void RunnerCreatesGravityDemoAndControlsIt()
        {
            GameObject runnerObject = new GameObject("Test Runner");
            try
            {
                SimulationDemoRunner runner = runnerObject.AddComponent<SimulationDemoRunner>();
                InvokeAwake(runner);

                Assert.That(runner.ActiveDemo, Is.Not.Null);
                Assert.That(runner.GetGravityDemo().Bodies.Count, Is.EqualTo(9));
                Assert.That(GravityPhysics.G, Is.EqualTo(6.67430e-11d).Within(1e-20d));
                Assert.That(runner.GetGravityDemo().Sun.Name, Is.EqualTo("Sun"));
                Assert.That(runner.GetGravityDemo().Sun.PositionMeters.Magnitude, Is.EqualTo(0d).Within(1e-6d));
                Assert.That(runner.GetGravityDemo().SelectedBody.Name, Is.EqualTo("Earth"));

                Vector2d startPosition = runner.GetGravityDemo().FindBodyByName("Earth").PositionMeters;
                runner.GetGravityDemo().StepOneTick();
                Assert.That((runner.GetGravityDemo().FindBodyByName("Earth").PositionMeters - startPosition).Magnitude, Is.GreaterThan(1d));

                runner.SetTimeScale(2f);
                Assert.That(runner.TimeScaleSecondsPerSecond, Is.EqualTo(172_800d).Within(0.0001d));

                runner.SetPaused(true);
                Assert.That(runner.IsPaused, Is.True);

                runner.GetGravityDemo().AddCustomBody("Test Probe", 1e12d, 0.12d, new Vector2d(UnitFormatter.AstronomicalUnitMeters * 1.2d, 0d), Vector2d.Zero, Color.white, true);
                Assert.That(runner.GetGravityDemo().Bodies.Count, Is.EqualTo(10));
                Assert.That(runner.GetGravityDemo().SelectedBody.Name, Is.EqualTo("Test Probe"));

                runner.ResetActiveDemo();
                Assert.That(runner.IsPaused, Is.False);
                Assert.That(runner.GetGravityDemo().Bodies.Count, Is.EqualTo(9));
            }
            finally
            {
                Object.DestroyImmediate(runnerObject);
            }
        }

        private static void InvokeAwake(SimulationDemoRunner runner)
        {
            MethodInfo awake = typeof(SimulationDemoRunner).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(awake, Is.Not.Null);
            awake.Invoke(runner, null);
        }
    }
}
