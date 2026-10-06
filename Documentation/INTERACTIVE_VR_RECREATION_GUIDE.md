# Recreate the Team 5 Interactive VR Upgrade

This guide explains the finished interactive upgrade in the order a beginner can rebuild it. It assumes the realistic room, qPCR instrument, prepared plate, scientific data objects, and desktop workflow already exist.

The finished scene is:

`Assets/Team5/Scenes/Team5_qPCR_RealisticLab.unity`

Unity uses metres. A position of `1` means one metre. The plate is approximately `0.128 × 0.085 m`, not 1.28 m or 12.8 m.

## 1. Understand the architecture first

The project deliberately separates four jobs:

```text
XR controller or desktop input
→ an interactable scene object
→ a scientific controller validates the action
→ the lesson controller advances and updates narration/cues
```

- Data classes describe the protocol, plate handoff, reaction mix, and lesson steps.
- Scientific controllers decide whether an action is valid.
- XRI components convert controller pointing, pressing, gripping, and socketing into Unity events.
- The lesson controller updates instructions, captions, labels, hints, and the session report.

Never let a decorative button advance the lesson directly. It must call the real scientific action first.

## 2. Verify packages

Open `Window → Package Management → Package Manager` and verify:

- Universal RP `17.3.0`
- Input System `1.20.0`
- Test Framework `1.6.0`
- XR Interaction Toolkit `3.3.2`
- XR Plug-in Management `4.6.1`
- OpenXR `1.16.1`
- Meta OpenXR `2.3.2`

Select XR Interaction Toolkit, open **Samples**, and import:

- Starter Assets
- XR Device Simulator or Interaction Simulator sample

Do not manually type package versions into `manifest.json` when Package Manager can install them.

## 3. Configure XR Plug-in Management

1. Open `Edit → Project Settings → XR Plug-in Management`.
2. On Windows, enable OpenXR for simulator and desktop headset testing.
3. On Android, enable OpenXR.
4. Open `OpenXR → Interaction Profiles`.
5. Add the Meta Quest Touch Controller profile.
6. Enable the Meta Quest support feature required by the installed Meta OpenXR package.
7. Keep optical hand tracking disabled until a real headset is available.

The project is controller-first. The blue hands are visual models driven by left and right controller poses; they are not camera-based optical hands.

## 4. Build the XR Origin correctly

Create it with `GameObject → XR → XR Origin (VR)` if it is missing. Rename the root:

`XR_ORIGIN_LEFT_RIGHT_CONTROLLERS`

The hierarchy must follow this idea:

```text
XR_ORIGIN_LEFT_RIGHT_CONTROLLERS
├─ Camera Offset
│  ├─ Main Camera
│  ├─ Left Controller
│  └─ Right Controller
├─ Input Action Manager
└─ XR Interaction Manager
```

On `Main Camera`:

- keep the Camera component;
- add Tracked Pose Driver/Input System pose tracking if the XR Origin template did not add it;
- use a near clipping plane near `0.05` m;
- add an Audio Listener;
- tag it `MainCamera` when XR mode is active.

Important: never animate this camera in VR. The headset controls its position and rotation. Guided camera animation is desktop-only.

Each controller needs:

- an action-based controller or input reader from the Starter Assets;
- an XR Ray Interactor for panels and distant controls;
- an XR Direct Interactor for grabbing the plate at close range;
- a Line Renderer or XR Interactor Line Visual for the ray;
- a controller or blue-glove visual.

Create one `XR Interaction Manager` in the scene. Do not create a different manager for every object.

## 5. Switch cameras by mode

The `LabShellController` owns the desktop/XR switch.

Desktop mode:

- `Desktop_Overview_Camera` is active;
- XR Origin is inactive;
- desktop Canvas is active;
- XR world-space Canvas is inactive;
- roof and upper front wall hide only in the exterior overview.

XR mode:

- desktop camera is inactive;
- XR Origin is active;
- XR world-space Canvas is active;
- roof and upper front wall remain visible;
- only the XR Origin camera renders.

