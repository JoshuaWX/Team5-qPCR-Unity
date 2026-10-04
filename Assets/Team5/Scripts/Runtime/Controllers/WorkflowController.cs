using System;
using UnityEngine;

namespace Team5.qPCR
{
    public sealed class WorkflowController : MonoBehaviour
    {
        [SerializeField] private PlateController plateController;
        [SerializeField] private InstrumentController instrumentController;
        [SerializeField] private RunSimulationController runSimulation;
        [SerializeField] private ResultsController resultsController;
        [SerializeField] private ProtocolSetupController protocolSetup;

        private readonly WorkflowStateMachine stateMachine = new WorkflowStateMachine();

        public event Action<WorkflowStage> StageChanged;
        public event Action<float, string> ProgressChanged;
        public event Action<string> StatusChanged;
        public event Action ExperienceReset;

        public WorkflowStage CurrentStage => stateMachine.CurrentStage;
        public bool IsBusy { get; private set; }
        public bool IsPrimaryActionAvailable => !IsBusy && CurrentStage != WorkflowStage.Amplification;
        public bool IsSecondaryActionAvailable => !IsBusy &&
            (CurrentStage == WorkflowStage.PlateInspection || CurrentStage == WorkflowStage.ResultsInterpretation);

        public void Configure(PlateController plate, InstrumentController instrument, RunSimulationController simulation, ResultsController results)
        {
            Configure(plate, instrument, simulation, results, null);
        }

        public void Configure(
            PlateController plate,
            InstrumentController instrument,
            RunSimulationController simulation,
            ResultsController results,
            ProtocolSetupController setup)
        {
            plateController = plate;
            instrumentController = instrument;
            runSimulation = simulation;
            resultsController = results;
            protocolSetup = setup;
        }

        private void OnEnable()
        {
            if (runSimulation != null)
            {
                runSimulation.ProgressChanged += OnRunProgress;
                runSimulation.RunCompleted += OnRunCompleted;
            }
        }

        private void OnDisable()
        {
            if (runSimulation != null)
            {
                runSimulation.ProgressChanged -= OnRunProgress;
                runSimulation.RunCompleted -= OnRunCompleted;
            }
        }

        private void Start()
        {
            ResetExperience();
        }

