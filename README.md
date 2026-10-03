# Team 5 qPCR Unity Project

Unity `6000.3.25f1` URP project for a desktop-first educational qPCR load-and-run simulation.

## Play

Open `Assets/Team5/Scenes/Team5_qPCR_LoadAndRun.unity` and enter Play mode.

- Action: UI button, `Space`, or `Enter`
- Reset: `R`
- Camera: right mouse drag and mouse wheel

## Architecture

- `Assets/Team5/Scripts/Runtime/Data` — experiment, protocol, and result data
- `Assets/Team5/Scripts/Runtime/Workflow` — ordered workflow and stage text
- `Assets/Team5/Scripts/Runtime/Controllers` — plate, instrument, simulation, results, and workflow behavior
- `Assets/Team5/Scripts/Runtime/UI` — mentor panel and amplification graph
- `Assets/Team5/Scripts/Editor` — project setup and build automation
- `Assets/Team5/Tests/EditMode` — deterministic result and workflow-guard tests
- `Assets/Team5/Prefabs/Processed` — normalized wrappers around imported Team 5 FBX models

## Scene rebuild

The editor menu **Team 5 > Build qPCR Experience** regenerates project-created materials, data assets, processed prefabs, and the station scene. It does not modify `C:\TEAM-5\PCR`.

## Full handoff

See `C:\TEAM-5\deliverables\Team5-qPCR\README.md` for builds, demonstration instructions, validation, provenance, and the scientific disclaimer.

