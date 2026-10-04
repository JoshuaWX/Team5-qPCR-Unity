# Asset provenance

## Original supplied resources — redistribution rights unverified

The original plate/instrument/pipette FBX files, pipette textures, reference image and Team 5 documents were previously copied from `C:\TEAM-5\PCR`. That original folder has not been modified by this upgrade. Their author/licence documentation is not available. On 4 October 2026 the repository owner confirmed that redistribution rights are unknown and explicitly requested publishing everything, intending to make the repository private afterwards. Inclusion is not a claim that these assets have an open licence. Do not reuse or redistribute them independently without permission.

These resources also occur in the initial Git commit. Deleting a latest-version file would not remove its earlier public history. The owner must manage repository visibility and obtain any necessary permissions.

## Newly generated art

`ArtSource/build_lab_assets.py` creates the plate, seal-compatible plate geometry, instrument housing, drawer, pipettes, tubes, cabinets, sink, hanging coats, centrifuge and rigged scientist for this project. Editable sources are in `ArtSource/Team5_Clinical_Assets.blend`; exports are in `Assets/Team5/Models/Redesign`. These are original project-generated meshes, not downloaded commercial models. Static meshes are grouped by material and the scientist uses one combined skinned mesh with Idle, Walk and Reach clips. No third-party model or texture was newly downloaded for this redesign.

## Font

Source Sans 3 Regular and Semibold were obtained from the official [Adobe Source Sans repository](https://github.com/adobe-fonts/source-sans), release branch, `TTF` directory. Copyright Adobe; SIL Open Font License 1.1. The complete licence is retained at `Assets/Team5/UI/Fonts/LICENSE.md`. Generated SDF atlases use these fonts without modifying their outlines.

## Unity dependencies

URP, Input System, OpenXR, Meta OpenXR and XR Interaction Toolkit retain their package licences. XRI Starter Assets and XR Interaction Simulator came from Unity Package Manager samples and remain covered by the XRI package licence; the resolved package/version is retained in the manifest and lock file. TextMeshPro default fonts/sprites/shaders retain their included notices. Unity editor/player redistribution and package use remain governed by Unity's terms; this repository does not relicense them.

Scientific PDFs are linked as references, not newly copied into the project. No endorsement by Bio-Rad, SLAS or Adobe is implied.
