using System.Collections;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Team5.qPCR.PlayModeTests
{
    public sealed class NavigationSceneTests
    {
        [UnitySetUp]
        public IEnumerator Setup(){yield return SceneManager.LoadSceneAsync("Team5_qPCR_RealisticLab");yield return null;}
        [UnityTest]
        public IEnumerator BothWalkthroughModesKeepRoofAndResetReturnsToOverview()
        {
            var director=Object.FindFirstObjectByType<LabCameraDirector>();var workflow=Object.FindFirstObjectByType<WorkflowController>();
            var roof=Resources.FindObjectsOfTypeAll<GameObject>().First(x=>x.scene.IsValid()&&x.name=="REMOVABLE_CEILING");
            director.FirstPerson();yield return null;Assert.IsTrue(roof.activeSelf);Assert.AreEqual(CameraMode.FirstPerson,director.Mode);
            director.ThirdPerson();yield return null;Assert.IsTrue(roof.activeSelf);Assert.IsNotNull(GameObject.Find("ScientistVisual"));
            workflow.ResetExperience();yield return null;Assert.AreEqual(CameraMode.Guided,director.Mode);Assert.IsFalse(roof.activeSelf);
        }
        [UnityTest]
        public IEnumerator RepeatedStageRefreshDoesNotReplayCameraTransition()
        {
            var director=Object.FindFirstObjectByType<LabCameraDirector>();
            director.FocusStage(WorkflowStage.PlateInspection,true);var count=director.TransitionCount;
            director.FocusStage(WorkflowStage.PlateInspection);director.FocusStage(WorkflowStage.PlateInspection);
            yield return null;Assert.AreEqual(count,director.TransitionCount);
        }
        [UnityTest]
        public IEnumerator BackAndReopenDoNotUndoOrSkipScientificProgress()
        {
            var workflow=Object.FindFirstObjectByType<WorkflowController>();
            var mentor=Object.FindObjectsByType<MentorPanelController>(FindObjectsSortMode.None).First(x=>x.name=="Reusable_Mentor_Dialogue");
            workflow.HandlePrimaryAction();workflow.HandlePrimaryAction();
            mentor.Back();yield return null;Assert.AreEqual(WorkflowStage.PowerOn,workflow.CurrentStage);
            mentor.Minimize();Assert.IsTrue(mentor.IsMinimized);mentor.Reopen();Assert.IsFalse(mentor.IsMinimized);
            Assert.AreEqual(WorkflowStage.PowerOn,workflow.CurrentStage);
        }
        [UnityTest]
        public IEnumerator PlateScaleAndDialogueFootprintMatchTheSpecification()
        {
            var plate=Object.FindFirstObjectByType<PlateController>();
            var collider=plate.transform.Find("Polypropylene_96_Well_Plate").GetComponent<BoxCollider>();
            Assert.AreEqual(.12776f,collider.bounds.size.x,.001f);Assert.AreEqual(.08548f,collider.bounds.size.z,.001f);
            var card=GameObject.Find("Reusable_Mentor_Dialogue").GetComponent<RectTransform>();
            Assert.AreEqual(480,card.rect.width,.01f);Assert.AreEqual(260,card.rect.height,.01f);
            Assert.Less(card.rect.width*card.rect.height,1920*1080/3f);
            Assert.IsTrue(plate.Handoff.ReactionMix.Validate(20,"SYBR Green",out _));yield return null;
        }
        [UnityTest]
        public IEnumerator WalkthroughColliderCannotCrossTheLaboratoryWall()
        {
            var director=Object.FindFirstObjectByType<LabCameraDirector>();director.FirstPerson();
            var player=Object.FindFirstObjectByType<CharacterController>();
            player.enabled=false;player.transform.position=new Vector3(5.3f,.05f,0);player.enabled=true;
            for(var i=0;i<20;i++){player.Move(Vector3.right*.2f);yield return null;}
            Assert.Less(player.transform.position.x,5.8f);
        }
        [UnityTest]
        public IEnumerator ProtocolInputCanReceiveFocusWithoutWorkflowProgress()
        {
            var workflow=Object.FindFirstObjectByType<WorkflowController>();
            workflow.HandlePrimaryAction();workflow.HandlePrimaryAction();workflow.HandlePrimaryAction();
            var input=GameObject.Find("Protocol_Field_00").GetComponent<TMP_InputField>();
            input.Select();input.ActivateInputField();yield return null;
            Assert.IsTrue(DesktopInputGuard.IsEditingText);Assert.AreEqual(WorkflowStage.ProtocolSetup,workflow.CurrentStage);
            input.DeactivateInputField();
        }
        [UnityTest]
        public IEnumerator InterruptingTravelPreservesTheCurrentPosition()
        {
            var director=Object.FindFirstObjectByType<LabCameraDirector>();
            director.FocusOn(CameraFocus.Plate);yield return null;
            var before=Camera.main.transform.position;director.InterruptTravel();yield return null;
            Assert.IsFalse(director.IsTransitioning);Assert.Less(Vector3.Distance(before,Camera.main.transform.position),.02f);
        }
        [UnityTest]
        public IEnumerator ResetReopensMinimisedCardAndModalBlocksWalking()
        {
            var workflow=Object.FindFirstObjectByType<WorkflowController>();
            var mentor=Object.FindObjectsByType<MentorPanelController>(FindObjectsSortMode.None).First(x=>x.name=="Reusable_Mentor_Dialogue");
            mentor.Minimize();workflow.ResetExperience();yield return null;Assert.IsFalse(mentor.IsMinimized);
            var hud=Object.FindFirstObjectByType<LabHudController>();var director=Object.FindFirstObjectByType<LabCameraDirector>();
            hud.OpenMix();yield return null;Assert.IsTrue(director.ModalOpen);
            hud.CloseMix();yield return null;Assert.IsFalse(director.ModalOpen);
        }
        [UnityTest]
        public IEnumerator XrHeadPoseIsNotChangedByGuidedFocus()
        {
            var director=Object.FindFirstObjectByType<LabCameraDirector>();
            var shell=Object.FindFirstObjectByType<LabShellController>();shell.SetMode(InteractionMode.XR);yield return null;
            // The simulator creates input devices first; modality activation follows a later input update.
            yield return new WaitForSeconds(.5f);
            var xrCamera=Camera.main;Assert.IsNotNull(xrCamera);
            var before=xrCamera.transform.localPosition;director.FocusOn(CameraFocus.Plate);yield return null;
            Assert.Less(Vector3.Distance(before,xrCamera.transform.localPosition),.001f);
            shell.SetMode(InteractionMode.Desktop);
        }
        [UnityTest]
        public IEnumerator BothXrControllersCanSelectThePreparedPlateAfterProtocolValidation()
        {
            var flow=Object.FindFirstObjectByType<WorkflowController>();
            var grab=Object.FindFirstObjectByType<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            Assert.IsFalse(grab.enabled,"Grabbing is locked before preparation review");
            var lesson=Object.FindFirstObjectByType<GuidedLessonController>(FindObjectsInactive.Include);
            lesson.StartGuidedTraining();
            var requiredBeforeLoading=new[]{
                TrainingAction.TourLabCoat,TrainingAction.TourGloves,TrainingAction.TourSink,
                TrainingAction.TourPreparedPlate,TrainingAction.TourOpticalSeal,TrainingAction.TourInstrument,
                TrainingAction.TourTouchscreen,TrainingAction.TourDrawer,TrainingAction.TourThermalBlock,
                TrainingAction.TourMonitor,TrainingAction.TourCentrifuge,TrainingAction.TourWaste,
                TrainingAction.PowerOnInstrument,TrainingAction.ValidateProtocol,TrainingAction.InspectPlateId,
                TrainingAction.InspectOpticalSeal,TrainingAction.InspectBubbles,TrainingAction.InspectA1Marker,
                TrainingAction.OpenDrawer};
            foreach(var action in requiredBeforeLoading)
                Assert.IsTrue(lesson.TryPerformAction(action,null),$"Lesson action {action} should unlock the next step.");
            Assert.AreEqual(TrainingAction.SeatPlate,lesson.CurrentAction);
            var shell=Object.FindFirstObjectByType<LabShellController>();shell.SetMode(InteractionMode.XR);yield return null;
            yield return new WaitForSeconds(.5f);
            var simulator=Object.FindFirstObjectByType<UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator>();
            var hands=Object.FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor>(FindObjectsSortMode.None);
            Assert.IsNotNull(simulator);
            Assert.IsNotNull(simulator.GetComponent<XRInteractionSimulatorInputBridge>());
            Assert.AreEqual(2,hands.Length,"Both simulated controllers must become tracked and active.");
            Assert.IsTrue(grab.enabled);
            var stageBeforeGrab=flow.CurrentStage;
            var manager=Object.FindFirstObjectByType<UnityEngine.XR.Interaction.Toolkit.XRInteractionManager>();
            foreach(var hand in hands)
            {
                manager.SelectEnter((UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor)hand,grab);
                Assert.IsTrue(grab.isSelected,"Each controller must be able to select the plate");
                manager.SelectExit((UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor)hand,grab);
            }
            Assert.AreEqual(stageBeforeGrab,flow.CurrentStage,"A grab must not skip a scientific gate");
            shell.SetMode(InteractionMode.Desktop);flow.ResetExperience();yield return null;
            Assert.IsFalse(grab.enabled);
        }
    }
}
