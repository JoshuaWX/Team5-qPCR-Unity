using UnityEngine;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

namespace Team5.qPCR
{
    /// <summary>Keeps the prepared plate inspectable but prevents early grabbing.</summary>
    public sealed class LessonPlateGrabGate : MonoBehaviour, IXRSelectFilter
    {
        [SerializeField] private GuidedLessonController lesson;
        [SerializeField] private WorkflowController workflow;
        [SerializeField] private XRGrabInteractable grab;
        [SerializeField] private XRSimpleInteractable earlyAttemptBlocker;
        [SerializeField] private LabObjectDescriptor feedbackTarget;
        [SerializeField] private float repeatThrottleSeconds = 0.65f;
        private float nextAllowedAttemptTime;

        public XRSimpleInteractable EarlyAttemptBlocker => earlyAttemptBlocker;
        public bool IsGrabEnabled => grab != null && grab.enabled;
        public bool canProcess => isActiveAndEnabled;
        private bool IsSocketSelected => grab != null && grab.interactorsSelecting.Any(item => item is OrientationAwarePlateSocket);

        public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
            => !IsSocketSelected || interactor is OrientationAwarePlateSocket;

        public void Configure(GuidedLessonController controller, WorkflowController workflowController,
            XRGrabInteractable interactable, XRSimpleInteractable blocker, LabObjectDescriptor visualFeedbackTarget)
        {
            lesson = controller;
            workflow = workflowController;
            grab = interactable;
            earlyAttemptBlocker = blocker;
            feedbackTarget = visualFeedbackTarget;
        }

        private void OnEnable()
        {
            grab?.selectFilters.Add(this);
            if (lesson != null) lesson.StepChanged += OnStepChanged;
            earlyAttemptBlocker?.selectEntered.AddListener(OnEarlyAttempt);
            if (workflow != null)
            {
                workflow.StageChanged += OnWorkflowStageChanged;
                workflow.ExperienceReset += Refresh;
            }
            Refresh();
        }

        private void OnDisable()
        {
            grab?.selectFilters.Remove(this);
            if (lesson != null) lesson.StepChanged -= OnStepChanged;
            earlyAttemptBlocker?.selectEntered.RemoveListener(OnEarlyAttempt);
            if (workflow != null)
            {
                workflow.StageChanged -= OnWorkflowStageChanged;
                workflow.ExperienceReset -= Refresh;
            }
        }

        private void Update()
        {
            var shouldEnable = ShouldEnable();
            if ((grab != null && grab.enabled != shouldEnable) ||
                (earlyAttemptBlocker != null && earlyAttemptBlocker.enabled == shouldEnable))
                Refresh();
        }

        private void OnStepChanged(LessonStepDefinition _) => Refresh();
        private void OnWorkflowStageChanged(WorkflowStage _) => Refresh();

        private void Refresh()
        {
            var shouldEnable = ShouldEnable();
            if (grab != null && earlyAttemptBlocker != null &&
                grab.enabled == shouldEnable && earlyAttemptBlocker.enabled != shouldEnable) return;
            // Both interactables share one collider. Unregister the previous owner BEFORE
            // registering the next one or XRI can lose its collider-to-interactable mapping.
            if (earlyAttemptBlocker != null) earlyAttemptBlocker.enabled = false;
            if (grab != null && !IsSocketSelected) grab.enabled = false;
            if (shouldEnable && grab != null) grab.enabled = true;
            if (!shouldEnable && earlyAttemptBlocker != null) earlyAttemptBlocker.enabled = true;
        }

        private bool ShouldEnable()
        {
            if (lesson != null)
                return lesson.IsActive && (lesson.CurrentAction == TrainingAction.SeatPlate || IsSocketSelected);
            return workflow != null && workflow.CurrentStage == WorkflowStage.PlateInspection;
        }

        public void ReportEarlyAttempt()
        {
            HandleEarlyAttempt(null);
        }

        private void OnEarlyAttempt(SelectEnterEventArgs args)
        {
            HandleEarlyAttempt(args.interactorObject as XRBaseInputInteractor);
        }

        private void HandleEarlyAttempt(XRBaseInputInteractor inputInteractor)
        {
            if (Time.unscaledTime < nextAllowedAttemptTime) return;
            nextAllowedAttemptTime = Time.unscaledTime + repeatThrottleSeconds;

            // During the Team 4 handoff tour, selecting the plate is the correct action even
            // though physically moving it must remain locked until the later SeatPlate step.
            // The root blocker owns the plate collider and can be the first XRI target hit by
            // either Trigger or Grip, so forward that selection instead of reporting a circular
            // "select the prepared plate" rejection.
            if (lesson != null && lesson.IsActive && lesson.CurrentAction == TrainingAction.TourPreparedPlate)
            {
                var accepted = lesson.TryPerformAction(TrainingAction.TourPreparedPlate, feedbackTarget);
                inputInteractor?.SendHapticImpulse(accepted ? 0.32f : 0.78f, accepted ? 0.06f : 0.14f);
                return;
            }

            var message = lesson != null && lesson.IsActive
                ? $"Complete the current task first: {lesson.CurrentTargetLabel.ToLowerInvariant()}."
                : "Choose Guided Training or Assessment Mode first.";
            lesson?.ReportRejectedAction(TrainingAction.SeatPlate, message);
            feedbackTarget?.FlashError();
            inputInteractor?.SendHapticImpulse(0.78f, 0.14f);
        }
    }
}
