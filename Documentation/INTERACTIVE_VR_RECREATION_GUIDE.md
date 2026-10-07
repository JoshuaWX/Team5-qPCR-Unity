# Recreate the Complete Team 5 qPCR Laboratory From Scratch

This is a zero-to-finished-project course for someone who has never used Unity. It covers the entire application: the 12 m × 9 m laboratory, furniture, models, materials, lighting, desktop interface, lesson logic, qPCR station, XR controllers, and real tracked-hand support. Follow it in order in a separate practice project. Do not practise inside the completed project until you understand what a change will affect.

The finished scene is:

`Assets/Team5/Scenes/Team5_qPCR_RealisticLab.unity`

Unity uses metres. A position of `1` means one metre. The plate is approximately `0.128 × 0.085 m`, not 1.28 m or 12.8 m.

## Part A — Build the project and laboratory

### A1. Learn the six words Unity keeps using

- **Project:** the complete application folder. It contains `Assets`, `Packages`, and `ProjectSettings`.
- **Scene:** one saved 3D environment. Our whole laboratory lesson uses one scene.
- **GameObject:** an item in the scene, such as a wall, camera, plate, or button.
- **Component:** a feature attached to a GameObject. A Transform places it, a Renderer draws it, a Collider gives it a physical boundary, and a script gives it behaviour.
- **Asset:** a saved file such as a model, material, script, image, font, scene, or prefab.
- **Prefab:** a reusable GameObject arrangement saved as an asset.

The main Editor windows are:

- **Hierarchy:** every GameObject in the open scene.
- **Scene:** the 3D editing workspace.
- **Game:** what the active camera sees.
- **Inspector:** the settings of the selected object.
- **Project:** files in the `Assets` folder.
- **Console:** errors, warnings, and messages.

Beginner exercise: create a Cube with `GameObject → 3D Object → Cube`, rename it `PracticeCube`, set Position to `(0, 1, 0)`, set Scale to `(2, 0.5, 1)`, and then delete it. This teaches the same actions used to make the room.

### A2. Install Unity and create the correct project

1. Install Unity Hub.
2. In Hub, install Unity Editor `6000.3.25f1`.
3. Include Windows Build Support.
4. Also include Android Build Support, Android SDK/NDK Tools, OpenJDK, and IL2CPP if you will make a Quest APK.
5. Click **New project**.
6. Choose **Universal 3D / URP**.
7. Name it `Team5-qPCR-Practice`.
8. Save it anywhere you can find it. `C:\Unity\Team5-qPCR-Practice` is valid; the location does not change the project.
9. Wait for package import to finish before clicking around.

URP is used because it gives us modern physically based materials and lighting while still supporting Windows and mobile VR. The VR template is convenient, but it is not required. VR is added through XR packages and an XR Origin, so URP gives us desktop, simulator, and headset support in the same project.

### A3. Save the real scene and make it the startup scene

1. Choose `File → Save As`.
2. Create `Assets/Team5/Scenes`.
3. Save as `Team5_qPCR_RealisticLab.unity`.
4. Open `File → Build Profiles`.
5. Select Windows and open **Scene List**.
6. Click **Add Open Scenes**, or drag the scene asset from Project into the Scene List.
7. Enable its checkbox.
8. Remove or disable `SampleScene` so the Team 5 scene is entry `0`.

Save often with `Ctrl+S`. An asterisk beside the scene name means it has unsaved changes.

### A4. Install the packages and samples

Open `Window → Package Management → Package Manager`. Select **Unity Registry**, search for each package, and install the version compatible with Unity `6000.3.25f1`:

| Purpose | Package | Finished-project version |
|---|---|---:|
| Rendering | Universal RP | 17.3.0 |
| Keyboard/mouse/XR input | Input System | 1.20.0 |
| Automated tests | Test Framework | 1.6.0 |
| VR interaction | XR Interaction Toolkit | 3.3.2 |
| XR lifecycle | XR Plug-in Management | 4.6.1 |
| Cross-headset API | OpenXR Plugin | 1.16.1 |
| Quest features | Meta OpenXR | 2.3.2 |
| Tracked hands | XR Hands | 1.9.0 |

