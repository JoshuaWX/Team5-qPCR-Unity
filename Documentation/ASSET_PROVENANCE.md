# Asset provenance

## Original supplied resources — redistribution rights unverified

The original plate/instrument/pipette FBX files, pipette textures, reference image and Team 5 documents were previously copied from `C:\TEAM-5\PCR`. That original folder has not been modified by this upgrade. Their author/licence documentation is not available. On 4 October 2026 the repository owner confirmed that redistribution rights are unknown and explicitly requested publishing everything, intending to make the repository private afterwards. Inclusion is not a claim that these assets have an open licence. Do not reuse or redistribute them independently without permission.

These resources also occur in the initial Git commit. Deleting a latest-version file would not remove its earlier public history. The owner must manage repository visibility and obtain any necessary permissions.

## Newly generated art

`ArtSource/build_lab_assets.py` creates the plate, seal-compatible plate geometry, instrument housing, drawer, pipettes, tubes, cabinets, sink, hanging coats, centrifuge and rigged scientist for this project. Editable sources are in `ArtSource/Team5_Clinical_Assets.blend`; exports are in `Assets/Team5/Models/Redesign`. These are original project-generated meshes, not downloaded commercial models. Static meshes are grouped by material and the scientist uses one combined skinned mesh with Idle, Walk and Reach clips. No third-party model or texture was newly downloaded for this redesign.

## Team-authored replacement models

On 7 October 2026, the user supplied ten FBX files in `Assets/Team5/Models/Our-design`. Eight are visible in the scene: `Q-PCR.fbx`, `Microtube_Rack.fbx`, `Microtube.fbx`, `Cabinet.fbx`, `Compact_plate_centrifuge.fbx`, `HangingCoat.fbx`, `Stainless_washbasin.fbx`, and `Laboratory_Scientist.fbx`. `RT PCR R4.fbx` is retained as an unused alternate; `Lab Drawer.fbx` is furniture and its temporary qPCR-tray instance is now inactive. These are supplied as the team's designs; no separate licence paperwork was provided. Functional Unity parents, simplified colliders, workflow scripts, physical controls, and the A1-sensitive socket remain separate from the visual meshes.

The selected Q-PCR body is approximately 404 mm wide × 400 mm high × 581 mm deep after a 0.1 scene-child scale correction. All housing panels remain visible. Copies of the source tray, gasket, thermal block and 96-well insert form a moving assembly driven by Unity; only their overlapping static originals are hidden. The microtube file contains three separately positioned body/cap pairs; scene overrides align them into rack slots. The supplied scientist is a static mesh without animation clips: it follows the desktop character, but it is not a newly rigged/animated character. Earlier generated plate and pipette meshes remain in use. The original generated-asset count describes development history, not the current scene's asset origins.

## Font

Source Sans 3 Regular and Semibold were obtained from the official [Adobe Source Sans repository](https://github.com/adobe-fonts/source-sans), release branch, `TTF` directory. Copyright Adobe; SIL Open Font License 1.1. The complete licence is retained at `Assets/Team5/UI/Fonts/LICENSE.md`. Generated SDF atlases use these fonts without modifying their outlines.

## Unity dependencies

URP, Input System, OpenXR, Meta OpenXR and XR Interaction Toolkit retain their package licences. XRI Starter Assets and XR Interaction Simulator came from Unity Package Manager samples and remain covered by the XRI package licence; the resolved package/version is retained in the manifest and lock file. TextMeshPro default fonts/sprites/shaders retain their included notices. Unity editor/player redistribution and package use remain governed by Unity's terms; this repository does not relicense them.

Scientific PDFs are linked as references, not newly copied into the project. No endorsement by Bio-Rad, SLAS or Adobe is implied.
