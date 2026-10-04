using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Team5.qPCR.PlayModeTests
{
    public sealed class RealisticLabSceneTests
    {
        [UnityTest]
        public IEnumerator SceneStartsAtIntroductionWithPreparedTeamFourHandoff()
        {
            yield return SceneManager.LoadSceneAsync("Team5_qPCR_RealisticLab", LoadSceneMode.Single);
            yield return null;

            var workflow = Object.FindFirstObjectByType<WorkflowController>();
            var plate = Object.FindFirstObjectByType<PlateController>();
            var shell = Object.FindFirstObjectByType<LabShellController>();

            Assert.IsNotNull(workflow);
            Assert.IsNotNull(plate);
            Assert.IsNotNull(shell);
            Assert.AreEqual(WorkflowStage.Introduction, workflow.CurrentStage);
            Assert.IsTrue(plate.IsPrepared);
            Assert.AreEqual(96, plate.WellCount);
            Assert.AreEqual(28, plate.ActiveReactionCount);
            Assert.AreEqual(InteractionMode.Desktop, shell.CurrentMode);
        }

        [UnityTest]
        public IEnumerator DesktopOverviewHidesOnlyRemovableRoofElements()
        {
            yield return SceneManager.LoadSceneAsync("Team5_qPCR_RealisticLab", LoadSceneMode.Single);
            yield return null;

            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(item => item.scene.IsValid())
                .ToArray();
            var ceiling = allObjects.Single(item => item.name == "REMOVABLE_CEILING");
            var front = allObjects.Single(item => item.name == "REMOVABLE_UPPER_FRONT_WALL");
            var floor = allObjects.Single(item => item.name == "Epoxy_Floor");

            Assert.IsFalse(ceiling.activeInHierarchy);
            Assert.IsFalse(front.activeInHierarchy);
            Assert.IsTrue(floor.activeInHierarchy);
        }

        [UnityTest]
        public IEnumerator XrSimulatorModeEnablesRoofWorldUiAndBothControllerInteractorSets()
        {
            yield return SceneManager.LoadSceneAsync("Team5_qPCR_RealisticLab", LoadSceneMode.Single);
            yield return null;

            var shell = Object.FindFirstObjectByType<LabShellController>();
            shell.SetMode(InteractionMode.XR);
            yield return null;

            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(item => item.scene.IsValid())
                .ToArray();
            Assert.IsTrue(allObjects.Single(item => item.name == "REMOVABLE_CEILING").activeInHierarchy);
            Assert.IsTrue(allObjects.Single(item => item.name == "XR_WORLD_SPACE_LEARNING_UI").activeInHierarchy);
            Assert.IsTrue(allObjects.Single(item => item.name == "XR_ORIGIN_LEFT_RIGHT_CONTROLLERS").activeInHierarchy);

            var xrComponents = allObjects
                .SelectMany(item => item.GetComponents<Component>())
                .Where(component => component != null)
                .Select(component => component.GetType().Name)
                .ToArray();
            Assert.GreaterOrEqual(xrComponents.Count(name => name.Contains("NearFarInteractor")), 2,
                "The modern XRI Near-Far interactors provide both controller UI rays and direct grabbing.");
            Assert.That(xrComponents, Has.Some.EqualTo("XRGrabInteractable"));

            shell.SetMode(InteractionMode.Desktop);
            yield return null;
            Assert.IsFalse(allObjects.Single(item => item.name == "XR_WORLD_SPACE_LEARNING_UI").activeInHierarchy);
        }

        [UnityTest]
        public IEnumerator ResetRestoresProtocolPlateInstrumentAndResults()
        {
            yield return SceneManager.LoadSceneAsync("Team5_qPCR_RealisticLab", LoadSceneMode.Single);
            yield return null;

            var workflow = Object.FindFirstObjectByType<WorkflowController>();
            var instrument = Object.FindFirstObjectByType<InstrumentController>();
            var plate = Object.FindFirstObjectByType<PlateController>();
            var setup = Object.FindFirstObjectByType<ProtocolSetupController>();
            var results = Object.FindFirstObjectByType<ResultsController>();

            instrument.PowerOn();
            plate.AlignA1();
            Assert.IsTrue(setup.ValidateAndReport());
            workflow.ResetExperience();
            yield return null;

            Assert.AreEqual(WorkflowStage.Introduction, workflow.CurrentStage);
            Assert.IsFalse(instrument.IsPowered);
            Assert.IsFalse(plate.IsInspected);
            Assert.IsFalse(plate.IsA1Aligned);
            Assert.IsFalse(setup.IsValid);
            Assert.IsNull(results.CurrentResults);
        }

        [UnityTest]
        [Timeout(50000)]
        public IEnumerator CompleteLessonRunsAllThirtyFiveCyclesAndRequiresCorrectInterpretation()
        {
            yield return SceneManager.LoadSceneAsync("Team5_qPCR_RealisticLab", LoadSceneMode.Single);
            yield return null;

            var workflow = Object.FindFirstObjectByType<WorkflowController>();
            var simulation = Object.FindFirstObjectByType<RunSimulationController>();
            var results = Object.FindFirstObjectByType<ResultsController>();
            var highestCycle = 0;
            simulation.ProgressChanged += (cycle, _, _) => highestCycle = Mathf.Max(highestCycle, cycle);

            workflow.HandlePrimaryAction(); // Introduction -> Handoff Review
            workflow.HandlePrimaryAction(); // Accept Team 4 handoff
            workflow.HandlePrimaryAction(); // Power on
            workflow.HandlePrimaryAction(); // Validate protocol
            Assert.AreEqual(WorkflowStage.PlateInspection, workflow.CurrentStage);

            workflow.HandlePrimaryAction(); // Deliberately reject wrong A1 orientation
            Assert.AreEqual(WorkflowStage.PlateInspection, workflow.CurrentStage);
            workflow.HandleSecondaryAction(); // Align A1
            workflow.HandlePrimaryAction(); // Inspect
            workflow.HandlePrimaryAction(); // Load instrument
            yield return new WaitUntil(() => workflow.CurrentStage == WorkflowStage.RunValidation);

            workflow.HandlePrimaryAction();
            Assert.AreEqual(WorkflowStage.Amplification, workflow.CurrentStage);
            yield return new WaitUntil(() => workflow.CurrentStage == WorkflowStage.ResultsInterpretation);

            Assert.AreEqual(35, highestCycle);
            Assert.AreEqual(28, results.CurrentResults.Count);
            Assert.IsTrue(results.CurrentResults.Any(item =>
                item.WellType == WellType.PositiveControl && item.IsAmplified));
            Assert.IsTrue(results.CurrentResults.Any(item =>
                item.WellType == WellType.NoTemplateControl && !item.IsAmplified));

            workflow.HandleSecondaryAction(); // Incorrect interpretation must not advance.
            Assert.AreEqual(WorkflowStage.ResultsInterpretation, workflow.CurrentStage);
            workflow.HandlePrimaryAction();
            Assert.AreEqual(WorkflowStage.Complete, workflow.CurrentStage);
        }
    }
}