If Input System asks to restart and enable the new input backend, accept. In `Edit → Project Settings → Player`, set **Active Input Handling** to **Input System Package (New)** or **Both**.

From the XR Interaction Toolkit **Samples** tab import:

- Starter Assets;
- XR Interaction Simulator;
- Hands Interaction Demo.

From the XR Hands `1.9.0` **Samples** tab import **HandVisualizer**.

Unity Pipeline is optional development tooling. The qPCR application does not require it.

### A5. Create an understandable folder structure

Create these folders in the Project window:

```text
Assets/
  Team5/
    Data/
    Materials/RealisticLab/
    Models/Redesign/
    Prefabs/Processed/
    Scenes/
    Scripts/Runtime/
      Controllers/
      Data/
      Lesson/
      UI/
    Tests/EditMode/
    Tests/PlayMode/
    UI/Fonts/
  Editor/ProjectBootstrap/
ArtSource/                 ← beside Assets, not inside it
```

Create/move folders inside Unity, not File Explorer while Unity is open. Unity places a `.meta` file beside every asset. Keep those files because their GUIDs preserve references.

### A6. Understand metres and axes before making the room

Unity uses:

```text
X = left and right
Y = up and down
Z = forward and backward
```

A new Cube is `1 m × 1 m × 1 m`. With Rotation `(0,0,0)`, Scale X is width, Scale Y is height, and Scale Z is depth. `0.1` means 10 cm. That is why a side wall uses X scale `0.1` for thickness but Z scale `9` for room length.

The room plan is:

```text
                     BACK: Z = +4.5
          ┌─────────────────────────────┐
LEFT      │ back bench and qPCR station │      RIGHT
X = -6    │                             │      X = +6
          │      open working area      │
          │ door, PPE and waste         │
          └─────────────────────────────┘
                     FRONT: Z = -4.5
Floor height = Y 0
```

### A7. Build the 12 m × 9 m room with exact values

Create an Empty GameObject, rename it `LABORATORY_SHELL_12m_x_9m`, and use the Transform component menu to **Reset** it. The parent must be Position `(0,0,0)`, Rotation `(0,0,0)`, Scale `(1,1,1)`.

For each row below, right-click the parent, choose `3D Object → Cube`, rename it, and type the values in Inspector. Keep every Rotation at `(0,0,0)`.

| Object | Position | Scale | Meaning |
|---|---|---|---|
| `Epoxy_Floor` | `(0, -0.08, 0)` | `(12, 0.16, 9)` | Its top surface is exactly Y 0. |
| `Back_Wall` | `(0, 1.6, 4.45)` | `(12, 3.2, 0.1)` | 12 m wide, 3.2 m high. |
| `Left_Wall` | `(-5.95, 1.6, 0)` | `(0.1, 3.2, 9)` | 10 cm thick, 9 m long. |
| `Right_Wall` | `(5.95, 1.6, 0)` | `(0.1, 3.2, 9)` | Mirrors the left wall. |

If a side wall looks like a thin pole, its Z scale is probably still `0.1`; change Z to `9`. Do not rotate the side walls by 90 degrees.

Create an Empty child named `REMOVABLE_UPPER_FRONT_WALL`, reset it, and create these child cubes:

| Object | Position | Scale |
|---|---|---|
| `Front_Header` | `(0, 2.85, -4.45)` | `(12, 0.7, 0.1)` |
| `Front_Left_Return` | `(-5.45, 1.25, -4.45)` | `(1.0, 2.5, 0.1)` |
| `Front_Right_Return` | `(0.75, 1.25, -4.45)` | `(10.4, 2.5, 0.1)` |

Create another Empty child named `REMOVABLE_CEILING`, reset it, and add `Ceiling_Slab` at Position `(0, 3.18, 0)`, Scale `(12, 0.12, 9)`.

Add six light-panel cubes under the ceiling. Each has Scale `(1.5, 0.04, 0.62)` and one of these positions:

```text
(-3.8, 3.08, -2.6)   (0, 3.08, -2.6)   (3.8, 3.08, -2.6)
(-3.8, 3.08,  2.6)   (0, 3.08,  2.6)   (3.8, 3.08,  2.6)
```

