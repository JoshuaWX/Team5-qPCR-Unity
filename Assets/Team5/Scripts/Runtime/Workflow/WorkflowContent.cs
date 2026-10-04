namespace Team5.qPCR
{
    public readonly struct StageContent
    {
        public StageContent(string title, string instruction, string why, string action)
        {
            Title = title;
            Instruction = instruction;
            Why = why;
            Action = action;
        }

        public string Title { get; }
        public string Instruction { get; }
        public string Why { get; }
        public string Action { get; }
    }

    public static class WorkflowContent
    {
        public static StageContent For(WorkflowStage stage)
        {
            switch (stage)
            {
                case WorkflowStage.Introduction:
                    return new StageContent(
                        "Inside the qPCR lab",
                        "You will configure a real-time PCR run, load Team 4's prepared plate, and explain the amplification result.",
                        "qPCR measures fluorescence during DNA copying, so the machine can show when amplification becomes detectable.",
                        "Begin lesson");
                case WorkflowStage.HandoffReview:
                    return new StageContent(
                        "Meet your prepared plate",
                        "Review Team 4's handoff: a filled, sealed, centrifuged 96-well plate with 28 active reactions.",
                        "Each active well already contains supermix, both primers and its sample or control. The NTC has water instead of DNA. Open Reaction mix to see the teaching recipe.",
                        "Accept handoff");
                case WorkflowStage.PowerOn:
                    return new StageContent(
                        "Wake up the instrument",
                        "Switch on the qPCR instrument and wait for its touchscreen to become ready.",
                        "The instrument performs startup checks before it can heat, cool, or measure fluorescence.",
                        "Power on");
                case WorkflowStage.ProtocolSetup:
                    return new StageContent(
                        "Configure the run",
                        "Check every value on the compact protocol panel. Correct settings protect the samples and make the results meaningful.",
                        "Each temperature performs a different job: separate DNA, attach primers, then build new DNA.",
                        "Check settings");
                case WorkflowStage.PlateInspection:
                    return new StageContent(
                        "Check the plate",
                        "Inspect the optical seal, confirm there are no bubbles, and align the A1 marker with the instrument.",
                        "Bubbles can disturb fluorescence readings, while A1 alignment keeps the digital plate map matched to the real wells.",
                        "Confirm inspection");
                case WorkflowStage.InstrumentLoading:
                    return new StageContent(
                        "Seat the plate",
                        "Open the drawer, place the plate in the block, and close the drawer securely.",
                        "Firm placement gives every well equal thermal contact during cycling.",
                        "Load instrument");
                case WorkflowStage.RunValidation:
                    return new StageContent(
                        "Ready to start?",
                        "Complete the final check: protocol valid, plate inspected, A1 aligned, plate inserted, and drawer closed.",
                        "The run stays locked until the machine and sample preparation checks all pass.",
                        "Start qPCR run");
                case WorkflowStage.Amplification:
                    return new StageContent(
                        "DNA copying in progress",
                        "Watch 35 condensed cycles. Red separates DNA, blue lets primers bind, and green builds new strands.",
                        "SYBR dye detects double-stranded DNA, including unwanted products. The coloured animation is a diagram, not the real colour of the liquid.",
                        "Running…");
                case WorkflowStage.ResultsInterpretation:
                    return new StageContent(
                        "Read the controls first",
                        "A valid run has a rising positive-control curve and a flat no-template-control curve. Choose the correct interpretation.",
                        "A rising NTC can mean contamination or primer-dimers. Controls and melt-curve evidence matter; a Cq number alone is not a diagnosis.",
                        "Controls passed");
                default:
                    return new StageContent(
                        "Lesson complete",
                        "You configured, loaded, ran, and interpreted a complete educational qPCR workflow.",
                        "These representative curves teach the process but are not patient data and cannot be used for diagnosis.",
                        "Start again");
            }
        }
    }
}
