using UnityEngine;

namespace Team5.qPCR
{
    [CreateAssetMenu(fileName = "RunProtocol", menuName = "Team 5/qPCR/Run Protocol")]
    public sealed class RunProtocol : ScriptableObject
    {
        [SerializeField, Min(1)] private int cycles = 40;
        [SerializeField] private float denaturationTemperature = 95f;
        [SerializeField] private float annealingTemperature = 60f;
        [SerializeField] private float extensionTemperature = 72f;
        [SerializeField, Min(0.5f)] private float simulatedDurationSeconds = 10f;

        public int Cycles => cycles;
        public float DenaturationTemperature => denaturationTemperature;
        public float AnnealingTemperature => annealingTemperature;
        public float ExtensionTemperature => extensionTemperature;
        public float SimulatedDurationSeconds => simulatedDurationSeconds;

        public void Configure(int cycleCount, float denaturation, float annealing, float extension, float durationSeconds)
        {
            cycles = Mathf.Max(1, cycleCount);
            denaturationTemperature = denaturation;
            annealingTemperature = annealing;
            extensionTemperature = extension;
            simulatedDurationSeconds = Mathf.Max(0.5f, durationSeconds);
        }

        public float TemperatureForCyclePhase(float normalizedCyclePhase)
        {
            if (normalizedCyclePhase < 0.34f)
            {
                return denaturationTemperature;
            }

            return normalizedCyclePhase < 0.67f ? annealingTemperature : extensionTemperature;
        }
    }
}
