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

        private readonly WorkflowStateMachine stateMachine = new WorkflowStateMachine();

        public event Action<WorkflowStage> StageChanged;
        public event Action<float, string> ProgressChanged;
        public event Action<string> StatusChanged;

        public WorkflowStage CurrentStage => stateMachine.CurrentStage;
        public bool IsBusy { get; private set; }
        public bool IsPrimaryActionAvailable => !IsBusy && CurrentStage != WorkflowStage.Amplification;

        public void Configure(
            PlateController plate,
            InstrumentController instrument,
            RunSimulationController simulation,
            ResultsController results)
        {
            plateController = plate;
            instrumentController = instrument;
            runSimulation = simulation;
            resultsController = results;
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
                case WorkflowStage.PlateReview:
                    Advance();
                    break;
                case WorkflowStage.PlateLoading:
                    IsBusy = true;
                    StatusChanged?.Invoke("Loading prepared reactions into all 96 wells...");
                    plateController.LoadAllWells(() =>
                    {
                        IsBusy = false;
                        StatusChanged?.Invoke("96 / 96 wells loaded and mapped.");
                        Advance();
                    });
                    RefreshCurrentStage();
                    break;
                case WorkflowStage.PlateSealing:
                    if (plateController.ApplySeal())
                    {
                        StatusChanged?.Invoke("Optical seal applied. Plate is ready for the instrument.");
                        Advance();
                    }
                    else
                    {
                        StatusChanged?.Invoke("Load all 96 wells before sealing the plate.");
                    }
                    break;
                case WorkflowStage.InstrumentLoading:
                    IsBusy = true;
                    StatusChanged?.Invoke("Opening drawer and aligning A1 orientation...");
                    instrumentController.InsertPlate(() =>
                    {
                        IsBusy = false;
                        StatusChanged?.Invoke("Plate inserted and drawer locked.");
                        Advance();
                    });
                    RefreshCurrentStage();
                    break;
                case WorkflowStage.RunSetup:
                    if (!RunEligibility.CanStart(plateController.AllWellsLoaded, plateController.IsSealed, instrumentController.IsPlateInserted))
                    {
                        StatusChanged?.Invoke("Run blocked: confirm loading, sealing, and instrument insertion.");
                        return;
                    }

                    Advance();
                    IsBusy = true;
                    StatusChanged?.Invoke("Thermal cycling and fluorescence acquisition started.");
                    runSimulation.StartRun();
                    break;
                case WorkflowStage.Results:
                    StatusChanged?.Invoke("Controls are valid. Interpretation recorded.");
                    Advance();
                    break;
                case WorkflowStage.Complete:
                    ResetExperience();
                    break;
            }
        }

        public void ResetExperience()
        {
            IsBusy = false;
            stateMachine.Reset();
            plateController?.ResetPlate();
            instrumentController?.ResetInstrument();
            runSimulation?.ResetRun();
            resultsController?.ResetResults();
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
            ProgressChanged?.Invoke(normalized, $"CYCLE {cycle:00} / 40  •  {temperature:0}°C");
        }

        private void OnRunCompleted()
        {
            IsBusy = false;
            if (stateMachine.TryCompleteAmplification())
            {
                StatusChanged?.Invoke("Run complete. Curves generated from representative educational data.");
                RefreshCurrentStage();
            }
        }

        private void RefreshCurrentStage()
        {
            StageChanged?.Invoke(CurrentStage);
        }
    }
}
