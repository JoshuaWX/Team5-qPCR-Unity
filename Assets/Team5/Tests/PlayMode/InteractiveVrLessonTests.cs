using System.Collections;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

namespace Team5.qPCR.PlayModeTests
{
    public sealed class InteractiveVrLessonTests
    {
        [UnitySetUp]
        public IEnumerator LoadLab()
        {
            yield return SceneManager.LoadSceneAsync("Team5_qPCR_RealisticLab", LoadSceneMode.Single);
            yield return null;
        }

        [Test]
        public void SceneContainsCompletePhysicalLessonArchitecture()
        {
            Assert.IsNotNull(Object.FindFirstObjectByType<GuidedLessonController>(FindObjectsInactive.Include));
            Assert.IsNotNull(Object.FindFirstObjectByType<NarrationController>(FindObjectsInactive.Include));
            Assert.IsNotNull(Object.FindFirstObjectByType<GuidanceCueController>(FindObjectsInactive.Include));
            Assert.IsNotNull(Object.FindFirstObjectByType<PresentationLauncherController>(FindObjectsInactive.Include));
            Assert.IsNotNull(Object.FindFirstObjectByType<OrientationAwarePlateSocket>(FindObjectsInactive.Include));
            Assert.GreaterOrEqual(Object.FindObjectsByType<PhysicalControlInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length, 4);
            Assert.GreaterOrEqual(Object.FindObjectsByType<LabObjectDescriptor>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length, 22);

            var sceneObjects = Resources.FindObjectsOfTypeAll<GameObject>().Where(item => item.scene.IsValid()).ToArray();
            Assert.AreEqual(1, sceneObjects.Count(item => item.name == "DESKTOP_INTERACTIVE_LESSON_UI"));
            Assert.AreEqual(1, sceneObjects.Count(item => item.name == "XR_INTERACTIVE_LESSON_UI"));
            Assert.AreEqual(1, sceneObjects.Count(item => item.name == "A1_ORIENTATION_SENSITIVE_PLATE_SOCKET"));
            Assert.AreEqual(1, sceneObjects.Count(item => item.name == "PHYSICAL_MACHINE_CONTROLS"));
            Assert.AreEqual(2, sceneObjects.Count(item => item.name == "Validate_Protocol"));
            Assert.AreEqual(2, sceneObjects.Count(item => item.name == "Controls_Pass"));
            Assert.AreEqual(2, sceneObjects.Count(item => item.name == "Controls_Fail"));

            var instrumentVisual = sceneObjects.SingleOrDefault(item => item.name == "ClinicalInstrumentMesh");
            Assert.IsNotNull(instrumentVisual, "The team's Q-PCR FBX must replace the generated instrument mesh.");
            Assert.That(instrumentVisual.transform.localScale.x, Is.EqualTo(.1f).Within(.0001f));
            Assert.IsNotNull(instrumentVisual.GetComponentInParent<InstrumentController>());
            Assert.That(instrumentVisual.GetComponentsInChildren<Transform>(true),
                Has.Some.Matches<Transform>(item => item.name == "Cube"));
            Assert.IsFalse(instrumentVisual.GetComponentsInChildren<Transform>(true)
                .Single(item => item.name == "Thermal_Block").gameObject.activeSelf,
                "The static FBX thermal block must stay hidden so the functional drawer remains authoritative.");
            var artistDrawer = sceneObjects.SingleOrDefault(item => item.name == "ClinicalDrawerMesh");
            Assert.IsNotNull(artistDrawer);
            Assert.That(artistDrawer.GetComponentsInChildren<Transform>(true),
                Has.Some.Matches<Transform>(item => item.name == "Cabinet Frame"));
            Assert.AreEqual(3, sceneObjects.Count(item => item.name == "ArtistRackMesh"));
            Assert.AreEqual(12, sceneObjects.Count(item => item.name.StartsWith("Refined_Base_Cabinet_") ||
                                                              item.name.StartsWith("Refined_Side_Cabinet_")));
            Assert.That(sceneObjects.Single(item => item.name == "Refined_HandwashingSink")
                .GetComponentsInChildren<Transform>(true), Has.Some.Matches<Transform>(item => item.name == "Cylinder"));
            Assert.That(sceneObjects.Single(item => item.name == "Refined_Centrifuge")
                .GetComponentsInChildren<Transform>(true), Has.Some.Matches<Transform>(item => item.name == "Plane"));
            Assert.AreEqual(3, sceneObjects.Count(item => item.name.StartsWith("Refined_Coat_")));
            Assert.That(sceneObjects.Single(item => item.name == "ScientistVisual")
                .GetComponentsInChildren<Transform>(true), Has.Some.Matches<Transform>(item => item.name == "Sphere.002"));
        }

