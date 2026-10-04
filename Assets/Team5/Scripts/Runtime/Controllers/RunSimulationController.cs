using System;
using System.Collections;
using UnityEngine;

namespace Team5.qPCR
{
    public sealed class RunSimulationController : MonoBehaviour
    {
        [SerializeField] private RunProtocol protocol;
        [SerializeField] private ExperimentDefinition experiment;
        [SerializeField] private ResultsController resultsController;
        [SerializeField] private DnaCycleVisualizer dnaVisualizer;

        public event Action<int, float, float> ProgressChanged;
        public event Action RunCompleted;

        public bool IsRunning { get; private set; }
        public int Cycles => protocol == null ? 35 : protocol.Cycles;

        public void Configure(RunProtocol runProtocol, ExperimentDefinition experimentDefinition, ResultsController results)
        {
            Configure(runProtocol, experimentDefinition, results, null);
        }

        public void Configure(RunProtocol runProtocol, ExperimentDefinition experimentDefinition, ResultsController results, DnaCycleVisualizer visualizer)
        {
            protocol = runProtocol;
            experiment = experimentDefinition;
            resultsController = results;
            dnaVisualizer = visualizer;
        }

        public void StartRun()
        {
            if (IsRunning || protocol == null || experiment == null)
            {
                return;
            }

            StartCoroutine(RunRoutine());
        }

        public void ResetRun()
        {
            StopAllCoroutines();
            IsRunning = false;
            dnaVisualizer?.ResetVisual();
            ProgressChanged?.Invoke(0, 0f, protocol == null ? 0f : protocol.DenaturationTemperature);
        }

        private IEnumerator RunRoutine()
        {
            IsRunning = true;
            var results = AssayResultGenerator.Generate(experiment);
            resultsController?.BeginRun(results);
            var elapsed = 0f;
            while (elapsed < protocol.SimulatedDurationSeconds)
            {
                elapsed += Time.deltaTime;
                var normalized = Mathf.Clamp01(elapsed / protocol.SimulatedDurationSeconds);
                var exactCycle = normalized * protocol.Cycles;
                var cycle = Mathf.Clamp(Mathf.FloorToInt(exactCycle) + 1, 1, protocol.Cycles);
                var cyclePhase = exactCycle - Mathf.Floor(exactCycle);
                var temperature = protocol.TemperatureForCyclePhase(cyclePhase);
                dnaVisualizer?.SetPhase(cyclePhase);
                resultsController?.UpdateVisibleCycle(cycle);
                ProgressChanged?.Invoke(cycle, normalized, temperature);
                yield return null;
            }

            resultsController?.DisplayResults(results);
            ProgressChanged?.Invoke(protocol.Cycles, 1f, protocol.ExtensionTemperature);
            IsRunning = false;
            dnaVisualizer?.ResetVisual();
            RunCompleted?.Invoke();
        }
    }
}
