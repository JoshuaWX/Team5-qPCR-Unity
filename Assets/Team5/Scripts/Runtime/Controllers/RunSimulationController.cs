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

        public event Action<int, float, float> ProgressChanged;
        public event Action RunCompleted;

        public bool IsRunning { get; private set; }

        public void Configure(RunProtocol runProtocol, ExperimentDefinition experimentDefinition, ResultsController results)
        {
            protocol = runProtocol;
            experiment = experimentDefinition;
            resultsController = results;
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
            ProgressChanged?.Invoke(0, 0f, protocol == null ? 0f : protocol.DenaturationTemperature);
        }

        private IEnumerator RunRoutine()
        {
            IsRunning = true;
            var elapsed = 0f;
            while (elapsed < protocol.SimulatedDurationSeconds)
            {
                elapsed += Time.deltaTime;
                var normalized = Mathf.Clamp01(elapsed / protocol.SimulatedDurationSeconds);
                var exactCycle = normalized * protocol.Cycles;
                var cycle = Mathf.Clamp(Mathf.FloorToInt(exactCycle) + 1, 1, protocol.Cycles);
                var cyclePhase = exactCycle - Mathf.Floor(exactCycle);
                var temperature = protocol.TemperatureForCyclePhase(cyclePhase);
                ProgressChanged?.Invoke(cycle, normalized, temperature);
                yield return null;
            }

            var results = AssayResultGenerator.Generate(experiment);
            resultsController?.DisplayResults(results);
            ProgressChanged?.Invoke(protocol.Cycles, 1f, protocol.ExtensionTemperature);
            IsRunning = false;
            RunCompleted?.Invoke();
        }
    }
}
