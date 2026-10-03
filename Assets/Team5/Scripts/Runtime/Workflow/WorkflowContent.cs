namespace Team5.qPCR
{
    public readonly struct StageContent
    {
        public StageContent(string title, string instruction, string action)
        {
            Title = title;
            Instruction = instruction;
            Action = action;
        }

        public string Title { get; }
        public string Instruction { get; }
        public string Action { get; }
    }

    public static class WorkflowContent
    {
        public static StageContent For(WorkflowStage stage)
        {
            switch (stage)
            {
                case WorkflowStage.Introduction:
                    return new StageContent("Load & Run", "You are operating the final qPCR station. Follow the validated plate from review to interpreted amplification results.", "BEGIN BRIEFING");
                case WorkflowStage.PlateReview:
                    return new StageContent("Review the Plate Map", "Confirm the full 96-well layout: patient samples, positive controls, and no-template controls are present and uniquely labelled.", "REVIEW PLATE MAP");
                case WorkflowStage.PlateLoading:
                    return new StageContent("Load All 96 Wells", "Load the prepared reaction mix into every assigned well. Controls must remain in their labelled positions.", "LOAD 96 WELLS");
                case WorkflowStage.PlateSealing:
                    return new StageContent("Apply Optical Seal", "Seal the plate evenly to reduce evaporation and cross-contamination during thermal cycling.", "APPLY OPTICAL SEAL");
                case WorkflowStage.InstrumentLoading:
                    return new StageContent("Insert the Plate", "Place the sealed plate in the qPCR instrument with the A1 orientation marker aligned correctly.", "INSERT PLATE");
                case WorkflowStage.RunSetup:
                    return new StageContent("Verify Run Protocol", "Review the 40-cycle protocol, fluorescence acquisition, and control positions before starting the run.", "START qPCR RUN");
                case WorkflowStage.Amplification:
                    return new StageContent("Amplification in Progress", "The instrument is cycling through denaturation, annealing, and extension while fluorescence is measured in real time.", "RUNNING...");
                case WorkflowStage.Results:
                    return new StageContent("Interpret Results", "Compare amplification curves with the controls. Positive samples cross the threshold; valid no-template controls remain flat.", "COMPLETE INTERPRETATION");
                default:
                    return new StageContent("Workflow Complete", "The Team 5 load-and-run workflow is complete. Results shown are representative educational data, not diagnostic output.", "RESTART SIMULATION");
            }
        }
    }
}