An Audio Listener exists on both possible cameras, but only the listener on the active camera is active because the other camera root is disabled.

## 6. Add the two lesson modes

Create the enum and session data used by `GuidedLessonController`:

```csharp
public enum LessonMode
{
    GuidedTraining,
    Assessment
}
```

Guided Training displays the current target, pulses it after 12 seconds, and gives a stronger hint after 20 seconds.

Assessment Mode shows minimal help. Pressing Help temporarily displays the target and increments `HintsUsed`. Invalid actions increment `Mistakes`.

The session report stores:

- selected mode;
- elapsed time;
- mistakes;
- hints used;
- completed steps.

## 7. Define the 24 required actions

Use one enum value per observable action. The finished lesson uses this order:

1. Select lab coat.
2. Select blue nitrile gloves.
3. Select handwashing sink.
4. Select prepared plate.
5. Select optical seal.
6. Select qPCR instrument.
7. Select touchscreen.
8. Select drawer.
9. Select thermal block.
10. Select results monitor.
11. Select plate centrifuge.
12. Select waste containers.
13. Press Power.
14. Validate protocol.
15. Inspect plate ID.
16. Inspect optical seal.
17. Inspect bubbles.
18. Inspect A1 marker.
19. Press Open.
20. Grip and seat the plate.
21. Press Close.
22. Press Start.
23. Observe amplification.
24. Interpret the controls correctly.

Each `LessonStepDefinition` contains the action, heading, short instruction, child-friendly reason, and matching narration clip.

## 8. Add object descriptors and floating labels

For every tour or inspection target:

1. Create a child object with a small Box Collider covering the intended target area.
2. Enable **Is Trigger** if the proxy must not block physics.
3. Add `XRSimpleInteractable`.
4. Add `LabObjectDescriptor`.
5. Enter the display name, explanation, and matching `TrainingAction`.
6. Add a world-space label above the object.
7. Configure the label to face the active camera.
8. Hide the label when it is far away.

Colour meaning:

- blue: current target;
- green: completed;
- amber: attention or waiting;
- red: rejected or incorrect.

The finished scene has descriptors for the PPE, sink, prepared plate, optical seal, instrument, touchscreen, drawer, thermal block, monitor, centrifuge, waste container, and four inspection checkpoints.

## 9. Create physical machine controls

Create `PHYSICAL_MACHINE_CONTROLS` under the qPCR instrument. Add separate children for Power, Open, Close, and Start.

For each control:

1. Give it visible button geometry.
2. Add a Box Collider sized tightly around the button.
3. Add `XRSimpleInteractable`.
4. Add `PhysicalControlInteractable`.
5. Choose its action: Power, Open, Close, or Start.
6. Assign `GuidedLessonController` and `InstrumentController` references.

The control must attempt the real operation. Examples:

- Open calls the instrument drawer method and only reports success when the workflow permits it.
- Close refuses when no correctly seated plate is present.
- Start refuses when protocol, plate, drawer, and workflow state are incomplete.

Use short haptic impulses for hover, successful press, grabbing, insertion, and errors. Error feedback should be stronger than hover feedback but still comfortable.

## 10. Make the plate grabbable

On the prepared plate root:

1. Keep a tight Box Collider around the plate.
2. Add a Rigidbody.
3. Add `XRGrabInteractable`.
4. Use both left and right direct/ray interactors as valid interactor groups.
5. Add `LessonPlateGrabGate` so grabbing is enabled only during the loading part.
6. Add `PlateDropRecovery`.
7. Store the handoff-station transform as the home pose.

`PlateDropRecovery` checks the Y position. If the plate falls below the floor, it clears velocity and returns the plate to its home pose.

Do not use an oversized collider. The physical size should remain near `0.128 × 0.085 m` so controller reach feels believable.

## 11. Create the A1-sensitive socket

Under the motorized drawer:

1. Create an empty object called `A1_ALIGNED_PLATE_SOCKET`.
2. Place it at the centre of the thermal block.
3. Rotate it so a correctly loaded plate has A1 in the reference corner.
4. Add a trigger Box Collider slightly larger than the plate.
5. Add `XRSocketInteractor`.
6. Add `OrientationAwarePlateSocket`.
7. Assign the plate, lesson controller, workflow controller, and instrument controller.

