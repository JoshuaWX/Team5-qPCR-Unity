namespace Team5.qPCR
{
    public enum WorkflowStage
    {
        Introduction,
        HandoffReview,
        PowerOn,
        ProtocolSetup,
        PlateInspection,
        InstrumentLoading,
        RunValidation,
        Amplification,
        ResultsInterpretation,
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

            CurrentStage = WorkflowStage.ResultsInterpretation;
            return true;
        }

        public void Reset()
        {
            CurrentStage = WorkflowStage.Introduction;
        }
    }

    public static class RunEligibility
    {
        public static bool CanStart(bool platePrepared, bool plateInspected, bool plateInserted, bool drawerClosed, bool protocolValid)
        {
            return platePrepared && plateInspected && plateInserted && drawerClosed && protocolValid;
        }

        public static bool CanStart(bool allWellsLoaded, bool plateSealed, bool plateInserted)
        {
            return allWellsLoaded && plateSealed && plateInserted;
        }
    }
}