        [Test]
        public void GuidedAndAssessmentSessionsTrackHelpDifferentlyFromAutomaticGuidance()
        {
            var lesson = Object.FindFirstObjectByType<GuidedLessonController>(FindObjectsInactive.Include);
            lesson.StartGuidedTraining();
            Assert.AreEqual(LessonMode.GuidedTraining, lesson.Mode);
            Assert.AreEqual(TrainingAction.TourLabCoat, lesson.CurrentAction);
            Assert.AreEqual(0, lesson.Report.HintsUsed);

            lesson.StartAssessment();
            lesson.RequestHelp();
            Assert.AreEqual(LessonMode.Assessment, lesson.Mode);
            Assert.AreEqual(1, lesson.Report.HintsUsed);
        }

        [UnityTest]
        public IEnumerator StartingEitherModeHidesEverySelectionPanelAndKeepsTheExactTarget()
        {
            var lesson = Object.FindFirstObjectByType<GuidedLessonController>(FindObjectsInactive.Include);
            lesson.StartGuidedTraining();
            yield return null;

            Assert.AreEqual("Lab coat", lesson.CurrentTargetLabel);
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>().Where(item => item.scene.IsValid()).ToArray();
            var modePanels = allObjects.Where(item => item.name == "Lesson_Mode_Selection").ToArray();
            Assert.AreEqual(2, modePanels.Length);
            foreach (var panel in modePanels)
            {
                Assert.IsFalse(panel.activeSelf);
                var group = panel.GetComponent<CanvasGroup>();
                Assert.IsNotNull(group);
                Assert.IsFalse(group.interactable);
                Assert.IsFalse(group.blocksRaycasts);
            }

            var cards = allObjects.Where(item => item.name == "Lesson_Instruction_Card").ToArray();
            Assert.AreEqual(2, cards.Length);
            Assert.IsTrue(cards.All(card => card.activeSelf));
            var targetLabels = Resources.FindObjectsOfTypeAll<TMP_Text>()
                .Where(label => label.gameObject.scene.IsValid() && label.name == "Current_Target").ToArray();
            Assert.AreEqual(2, targetLabels.Length);
            Assert.IsTrue(targetLabels.All(label => label.text == "Current target: Lab coat"));

            lesson.ResetLesson();
            lesson.StartAssessment();
            yield return null;
            Assert.IsTrue(modePanels.All(panel => !panel.activeSelf && !panel.GetComponent<CanvasGroup>().blocksRaycasts));
            Assert.AreEqual("Lab coat", lesson.CurrentTargetLabel);
        }

        [UnityTest]
        public IEnumerator EarlyPlateGrabIsBlockedAndReportsReadableFeedbackOnce()
        {
            var lesson = Object.FindFirstObjectByType<GuidedLessonController>(FindObjectsInactive.Include);
            var gate = Object.FindFirstObjectByType<LessonPlateGrabGate>(FindObjectsInactive.Include);
            var startPosition = gate.transform.position;
            lesson.StartGuidedTraining();
            yield return null;

            Assert.IsFalse(gate.IsGrabEnabled);
            Assert.IsTrue(gate.EarlyAttemptBlocker.enabled);
            gate.ReportEarlyAttempt();
            gate.ReportEarlyAttempt();
            yield return null;

            Assert.AreEqual(startPosition, gate.transform.position);
            Assert.AreEqual(1, lesson.Report.Mistakes);
            var feedback = Resources.FindObjectsOfTypeAll<TMP_Text>()
                .Where(label => label.gameObject.scene.IsValid() && label.name == "Action_Feedback").ToArray();
            Assert.AreEqual(2, feedback.Length);
            Assert.IsTrue(feedback.All(label =>
                label.text == "Complete the current task first: lab coat."));
        }