Name them `LED_Panel_1` to `LED_Panel_6`.

Add the doorway and windows under the laboratory parent:

| Object | Position | Scale |
|---|---|---|
| `Door_Frame` | `(-4.45, 1.25, -4.36)` | `(1.45, 2.5, 0.12)` |
| `Door_Glass` | `(-4.45, 1.25, -4.25)` | `(1.24, 2.25, 0.05)` |
| `Door_Handle` | `(-3.98, 1.25, -4.14)` | `(0.06, 0.42, 0.06)` |
| `Window_Frame_Left` | `(-3.7, 2.04, 4.37)` | `(2.35, 1.35, 0.08)` |
| `Window_Glass_Left` | `(-3.7, 2.04, 4.30)` | `(2.12, 1.15, 0.035)` |
| `Window_Frame_Right` | `(3.7, 2.04, 4.37)` | `(2.35, 1.35, 0.08)` |
| `Window_Glass_Right` | `(3.7, 2.04, 4.30)` | `(2.12, 1.15, 0.035)` |

The ceiling and front-wall pieces are separate so the desktop overview can hide them for a dollhouse view while first-person and XR keep them visible.

### A8. Add benches, cabinets, sink, PPE, and waste

Create a root Empty named `LAB_FURNITURE_AND_STORAGE`, reset it, and add:

| Object | Position | Scale |
|---|---|---|
| `Back_Bench_Worktop` | `(0, 0.92, 3.55)` | `(10.8, 0.12, 1.35)` |
| `Left_Bench_Worktop` | `(-5.25, 0.92, 0.15)` | `(1.25, 0.12, 5.6)` |

For the back bench, create nine cabinet cubes with Y `0.43`, Z `3.74`, Scale `(1.04, 0.86, 0.88)`, and X positions:

```text
-4.60, -3.45, -2.30, -1.15, 0, 1.15, 2.30, 3.45, 4.60
```

For the left bench, create five cubes with X `-5.45`, Y `0.43`, Scale `(0.8, 0.86, 1.0)`, and Z positions `-2.00, -0.85, 0.30, 1.45, 2.60`.

Create `Handwashing_Sink` at Position `(-4.35, 1.01, 3.50)`, Scale `(1.35, 0.12, 0.95)`. Add these children using **local** positions and scales:

| Child | Local Position | Local Scale |
|---|---|---|
| `Sink_Basin` | `(0, 0.04, 0)` | `(0.9, 0.08, 0.58)` |
| `Faucet_Stem` | `(0, 0.38, 0.28)` | `(0.08, 0.7, 0.08)` |
| `Faucet_Neck` | `(0, 0.7, 0.05)` | `(0.08, 0.08, 0.46)` |

Create `Soap_Dispenser` at Position `(-3.45, 1.34, 4.10)`, Scale `(0.22, 0.50, 0.18)`.

Create `PPE_AND_LAB_COAT_AREA` at `(4.85, 0, -3.45)`. Give it a backboard at local Position `(0, 1.65, 0.38)`, Scale `(1.55, 2.5, 0.1)`, plus three coat shapes at local X `-0.47`, `0`, and `0.47`, local Y `1.45`, local Z `0.19`, Scale `(0.34, 1.55, 0.08)`.

Add `Biohazard_Waste` at `(4.4, 0.42, -1.9)` and `Ordinary_Waste` at `(5.2, 0.42, -1.9)`, each Scale `(0.52, 0.42, 0.52)`.

### A9. Add the qPCR station and background equipment

Create these scene-root Empty parents and reset them:

```text
BACKGROUND_EQUIPMENT_NON_INTERACTIVE
TEAM4_HANDOFF_PLATE_STATION
TEAM5_REAL_TIME_QPCR_INSTRUMENT
CLINICAL_LIGHTING
Team5_qPCR_Systems
COMPACT_DESKTOP_AND_XR_UI
XR_INTERACTION_SYSTEMS
```