On select-enter, calculate the angle difference between the plate and socket. If it is outside the allowed tolerance:

- reject the socket;
- show red feedback;
- play error haptics;
- count an assessment mistake;
- keep the lesson on the same step.

If it is correct, snap the plate, mark it seated, turn the cue green, and unlock Close.

## 12. Require four separate plate inspections

Do not use one generic “Inspect plate” click. Add four selectable proxy regions:

- `Inspect_PlateId_Target`
- `Inspect_Seal_Target`
- `Inspect_Bubbles_Target`
- `Inspect_A1_Target`

Each proxy has its own label, explanation, `XRSimpleInteractable`, and `TrainingAction`. Loading stays locked until all four are completed.

## 13. Reuse the real protocol validator

The protocol panel must edit the learner’s runtime configuration, not the correct reference asset.

Required values:

- reaction volume: 20 µL;
- lid: 105°C;
- chemistry: SYBR Green;
- initial hold: 95°C for 2 minutes;
- cycles: 35;
- denaturation: 95°C for 15 seconds;
- annealing/acquisition: 60°C for 30 seconds;
- extension: 72°C for 30 seconds;
- melt curve: 65–95°C.

Add a `Validate protocol` button to the desktop and XR panel. The button calls `ProtocolSetupController.ValidateAndApply()`. Invalid fields remain red and the lesson does not advance.

## 14. Build the compact desktop and XR UI

Desktop UI uses a Screen Space Overlay Canvas. XR UI uses a World Space Canvas placed at a readable distance.

Create these panels:

- mode selection;
- current-step card;
- caption strip;
- simulator help;
- completion report;
- contextual protocol/results panels.

Use white cards, pale blue backgrounds, navy text, medium-blue primary buttons, rounded corners, and Source Sans 3. Keep normal desktop instructions under one-third of the screen.

Buttons:

- Guided Training
- Assessment Mode
- Help
- Replay
- Mute
- Reset Lesson
- Desktop Preview
- XR Simulator Preview
- Show Simulator Controls

The generic Next button is not used for scientific actions.

## 15. Add narration and captions

Add one non-spatial Audio Source to `INTERACTIVE_VR_LESSON`:

- Play On Awake: off
- Loop: off
- Spatial Blend: 0
- Volume: about 0.82

Import narration WAV files into `Assets/Team5/Audio/Narration`.

For every cue, store:

- an ID matching the lesson step;
- an AudioClip;
- caption title;
- caption text.

`NarrationController` stops the previous clip before playing the next so clips never overlap. Replay restarts the current cue. Mute and the volume slider update the same source. Captions remain visible while the action is pending.

The current narration is temporary computer narration and can be replaced later without changing workflow logic.

## 16. Add guidance and recovery

`GuidanceCueController` receives every `LabObjectDescriptor` plus a cue arrow.

When a step begins:

1. clear the previous cue;
2. mark the new target blue;
3. show its label;
4. wait 12 seconds;
5. pulse the outline;
6. after 20 seconds, display the stronger hint and arrow.

On a successful action, make the target green and move to the next step. On an invalid action, flash red, play error haptics, and keep the current step.

Reset must restore workflow stage, plate pose, protocol runtime copy, drawer, instrument power, graph, results, labels, narration, elapsed time, mistakes, hints, and the mode chooser.

## 17. Test without a headset

Start from the visible launcher and choose **XR Simulator Preview**.

Simulator controls shown by the application:

```text
WASD / QE     Move selected simulated device
H             Manipulate the head
[ and ]       Select left or right controller
Right mouse   Rotate
T             Trigger
G             Grip
Tab           Cycle simulated devices
R             Reset simulator pose
```

If F8 opens Windows Project instead of VR, ignore it. F8 is only an optional developer shortcut. The visible launcher is the supported way to enter simulator mode.

