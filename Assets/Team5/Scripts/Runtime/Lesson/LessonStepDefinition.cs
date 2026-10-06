using System;
using UnityEngine;

namespace Team5.qPCR
{
    public enum LessonMode
    {
        GuidedTraining,
        Assessment
    }

    public enum TrainingAction
    {
        None,
        TourLabCoat,
        TourGloves,
        TourSink,
        TourPreparedPlate,
        TourOpticalSeal,
        TourInstrument,
        TourTouchscreen,
        TourDrawer,
        TourThermalBlock,
        TourMonitor,
        TourCentrifuge,
        TourWaste,
        PowerOnInstrument,
        ValidateProtocol,
        InspectPlateId,
        InspectOpticalSeal,
        InspectBubbles,
        InspectA1Marker,
        OpenDrawer,
        SeatPlate,
        CloseDrawer,
        StartRun,
        InterpretControlsPassed,
        InterpretControlsFailed,
        ObserveAmplification,
        Complete
    }

    [Serializable]
    public sealed class LessonStepDefinition
    {
        [SerializeField] private TrainingAction requiredAction;
        [SerializeField] private string title;
        [SerializeField, TextArea(2, 4)] private string instruction;
        [SerializeField, TextArea(2, 4)] private string explanation;
        [SerializeField] private string narrationKey;

        public TrainingAction RequiredAction => requiredAction;
        public string Title => title;
        public string Instruction => instruction;
        public string Explanation => explanation;
        public string NarrationKey => narrationKey;

        public LessonStepDefinition(TrainingAction action, string heading, string prompt, string why, string key = null)
        {
            requiredAction = action;
            title = heading;
            instruction = prompt;
            explanation = why;
            narrationKey = string.IsNullOrWhiteSpace(key) ? action.ToString() : key;
        }
    }
}
