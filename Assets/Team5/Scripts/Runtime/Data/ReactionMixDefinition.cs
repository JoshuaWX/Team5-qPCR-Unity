using System;
using UnityEngine;

namespace Team5.qPCR
{
    /// <summary>A teaching composition, not a validated wet-laboratory assay.</summary>
    [CreateAssetMenu(fileName = "ReactionMixDefinition", menuName = "Team 5/qPCR/Reaction Mix")]
    public sealed class ReactionMixDefinition : ScriptableObject
    {
        public const string ReferenceUrl = "https://www.bio-rad.com/sites/default/files/webroot/web/pdf/lsr/literature/10000068167.pdf";
        [SerializeField] private string chemistry = "SYBR Green";
        [SerializeField] private float reactionVolume = 20f;
        [SerializeField] private float supermixVolume = 10f;
        [SerializeField] private float supermixConcentration = 2f;
        [SerializeField] private float forwardPrimerVolume = 0.6f;
        [SerializeField] private float reversePrimerVolume = 0.6f;
        [SerializeField] private float primerStockMicromolar = 10f;
        [SerializeField] private float templateVolume = 2f;
        [SerializeField] private float waterVolume = 6.8f;

        public string Chemistry => chemistry;
        public float ReactionVolume => reactionVolume;
        public float SupermixVolume => supermixVolume;
        public float ForwardPrimerVolume => forwardPrimerVolume;
        public float ReversePrimerVolume => reversePrimerVolume;
        public float TemplateVolume => templateVolume;
        public float WaterVolume => waterVolume;
        public float TotalVolume => supermixVolume + forwardPrimerVolume + reversePrimerVolume + templateVolume + waterVolume;
        public float FinalSupermixConcentration => supermixConcentration * supermixVolume / reactionVolume;
        public float ForwardPrimerNanomolar => primerStockMicromolar * 1000f * forwardPrimerVolume / reactionVolume;
        public float ReversePrimerNanomolar => primerStockMicromolar * 1000f * reversePrimerVolume / reactionVolume;

        public ReactionContents ForWell(WellType type) => type == WellType.Unused
            ? new ReactionContents(0, 0, 0, 0, 0)
            : new ReactionContents(supermixVolume, forwardPrimerVolume, reversePrimerVolume,
                type == WellType.NoTemplateControl ? 0 : templateVolume,
                type == WellType.NoTemplateControl ? waterVolume + templateVolume : waterVolume);

        public bool Validate(float expectedVolume, string expectedChemistry, out string issue)
        {
            issue = string.Empty;
            if (!string.Equals(chemistry, expectedChemistry, StringComparison.OrdinalIgnoreCase))
                issue = "The plate chemistry must match the SYBR Green protocol.";
            else if (!FinitePositive(reactionVolume) || !FinitePositive(supermixVolume) ||
                     !FinitePositive(forwardPrimerVolume) || !FinitePositive(reversePrimerVolume) ||
                     !FinitePositive(templateVolume) || !FinitePositive(primerStockMicromolar) ||
                     !FinitePositive(supermixConcentration) || float.IsNaN(waterVolume) || waterVolume < 0)
                issue = "Reaction volumes and stock concentrations must be finite and positive.";
            else if (Mathf.Abs(TotalVolume - expectedVolume) > 0.01f || Mathf.Abs(reactionVolume - expectedVolume) > 0.01f)
                issue = $"Each active well must contain {expectedVolume:0.#} µL in total.";
            else if (Mathf.Abs(FinalSupermixConcentration - 1f) > 0.01f)
                issue = "Dilute the 2× supermix to a final concentration of 1×.";
            else if (ForwardPrimerNanomolar < 299.9f || ForwardPrimerNanomolar > 500.1f ||
                     ReversePrimerNanomolar < 299.9f || ReversePrimerNanomolar > 500.1f)
                issue = "This teaching example requires 300–500 nM of each primer.";
            return issue.Length == 0;
        }

        private static bool FinitePositive(float value) => !float.IsNaN(value) && !float.IsInfinity(value) && value > 0;
    }

    public readonly struct ReactionContents
    {
        public readonly float Supermix, ForwardPrimer, ReversePrimer, Template, Water;
        public float Total => Supermix + ForwardPrimer + ReversePrimer + Template + Water;
        public ReactionContents(float mix, float forward, float reverse, float template, float water)
        { Supermix = mix; ForwardPrimer = forward; ReversePrimer = reverse; Template = template; Water = water; }
    }
}
