using TMPro;
using UnityEngine;

namespace Team5.qPCR
{
    public sealed class PresentationLauncherController : MonoBehaviour
    {
        [SerializeField] private LabShellController shell;
        [SerializeField] private GuidedLessonController lesson;
        [SerializeField] private GameObject[] simulatorControlsPanels = System.Array.Empty<GameObject>();
        [SerializeField] private TMP_Text[] previewStatusLabels = System.Array.Empty<TMP_Text>();

        public void Configure(LabShellController labShell, GuidedLessonController lessonController,
            GameObject[] controlsPanels, TMP_Text[] statusLabels)
        {
            shell = labShell;
            lesson = lessonController;
            simulatorControlsPanels = controlsPanels ?? System.Array.Empty<GameObject>();
            previewStatusLabels = statusLabels ?? System.Array.Empty<TMP_Text>();
        }

        public void DesktopPreview()
        {
            shell?.SetMode(InteractionMode.Desktop);
            SetStatus("Desktop Preview · click physical controls or use the walkthrough camera");
        }

        public void XrSimulatorPreview()
        {
            shell?.SetMode(InteractionMode.XR);
            SetStatus("XR Simulator Preview · Game view shows the XR Origin camera");
        }

        public void ResetLesson() => lesson?.ResetLesson();

        public void ShowSimulatorControls()
        {
            if (simulatorControlsPanels == null || simulatorControlsPanels.Length == 0) return;
            var show = !System.Array.Exists(simulatorControlsPanels, panel => panel != null && panel.activeSelf);
            foreach (var panel in simulatorControlsPanels) if (panel != null) panel.SetActive(show);
        }

        public void HideSimulatorControls()
        {
            if (simulatorControlsPanels != null)
                foreach (var panel in simulatorControlsPanels) if (panel != null) panel.SetActive(false);
        }

        private void SetStatus(string value)
        {
            if (previewStatusLabels == null) return;
            foreach (var label in previewStatusLabels) if (label != null) label.text = value;
        }
    }
}
