using System;
using System.Collections.Generic;
using UnityEngine;

namespace Team5.qPCR
{
    public enum AssayClassification
    {
        Positive,
        Negative,
        ValidPositiveControl,
        ValidNoTemplateControl
    }

    [Serializable]
    public sealed class AssayResult
    {
        public string WellId;
        public string SampleId;
        public WellType WellType;
        public AssayClassification Classification;
        public float Cq;
        public List<float> Fluorescence = new List<float>();

        public bool IsAmplified => Cq > 0f;
    }

    public static class AssayResultGenerator
    {
        public const int DefaultSeed = 52026;
        public const float DetectionThreshold = 0.22f;

        public static List<AssayResult> Generate(ExperimentDefinition experiment, int seed = DefaultSeed)
        {
            var results = new List<AssayResult>(28);
            if (experiment == null || experiment.Wells == null)
            {
                return results;
            }

            var random = new System.Random(seed);
            for (var index = 0; index < experiment.Wells.Count; index++)
            {
                var well = experiment.Wells[index];
                if (well == null || well.Type == WellType.Unused)
                {
                    continue;
                }

                var result = new AssayResult
                {
                    WellId = well.WellId,
                    SampleId = well.SampleId,
                    WellType = well.Type
                };

                var shouldAmplify = well.Type == WellType.PositiveControl ||
                                    (well.Type == WellType.Sample && ((well.Row * 12) + well.Column) % 4 != 0);

                if (well.Type == WellType.NoTemplateControl)
                {
                    result.Classification = AssayClassification.ValidNoTemplateControl;
                    result.Cq = -1f;
                }
                else if (well.Type == WellType.PositiveControl)
                {
                    result.Classification = AssayClassification.ValidPositiveControl;
                    result.Cq = 19.2f + ((float)random.NextDouble() * 1.4f);
                }
                else if (shouldAmplify)
                {
                    result.Classification = AssayClassification.Positive;
                    result.Cq = 20.5f + ((float)random.NextDouble() * 10.5f);
                }
                else
                {
                    result.Classification = AssayClassification.Negative;
                    result.Cq = -1f;
                }

                for (var cycle = 0; cycle <= 35; cycle++)
                {
                    var baselineNoise = ((float)random.NextDouble() - 0.5f) * 0.012f;
                    var fluorescence = 0.035f + baselineNoise;
                    if (result.IsAmplified)
                    {
                        var midpoint = result.Cq + 3.5f;
                        fluorescence += 0.92f / (1f + Mathf.Exp(-0.58f * (cycle - midpoint)));
                    }

                    result.Fluorescence.Add(Mathf.Clamp01(fluorescence));
                }

                results.Add(result);
            }

            return results;
        }
    }
}
