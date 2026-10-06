using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

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
        }
    }
}
