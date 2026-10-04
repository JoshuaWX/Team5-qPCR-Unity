using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;

namespace Team5.qPCR
{
    /// <summary>Opt-in built-player verification; never runs during an ordinary lesson.</summary>
    public sealed class DesktopSmokeWalkthrough : MonoBehaviour
    {
        [Serializable] private sealed class Report
        {
            public string platform, resolution, verdict;
            public int frames, activeRenderers, skinnedRenderers, errors;
            public long allocatedMemoryBytes;
            public float meanFrameMilliseconds;
            public string[] checks;
        }
        private readonly List<string> checks = new List<string>();
        private string output;
        private int errors, frames;
        private double frameSeconds;
        private float started;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (Application.isEditor) return;
            var args = Environment.GetCommandLineArgs();
            var index = Array.IndexOf(args, "--team5-smoke-output");
            if (index < 0 || index + 1 >= args.Length) return;
            var runner = new GameObject("OptIn_Smoke_Walkthrough").AddComponent<DesktopSmokeWalkthrough>();
            runner.output = Path.GetFullPath(args[index + 1]);
        }
        private void OnEnable() { Application.logMessageReceived += Log; started = Time.realtimeSinceStartup; }
        private void OnDisable() => Application.logMessageReceived -= Log;
        private void Log(string message, string trace, LogType type)
        { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors++; }
        private void Update()
        {
            frames++; frameSeconds += Time.unscaledDeltaTime;
            if (Time.realtimeSinceStartup - started > 150) Finish("Timed out");
        }
        private void Check(bool condition, string description)
        { checks.Add((condition ? "PASS: " : "FAIL: ") + description); if (!condition) errors++; }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForSecondsRealtime(1.5f);
            CaptureRenderedFrame(Path.Combine(output, name + ".png"));
            yield return null;
            yield return null;
        }
        public static void CaptureRenderedFrame(string path)
        {
            // Hidden Windows sessions do not present a backbuffer. Explicit rendering still
            // exercises the built shaders, geometry, canvas layout and text atlases.
            var camera=Camera.main;
            var canvas=GameObject.Find("COMPACT_DESKTOP_AND_XR_UI").GetComponent<Canvas>();
            var oldMode=canvas.renderMode; var oldCamera=canvas.worldCamera; var oldDistance=canvas.planeDistance;
            var oldTarget=camera.targetTexture; var oldActive=RenderTexture.active;
            var target=new RenderTexture(Screen.width,Screen.height,24,RenderTextureFormat.ARGB32);
            var texture=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture=target; canvas.renderMode=RenderMode.ScreenSpaceCamera;
                canvas.worldCamera=camera; canvas.planeDistance=.08f;
                Canvas.ForceUpdateCanvases();
                foreach(var text in canvas.GetComponentsInChildren<TMP_Text>())text.ForceMeshUpdate();
                Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;
                texture.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);texture.Apply();
                File.WriteAllBytes(path,texture.EncodeToPNG());
            }
            finally
            {
                canvas.renderMode=oldMode;canvas.worldCamera=oldCamera;canvas.planeDistance=oldDistance;
                camera.targetTexture=oldTarget;RenderTexture.active=oldActive;
                Destroy(target);Destroy(texture);
            }
        }
        private IEnumerator Start()
        {
            Directory.CreateDirectory(output);
            yield return null;
            var flow = FindFirstObjectByType<WorkflowController>();
            var director = FindFirstObjectByType<LabCameraDirector>();
            var plate = FindFirstObjectByType<PlateController>();
            var results = FindFirstObjectByType<ResultsController>();
            var mentor = FindObjectsByType<MentorPanelController>(FindObjectsSortMode.None).First(x => x.name == "Reusable_Mentor_Dialogue");
            Check(flow != null && director != null, "Correct laboratory scene loaded");
            Check(plate.WellCount == 96 && plate.ActiveReactionCount == 28, "96 wells, 28 complete active reactions");
            yield return Shot("01_Guided_Overview");
            director.FirstPerson(); yield return Shot("02_First_Person");
            director.ThirdPerson(); yield return Shot("03_Third_Person");
            director.Guided();
            flow.HandlePrimaryAction(); yield return Shot("04_Prepared_Plate");
            flow.HandlePrimaryAction(); flow.HandlePrimaryAction();
            Check(flow.CurrentStage == WorkflowStage.ProtocolSetup, "Configure chapter unlocked in order");
            var field = GameObject.Find("Protocol_Field_00").GetComponent<TMP_InputField>();
            field.text = "19"; flow.HandlePrimaryAction();
            Check(flow.CurrentStage == WorkflowStage.ProtocolSetup, "Incorrect reaction volume blocks loading");
            field.text = "20";
            yield return Shot("05_Protocol");
            flow.HandlePrimaryAction(); flow.HandlePrimaryAction();
            Check(flow.CurrentStage == WorkflowStage.PlateInspection, "Wrong A1 orientation rejected");
            flow.HandleSecondaryAction(); flow.HandlePrimaryAction(); flow.HandlePrimaryAction();
            yield return new WaitUntil(() => flow.CurrentStage == WorkflowStage.RunValidation);
            yield return Shot("06_Loaded_Instrument");
            flow.HandlePrimaryAction();
            yield return new WaitUntil(() => flow.CurrentStage == WorkflowStage.ResultsInterpretation);
            Check(results.CurrentResults.Count == 28, "35-cycle run returns 28 well results");
            Check(results.CurrentResults.Any(x => x.WellType == WellType.PositiveControl && x.IsAmplified) &&
                results.CurrentResults.Any(x => x.WellType == WellType.NoTemplateControl && !x.IsAmplified), "Control behavior correct");
            yield return Shot("07_Results");
            flow.HandleSecondaryAction();
            Check(flow.CurrentStage == WorkflowStage.ResultsInterpretation, "Incorrect interpretation cannot finish");
            flow.HandlePrimaryAction(); Check(flow.CurrentStage == WorkflowStage.Complete, "Lesson completes");
            flow.ResetExperience(); mentor.Minimize(); flow.ResetExperience();
            yield return null;
            Check(director.Mode == CameraMode.Guided && !mentor.IsMinimized && results.CurrentResults == null, "Reset restores camera, dialogue and results");
            var hud = FindFirstObjectByType<LabHudController>(); hud.OpenMix(); yield return Shot("08_Reaction_Mix");
            hud.CloseMix(); Finish(errors == 0 ? "Passed" : "Failed");
        }
        private void Finish(string verdict)
        {
            var renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            var report = new Report { platform = Application.platform.ToString(), resolution = Screen.width + "x" + Screen.height,
                verdict = verdict, frames = frames, activeRenderers = renderers.Count(x => x.enabled),
                skinnedRenderers = renderers.OfType<SkinnedMeshRenderer>().Count(), errors = errors,
                allocatedMemoryBytes = Profiler.GetTotalAllocatedMemoryLong(), meanFrameMilliseconds = (float)(frameSeconds * 1000 / Math.Max(1, frames)),
                checks = checks.ToArray() };
            File.WriteAllText(Path.Combine(output,"smoke-report.json"), JsonUtility.ToJson(report,true));
            Debug.Log("TEAM5_SMOKE_" + verdict.ToUpperInvariant());
            enabled = false; Application.Quit(verdict == "Passed" ? 0 : 1);
        }
    }
}