Test both simulated controllers. Point at labels and buttons, grip the plate, intentionally rotate it incorrectly once, then correct it. Confirm only the XR Origin camera is active.

## 18. Run automated tests

Open `Window → General → Test Runner`.

Run EditMode tests first. They check data and validation without entering the scene.

Run PlayMode tests second. They load the real scene and verify cameras, controller objects, socket rules, reset, 96 wells, 28 active reactions, the 35-cycle run, positive-control amplification, and flat NTC.

After tests, open Console and resolve red compilation/runtime errors before building. A test is evidence only when it finishes and passes.

## 19. Build Windows

1. Open `File → Build Profiles`.
2. Select Windows.
3. Confirm `Team5_qPCR_RealisticLab` is the only enabled scene.
4. Set architecture to Intel 64-bit.
5. Choose `C:\TEAM-5\deliverables\Team5-qPCR\Builds\Windows`.
6. Click Build.

The executable is `Team5-qPCR.exe`. Keep its `_Data` folder beside it.

## 20. Build Quest APK

1. Install Android Build Support, SDK, NDK, OpenJDK, and IL2CPP for Unity `6000.3.25f1`.
2. Open Build Profiles and activate Android.
3. Set Scripting Backend to IL2CPP.
4. Enable ARM64 and disable unsupported desktop architectures.
5. Confirm OpenXR and Meta Quest support are enabled for Android.
6. Build to `C:\TEAM-5\deliverables\Team5-qPCR\Builds\Android\Team5-qPCR.apk`.

The APK may be called Quest-ready only after build validation. Until it runs on physical hardware, describe it exactly as:

**Simulator-tested; physical-headset validation pending.**

## 21. Present it on Zoom

1. Open the finished scene.
2. Enter Play mode.
3. Choose XR Simulator Preview.
4. Maximise Game view at 1920×1080.
5. Share only the Game view/window.
6. Show both blue hands, select at least one labelled object, press Power, inspect the plate, load it, start the run, and interpret the controls.
7. Keep the Windows desktop build ready as a fallback.
8. Keep the recorded 1080p walkthrough ready if Unity or Zoom becomes unstable.

## 22. Rebuild the finished upgrade automatically

The manual steps above teach what every part means. In the finished project, the same generated subtree can be recreated consistently:

1. Save the scene.
2. Exit Play mode.
3. Choose `Team 5 → Apply complete interactive VR upgrade`.
4. Wait for Unity to finish compiling/importing.
5. Open Console and confirm the success message.

The builder deletes and recreates only `INTERACTIVE_VR_LESSON`. It preserves the laboratory, scientific systems, and unrelated user work.

## Files to study in order

1. `Assets/Team5/Scripts/Runtime/Lesson/LessonStepDefinition.cs`
2. `Assets/Team5/Scripts/Runtime/Lesson/LessonSessionReport.cs`
3. `Assets/Team5/Scripts/Runtime/Lesson/GuidedLessonController.cs`
4. `Assets/Team5/Scripts/Runtime/Lesson/LabObjectDescriptor.cs`
5. `Assets/Team5/Scripts/Runtime/Lesson/GuidanceCueController.cs`
6. `Assets/Team5/Scripts/Runtime/Lesson/NarrationController.cs`
7. `Assets/Team5/Scripts/Runtime/Lesson/PhysicalControlInteractable.cs`
8. `Assets/Team5/Scripts/Runtime/Lesson/OrientationAwarePlateSocket.cs`
9. `Assets/Team5/Scripts/Runtime/Lesson/PlateDropRecovery.cs`
10. `Assets/Team5/Scripts/Runtime/Lesson/PresentationLauncherController.cs`
11. `Assets/Editor/ProjectBootstrap/Team5InteractiveVrUpgrade.cs`
12. `Assets/Team5/Tests/EditMode/LessonSessionReportTests.cs`
13. `Assets/Team5/Tests/PlayMode/InteractiveVrLessonTests.cs`

For each file, be able to answer: what job does it perform, what object owns it, what references it needs, what success looks like, what failure it blocks, and how reset affects it.