        [UnityTest]
        public IEnumerator PlateBlockerSelectsPreparedPlateDuringTheHandoffTourInsteadOfRejectingIt()
        {
            var lesson = Object.FindFirstObjectByType<GuidedLessonController>(FindObjectsInactive.Include);
            var gate = Object.FindFirstObjectByType<LessonPlateGrabGate>(FindObjectsInactive.Include);
            lesson.StartGuidedTraining();
            Assert.IsTrue(lesson.TryPerformAction(TrainingAction.TourLabCoat, null));
            Assert.IsTrue(lesson.TryPerformAction(TrainingAction.TourGloves, null));
            Assert.IsTrue(lesson.TryPerformAction(TrainingAction.TourSink, null));
            Assert.AreEqual(TrainingAction.TourPreparedPlate, lesson.CurrentAction);
            var mistakesBefore = lesson.Report.Mistakes;

            gate.ReportEarlyAttempt();
            yield return null;

            Assert.AreEqual(TrainingAction.TourOpticalSeal, lesson.CurrentAction,
                "Selecting the prepared plate must advance the handoff tour even though physical grabbing is still locked.");
            Assert.AreEqual(mistakesBefore, lesson.Report.Mistakes);
            Assert.IsFalse(gate.IsGrabEnabled, "The plate must remain physically locked until the later SeatPlate step.");
        }

        [UnityTest]
        public IEnumerator PreviewModeLabelsAndStatusFollowTheShellEvent()
        {
            var shell = Object.FindFirstObjectByType<LabShellController>(FindObjectsInactive.Include);
            var launcher = Object.FindFirstObjectByType<PresentationLauncherController>(FindObjectsInactive.Include);

            shell.SetMode(InteractionMode.XR);
            yield return null;
            Assert.That(launcher.CurrentStatusText, Does.Contain("XR Origin camera"));
            var xrLabels = ButtonLabels("XR_Simulator_Preview");
            var desktopLabels = ButtonLabels("Desktop_Preview");
            Assert.IsTrue(xrLabels.All(label => label.text == "XR Simulator Preview · Active"));
            Assert.IsTrue(desktopLabels.All(label => label.text == "Desktop Preview"));

            shell.SetMode(InteractionMode.Desktop);
            yield return null;
            Assert.That(launcher.CurrentStatusText, Does.Contain("walkthrough camera"));
            Assert.IsTrue(ButtonLabels("Desktop_Preview").All(label => label.text == "Desktop Preview · Active"));
            Assert.IsTrue(ButtonLabels("XR_Simulator_Preview").All(label => label.text == "XR Simulator Preview"));
        }

        [UnityTest]
        public IEnumerator XrPreviewUsesOnlyTheXrOriginCamera()
        {
            var shell = Object.FindFirstObjectByType<LabShellController>(FindObjectsInactive.Include);
            shell.SetMode(InteractionMode.XR);
            yield return null;

            var activeCameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)
                .Where(camera => camera.isActiveAndEnabled).ToArray();
            Assert.AreEqual(1, activeCameras.Length);
            Assert.IsTrue(activeCameras[0].transform.IsChildOf(
                Resources.FindObjectsOfTypeAll<GameObject>().Single(item => item.name == "XR_ORIGIN_LEFT_RIGHT_CONTROLLERS").transform));
        }

