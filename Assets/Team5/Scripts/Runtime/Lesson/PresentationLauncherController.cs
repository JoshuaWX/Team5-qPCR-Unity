using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace Team5.qPCR
{
    public sealed class PresentationLauncherController : MonoBehaviour
    {
        [SerializeField] private LabShellController shell;
        [SerializeField] private GuidedLessonController lesson;
        [SerializeField] private GameObject[] simulatorControlsPanels = System.Array.Empty<GameObject>();
        [SerializeField] private TMP_Text[] previewStatusLabels = System.Array.Empty<TMP_Text>();
        [SerializeField] private Button[] desktopPreviewButtons = System.Array.Empty<Button>();
        [SerializeField] private Button[] xrPreviewButtons = System.Array.Empty<Button>();
        private XRInputModalityManager.InputMode lastInputMode = (XRInputModalityManager.InputMode)(-1);

        public string CurrentStatusText { get; private set; } = string.Empty;

        public void Configure(LabShellController labShell, GuidedLessonController lessonController,
            GameObject[] controlsPanels, TMP_Text[] statusLabels, Button[] desktopButtons, Button[] xrButtons)
        {
            if (shell != null) shell.ModeChanged -= OnModeChanged;
            shell = labShell;
            lesson = lessonController;
            simulatorControlsPanels = controlsPanels ?? System.Array.Empty<GameObject>();
            previewStatusLabels = statusLabels ?? System.Array.Empty<TMP_Text>();
            desktopPreviewButtons = desktopButtons ?? System.Array.Empty<Button>();
            xrPreviewButtons = xrButtons ?? System.Array.Empty<Button>();
            if (isActiveAndEnabled && shell != null) shell.ModeChanged += OnModeChanged;
            Refresh(shell == null ? InteractionMode.Desktop : shell.CurrentMode);
        }

        private void OnEnable()
        {
            if (shell != null) shell.ModeChanged += OnModeChanged;
            if (shell != null) Refresh(shell.CurrentMode);
        }

        private void OnDisable()
        {
            if (shell != null) shell.ModeChanged -= OnModeChanged;
        }

        private void Update()
        {
            if (shell == null || shell.CurrentMode != InteractionMode.XR) return;
            var inputMode = XRInputModalityManager.currentInputMode.Value;
            if (inputMode == lastInputMode) return;
            lastInputMode = inputMode;
            Refresh(InteractionMode.XR);
        }

        public void DesktopPreview()
        {
            shell?.SetMode(InteractionMode.Desktop);
        }

        public void XrSimulatorPreview()
        {
            shell?.SetMode(InteractionMode.XR);
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

        private void OnModeChanged(InteractionMode mode) => Refresh(mode);

        private void Refresh(InteractionMode mode)
        {
            var xr = mode == InteractionMode.XR;
            SetButtonState(desktopPreviewButtons, !xr, "Desktop Preview");
            SetButtonState(xrPreviewButtons, xr, "XR Simulator Preview");
            if (!xr)
            {
                SetStatus("Active preview: Desktop · walkthrough camera");
                return;
            }

            lastInputMode = XRInputModalityManager.currentInputMode.Value;
            var modality = lastInputMode switch
            {
                XRInputModalityManager.InputMode.TrackedHand => "tracked hands active",
                XRInputModalityManager.InputMode.MotionController => "controllers active",
                _ => "controllers or simulated hands ready"
            };
            SetStatus("Active preview: XR Simulator · XR Origin camera · " + modality);
        }

        private static void SetButtonState(Button[] buttons, bool active, string normalLabel)
        {
            if (buttons == null) return;
            var background = active ? new Color32(29, 95, 208, 255) : new Color32(238, 245, 255, 255);
            Color foreground = active ? Color.white : new Color32(20, 51, 97, 255);
            foreach (var button in buttons)
            {
                if (button == null) continue;
                if (button.targetGraphic != null) button.targetGraphic.color = background;
                var label = button.GetComponentInChildren<TMP_Text>(true);
                if (label == null) continue;
                label.text = normalLabel + (active ? " · Active" : string.Empty);
                label.color = foreground;
                label.enableAutoSizing = true;
                label.fontSizeMin = 10f;
            }
        }

        private void SetStatus(string value)
        {
            CurrentStatusText = value;
            if (previewStatusLabels == null) return;
            foreach (var label in previewStatusLabels) if (label != null) label.text = value;
        }
    }
}
