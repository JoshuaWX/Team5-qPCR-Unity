using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Team5.qPCR
{
    /// <summary>Keeps the prepared plate inspectable but prevents early grabbing.</summary>
    public sealed class LessonPlateGrabGate : MonoBehaviour
    {
        [SerializeField] private GuidedLessonController lesson;
        [SerializeField] private WorkflowController workflow;
        [SerializeField] private XRGrabInteractable grab;

        public void Configure(GuidedLessonController controller, WorkflowController workflowController,
            XRGrabInteractable interactable)
        {
            lesson = controller;
            workflow = workflowController;
            grab = interactable;
        }

        private void OnEnable()
        {
            if (lesson != null) lesson.StepChanged += OnStepChanged;
            if (workflow != null)
            {
                workflow.StageChanged += OnWorkflowStageChanged;
                workflow.ExperienceReset += Refresh;
            }
            Refresh();
        }

        private void OnDisable()
        {
            if (lesson != null) lesson.StepChanged -= OnStepChanged;
            if (workflow != null)
            {
                workflow.StageChanged -= OnWorkflowStageChanged;
                workflow.ExperienceReset -= Refresh;
            }
        }

        private void Update()
        {
            if (grab != null && grab.enabled != ShouldEnable())
                Refresh();
        }

        private void OnStepChanged(LessonStepDefinition _) => Refresh();
        private void OnWorkflowStageChanged(WorkflowStage _) => Refresh();

        private void Refresh()
        {
            if (grab != null) grab.enabled = ShouldEnable();
        }

        private bool ShouldEnable()
        {
            if (lesson != null && lesson.IsActive)
                return lesson.CurrentAction == TrainingAction.SeatPlate;
            return workflow != null && workflow.CurrentStage == WorkflowStage.PlateInspection;
        }
    }
}
