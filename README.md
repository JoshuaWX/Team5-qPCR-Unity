# Team 5 Realistic qPCR Laboratory

Unity `6000.3.25f1` URP project for a realistic, single-room educational qPCR workflow. Team 5 receives Team 4's already prepared plate, configures the run, loads the instrument, watches 35 simulated cycles, and interprets representative controls and amplification curves.

## Play on a Windows desktop or laptop (no Unity needed)

1. Get the current Windows build from `C:\TEAM-5\deliverables\Team5-qPCR\Builds\Windows`. If your teammate provides a ZIP, use the extraction steps below. No GitHub Release is implied by a source-code push.
2. Right-click the ZIP file and choose **Extract All**. Do not run the game while it is still inside the ZIP file.
3. Open the extracted `Team5-qPCR-Windows-x64` folder and double-click `Team5-qPCR.exe`.
4. If Windows SmartScreen appears, choose **More info** then **Run anyway** only if you got the file from the Team 5 project or another trusted teammate.

You do not need Unity to play. Keep the EXE beside its `Team5-qPCR_Data` folder; both are required.

- Choose **Desktop Preview** or **XR Simulator Preview**, then choose **Guided Training** or **Assessment Mode**.
- Progress now comes from the laboratory itself: select the labelled equipment, press the machine controls, validate the protocol, grip and seat the plate, press Start, and interpret the controls.
- Guided Training supplies narration, captions, labels, target pulses, and timed hints. Assessment Mode records mistakes and requested hints.
- Use **Reset Lesson** to restore the complete experience. In the simulator, `R` resets the device pose, not the lesson.

## Open and edit in Unity (developers)

