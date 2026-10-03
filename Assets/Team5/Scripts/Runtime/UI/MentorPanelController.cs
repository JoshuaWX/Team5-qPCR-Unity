using TMPro;
using UnityEngine;

namespace Team5.qPCR
{
    public sealed class MentorPanelController : MonoBehaviour
    {
        [SerializeField] private WorkflowController workflow;
        [SerializeField] private TMP_Text stageTitle;
        [SerializeField] private TMP_Text instruction;
        [SerializeField] private TMP_Text stageCounter;
        [SerializeField] private TMP_Text status;
        [SerializeField] private TMP_Text actionLabel;
        [SerializeField] private TMP_Text disclaimer;
        [SerializeField] private UnityEngine.UI.Button actionButton;
        [SerializeField] private UnityEngine.UI.Slider progressSlider;

        public void Configure(
            WorkflowController controller,
            TMP_Text titleText,
            TMP_Text instructionText,
            TMP_Text counterText,
            TMP_Text statusText,
            TMP_Text buttonText,
            TMP_Text disclaimerText,
            UnityEngine.UI.Button button,
            UnityEngine.UI.Slider progress)
        {
            workflow = controller;
            stageTitle = titleText;
            instruction = instructionText;
            stageCounter = counterText;
            status = statusText;
            actionLabel = buttonText;
            disclaimer = disclaimerText;
            actionButton = button;
            progressSlider = progress;
        }

        private void OnEnable()
        {
            if (workflow != null)
            {
                workflow.StageChanged += OnStageChanged;
                workflow.ProgressChanged += OnProgressChanged;
                workflow.StatusChanged += OnStatusChanged;
            }

            if (actionButton != null)
            {
                actionButton.onClick.AddListener(HandleAction);
            }
        }

        private void Start()
        {
            if (disclaimer != null)
            {
                disclaimer.text = "EDUCATIONAL SIMULATION  •  REPRESENTATIVE DATA  •  NOT FOR DIAGNOSIS";
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
            }

            if (actionButton != null)
            {
                actionButton.onClick.RemoveListener(HandleAction);
            }
        }

        private void HandleAction()
        {
            workflow?.HandlePrimaryAction();
            Refresh();
        }

        private void OnStageChanged(WorkflowStage stage)
        {
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

            var content = WorkflowContent.For(workflow.CurrentStage);
            if (stageTitle != null)
            {
                stageTitle.text = content.Title;
            }

            if (instruction != null)
            {
                instruction.text = content.Instruction;
            }

            if (actionLabel != null)
            {
                actionLabel.text = content.Action;
            }

            if (stageCounter != null)
            {
                stageCounter.text = $"STEP {((int)workflow.CurrentStage + 1):00} / 09";
            }

            RefreshButtonState();
        }

        private void RefreshButtonState()
        {
            if (actionButton != null && workflow != null)
            {
                actionButton.interactable = workflow.IsPrimaryActionAvailable;
            }
        }
    }
}