Set the instrument parent to Position `(2.80, 0.985, 3.43)`, Rotation `(0,0,0)`, Scale `(1,1,1)`. Its simplified Box Collider uses Centre `(0, 0.20, 0)` and Size `(0.404, 0.40, 0.581)`. Set this parent alone to the **Ignore Raycast** layer, choosing **No, this object only** when Unity asks about children. Its collider still blocks physical movement, but it cannot intercept controller rays intended for the buttons. Buttons and target children stay on Default.

Add `Motorized_Plate_Drawer` at local Position `(0, 0.105, -0.205)`, Rotation `(0,0,0)`, Scale `(1,1,1)`, Collider Size `(0.404, 0.183, 0.415)`. Set this parent alone to Ignore Raycast too. Add `A1_ALIGNED_PLATE_ANCHOR` inside the drawer at local Position `(-.0009, -.0079, .08)`. The drawer opens by moving `0.22 m` toward negative Z.

Under the Team 4 station, create `Prepared_Plate_Home_Anchor` at world Position `(2.24, 0.987, 3.15)`. The plate later becomes its child.

Place the non-interactive background objects:

| Object | Position | Scale or note |
|---|---|---|
| `Refined_Centrifuge` | `(-2.9, 0.2113, 3.5525)` | Use the supplied `Compact_plate_centrifuge.fbx`; rotation Y 180. Its source origin is below its base. |
| `Cold_Block` | `(1.85, 1.001, 3.23)` | `(0.15, 0.04, 0.10)` |
| `Tube_Rack_1` | `(-5.2, 0.992, 0.40)` | `(0.14, 0.025, 0.11)` |
| `Tube_Rack_2` | `(-5.2, 0.992, -0.10)` | `(0.14, 0.025, 0.11)` |
| `Pipette_Stand` | `(-4.95, 1.01, 1.7)` | `(0.12, 0.025, 0.14)` |
| `Computer_Monitor` | `(3.60, 1.30, 3.57)` | `(0.53, 0.33, 0.026)` |
| `Computer_Screen` | `(3.60, 1.30, 3.549)` | `(0.49, 0.28, 0.006)` |
| `Monitor_Stand` | `(3.60, 1.06, 3.57)` | `(0.06, 0.17, 0.06)` |
| `Keyboard` | `(3.60, 1.004, 3.12)` | `(0.44, 0.025, 0.15)` |

Only the Team 5 station should advance the lesson. Background objects provide realism without distracting interactions.

### A10. Import FBX models without destroying scale

In Blender, set units to Metric, model at real size, place the origin where the object should rotate, and use `Ctrl+A → Rotation and Scale` before exporting FBX. The plate footprint is about `127.76 × 85.48 mm`.

In Unity:

1. Put the artist's replacement FBX files in `Assets/Team5/Models/Our-design`. Keep the existing `Redesign` folder for earlier generated assets still used by the plate and pipettes. Folder names organise files; they do not make a model functional.
2. Select an FBX in Project and inspect its **Model** tab.
3. Use a 1 m Cube as a ruler before changing scale randomly.
4. Apply import changes.
5. Drag the FBX into the scene under a clean parent.
6. Correct the child orientation while keeping the functional parent at Scale `(1,1,1)`.
7. Put simple Box/Capsule Colliders on the functional parent.

A **Mesh Renderer** only draws an object. A **Collider** is the invisible physical boundary. For an old cube blockout, you can untick only its Mesh Renderer while keeping its Box Collider enabled; then place the detailed FBX on top. Do not untick the checkbox at the very top of Inspector, because that disables the entire GameObject and its collider.

For the current supplied sink, keep the hidden `Handwashing_Sink` blockout/collider and place `Stainless_washbasin.fbx` at `(-4.35, 1.10, 3.465)`, Rotation `(0,180,0)`, Scale `(1,1,1)`. Do not reuse the old `Clinical_Sink.fbx` import correction on this different FBX.

Create processed prefabs by putting each model under a clean parent, adding materials/colliders/anchors, and dragging the finished parent into `Assets/Team5/Prefabs/Processed`.

#### Exact replacement-model recipe — current supplied files

