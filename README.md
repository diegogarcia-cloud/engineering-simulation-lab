# Engineering Simulation Lab — Gravity Simulator

A small Unity 6 sandbox that integrates real-SI Newtonian gravity in 2D and renders the solar system as an instrumented dashboard. Built to be a useful teaching aid for orbital mechanics: every value the simulator shows is the physics value, not a stylised number.

![Gravity Simulator screenshot](Docs/gravity-simulator.png)

## Highlights

- Custom 2D velocity-Verlet integration (no `Rigidbody2D`), `G = 6.67430 × 10⁻¹¹`
- Real SI units internally; rendered through a configurable display scale
- Sun-fixed solar-system preset (Mercury through Neptune) with eccentric orbit rings sampled from initial state
- Glowing Sun with multi-layer corona + URP Bloom
- Per-planet procedural textures: Earth blue/green, Mars rust, Jupiter banded, Saturn with billboarded rings
- Cyan velocity / orange acceleration vectors with arrowhead caps on the selected body
- Two-layer parallax starfield
- Monospace, right-aligned, two-column stats panel for the selected body
- Equation glyphs rendered with Unicode subscripts/superscripts/middle-dot
- Auto-culls runaway bodies that drift beyond 9 × 10¹² m (2× Neptune's orbit), falls back to Earth selection
- Spawn custom bodies with mass presets, optional auto-orbit-Sun velocity, or free-release vectors

## Controls

- Pause / resume the simulation
- Step the simulation forward by one hour
- Reset the solar system to its initial state
- Time scale slider, expressed in real seconds per real second
- Toggle orbit trails and show/hide the velocity & acceleration vectors of the selected body
- Trail length and vector scale sliders
- Click any body to select it; the right panel shows live SI values for that body
- Compact custom-body spawner with mass presets and auto-orbit or free-release modes
- Middle-drag to pan, scroll wheel to zoom the map

## Requirements

- Unity **6000.3.15f1** (Unity 6) with the Universal Render Pipeline (URP).
- Windows 10/11 x64 for the prebuilt binary.

## Run from source

1. Open the project folder with Unity Hub using Unity `6000.3.15f1`.
2. Open `Assets/Scenes/MainMenuScene.unity` (also configured as the first build scene).
3. Press Play in the Editor.

## Build a Windows player

From the Unity Editor menu: **Engineering Lab → Build Windows x64 (Release)**.
The build is written to `Build/GravitySimulator/EngineeringSimulationLab.exe` with the project's Mono backend at 1920×1080 windowed.

To switch to IL2CPP, install the Unity IL2CPP module and the Visual Studio 2022 C++ workload, then edit `Assets/Editor/BuildLab.cs` and change the scripting backend in `ConfigurePlayerSettings` to `ScriptingImplementation.IL2CPP`.

## Download (Windows)

The latest release archive is published on the [GitHub Releases page](../../releases). Unzip and run `EngineeringSimulationLab.exe`.

## Project layout

```
Assets/
  Editor/                       Build + screenshot menu items
  Scripts/
    Core/                       Bootstrap, ISimulationDemo runner
    Simulations/Common/         Vector2d, unit formatters, math
    Simulations/Gravity/        Physics, controller, view renderers
    UI/                         Runtime-built dashboard, label/stat panels
  Scenes/MainMenuScene.unity    Entry scene
  Tests/EditMode/               EditMode test scaffolding
```

The full development log lives in [`DEVLOG.md`](DEVLOG.md).

## License

[MIT](LICENSE) © Engineering Simulation Lab.
