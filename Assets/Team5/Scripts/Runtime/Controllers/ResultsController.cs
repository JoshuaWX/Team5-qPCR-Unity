using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Team5.qPCR
{
    public sealed class ResultsController : MonoBehaviour
    {
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private AmplificationCurveGraphic curveGraphic;
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private TMP_Text calloutText;
        [SerializeField] private AmplificationCurveGraphic secondaryCurveGraphic;
        [SerializeField] private TMP_Text secondarySummaryText;
        [SerializeField] private TMP_Text secondaryCalloutText;

        public IReadOnlyList<AssayResult> CurrentResults { get; private set; }
        public bool ControlsValid => CurrentResults != null &&
            CurrentResults.Any(x=>x.WellType==WellType.PositiveControl && x.IsAmplified) &&
            CurrentResults.Any(x=>x.WellType==WellType.NoTemplateControl && !x.IsAmplified);

        public void Configure(GameObject panel, AmplificationCurveGraphic graphic, TMP_Text summary, TMP_Text callout)
        {
            resultsPanel = panel;
            curveGraphic = graphic;
            summaryText = summary;
            calloutText = callout;
        }

        public void ConfigureSecondaryView(AmplificationCurveGraphic graphic, TMP_Text summary, TMP_Text callout)
        {
            secondaryCurveGraphic = graphic;
            secondarySummaryText = summary;
            secondaryCalloutText = callout;
        }

        public void ResetResults()
        {
            CurrentResults = null;
            curveGraphic?.ClearSeries();
            secondaryCurveGraphic?.ClearSeries();
            SetSummary("AWAITING RUN\n35 cycles • real-time fluorescence");
            SetCallout("Threshold 0.22 ΔRn\nPositive samples cross the threshold; valid NTC wells remain flat.");
        }

        public void BeginRun(IReadOnlyList<AssayResult> results)
        {
            CurrentResults = results;
            var curves = BuildRepresentativeCurves(results);
            curveGraphic?.SetSeries(curves);
            secondaryCurveGraphic?.SetSeries(curves);
            curveGraphic?.SetVisiblePointCount(1);
            secondaryCurveGraphic?.SetVisiblePointCount(1);
            SetSummary("RUNNING · CYCLE 01 / 35\nFluorescence is collected at 60°C");
            SetCallout("Threshold 0.22 ΔRn\nCurves appear one cycle at a time.");
        }

        public void UpdateVisibleCycle(int cycle)
        {
            curveGraphic?.SetVisiblePointCount(cycle + 1);
            secondaryCurveGraphic?.SetVisiblePointCount(cycle + 1);
            if (CurrentResults != null)
            {
                SetSummary($"RUNNING · CYCLE {cycle:00} / 35\nFluorescence is collected at 60°C");
            }
        }

        public void DisplayResults(IReadOnlyList<AssayResult> results)
        {
            CurrentResults = results;
            if (results == null)
            {
                return;
            }

            var positiveSamples = 0;
            var negativeSamples = 0;
            AssayResult positiveControl = null;
            AssayResult representativeSample = null;
            AssayResult noTemplateControl = null;

            for (var index = 0; index < results.Count; index++)
            {
                var result = results[index];
                if (result.WellType == WellType.PositiveControl && positiveControl == null)
                {
                    positiveControl = result;
                }
                else if (result.WellType == WellType.NoTemplateControl && noTemplateControl == null)
                {
                    noTemplateControl = result;
                }
                else if (result.Classification == AssayClassification.Positive)
                {
                    positiveSamples++;
                    if (representativeSample == null)
                    {
                        representativeSample = result;
                    }
                }
                else if (result.Classification == AssayClassification.Negative)
                {
                    negativeSamples++;
                }
            }

            var representativeCurves = BuildRepresentativeCurves(results);
            curveGraphic?.SetSeries(representativeCurves);
            secondaryCurveGraphic?.SetSeries(representativeCurves);
            curveGraphic?.SetVisiblePointCount(int.MaxValue);
            secondaryCurveGraphic?.SetVisiblePointCount(int.MaxValue);

            var controlsValid = positiveControl != null && positiveControl.IsAmplified && noTemplateControl != null && !noTemplateControl.IsAmplified;
            SetSummary($"Run complete · Controls {(controlsValid ? "passed" : "failed")}\n{positiveSamples:00} amplified samples   {negativeSamples:00} not amplified   02 controls");
            var cq = representativeSample == null ? "—" : representativeSample.Cq.ToString("0.0");
            var controlCq = positiveControl == null ? "—" : positiveControl.Cq.ToString("0.0");
            SetCallout($"Example sample Cq: {cq}   ·   Positive control Cq: {controlCq}   ·   NTC: {(noTemplateControl != null && !noTemplateControl.IsAmplified ? "flat" : "check required")}\nThreshold: 0.22 ΔRn. SYBR signal alone does not establish product identity.\nRepresentative teaching data, not a diagnostic cutoff or patient result.");
        }

        private static List<CurveSeries> BuildRepresentativeCurves(IReadOnlyList<AssayResult> results)
        {
            var curves = new List<CurveSeries>();
            if (results == null)
            {
                return curves;
            }

            AssayResult positiveControl = null;
            AssayResult representativeSample = null;
            AssayResult noTemplateControl = null;
            for (var index = 0; index < results.Count; index++)
            {
                var result = results[index];
                if (result.WellType == WellType.PositiveControl && positiveControl == null)
                {
                    positiveControl = result;
                }
                else if (result.WellType == WellType.NoTemplateControl && noTemplateControl == null)
                {
                    noTemplateControl = result;
                }
                else if (result.WellType == WellType.Sample && result.Classification == AssayClassification.Positive && representativeSample == null)
                {
                    representativeSample = result;
                }
            }

            AddCurve(curves, positiveControl, "Positive control", new Color(0.08f, 0.20f, 0.38f, 1f));
            AddCurve(curves, representativeSample, "Representative positive", new Color(0.11f, 0.37f, 0.81f, 1f));
            AddCurve(curves, noTemplateControl, "No-template control", new Color(0.54f, 0.27f, 0.15f, 1f));
            return curves;
        }

        private static void AddCurve(List<CurveSeries> curves, AssayResult result, string label, Color color)
        {
            if (result == null)
            {
                return;
            }

            curves.Add(new CurveSeries
            {
                Label = label,
                Color = color,
                Values = new List<float>(result.Fluorescence)
            });
        }

        private void SetSummary(string value)
        {
            if (summaryText != null) summaryText.text = value;
            if (secondarySummaryText != null) secondarySummaryText.text = value;
        }

        private void SetCallout(string value)
        {
            if (calloutText != null) calloutText.text = value;
            if (secondaryCalloutText != null) secondaryCalloutText.text = value;
        }
    }
}