Select a **scene instance in Hierarchy**, not the FBX file in Project, to type these Transform values. Local means relative to the parent: a parent at `(2.8,.985,3.43)` plus a child at `(0,.1,0)` puts that child at world `(2.8,1.085,3.43)`. Keep all organisational parents at Scale `(1,1,1)`.

| Supplied FBX | Scene placement and correction | Approximate finished size |
|---|---|---|
| `Q-PCR.fbx` | Child named `ClinicalInstrumentMesh` inside the instrument parent. Local Position `(-.03408,-.03097,0)`, Rotation `(0,0,0)`, Scale `(.1,.1,.1)`. | W .404 × H .400 × D .581 m |
| `Lab Drawer.fbx` | Retained as a furniture asset, **not the qPCR tray**. Its old `ClinicalDrawerMesh` instance is inactive. Use the Q-PCR tray assembly below instead. | W .140 × H .048 × D .210 m when rotated Y 270 |
| `Stainless_washbasin.fbx` | World Position `(-4.35,1.10,3.465)`, Rotation `(0,180,0)`, Scale `(1,1,1)`. | W .610 × H .312 × D .655 m, including tap |
| `Compact_plate_centrifuge.fbx` | World Position `(-2.9,.2113,3.5525)`, Rotation `(0,180,0)`, Scale `(1,1,1)`. | W .508 × H .285 × D .399 m |
| `Cabinet.fbx` | Back-bench visual: old cabinet X plus `.8923`, Y `.1717`, Z `3.74775`; Rotation Y `180`; Scale `(1,1,.63)`. | W 1.040 × H .850 × D .599 m |
| `Microtube_Rack.fbx` | Under a unit-scale rack visual parent, local Position `(-.0642,.0105,-.0411)`, Rotation Y `180`, Scale `(1,1,1)`. | W .192 × H .035 × D .120 m |
| `HangingCoat.fbx` | Under the existing PPE parent: local Position `(-.47,1.3,.19)`, `(0,1.3,.19)`, or `(.47,1.3,.19)`; Rotation Y `180`; Scale 1. | W .466 × H .904 × D .073 m |
| `Laboratory_Scientist.fbx` | Replace only `ScientistVisual`, preserving its character parent and controller. Scale 1. | Height about 1.764 m; static mesh, no walk clips supplied |

For side cabinets, preserve each old cabinet's Z but add `.8923`; set world X `-5.45775`, Y `.1717`, Rotation Y `90`, Scale `(1,1,.63)`. The offsets compensate for the FBX origins; they are not extra cabinet size.

Expand `ClinicalInstrumentMesh` in Hierarchy. Keep **`Cube`, `Plane` and `Plane.001` visible**: they form the machine housing. Do not hide the lower panels. The other four parts must also be visible, but under the moving drawer parent rather than fixed to the body:

1. Under `Motorized_Plate_Drawer`, create an Empty named `Artist_Moving_Drawer_Assembly`.
2. Set its local Position `(-.03408,-.13597,.425)`, Rotation `(0,0,0)`, Scale `(.1,.1,.1)`.
3. Duplicate `Plane.002`, `Gasket_Frame`, `Thermal_Block`, and `Wells_96` into that Empty, preserving their original **local** transforms from the FBX. The builder performs this copy for you.
4. Enable these four moving copies. Disable only their static originals beneath `ClinicalInstrumentMesh`, to avoid overlapping duplicates.
5. Leave the functional parent, power light, controller scripts and socket intact. The socket stays under the moving drawer, so the loaded plate travels with it.
6. Verify Power → Open visibly moves the complete tray outward; Close moves it back only after a correctly aligned plate has been seated.

The FBX was authored with its drawer open. The assembly's Z offset places that same geometry in the closed position; the `.22 m` animation returns it to its authored open position. No source FBX meshes were deleted.

`Microtube.fbx` contains **three** tubes, not one. Each `Tube body`, `.001`, `.002` has a matching `Tube cover` suffix. Move each body and its cap by the same amount. In a clean cluster parent, place body centres at X `-.024`, `0`, `.024`, Y `.0175`, Z `0`; do not move only the caps. Duplicate the completed cluster twice per rack at local X `-.045` and `+.045`. `Team5VisualUpgrade.Model()` performs these paired corrections reproducibly without changing the FBX.

