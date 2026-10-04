using UnityEngine;

namespace Team5.qPCR
{
    [CreateAssetMenu(fileName = "RunProtocol", menuName = "Team 5/qPCR/Run Protocol")]
    public sealed class RunProtocol : ScriptableObject
    {
        [SerializeField, Min(1)] private int cycles = 35;
        [SerializeField] private float reactionVolumeMicrolitres = 20f;
        [SerializeField] private float lidTemperature = 105f;
        [SerializeField] private string fluorophore = "SYBR Green";
        [SerializeField] private float initialHoldTemperature = 95f;
        [SerializeField] private int initialHoldSeconds = 120;
        [SerializeField] private float denaturationTemperature = 95f;
        [SerializeField] private int denaturationSeconds = 15;
        [SerializeField] private float annealingTemperature = 60f;
        [SerializeField] private int annealingSeconds = 30;
        [SerializeField] private float extensionTemperature = 72f;
        [SerializeField] private int extensionSeconds = 30;
        [SerializeField] private float acquisitionTemperature = 60f;
        [SerializeField] private float meltCurveStartTemperature = 65f;
        [SerializeField] private float meltCurveEndTemperature = 95f;
        [SerializeField, Min(0.5f)] private float simulatedDurationSeconds = 30f;

        public int Cycles => cycles;
        public float ReactionVolumeMicrolitres => reactionVolumeMicrolitres;
        public float LidTemperature => lidTemperature;
        public string Fluorophore => fluorophore;
        public float InitialHoldTemperature => initialHoldTemperature;
        public int InitialHoldSeconds => initialHoldSeconds;
        public float DenaturationTemperature => denaturationTemperature;
        public int DenaturationSeconds => denaturationSeconds;
        public float AnnealingTemperature => annealingTemperature;
        public int AnnealingSeconds => annealingSeconds;
        public float ExtensionTemperature => extensionTemperature;
        public int ExtensionSeconds => extensionSeconds;
        public float AcquisitionTemperature => acquisitionTemperature;
        public float MeltCurveStartTemperature => meltCurveStartTemperature;
        public float MeltCurveEndTemperature => meltCurveEndTemperature;
        public float SimulatedDurationSeconds => simulatedDurationSeconds;

        public RunConfiguration ExpectedConfiguration => new RunConfiguration
        {
            ReactionVolumeMicrolitres = reactionVolumeMicrolitres,
            LidTemperature = lidTemperature,
            Fluorophore = fluorophore,
            InitialHoldTemperature = initialHoldTemperature,
            InitialHoldSeconds = initialHoldSeconds,
            Cycles = cycles,
            DenaturationTemperature = denaturationTemperature,
            DenaturationSeconds = denaturationSeconds,
            AnnealingTemperature = annealingTemperature,
            AnnealingSeconds = annealingSeconds,
            ExtensionTemperature = extensionTemperature,
            ExtensionSeconds = extensionSeconds,
            AcquisitionTemperature = acquisitionTemperature,
            MeltCurveStartTemperature = meltCurveStartTemperature,
            MeltCurveEndTemperature = meltCurveEndTemperature
        };

        public void Configure(int cycleCount, float denaturation, float annealing, float extension, float durationSeconds)
        {
            cycles = Mathf.Max(1, cycleCount);
            denaturationTemperature = denaturation;
            annealingTemperature = annealing;
            extensionTemperature = extension;
            simulatedDurationSeconds = Mathf.Max(0.5f, durationSeconds);
        }

        public void Configure(RunConfiguration configuration, float durationSeconds)
        {
            if (configuration == null)
            {
                return;
            }

            reactionVolumeMicrolitres = configuration.ReactionVolumeMicrolitres;
            lidTemperature = configuration.LidTemperature;
            fluorophore = configuration.Fluorophore;
            initialHoldTemperature = configuration.InitialHoldTemperature;
            initialHoldSeconds = configuration.InitialHoldSeconds;
            cycles = Mathf.Max(1, configuration.Cycles);
            denaturationTemperature = configuration.DenaturationTemperature;
            denaturationSeconds = configuration.DenaturationSeconds;
            annealingTemperature = configuration.AnnealingTemperature;
            annealingSeconds = configuration.AnnealingSeconds;
            extensionTemperature = configuration.ExtensionTemperature;
            extensionSeconds = configuration.ExtensionSeconds;
            acquisitionTemperature = configuration.AcquisitionTemperature;
            meltCurveStartTemperature = configuration.MeltCurveStartTemperature;
            meltCurveEndTemperature = configuration.MeltCurveEndTemperature;
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
