# Testing and build guide

Unity version: 6000.3.25f1. Use the saved realistic-lab scene. The project uses URP 17.3.0 and retains the resolved package lock.

## Automated checks

In Unity: **Window → General → Test Runner**, run Team5 EditMode and PlayMode tests. Tests cover scientific configuration, all 96 well identities, complete reaction composition, NTC exclusion of DNA, workflow gates, 35 cycles, interpretation, reset, camera modes, roof visibility, character wall collision and input focus. See the delivery folder's current validation report for actual results and known gaps.

With the Editor closed and Unity CLI installed:

```powershell
unity test . --mode EditMode --filter Team5 --output TestResults/editmode.xml
unity test . --mode PlayMode --filter Team5 --output TestResults/playmode.xml
unity build . --target StandaloneWindows64 --execute-method Team5.qPCR.Editor.Team5Build.BuildWindows --output-path ../deliverables/Team5-qPCR/Builds/Windows/Team5-qPCR.exe --allow-dirty-build
unity build . --target Android --execute-method Team5.qPCR.Editor.Team5Build.BuildAndroid --output-path ../deliverables/Team5-qPCR/Builds/Android/Team5-qPCR-Redesign.apk --allow-dirty-build
```

The build helpers constrain cleanup to the delivery Builds folder. Back up an older build before replacing it. Android requires Unity's Android Build Support, SDK/NDK and OpenJDK modules. The final verified run produced **27/27 EditMode** and **15/15 PlayMode** passes. The APK is an OpenXR/Quest target; physical-headset validation is pending. No store upload or device installation is performed automatically.

## Built Windows walkthrough

The opt-in smoke runner operates the normal workflow, checks errors and saves eight screenshots plus a JSON report. It never runs during ordinary use. Run with graphics enabled (not `-nographics`):

```powershell
& ../deliverables/Team5-qPCR/Builds/Windows/Team5-qPCR.exe -screen-fullscreen 0 -screen-width 1920 -screen-height 1080 --team5-smoke-output C:/TEAM-5/deliverables/Team5-qPCR/TestResults/Windows-1080
```

Repeat with 1280 × 720 and a different output folder. The final 1920 × 1080 and 1280 × 720 walkthroughs both passed with zero runtime errors. The smoke runner exits when complete. Its mean frame time includes screenshot work and is not a representative GPU benchmark.

For XR in the Editor press F8. Use the imported XR Interaction Simulator's on-screen bindings to move the simulated headset and each controller. Confirm both controller rays operate the same UI actions, direct grabbing reaches the plate, and the headset is never moved by Guided camera transitions. Re-test reach and comfort on a physical Quest. Optical hand tracking is not implemented.
