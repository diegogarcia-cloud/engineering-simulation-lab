# Unity Engineering Simulation Lab

## Project Guide

Build the project in Unity 6 with C#. The Universal 2D template is a good fit for orbit demos, cellular automata, and 2D engineering visualizations.

Current project root: `D:\Use\Proyect\Lab`. Open this folder from Unity Hub with Unity `6000.3.15f1`.

Start testing from `Assets/Scenes/MainMenuScene.unity`; it is also the first scene in `ProjectSettings/EditorBuildSettings.asset`.

## Current Status

- Implemented a single-focus real-SI Gravity Simulator interface.
- The runtime bootstraps itself through `MainMenuBootstrap` and `SimulationDemoRunner`.
- UI is generated at runtime by `DemoLauncherUI`; the visible demo-selection menu has been removed for now.
- Gravity simulation uses custom 2D Newtonian integration with real SI units, not `Rigidbody2D`.
- Bodies render as generated sprites with `LineRenderer` orbit trails, larger visual-only planet sizes, planet labels, and selected-body ring highlighting.
- A generated starfield backdrop is created at runtime by `SpaceBackdropRenderer`.
- The current visual target is `D:\Use\Proyect\screenshots\model.png`; current QA captures are in `D:\Use\Proyect\screenshots`.
- Controls currently include:
  - pause/play
  - one-hour step
  - reset
  - time scale slider in real seconds per real second
  - show/hide trails and vectors
  - trail length and vector scale
  - planet selection
  - live selected-body SI stats panel
  - compact custom body spawner with mass presets and auto-orbit/free-release modes

## Key Changes

- Created a runtime simulator dashboard UI:
  - `MainMenuScene`
  - top title/time badge
  - compact left simulation/display controls
  - right live selected-body data, equations, and spawn panel
  - bottom status bar
- First demo: `Gravity Simulator`
  - Sun fixed at origin
  - Mercury through Neptune solar-system preset
  - real gravitational constant `G = 6.67430e-11`
  - distances in meters internally, rendered through a visual display scale
  - velocity-Verlet Sun-only gravity by default
  - orbit trails, selection ring highlighting, labels, velocity/acceleration vectors
  - click-to-place custom body spawn point
- Recent UI pass:
  - removed the visible demo selector
  - made the right-side stats panel update every frame for the selected body
  - reformatted selected-body data as readable instrumentation rows
  - added denser starfield rendering
  - increased visual-only planet sizes without changing physics
  - made spawn controls more compact to reduce clipping
  - fixed Unity layout-group sizing so controls use compact intended heights
  - spread initial planetary orbital phases around the Sun to avoid label clustering
  - adjusted default camera zoom to show more of the solar-system map
  - hid planet labels when they would overlap fixed UI panels
  - added visual-only zoom compensation so planets remain visible at wider map zoom
  - latest inspected capture `codex-unity-window-7.png` shows stable panels with no major control overflow, but the center map still needs more polish to approach `model.png`
- C# scripts are organized by purpose:
  - `Scripts/Core`
  - `Scripts/UI`
  - `Scripts/Simulations/Gravity`
  - `Scripts/Simulations/Common`

## Main Interfaces

- `ISimulationDemo`
  - `Initialize()`
  - `TickSimulation(float dt)`
  - `ResetSimulation()`
  - `SetPaused(bool paused)`
- `GravityBody`
  - mass in kg
  - physical radius in meters
  - visual radius in Unity units
  - position, velocity, acceleration, force in SI units
- `GravitySimulationController`
  - owns bodies
  - advances real SI physics
  - converts meters to Unity units for rendering
  - exposes UI parameters
- `SpaceBackdropRenderer`
  - creates the generated starfield background
- `GravityBodyLabelOverlay`
  - keeps planet labels aligned to screen-space body positions
- `GravityStatsPanel`
  - formats real-time selected-body values from the simulation state
- `GravitySimulatorScreenshotCapture`
  - editor utility for opening the scene in Play Mode and writing screenshots to `D:\Use\Proyect\screenshots`
  - direct Unity `ScreenCapture` can fail in automated/batch contexts; Windows window capture was the reliable path during the last session

## Test Plan

- Run scene in Unity Editor.
- Use Game view Full HD or QHD when judging UI, because Unity Editor side panels change the captured composition.
- Confirm the Sun remains fixed at origin.
- Confirm all 8 planets are present and Earth is selected by default.
- Confirm planets orbit under Sun-only Newtonian gravity.
- Confirm selected-body ring and right-panel header match the clicked body.
- Confirm selected-body values update live while the simulation runs.
- Confirm pause, reset, and time scale work.
- Confirm custom bodies can be placed by clicking the world view and added with auto-orbit or free-release velocity.
- Confirm stats show real SI values and readable units.
- Confirm middle-mouse pan and mouse-wheel zoom work while UI remains fixed.
- Confirm UI does not block the simulation view.
- Confirm left panels, right panels, and bottom bar do not overlap at the active Game view resolution.
- Confirm labels do not appear under the right stats panel or left controls.
- Confirm adding future demos does not require rewriting gravity code.

## Verification Notes

- Unity EditMode test execution passed with Unity `6000.3.15f1`.
- Edit Mode test scaffold exists at `Assets/Tests/EditMode/SimulationLabEditModeTests.cs`.
- Latest aggressive UI pass was checked with `dotnet restore EngineeringSimulationLab.csproj` and `dotnet build EngineeringSimulationLab.csproj --no-restore`; build passed with 0 errors.
- Unity batchmode verification can be blocked if the same project is already open in the Unity Editor.
- Screenshot workflow added at `Assets/Editor/GravitySimulatorScreenshotCapture.cs`.
- Batch `ScreenCapture` returned black/null frames in this environment, so visual QA used normal Unity Editor Play Mode plus Windows window capture.
- Latest visual QA screenshot saved to `D:\Use\Proyect\screenshots\codex-unity-window-7.png`.
- Last known good compile check:
  - `dotnet restore EngineeringSimulationLab.csproj`
  - `dotnet build EngineeringSimulationLab.csproj --no-restore`
  - result: 0 warnings, 0 errors

## Next UI Improvements

- Keep iterating toward `model.png`.
- The dashboard is now structurally stable, but the central solar-system visualization should be improved next:
  - stronger orbit lines
  - richer star/space background
  - larger/more distinctive planet rendering
  - better selected-body vector arrows
  - improved stats typography and value alignment
  - optional labels that avoid clustering near the Sun
- Consider hiding the Unity Editor side panels or using a standalone build for cleaner screenshot QA.

## Assumptions

- Use Unity 6.0+ and C#.
- Start with 2D simulations, not 3D.
- Use simple custom physics for gravity, not Unity Rigidbody2D.
- Keep the app educational and engineering-oriented.
- Visual accuracy is useful, but clear controls and readable motion matter first.