        [Test]
        public void SimulatorHelpAndBothBlueGlovedHandsExist()
        {
            var all = Resources.FindObjectsOfTypeAll<GameObject>().Where(item => item.scene.IsValid()).ToArray();
            Assert.That(all, Has.Some.Matches<GameObject>(item => item.name == "Simulator_Control_Guide"));
            Assert.That(all, Has.Some.Matches<GameObject>(item => item.name == "Left_Blue_Gloved_Hand"));
            Assert.That(all, Has.Some.Matches<GameObject>(item => item.name == "Right_Blue_Gloved_Hand"));
            Assert.AreEqual(1, all.Count(item => item.name == "Left Tracked Hand"));
            Assert.AreEqual(1, all.Count(item => item.name == "Right Tracked Hand"));
            var modality = Object.FindFirstObjectByType<XRInputModalityManager>(FindObjectsInactive.Include);
            Assert.IsNotNull(modality);
            Assert.AreEqual("Left Tracked Hand", modality.leftHand.name);
            Assert.AreEqual("Right Tracked Hand", modality.rightHand.name);
        }

        [UnityTest]
        public IEnumerator SocketRetainsPlateAfterLoadingAndWhileDrawerCloses()
        {
            Object.FindFirstObjectByType<LabShellController>().SetMode(InteractionMode.XR);
            yield return new WaitForSeconds(.5f);
            var lesson = Object.FindFirstObjectByType<GuidedLessonController>();
            var socket = Object.FindFirstObjectByType<OrientationAwarePlateSocket>();
            var gate = Object.FindFirstObjectByType<LessonPlateGrabGate>();
            var instrument = Object.FindFirstObjectByType<InstrumentController>();
            lesson.StartGuidedTraining();
            for (var count = 0; count < 24 && lesson.CurrentAction != TrainingAction.SeatPlate; count++)
            {
                Assert.IsTrue(lesson.TryPerformAction(lesson.CurrentAction, null), lesson.CurrentAction.ToString());
                yield return null;
            }
            Assert.AreEqual(TrainingAction.SeatPlate, lesson.CurrentAction);
            yield return new WaitForSeconds(.7f);
            Assert.IsTrue(gate.IsGrabEnabled);
            gate.transform.SetPositionAndRotation(socket.attachTransform.position, socket.attachTransform.rotation);
            Physics.SyncTransforms();
            yield return new WaitForSeconds(.6f);
            var socketTargets = new System.Collections.Generic.List<UnityEngine.XR.Interaction.Toolkit.Interactables.IXRInteractable>();
            socket.GetValidTargets(socketTargets);
            var realGrab = gate.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            Assert.AreEqual(TrainingAction.CloseDrawer, lesson.CurrentAction,
                $"socket active={socket.isActiveAndEnabled}, canSelect={socket.CanSelect((UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable)realGrab)}, targets={string.Join(",", socketTargets.Select(x=>x.transform.name))}, plate={gate.transform.position}, attach={socket.attachTransform.position}, enabled={realGrab.enabled}");
            Assert.IsTrue(socket.IsPlateSeated, "Socket must retain the plate after the lesson advances.");
            Assert.IsTrue(instrument.IsPlateInserted);
            Assert.IsTrue(lesson.TryPerformAction(TrainingAction.CloseDrawer, null));
            yield return new WaitForSeconds(.9f);
            Assert.IsTrue(instrument.IsDrawerClosed);
            Assert.IsTrue(socket.IsPlateSeated);
            Assert.AreEqual(TrainingAction.StartRun, lesson.CurrentAction);
        }