Measure rather than guess: compare each finished model against a Cube with Scale `(1,1,1)`. The qPCR machine should be roughly half that cube's width, and the plate about one eighth. A scene Scale of 1 does **not** guarantee a one-metre object; the model's original mesh dimensions and import units also matter.

If following the supplied-source route, copy `Assets/Team5`, `Assets/Editor/ProjectBootstrap`, `ArtSource` (optional editable source), Packages and ProjectSettings into a separate project only after backing it up. Preserve `.meta` files with their matching assets. The reusable builders are code examples for the individual construction steps, not a substitute for understanding local/world transforms. Use **Team 5 → Apply blue-white visual upgrade (preserve scene)** followed by **Apply complete interactive VR upgrade** only on a scene with the expected named roots; do not run a full scene-creation command over your unsaved practice work.

### A11. Create realistic URP materials

Right-click `Assets/Team5/Materials/RealisticLab`, choose `Create → Material`, and use shader `Universal Render Pipeline/Lit`.

| Material | Base colour | Metallic | Smoothness |
|---|---|---:|---:|
| `M_EpoxyFloor` | `#8F9EAC` | 0.05 | 0.72 |
| `M_PaintedWall` | `#D5DEE8` | 0.00 | 0.32 |
| `M_LabWorktop` | `#3B4C5D` | 0.08 | 0.65 |
| `M_LabCabinet` | `#C7D2DE` | 0.15 | 0.52 |
| `M_StainlessSteel` | `#AAB8B9` | 0.86 | 0.78 |
| `M_InstrumentPlastic` | `#34455C` | 0.12 | 0.70 |
| `M_Rubber` | `#151A1B` | 0.00 | 0.16 |
| `M_PlatePolypropylene` | `#CFDADD` | 0.00 | 0.48 |

Use Transparent surface type, low alpha, and high smoothness for glass and the optical seal. Use emission only for status lights, screens, and interaction accents—not the room.

### A12. Add clinical lighting and a restrained finish

Create `CLINICAL_LIGHTING`. Add:

- a Directional Light rotated `(42, -28, 0)`, intensity `0.9`, soft shadows, warm-neutral white;
- six Point Lights at `(-3.5,2.85,-2.3)`, `(0,2.85,-2.3)`, `(3.5,2.85,-2.3)`, `(-3.5,2.85,2.2)`, `(0,2.85,2.2)`, `(3.5,2.85,2.2)`;
- each Point Light range `6.2`, intensity `2.8`, no realtime shadows for the final mobile-friendly pass;
- a Reflection Probe at `(0,1.5,0)` with Box Size `(11.5,3,8.5)`, resolution `128`, intensity `0.75`.

Create a Global Volume. Add Neutral Tonemapping, Color Adjustments (Post Exposure `-0.08`, Contrast `6`, Saturation `-4`), Bloom `0.035`, and Vignette `0.025`. Tick the override checkbox for every value you change.

### A13. Build the scientific objects before adding VR

Create the prepared plate as a correctly sized parent with a tight Box Collider and 96 well children arranged as 8 rows × 12 columns. Store each well’s ID and type in data, not in its colour alone. The finished default is 26 samples, one positive control, one NTC, and 68 unused wells.

Build the instrument hierarchy with separate objects for body, drawer, plate anchor, power indicator, touchscreen, and physical controls. Put behaviour in scripts such as `WorkflowController`, `PlateController`, `InstrumentController`, `ProtocolSetupController`, `AmplificationSimulator`, and `ResultsController`. Do not put all logic in one giant script.

Create the desktop Canvas and compact instruction card before VR. Use white cards, pale blue backgrounds, navy text, medium-blue buttons, rounded corners, and Source Sans 3. Create Guided, First-person, and Third-person desktop cameras, but let only `LabCameraDirector` move them. XR gets its own headset-controlled camera later.

At this checkpoint you should have a complete-looking laboratory, a real-scale qPCR station and plate, desktop UI, and the scientific workflow. The rest of this guide adds the interactive lesson and VR/hand systems.

