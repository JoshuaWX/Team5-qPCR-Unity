using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Team5.qPCR
{
    public sealed class GuidedLessonController : MonoBehaviour
    {
        [SerializeField] private WorkflowController workflow;
        [SerializeField] private PlateController plate;
        [SerializeField] private InstrumentController instrument;
        [SerializeField] private ProtocolSetupController protocol;
        [SerializeField] private ResultsController results;
        [SerializeField] private NarrationController narration;
        [SerializeField] private GuidanceCueController guidance;
        [SerializeField] private OrientationAwarePlateSocket plateSocket;
        [SerializeField] private LessonStepDefinition[] steps = Array.Empty<LessonStepDefinition>();
        [SerializeField] private TMP_Text[] modeLabels = Array.Empty<TMP_Text>();
        [SerializeField] private TMP_Text[] stepLabels = Array.Empty<TMP_Text>();
        [SerializeField] private TMP_Text[] targetLabels = Array.Empty<TMP_Text>();
        [SerializeField] private TMP_Text[] instructionLabels = Array.Empty<TMP_Text>();
        [SerializeField] private TMP_Text[] feedbackLabels = Array.Empty<TMP_Text>();
        [SerializeField] private TMP_Text[] timerLabels = Array.Empty<TMP_Text>();
        [SerializeField] private TMP_Text[] reportLabels = Array.Empty<TMP_Text>();
        [SerializeField] private GameObject[] modeSelectionPanels = Array.Empty<GameObject>();
        [SerializeField] private GameObject[] completionPanels = Array.Empty<GameObject>();
        [SerializeField] private GameObject[] protocolPanels = Array.Empty<GameObject>();
        [SerializeField] private GameObject[] resultsPanels = Array.Empty<GameObject>();

        private readonly LessonSessionReport report = new LessonSessionReport();
        private int stepIndex = -1;
        private float stepElapsed;
        private float forcedHintRemaining;
        private float feedbackRemaining;
        private bool active;
        private bool resetting;

        public event Action<LessonStepDefinition> StepChanged;
        public event Action<TrainingAction, bool> ActionEvaluated;
        public LessonMode Mode { get; private set; }
        public LessonSessionReport Report => report;
        public bool IsActive => active;
        public LessonStepDefinition CurrentStep => stepIndex >= 0 && stepIndex < steps.Length ? steps[stepIndex] : null;
        public TrainingAction CurrentAction => CurrentStep?.RequiredAction ?? TrainingAction.None;
        public string CurrentTargetLabel => CurrentStep?.TargetLabel ?? string.Empty;

        public void Configure(WorkflowController flow, PlateController plateController, InstrumentController instrumentController,
            ProtocolSetupController protocolController, ResultsController resultController, NarrationController narrator,
            GuidanceCueController cueController, LessonStepDefinition[] lessonSteps, TMP_Text[] selectedModeLabels,
            TMP_Text[] currentStepLabels, TMP_Text[] currentTargetLabels, TMP_Text[] currentInstructionLabels,
            TMP_Text[] actionFeedbackLabels,
            TMP_Text[] elapsedLabels, TMP_Text[] summaryLabels, GameObject[] selectionPanels, GameObject[] finalPanels)
        {
            workflow = flow;
            plate = plateController;
            instrument = instrumentController;
            protocol = protocolController;
            results = resultController;
            narration = narrator;
            guidance = cueController;
            steps = lessonSteps ?? Array.Empty<LessonStepDefinition>();
            modeLabels = selectedModeLabels ?? Array.Empty<TMP_Text>();
            stepLabels = currentStepLabels ?? Array.Empty<TMP_Text>();
            targetLabels = currentTargetLabels ?? Array.Empty<TMP_Text>();
            instructionLabels = currentInstructionLabels ?? Array.Empty<TMP_Text>();
            feedbackLabels = actionFeedbackLabels ?? Array.Empty<TMP_Text>();
            timerLabels = elapsedLabels ?? Array.Empty<TMP_Text>();
            reportLabels = summaryLabels ?? Array.Empty<TMP_Text>();
            modeSelectionPanels = selectionPanels ?? Array.Empty<GameObject>();
            completionPanels = finalPanels ?? Array.Empty<GameObject>();
        }

        public void SetPlateSocket(OrientationAwarePlateSocket socket) => plateSocket = socket;
        public void SetContextPanels(GameObject[] protocolViews, GameObject[] resultViews)
        {
            protocolPanels = protocolViews ?? Array.Empty<GameObject>();
            resultsPanels = resultViews ?? Array.Empty<GameObject>();
        }

        private void OnEnable()
        {
            if (workflow != null)
            {
                workflow.StageChanged += OnWorkflowStageChanged;
                workflow.ExperienceReset += OnWorkflowReset;
            }
        }

        private void OnDisable()
        {
            if (workflow != null)
            {
                workflow.StageChanged -= OnWorkflowStageChanged;
                workflow.ExperienceReset -= OnWorkflowReset;
            }
        }

        private void Start() => ResetLesson();

        private void Update()
        {
            if (!active) return;
            var delta = Time.unscaledDeltaTime;
            report.Tick(delta);
            stepElapsed += delta;
            forcedHintRemaining = Mathf.Max(0f, forcedHintRemaining - delta);
            if (feedbackRemaining > 0f)
            {
                feedbackRemaining = Mathf.Max(0f, feedbackRemaining - delta);
                if (feedbackRemaining <= 0f) SetText(feedbackLabels, string.Empty);
            }
            SetText(timerLabels, $"{Mathf.FloorToInt(report.ElapsedSeconds / 60f):00}:{Mathf.FloorToInt(report.ElapsedSeconds % 60f):00}   " +
                $"Mistakes {report.Mistakes}   Hints {report.HintsUsed}");
            guidance?.UpdateCue(stepElapsed, Mode == LessonMode.GuidedTraining, forcedHintRemaining > 0f,
                CurrentStep?.Instruction ?? string.Empty);
        }

        public void StartGuidedTraining() => StartLesson(LessonMode.GuidedTraining);
        public void StartAssessment() => StartLesson(LessonMode.Assessment);

        public void StartLesson(LessonMode selectedMode)
        {
            ResetLessonState(false);
            Mode = selectedMode;
            report.Begin(selectedMode);
            active = true;
            SetModeSelectionVisible(false);
            SetActive(completionPanels, false);
            SetText(modeLabels, selectedMode == LessonMode.GuidedTraining ? "Guided Training" : "Assessment Mode");
            workflow?.BeginLesson();
            SetStep(0);
        }

        public bool TryPerformAction(TrainingAction action, LabObjectDescriptor source)
        {
            if (!active || CurrentStep == null)
            {
                ReportRejectedAction(action, "Choose Guided Training or Assessment Mode first.");
                return false;
            }

            if (action == TrainingAction.InterpretControlsFailed)
            {
                ReportRejectedAction(action, "The controls do not pass: the positive control must amplify and the NTC must remain flat.");
                workflow?.InterpretControls(false);
                return false;
            }

            if (action != CurrentStep.RequiredAction)
            {
                ReportRejectedAction(action, $"Complete the current task first: {CurrentTargetLabel.ToLowerInvariant()}.");
                return false;
            }

            var accepted = PerformScientificAction(action);
            if (!accepted)
            {
                ReportRejectedAction(action, $"That action is not ready yet. Check the current target: {CurrentTargetLabel}.");
                return false;
            }

            guidance?.MarkActionComplete(action);
            source?.MarkComplete();
            ActionEvaluated?.Invoke(action, true);

            if (action != TrainingAction.CloseDrawer)
            {
                AdvanceAfterAction(action);
            }
            return true;
        }

        private bool PerformScientificAction(TrainingAction action)
        {
            switch (action)
            {
                case TrainingAction.PowerOnInstrument:
                    return workflow != null && workflow.PowerOnInstrument();
                case TrainingAction.ValidateProtocol:
                    return workflow != null && workflow.ValidateProtocol();
                case TrainingAction.InspectA1Marker:
                    return workflow != null && workflow.CompletePreparedPlateInspection();
                case TrainingAction.OpenDrawer:
                    return instrument != null && instrument.OpenDrawer();
                case TrainingAction.SeatPlate:
                    return instrument != null && instrument.IsPlateInserted;
                case TrainingAction.CloseDrawer:
                    return instrument != null && instrument.CloseDrawer(() =>
                    {
                        if (workflow != null && workflow.ConfirmPhysicalLoadingComplete())
                            AdvanceAfterAction(TrainingAction.CloseDrawer);
                    });
                case TrainingAction.StartRun:
                    return workflow != null && workflow.StartValidatedRun();
                case TrainingAction.InterpretControlsPassed:
                    return workflow != null && workflow.InterpretControls(true);
                default:
                    return true;
            }
        }

        private void AdvanceAfterAction(TrainingAction completedAction)
        {
            if (completedAction == TrainingAction.TourWaste)
            {
                if (workflow == null || !workflow.AcceptPreparedHandoff()) return;
            }

            if (completedAction == TrainingAction.StartRun)
            {
                SetStepByAction(TrainingAction.ObserveAmplification);
                return;
            }

            if (completedAction == TrainingAction.InterpretControlsPassed)
            {
                FinishLesson();
                return;
            }

            SetStep(stepIndex + 1);
        }

        public void RequestHelp()
        {
            if (!active || CurrentStep == null) return;
            report.RecordHint();
            forcedHintRemaining = 6f;
            guidance?.UpdateCue(stepElapsed, true, true, CurrentStep.Instruction);
        }

        public void ReportRejectedAction(TrainingAction attemptedAction)
        {
            ReportRejectedAction(attemptedAction,
                active && CurrentStep != null
                    ? $"Complete the current task first: {CurrentTargetLabel.ToLowerInvariant()}."
                    : "Choose Guided Training or Assessment Mode first.");
        }

        public void ReportRejectedAction(TrainingAction attemptedAction, string message)
        {
            if (active) report.RecordMistake();
            SetText(feedbackLabels, message ?? string.Empty);
            feedbackRemaining = 4f;
            ActionEvaluated?.Invoke(attemptedAction, false);
        }

        public void ReplayNarration()
        {
            narration?.Replay();
        }

        public void ResetLesson()
        {
            ResetLessonState(true);
        }

        private void ResetLessonState(bool showModeSelection)
        {
            if (resetting) return;
            resetting = true;
            active = false;
            stepIndex = -1;
            stepElapsed = 0f;
            forcedHintRemaining = 0f;
            feedbackRemaining = 0f;
            plateSocket?.ResetSocket();
            workflow?.ResetExperience();
            narration?.ResetNarration();
            guidance?.HideAll();
            SetModeSelectionVisible(showModeSelection);
            SetActive(completionPanels, false);
            SetActive(protocolPanels, false);
            SetActive(resultsPanels, false);
            SetText(modeLabels, "Choose a mode");
            SetText(stepLabels, "IGH Genomics Training Lab");
            SetText(targetLabels, "Current target: Choose a lesson mode");
            SetText(instructionLabels, "Select Guided Training for full narration and cues, or Assessment Mode to practise with minimal help.");
            SetText(feedbackLabels, string.Empty);
            SetText(timerLabels, "00:00   Mistakes 0   Hints 0");
            resetting = false;
        }

        private void OnWorkflowReset()
        {
            if (!resetting) ResetLesson();
        }

        private void OnWorkflowStageChanged(WorkflowStage stage)
        {
            if (!active) return;
            SetActive(protocolPanels, stage == WorkflowStage.ProtocolSetup);
            SetActive(resultsPanels, stage == WorkflowStage.Amplification || stage == WorkflowStage.ResultsInterpretation);
            if (stage == WorkflowStage.ResultsInterpretation)
                SetStepByAction(TrainingAction.InterpretControlsPassed);
        }

        private void SetStepByAction(TrainingAction action)
        {
            var index = Array.FindIndex(steps, step => step != null && step.RequiredAction == action);
            if (index >= 0) SetStep(index);
        }

        private void SetStep(int index)
        {
            if (steps.Length == 0) return;
            stepIndex = Mathf.Clamp(index, 0, steps.Length - 1);
            stepElapsed = 0f;
            forcedHintRemaining = 0f;
            var step = CurrentStep;
            if (step == null) return;
            SetText(stepLabels, step.Title);
            SetText(targetLabels, "Current target: " + step.TargetLabel);
            SetText(instructionLabels, step.Instruction);
            SetText(feedbackLabels, string.Empty);
            feedbackRemaining = 0f;
            guidance?.SetTarget(step.RequiredAction, Mode == LessonMode.GuidedTraining);
            narration?.Play(step.NarrationKey, step.Title, step.Instruction + " " + step.Explanation);
            StepChanged?.Invoke(step);
        }

        private void FinishLesson()
        {
            active = false;
            report.Complete();
            guidance?.HideAll();
            SetText(reportLabels, report.Summary);
            SetActive(completionPanels, true);
            SetText(stepLabels, "Lesson complete");
            SetText(targetLabels, "Current target: Complete");
            const string completeMessage = "You inspected the handoff, configured the instrument, loaded the plate and interpreted the controls.";
            SetText(instructionLabels, completeMessage);
            narration?.Play("Complete", "Lesson complete", completeMessage);
        }

        public bool HasCompletedInspectionAction(TrainingAction action)
        {
            var index = Array.FindIndex(steps, step => step != null && step.RequiredAction == action);
            return index >= 0 && stepIndex > index;
        }

        private static void SetText(TMP_Text[] targets, string value)
        {
            if (targets == null) return;
            foreach (var target in targets) if (target != null) target.text = value;
        }

        private static void SetActive(GameObject[] targets, bool value)
        {
            if (targets == null) return;
            foreach (var target in targets) if (target != null) target.SetActive(value);
        }

        private void SetModeSelectionVisible(bool visible)
        {
            if (modeSelectionPanels == null) return;
            foreach (var panel in modeSelectionPanels)
            {
                if (panel == null) continue;
                var group = panel.GetComponent<CanvasGroup>();
                if (group != null)
                {
                    group.alpha = visible ? 1f : 0f;
                    group.interactable = visible;
                    group.blocksRaycasts = visible;
                }
                panel.SetActive(visible);
            }
        }
    }
}
