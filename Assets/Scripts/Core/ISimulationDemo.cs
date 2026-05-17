namespace EngineeringSimulationLab.Core
{
    public interface ISimulationDemo
    {
        string DemoName { get; }
        void Initialize();
        void TickSimulation(float dt);
        void ResetSimulation();
        void SetPaused(bool paused);
    }
}
