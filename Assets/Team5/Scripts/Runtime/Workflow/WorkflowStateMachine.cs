namespace Team5.qPCR
{
    public enum WorkflowStage
    {
        Introduction,
        PlateReview,
        PlateLoading,
        PlateSealing,
        InstrumentLoading,
        RunSetup,
        Amplification,
        Results,
        Complete
    }

    public sealed class WorkflowStateMachine
    {
        public WorkflowStage CurrentStage { get; private set; } = WorkflowStage.Introduction;

        public bool TryAdvance()
        {
            if (CurrentStage == WorkflowStage.Amplification || CurrentStage == WorkflowStage.Complete)
            {
                return false;
            }

            CurrentStage = (WorkflowStage)((int)CurrentStage + 1);
            return true;
        }

        public bool TryCompleteAmplification()
        {
            if (CurrentStage != WorkflowStage.Amplification)
            {
                return false;
            }

            CurrentStage = WorkflowStage.Results;
            return true;
        }

        public void Reset()
        {
            CurrentStage = WorkflowStage.Introduction;
        }
    }

    public static class RunEligibility
    {
        public static bool CanStart(bool allWellsLoaded, bool plateSealed, bool plateInserted)
        {
            return allWellsLoaded && plateSealed && plateInserted;
        }
    }
}
