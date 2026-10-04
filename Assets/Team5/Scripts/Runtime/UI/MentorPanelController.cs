using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Team5.qPCR
{
    public sealed class MentorPanelController : MonoBehaviour
    {
        [SerializeField] private WorkflowController workflow;
        [SerializeField] private DialogueSequence dialogueSequence;
        [SerializeField] private TMP_Text stageTitle;
        [SerializeField] private TMP_Text instruction;
        [SerializeField] private TMP_Text stageCounter;
        [SerializeField] private TMP_Text status;
        [SerializeField] private TMP_Text actionLabel;
        [SerializeField] private TMP_Text secondaryActionLabel;
        [SerializeField] private TMP_Text disclaimer;
        [SerializeField] private TMP_Text whyText;
        [SerializeField] private Button actionButton;
        [SerializeField] private Button secondaryActionButton;
        [SerializeField] private Button whyButton;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private GameObject whyPanel;
        [SerializeField] private GameObject protocolPanel;
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private LabCameraDirector cameraDirector;
        [SerializeField] private CanvasGroup cardVisibility;
        private WorkflowStage viewedStage;
        private WorkflowStage latestStage = (WorkflowStage)(-1);
        private bool reviewing;
        private bool showingWhy;
        public bool IsMinimized { get; private set; }

        public void ConfigureNavigation(LabCameraDirector director, CanvasGroup visibility)
        { cameraDirector = director; cardVisibility = visibility; }

        public void SetMinimized(bool minimized)
        {
            IsMinimized = minimized;
            if (cardVisibility != null)
            { cardVisibility.alpha = minimized ? 0 : 1; cardVisibility.blocksRaycasts = !minimized; cardVisibility.interactable = !minimized; }
            cameraDirector?.SetDialogueOpen(!minimized || (protocolPanel != null && protocolPanel.activeInHierarchy));
        }
        public void Minimize() => SetMinimized(true);
        public void Reopen()
        {
            reviewing = false; showingWhy = false; SetMinimized(false); Refresh();
            if (workflow != null) cameraDirector?.FocusStage(workflow.CurrentStage, true);
        }
        public void Back()
        {
            if (workflow == null) return;
            viewedStage = (WorkflowStage)Mathf.Max(0, (int)(reviewing ? viewedStage : workflow.CurrentStage) - 1);
            reviewing = viewedStage != workflow.CurrentStage; showingWhy = false;
            cameraDirector?.FocusStage(viewedStage, true); Refresh();
        }

        public void Configure(
            WorkflowController controller,
            TMP_Text titleText,
            TMP_Text instructionText,
            TMP_Text counterText,
            TMP_Text statusText,
            TMP_Text buttonText,
            TMP_Text disclaimerText,
            Button button,
            Slider progress)
        {
            Configure(controller, null, titleText, instructionText, counterText, statusText, buttonText, null,
                disclaimerText, button, null, null, progress, null, null, null, null);
        }

        public void Configure(
            WorkflowController controller,
            DialogueSequence sequence,
            TMP_Text titleText,
            TMP_Text instructionText,
            TMP_Text counterText,
            TMP_Text statusText,
            TMP_Text buttonText,
            TMP_Text secondaryButtonText,
            TMP_Text disclaimerText,
            Button button,
            Button secondaryButton,
            Button explanationButton,
            Slider progress,
            GameObject explanationPanel,
            TMP_Text explanationText,
            GameObject setupPanel,
            GameObject resultPanel)
        {
            workflow = controller;
            dialogueSequence = sequence;
            stageTitle = titleText;
            instruction = instructionText;
            stageCounter = counterText;
            status = statusText;
            actionLabel = buttonText;
            secondaryActionLabel = secondaryButtonText;
            disclaimer = disclaimerText;
            actionButton = button;
            secondaryActionButton = secondaryButton;
            whyButton = explanationButton;
            progressSlider = progress;
            whyPanel = explanationPanel;
            whyText = explanationText;
            protocolPanel = setupPanel;
            resultsPanel = resultPanel;
        }

        private void OnEnable()
        {
            if (workflow != null)
            {
                workflow.StageChanged += OnStageChanged;
                workflow.ProgressChanged += OnProgressChanged;
                workflow.StatusChanged += OnStatusChanged;
                workflow.ExperienceReset += ResetCard;
            }

            actionButton?.onClick.AddListener(HandleAction);
            secondaryActionButton?.onClick.AddListener(HandleSecondaryAction);
            whyButton?.onClick.AddListener(ToggleWhy);
        }

        private void Start()
        {
            if (disclaimer != null)
            {
                disclaimer.text = "Teaching simulation · Not for diagnosis";
            }

            if (whyPanel != null)
            {
                whyPanel.SetActive(false);
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (workflow != null)
            {
                workflow.StageChanged -= OnStageChanged;
                workflow.ProgressChanged -= OnProgressChanged;
                workflow.StatusChanged -= OnStatusChanged;
                workflow.ExperienceReset -= ResetCard;
            }

            actionButton?.onClick.RemoveListener(HandleAction);
            secondaryActionButton?.onClick.RemoveListener(HandleSecondaryAction);
            whyButton?.onClick.RemoveListener(ToggleWhy);
        }

        private void HandleAction()
        {
            if (reviewing)
            {
                viewedStage = (WorkflowStage)Mathf.Min((int)workflow.CurrentStage, (int)viewedStage + 1);
                reviewing = viewedStage != workflow.CurrentStage;
                showingWhy = false; cameraDirector?.FocusStage(viewedStage, true); Refresh(); return;
            }
            workflow?.HandlePrimaryAction();
            Refresh();
        }

        private void ResetCard()
        { reviewing = false; showingWhy = false; latestStage = (WorkflowStage)(-1); SetMinimized(false); }

        private void HandleSecondaryAction()
        {
            workflow?.HandleSecondaryAction();
            Refresh();
        }

        private void ToggleWhy()
        {
            showingWhy = !showingWhy;
            Refresh();
        }

        private void OnStageChanged(WorkflowStage stage)
        {
            if (stage != latestStage)
            { reviewing = false; showingWhy = false; latestStage = stage; SetMinimized(false); }
            if (whyPanel != null)
            {
                whyPanel.SetActive(false);
            }

            Refresh();
        }

        private void OnProgressChanged(float progress, string label)
        {
            if (progressSlider != null)
            {
                progressSlider.value = progress;
            }

            if (workflow != null && workflow.CurrentStage == WorkflowStage.Amplification && status != null)
            {
                status.text = label;
            }

            RefreshButtonState();
        }

        private void OnStatusChanged(string message)
        {
            if (status != null)
            {
                status.text = message;
            }

            RefreshButtonState();
        }

        private void Refresh()
        {
            if (workflow == null)
            {
                return;
            }

            var stage = reviewing ? viewedStage : workflow.CurrentStage;
            var fallback = WorkflowContent.For(stage);
            var entry = dialogueSequence == null ? null : dialogueSequence.For(stage);
            var title = entry == null ? fallback.Title : entry.Title;
            var body = entry == null ? fallback.Instruction : entry.Instruction;
            var why = entry == null ? fallback.Why : entry.Why;
            var action = entry == null ? fallback.Action : entry.Action;

            if (stageTitle != null) stageTitle.text = title;
            if (instruction != null) instruction.text = showingWhy ? why : body;
            if(instruction!=null && cameraDirector!=null)
            {
                instruction.rectTransform.anchoredPosition=new Vector2(16,showingWhy?62:108);
                instruction.rectTransform.sizeDelta=new Vector2(448,showingWhy?118:76);
                if(status!=null)status.gameObject.SetActive(!showingWhy);
            }
            if (whyText != null) whyText.text = why;
            if (actionLabel != null) actionLabel.text = reviewing ? "Next explanation" : action;
            if (stageCounter != null) stageCounter.text = $"{(reviewing ? "Review" : "Step")} {(int)stage + 1} of 10";

            if (secondaryActionLabel != null)
            {
                secondaryActionLabel.text = stage == WorkflowStage.PlateInspection
                    ? "Align A1"
                    : "Controls failed";
            }

            if (protocolPanel != null)
            {
                protocolPanel.SetActive(workflow.CurrentStage == WorkflowStage.ProtocolSetup && !reviewing);
            }

            if (resultsPanel != null)
            {
                resultsPanel.SetActive(!reviewing && (stage == WorkflowStage.Amplification || stage == WorkflowStage.ResultsInterpretation));
            }

            RefreshButtonState();
        }

        private void RefreshButtonState()
        {
            if (workflow == null)
            {
                return;
            }

            if (actionButton != null)
            {
                actionButton.interactable = reviewing || workflow.IsPrimaryActionAvailable;
            }

            if (secondaryActionButton != null)
            {
                secondaryActionButton.gameObject.SetActive(!reviewing && workflow.IsSecondaryActionAvailable);
                secondaryActionButton.interactable = workflow.IsSecondaryActionAvailable;
            }
        }
    }
}