## Part B — Add the interactive lesson, XR, and tracked hands

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
- XR Hands `1.9.0`

Select XR Interaction Toolkit, open **Samples**, and import:

- Starter Assets
- XR Device Simulator or Interaction Simulator sample
- Hands Interaction Demo

Select XR Hands, open **Samples**, and import **HandVisualizer**.

Do not manually type package versions into `manifest.json` when Package Manager can install them.

## 3. Configure XR Plug-in Management

1. Open `Edit → Project Settings → XR Plug-in Management`.
2. On Windows, enable OpenXR for simulator and desktop headset testing.
3. On Android, enable OpenXR.
4. Open `OpenXR → Interaction Profiles`.
5. Add the Meta Quest Touch Controller profile.
6. For Android, enable **Hand Tracking Subsystem**, **Meta Hand Tracking Aim**, and **Meta Quest Support**.
7. Keep the Oculus Touch Controller Profile enabled as the reliable fallback.

The finished project supports both Touch controllers and actual tracked hands. On a Quest, cameras on the headset track the learner's hands and Unity XR Hands supplies joint poses. In the Editor, the XRI simulator supplies simulated poses. This is medium-complexity—not a one-click replacement—so controllers remain available at all times.

## 4. Build the XR Origin correctly

Create it with `GameObject → XR → XR Origin (VR)` if it is missing. Rename the root:

`XR_ORIGIN_LEFT_RIGHT_CONTROLLERS`

The hierarchy must follow this idea:

```text
XR_ORIGIN_LEFT_RIGHT_CONTROLLERS
├─ Camera Offset
│  ├─ Main Camera
│  ├─ Left Controller
│  ├─ Right Controller
│  └─ XR Origin Hands (XR Rig)
│     ├─ Left Hand
│     └─ Right Hand
├─ Input Action Manager
├─ XR Input Modality Manager
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

Each tracked hand needs a hand mesh, direct/pinch grab interaction, poke interaction for buttons/UI, and near/far interaction for screens. Apply the blue-glove material to the hand mesh.

Add one `XRInputModalityManager`. Assign its left/right hand groups and left/right controller groups. It hides controller visuals while hands are tracked and restores them when controllers are picked up. Switching input does not reset the lesson.

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

Put the mode-selection content inside a `CanvasGroup`. Starting either lesson sets Alpha to `0`, turns **Interactable** off, turns **Blocks Raycasts** off, and deactivates the panel. This prevents an invisible panel from blocking clicks. Reset Lesson intentionally shows it again. The instruction card stays active and always displays `Current target: ...`.

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

Each `LessonStepDefinition` contains the action, exact `TargetLabel`, heading, short instruction, child-friendly reason, and matching narration clip.

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

1. Add a Box Collider to the plate root: Centre `(0, .008, 0)`, Size `(.12776, .018, .08548)`. Keep it enabled. A missing collider can leave the plate visible but impossible to select.
2. Add a Rigidbody: Mass `.04`, Use Gravity on, Is Kinematic on initially, Collision Detection `Continuous Speculative`.
3. Add `XRGrabInteractable`: Movement Type `Instantaneous`, Throw On Detach off, Use Dynamic Attach on. Expand its Colliders list and assign the root Box Collider explicitly. Initially disable this component, not the whole plate.
4. Use both left and right direct/ray interactors as valid interactor groups.
5. Add `LessonPlateGrabGate` so grabbing is enabled only during the loading part.
6. Add a second `XRSimpleInteractable` on the plate collider as the early-attempt blocker.
7. Configure the gate with the real grab, blocker, and plate descriptor used for red visual feedback.
8. Add `PlateDropRecovery`.
9. Store the handoff-station transform as the home pose.

`PlateDropRecovery` checks the Y position. If the plate falls below the floor, it clears velocity and returns the plate to its home pose.

Before Plate Loading, the blocker stays enabled and the real `XRGrabInteractable` stays disabled. An early attempt leaves the plate in place, flashes it red, provides controller haptics when a controller is used, and shows `Complete the current task first: ...` for about four seconds. Before a lesson is chosen, it shows `Choose Guided Training or Assessment Mode first.` A short throttle prevents one squeeze from counting repeatedly. At Plate Loading, the blocker disables and the real grab enables.

There is one important exception: when the current tour instruction is **Select the prepared qPCR plate**, selecting the blocker completes that identification step without moving the plate. It must not reject the very action it asks for. Small seal/bubble inspection colliders are enabled only for their own current step so they cannot intercept this plate selection.

Only `LessonPlateGrabGate` owns enabling/disabling the grab. The older `PreparedPlateInteraction` yields control when the lesson gate exists. Otherwise two scripts can fight over the grab component and release a correctly seated plate when the drawer starts closing. Switch collider ownership by disabling the old interactable before enabling the new one.

Do not use an oversized collider. The physical size should remain near `0.128 × 0.085 m` so controller reach feels believable.

## 11. Create the A1-sensitive socket

Under the motorized drawer:

1. Create an empty object called `A1_ORIENTATION_SENSITIVE_PLATE_SOCKET`.
2. For the supplied Q-PCR model, use local Position `(-.0009,-.0079,.08)` beneath `Motorized_Plate_Drawer`.
3. Use Rotation `(0,0,0)`, Scale `(1,1,1)`. Its Attach Transform is `A1_ALIGNED_PLATE_ANCHOR` at the same local pose.
4. Add a Box Collider, tick **Is Trigger**, and set Size `(.145,.055,.105)`.
5. Add **only `OrientationAwarePlateSocket`**. It already inherits `XRSocketInteractor`; do not add a separate second socket component.
6. Assign the plate, lesson controller, instrument controller and target descriptor; set orientation tolerance to `18` degrees and Keep Selected Target Valid on.
7. Keep the plate's grab enabled while the socket holds it, even after the lesson advances. Block controller re-grabbing while seated; Reset explicitly releases the socket.

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

On both instruction cards, use separate fields for:

- the persistent exact target, such as `Current target: Lab coat`;
- normal instruction/hint text;
- temporary red rejected-action feedback.

Do not let timed hints overwrite an error message. Keep the error field separate and readable for about four seconds.

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
G             Select/grip 3D objects
T             UI trigger/activate
Tab           Cycle controllers/hands
K             Grab with selected hand
M             Pinch with selected hand
N             Poke with selected hand
O             Open selected hand
P             Make a fist
R             Reset simulator pose
```