        public void HandlePrimaryAction()
        {
            if (IsBusy)
            {
                return;
            }

            switch (CurrentStage)
            {
                case WorkflowStage.Introduction:
                    Advance();
                    break;
                case WorkflowStage.HandoffReview:
                    if (plateController != null && plateController.IsPrepared)
                    {
                        StatusChanged?.Invoke("Team 4 handoff accepted: 96 wells mapped, 28 active reactions, sealed and bubble-free.");
                        Advance();
                    }
                    else
                    {
                        StatusChanged?.Invoke("Handoff blocked: the prepared plate record is incomplete.");
                    }
                    break;
                case WorkflowStage.PowerOn:
                    instrumentController?.PowerOn();
                    StatusChanged?.Invoke("Instrument ready. The protocol touchscreen is unlocked.");
                    Advance();
                    break;
                case WorkflowStage.ProtocolSetup:
                    if (protocolSetup != null && protocolSetup.ValidateAndReport())
                    {
                        StatusChanged?.Invoke("Protocol accepted. Chapter 2 is now unlocked.");
                        Advance();
                    }
                    else
                    {
                        StatusChanged?.Invoke("Protocol blocked. Correct the highlighted setting on the touchscreen.");
                    }
                    break;
                case WorkflowStage.PlateInspection:
                    if (plateController == null || !plateController.IsA1Aligned)
                    {
                        StatusChanged?.Invoke("Wrong orientation. Use ALIGN A1, then inspect the plate.");
                        return;
                    }

                    if (plateController.InspectPlate())
                    {
                        StatusChanged?.Invoke("Plate ID, optical seal, bubbles, and A1 orientation all pass inspection.");
                        Advance();
                    }
                    break;
                case WorkflowStage.InstrumentLoading:
                    IsBusy = true;
                    StatusChanged?.Invoke("Opening the drawer and seating the prepared plate...");
                    if (instrumentController == null)
                    {
                        IsBusy = false;
                        StatusChanged?.Invoke("Instrument controller is unavailable. Reset the lesson.");
                        break;
                    }

                    instrumentController.InsertPlate(() =>
                    {
                        IsBusy = false;
                        if (instrumentController.IsPlateInserted && instrumentController.IsDrawerClosed)
                        {
                            StatusChanged?.Invoke("Plate inserted with full thermal contact; drawer closed.");
                            Advance();
                        }
                        else
                        {
                            StatusChanged?.Invoke("Loading failed. Confirm the instrument is powered and try again.");
                            RefreshCurrentStage();
                        }
                    });
                    RefreshCurrentStage();
                    break;
                case WorkflowStage.RunValidation:
                    if (!RunEligibility.CanStart(
                            plateController != null && plateController.IsPrepared,
                            plateController != null && plateController.IsInspected,
                            instrumentController != null && instrumentController.IsPlateInserted,
                            instrumentController != null && instrumentController.IsDrawerClosed,
                            protocolSetup == null || protocolSetup.IsValid))
                    {
                        StatusChanged?.Invoke("Run blocked: protocol, plate inspection, insertion, and drawer checks must all pass.");
                        return;
                    }

                    Advance();
                    IsBusy = true;
                    StatusChanged?.Invoke("35-cycle qPCR run started. Educational time is compressed to 30 seconds.");
                    runSimulation?.StartRun();
                    break;
                case WorkflowStage.ResultsInterpretation:
                    if(resultsController==null || !resultsController.ControlsValid)
                    { StatusChanged?.Invoke("Controls do not pass. Do not accept this run; reset and investigate."); return; }
                    StatusChanged?.Invoke("Correct: the positive control rises and the NTC stays flat. This run is valid.");
                    Advance();
                    break;
                case WorkflowStage.Complete:
                    ResetExperience();
                    break;
            }
        }

        public void HandleSecondaryAction()
        {
            if (!IsSecondaryActionAvailable)
            {
                return;
            }

            if (CurrentStage == WorkflowStage.PlateInspection)
            {
                plateController?.AlignA1();
                StatusChanged?.Invoke("A1 is now aligned with the instrument marker. Inspect the plate to continue.");
            }
            else if (CurrentStage == WorkflowStage.ResultsInterpretation)
            {
                StatusChanged?.Invoke("Try again: the positive control amplified and the NTC stayed flat, so contamination is not indicated.");
            }

            RefreshCurrentStage();
        }

        public void ResetExperience()
        {
            IsBusy = false;
            stateMachine.Reset();
            instrumentController?.ResetInstrument();
            plateController?.ResetPlate();
            protocolSetup?.ResetConfiguration();
            runSimulation?.ResetRun();
            resultsController?.ResetResults();
            ExperienceReset?.Invoke();
            StatusChanged?.Invoke("Station ready. Press Space, Enter, or the action button to continue.");
            ProgressChanged?.Invoke(0f, "READY");
            RefreshCurrentStage();
        }

        private void Advance()
        {
            if (stateMachine.TryAdvance())
            {
                RefreshCurrentStage();
            }
        }

        private void OnRunProgress(int cycle, float normalized, float temperature)
        {
            var total = runSimulation == null ? 35 : runSimulation.Cycles;
            ProgressChanged?.Invoke(normalized, $"CYCLE {cycle:00} / {total:00}  ·  {temperature:0}°C");
        }

        private void OnRunCompleted()
        {
            IsBusy = false;
            if (stateMachine.TryCompleteAmplification())
            {
                StatusChanged?.Invoke("Run complete. Inspect the controls before accepting the results.");
                RefreshCurrentStage();
            }
        }

        private void RefreshCurrentStage()
        {
            StageChanged?.Invoke(CurrentStage);
        }
    }
}