1. Install **Unity Hub** and Unity Editor **`6000.3.25f1`** with **Windows Build Support**. Install Android Build Support (SDK, NDK, OpenJDK, and IL2CPP) too if you need to make the Android/Quest build.
2. Install [Git LFS](https://git-lfs.com/), then clone the repository and retrieve its large models and textures:

   ```powershell
   git lfs install
   git clone https://github.com/JoshuaWX/Team5-qPCR-Unity.git
   cd Team5-qPCR-Unity
   git lfs pull
   ```

3. In Unity Hub, choose **Add** / **Open**, select the cloned `Team5-qPCR-Unity` folder, and wait for package installation and asset importing to finish.
4. In the Project window, open `Assets/Team5/Scenes/Team5_qPCR_RealisticLab.unity`, then press the Unity **Play** button.

Do not delete `.meta` files, `Packages`, or `ProjectSettings`. Edit the scene, scripts, data, and Blender sources in the locations listed below. Read the [testing and build notes](Documentation/TESTING.md) and [GitHub update instructions](Documentation/GITHUB.md) before making a release.

## Android / Quest installation

The included `Team5-qPCR.apk` is an **Android/Quest OpenXR build**, intended for a Quest-class headset; it is not a tested touchscreen-phone app. It is simulator-tested, but still needs physical-headset validation and production signing.

1. Copy `C:\TEAM-5\deliverables\Team5-qPCR\Builds\Android\Team5-qPCR.apk` to the headset using a trusted sideloading method such as Meta Quest Developer Hub or SideQuest.
2. Enable Developer Mode on the headset, install the APK, then launch it from **Unknown Sources**.
3. Use Quest Touch controllers or tracked hands. Controllers remain the reliable fallback; hands use poke for buttons, pinch/grab for the plate, and a hand ray for the protocol screen.

An ordinary Android phone is not a supported release target yet because the current APK expects OpenXR input rather than touchscreen controls. A separate phone build and device test would be needed before giving this to phone users.

## Open and play

Open `Assets/Team5/Scenes/Team5_qPCR_RealisticLab.unity` and enter Play mode.

- Use the visible launcher; F8 is only an optional developer shortcut.
- Desktop Preview keeps the mouse/keyboard camera and compact desktop UI.
- XR Simulator Preview activates the XR Origin camera, controller and tracked-hand interaction paths, blue-gloved hand meshes, automatic modality switching, and the world-space UI.
- Select the highlighted lab objects in order. Scientific steps have no generic Next button.
- After selecting a lesson mode, its chooser becomes inactive and cannot block raycasts. The instruction card remains visible with an exact `Current target:` line and a separate red rejected-action line.
- Use **Replay**, **Mute**, the volume slider, **Help**, and **Show Simulator Controls** when needed.
- Simulator: `H` manipulates the head; `[` / `]` selects a controller; `G` selects/grips 3D objects; `T` operates UI/activation; `Tab` cycles controllers/hands; `K/M/N/O/P` selects grab, pinch, poke, open-hand, and fist poses; `R` resets pose. Use WASD/QE and right mouse to move/rotate the selected device.
- In desktop Guided camera mode, right-drag orbits and the wheel zooms about 12% per notch. First- and third-person remain desktop presentation options only; the headset always controls the VR camera.

## Implemented workflow

`Introduction → Handoff Review → Power On → Protocol Setup → Plate Inspection → Instrument Loading → Run Validation → Amplification → Results Interpretation → Complete`

The protocol is 20 µL, 105°C lid, SYBR Green, 95°C for 2 minutes, 35 cycles of 95°C/15 s, 60°C/30 s and 72°C/30 s, fluorescence acquisition at 60°C, and a 65–95°C melt curve.

## Important folders

- `Assets/Team5/Scripts/Runtime/Data` — scientific configuration, handoff, protocol validation, and generated results.
- `Assets/Team5/Scripts/Runtime/Controllers` — workflow, plate, instrument, run, UI, roof, and interaction-mode behavior.
- `Assets/Team5/Scripts/Runtime/Lesson` — Guided/Assessment modes, narration, cues, physical controls, plate socket, recovery, and reporting.
- `Assets/Team5/Scripts/Runtime/UI` — compact mentor card and progressive curve renderer.
- `Assets/Editor/ProjectBootstrap` — reproducible scene and player builders.
- `Assets/Team5/Tests/EditMode` and `PlayMode` — scientific, workflow, reset, scene, and XR simulator checks.
- `Assets/Samples/XR Interaction Toolkit/3.3.2` — imported Starter Assets and XR Interaction Simulator.
- `Assets/Samples/XR Hands/1.9.0` and XRI Hands Interaction Demo — tracked-hand visuals and interactors.
- `Assets/Team5/Models/Our-design` — supplied Team 5 artist models, including the Q-PCR machine and its own moving tray, cabinets, sink, coats, centrifuge, racks, tubes and static scientist.

## Maintain the scene

Run this editor method through Unity:

```powershell
unity run C:\TEAM-5\Team5-qPCR-Unity --editor-version 6000.3.25f1 -- -executeMethod Team5.qPCR.Editor.Team5RealisticLabBuilder.Build
```

For the existing scene this performs an in-place upgrade, not a destructive regeneration. In the Editor use **Team 5 → Apply blue-white visual upgrade (preserve scene)** and then **Team 5 → Apply complete interactive VR upgrade**. Save your own edits first. The interactive builder recreates only the generated `INTERACTIVE_VR_LESSON` subtree; the laboratory and scientific systems remain intact. Backups are saved outside the project. Existing scene GUIDs and scientific bindings are retained. Original PCR resources are never edited.

Earlier editable generated art is in `ArtSource/Team5_Clinical_Assets.blend`; its generator is `ArtSource/build_lab_assets.py`. Retained generated plate and pipette assets are in `Assets/Team5/Models/Redesign`. The visible scientist now uses the supplied static FBX; it has no verified walk/reach animation. See provenance for the exact sources and replacements.

## Science and publishing

Each active well is a complete 20 µL reaction. There are 26 samples, one positive control and one no-template control; the other 68 wells are unused. The NTC contains water instead of DNA. Team 5 does not mix or seal the handoff plate again.

Read the [interactive VR recreation guide](Documentation/INTERACTIVE_VR_RECREATION_GUIDE.md), [science notes](Documentation/SCIENCE.md), [asset provenance](Documentation/ASSET_PROVENANCE.md), [testing and builds](Documentation/TESTING.md), and [GitHub updates](Documentation/GITHUB.md).

Android is an OpenXR/Quest build target. Touch controllers and OpenXR tracked hands are configured together. **Physical-headset validation pending.** Automated simulator checks are not a substitute for testing controller/hand reach, comfort, tracking reliability, and performance on a Quest.

## Full handoff

See `C:\TEAM-5\deliverables\Team5-qPCR\README.md` for executable builds, the demo script, validation evidence, provenance, and the scientific disclaimer.