If F8 opens Windows Project instead of VR, ignore it. F8 is only an optional developer shortcut. The visible launcher is the supported way to enter simulator mode.

In this project's simulator compatibility mode, **hold G to grip and release G to let go**. Release between selecting different tour objects. `T` is for UI/activation; it is not the plate-grab key. Real headset controllers retain the standard XRI press/release behaviour.

Test both simulated controllers, then use `Tab` to switch to hands. Poke a button, pinch/grab the plate, and operate the protocol screen with a hand ray. Switch back to controllers without resetting. Try grabbing the plate early and confirm it stays still while the exact current-task message appears. Intentionally rotate it incorrectly during loading, then correct it. Confirm only the XR Origin camera is active.

## 18. Run automated tests

Open `Window → General → Test Runner`.

Run EditMode tests first. They check data and validation without entering the scene.

Run PlayMode tests second. They load the real scene and verify cameras, controllers, tracked-hand groups, modality references, unique HUD/socket/control counts, non-blocking mode panels, target text, early-grab rejection, socket rules, reset, 96 wells, 28 active reactions, the 35-cycle run, positive-control amplification, and flat NTC.

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
5. Confirm OpenXR, Meta Quest Support, Hand Tracking Subsystem, Meta Hand Tracking Aim, and Oculus Touch Controller Profile are enabled for Android.
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

The builder removes objects marked with `InteractiveVrGeneratedArtifact`, cleans old generated duplicates, and recreates exactly one desktop HUD, one XR HUD, one plate socket, one physical-control set, and one tracked-hand rig. Mark generated proxies parented outside `INTERACTIVE_VR_LESSON` too. This makes repeated builder runs idempotent while preserving the laboratory, scientific systems, and unrelated user work.

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
