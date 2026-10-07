# Testing and build guide

Unity version: 6000.3.25f1. Use the saved realistic-lab scene. The project uses URP 17.3.0 and retains the resolved package lock.

## Automated checks

On 7 October 2026 the artist-model revision passed **30 EditMode and 25 PlayMode tests**. The latter includes left/right virtual-controller input selecting the tour plate and gripping the movable plate, plus socket insertion and retention through drawer closing. Raw reports are in the local deliverable `TestResults/Artist-EditMode-Final.json` and `Artist-PlayMode-Final.json`. This is not a full manual Quest or optical-hand validation.

In Unity: **Window → General → Test Runner**, run Team5 EditMode and PlayMode tests. Tests cover scientific configuration, all 96 well identities, complete reaction composition, NTC exclusion of DNA, workflow gates, 35 cycles, interpretation, reset, camera modes, roof visibility, character wall collision and input focus. See the delivery folder's current validation report for actual results and known gaps.

With the Editor closed and Unity CLI installed:

```powershell
unity test . --mode EditMode --filter Team5 --output TestResults/editmode.xml
unity test . --mode PlayMode --filter Team5 --output TestResults/playmode.xml
unity build . --target StandaloneWindows64 --execute-method Team5.qPCR.Editor.Team5Build.BuildWindows --output-path ../deliverables/Team5-qPCR/Builds/Windows/Team5-qPCR.exe --allow-dirty-build
unity build . --target Android --execute-method Team5.qPCR.Editor.Team5Build.BuildAndroid --output-path ../deliverables/Team5-qPCR/Builds/Android/Team5-qPCR.apk --allow-dirty-build
```

The build helpers constrain cleanup to the delivery Builds folder. Back up an older build before replacing it. Android requires Unity's Android Build Support, SDK/NDK and OpenJDK modules. The final hand-tracking upgrade is checked by **30 EditMode tests** and an expanded PlayMode suite. It covers both lesson modes, non-blocking mode panels, exact target text, rejected-action feedback, idempotent HUD/socket/control counts, the early plate-grab gate, narration, physical controls, four inspection targets, controllers, tracked-hand groups, modality references, XR Origin camera exclusivity, A1-sensitive loading, reset and all 35 cycles. Record the final pass count in the delivery validation report after the last full run. The APK is an OpenXR/Quest target; physical-headset validation is pending. No store upload or device installation is performed automatically.

## Built Windows walkthrough

The opt-in smoke runner operates the normal workflow, checks errors and saves eight screenshots plus a JSON report. It never runs during ordinary use. Run with graphics enabled (not `-nographics`):

```powershell
& ../deliverables/Team5-qPCR/Builds/Windows/Team5-qPCR.exe -screen-fullscreen 0 -screen-width 1920 -screen-height 1080 --team5-smoke-output C:/TEAM-5/deliverables/Team5-qPCR/TestResults/Windows-1080
```

Those 1920 × 1080 and 1280 × 720 smoke passes belong to the earlier desktop workflow. The runner predates the physical narrated lesson and is not evidence that the current controller lesson passes end-to-end. Use the current delivery validation report and manual checklist for this revision. Its mean frame time includes screenshot work and is not a representative GPU benchmark.

For XR, enter Play mode and select **XR Simulator Preview** from the visible launcher. F8 remains an optional developer shortcut only. Use the in-app guide to move the simulated headset, controllers and hands. `G` selects/grips 3D interactables; `T` is the UI/activation trigger. Confirm controller rays and hand rays operate the same UI, poke presses controls, pinch/grab reaches the plate, early grabbing and wrong A1 rotation are rejected, and modality can change without resetting. Re-test reach, comfort, offsets, tracking, text size and performance on a physical Quest. OpenXR hand tracking is configured; scene-structure tests alone do not verify full pinch/poke interaction or physical tracking reliability.