        [UnityTest]
        public IEnumerator SimulatedGripInputSelectsPlateThroughTheControllerRay()
        {
            var shell = Object.FindFirstObjectByType<LabShellController>();
            shell.SetMode(InteractionMode.XR);
            yield return new WaitForSeconds(.5f);
            var simulator = Object.FindFirstObjectByType<XRInteractionSimulator>();
            simulator.gameObject.SetActive(false);
            yield return null;
            var lesson = Object.FindFirstObjectByType<GuidedLessonController>();
            var gate = Object.FindFirstObjectByType<LessonPlateGrabGate>();
            var origin = Resources.FindObjectsOfTypeAll<GameObject>()
                .Single(item => item.name == "XR_ORIGIN_LEFT_RIGHT_CONTROLLERS");
            var offset = origin.GetComponentsInChildren<Transform>(true).First(item => item.name == "Camera Offset");
            foreach (var usage in new[] { CommonUsages.LeftHand, CommonUsages.RightHand })
            {
                lesson.StartGuidedTraining();
                lesson.TryPerformAction(TrainingAction.TourLabCoat, null);
                lesson.TryPerformAction(TrainingAction.TourGloves, null);
                lesson.TryPerformAction(TrainingAction.TourSink, null);
                var device = InputSystem.AddDevice<XRSimulatedController>();
                InputSystem.SetDeviceUsage(device, usage);
                try
                {
                    var target = gate.transform.position;
                    var position = target + new Vector3(0f, .20f, -.45f);
                    var rotation = Quaternion.LookRotation(target - position);
                    var state = new XRSimulatedControllerState {
                        isTracked = true, trackingState = 3,
                        devicePosition = offset.InverseTransformPoint(position),
                        deviceRotation = Quaternion.Inverse(offset.rotation) * rotation
                    };
                    for (var frame = 0; frame < 15; frame++)
                    { InputSystem.QueueStateEvent(device, state); yield return null; }
                    state.grip = 1f;
                    state = state.WithButton(ControllerButton.GripButton, true);
                    for (var frame = 0; frame < 15; frame++)
                    { InputSystem.QueueStateEvent(device, state); yield return null; }
                    Assert.AreEqual(TrainingAction.TourOpticalSeal, lesson.CurrentAction,
                        usage + " grip input must reach the prepared plate through XRI, not a direct lesson call. " +
                        string.Join("; ", Object.FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                        .Select(x => x.name+" active="+x.isActiveAndEnabled+" select="+x.isSelectActive+" allow="+x.allowSelect+" logical="+x.logicalSelectState.isPerformed+" performed="+x.selectInput.ReadIsPerformed()+" enabled="+x.selectInput.inputActionReferencePerformed?.action?.enabled+" deviceGrip="+device.grip.ReadValue()+" hover="+string.Join(",",x.interactablesHovered.Select(i=>i.GetType().Name+":"+i.transform.name))+" canBlock="+x.CanSelect((UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable)gate.EarlyAttemptBlocker))));
                    Assert.IsFalse(gate.IsGrabEnabled);

                    state.grip = 0f;
                    state = state.WithButton(ControllerButton.GripButton, false);
                    for (var frame = 0; frame < 5; frame++)
                    { InputSystem.QueueStateEvent(device, state); yield return null; }
                    for (var count = 0; count < 24 && lesson.CurrentAction != TrainingAction.SeatPlate; count++)
                    { Assert.IsTrue(lesson.TryPerformAction(lesson.CurrentAction, null)); yield return null; }
                    yield return new WaitForSeconds(.7f);
                    Assert.IsTrue(gate.IsGrabEnabled);
                    state.grip = 1f;
                    state = state.WithButton(ControllerButton.GripButton, true);
                    for (var frame = 0; frame < 15; frame++)
                    { InputSystem.QueueStateEvent(device, state); yield return null; }
                    Assert.IsTrue(gate.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>().isSelected,
                        usage + " must physically select the movable plate at the loading step.");
                    state.grip = 0f;
                    state = state.WithButton(ControllerButton.GripButton, false);
                    for (var frame = 0; frame < 5; frame++)
                    { InputSystem.QueueStateEvent(device, state); yield return null; }
                }
                finally { InputSystem.RemoveDevice(device); }
            }
        }

        private static TMP_Text[] ButtonLabels(string buttonName)
        {
            return Resources.FindObjectsOfTypeAll<Button>()
                .Where(button => button.gameObject.scene.IsValid() && button.name == buttonName)
                .Select(button => button.GetComponentInChildren<TMP_Text>(true))
                .Where(label => label != null)
                .ToArray();
        }
    }
}
