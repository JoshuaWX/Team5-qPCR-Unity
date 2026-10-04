using TMPro;
using UnityEngine;

namespace Team5.qPCR
{
    public sealed class LabHudController : MonoBehaviour
    {
        [SerializeField] private WorkflowController workflow;
        [SerializeField] private LabCameraDirector director;
        [SerializeField] private TMP_Text chapters, viewLabel;
        [SerializeField] private TMP_Text motionLabel;
        [SerializeField] private GameObject mixSheet, protocolPanel, resultsPanel;
        [SerializeField] private GameObject mentorPanel;
        private bool protocolWasOpen, resultsWereOpen;
        private bool mentorWasOpen;
        public void SetMotionLabel(TMP_Text label) => motionLabel = label;
        public void ToggleMotion() { director.ToggleReducedMotion(); RefreshMotion(); }
        private void RefreshMotion() { if (motionLabel != null) motionLabel.text = director.ReducedMotion ? "Motion: low" : "Motion: full"; }
        private void Update()
        {
            if (director == null) return;
            director.ModalOpen = (mixSheet != null && mixSheet.activeInHierarchy) ||
                (protocolPanel != null && protocolPanel.activeInHierarchy) ||
                (resultsPanel != null && resultsPanel.activeInHierarchy);
            if (director.ModalOpen) director.SetDialogueOpen(true);
        }
        public void Configure(WorkflowController flow, LabCameraDirector cameraDirector, TMP_Text chapterText,
            TMP_Text viewText, GameObject mix, GameObject protocol, GameObject results)
        { workflow = flow; director = cameraDirector; chapters = chapterText; viewLabel = viewText;
            mixSheet = mix; protocolPanel = protocol; resultsPanel = results; }
        private void OnEnable()
        {
            if (workflow != null) { workflow.StageChanged += Stage; workflow.ExperienceReset += ResetHud; }
            if (director != null) director.ModeChanged += View;
        }
        private void OnDisable()
        {
            if (workflow != null) { workflow.StageChanged -= Stage; workflow.ExperienceReset -= ResetHud; }
            if (director != null) director.ModeChanged -= View;
        }
        private void Start() { Stage(workflow.CurrentStage); View(director.Mode); RefreshMotion(); }
        private void Stage(WorkflowStage stage)
        {
            if (mixSheet != null && mixSheet.activeSelf) CloseMix();
            var chapter = stage <= WorkflowStage.ProtocolSetup ? 0 : stage <= WorkflowStage.InstrumentLoading ? 1 : 2;
            var labels = new[] { "1  Configure", "2  Load", "3  Run & results" };
            if (chapters != null) chapters.text = string.Join("     /     ", System.Array.ConvertAll(labels, label =>
                label == labels[chapter] ? "<color=#1D5FD0><b>" + label + "</b></color>" : label));
        }
        private void View(CameraMode mode)
        {
            if (viewLabel != null) viewLabel.text = mode == CameraMode.Guided
                ? "Guided view · Right-drag to orbit · Wheel to zoom"
                : (mode == CameraMode.FirstPerson ? "First person" : "Third person") + " · Tab to walk / use cursor · WASD to move · Esc to release";
        }
        private void ResetHud()
        {
            if (mixSheet != null) mixSheet.SetActive(false);
            if (mentorPanel != null) mentorPanel.SetActive(true);
        }
        public void OpenMix()
        {
            if (mixSheet == null || mixSheet.activeSelf) return;
            if (mentorPanel == null) mentorPanel = GameObject.Find("Reusable_Mentor_Dialogue");
            protocolWasOpen = protocolPanel != null && protocolPanel.activeSelf;
            resultsWereOpen = resultsPanel != null && resultsPanel.activeSelf;
            mentorWasOpen = mentorPanel != null && mentorPanel.activeSelf;
            if (protocolPanel != null) protocolPanel.SetActive(false);
            if (resultsPanel != null) resultsPanel.SetActive(false);
            if (mentorPanel != null) mentorPanel.SetActive(false);
            mixSheet.SetActive(true); director?.SetDialogueOpen(true);
        }
        public void CloseMix()
        {
            if (mixSheet != null) mixSheet.SetActive(false);
            if (mentorPanel != null) mentorPanel.SetActive(mentorWasOpen);
            if (protocolPanel != null) protocolPanel.SetActive(protocolWasOpen && workflow.CurrentStage == WorkflowStage.ProtocolSetup);
            if (resultsPanel != null) resultsPanel.SetActive(resultsWereOpen &&
                (workflow.CurrentStage == WorkflowStage.Amplification || workflow.CurrentStage == WorkflowStage.ResultsInterpretation));
        }
    }
}
