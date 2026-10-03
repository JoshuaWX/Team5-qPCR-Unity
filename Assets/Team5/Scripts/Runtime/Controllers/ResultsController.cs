using System.Collections.Generic;
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

        public IReadOnlyList<AssayResult> CurrentResults { get; private set; }

        public void Configure(GameObject panel, AmplificationCurveGraphic graphic, TMP_Text summary, TMP_Text callout)
        {
            resultsPanel = panel;
            curveGraphic = graphic;
            summaryText = summary;
            calloutText = callout;
        }

        public void ResetResults()
        {
            CurrentResults = null;
            curveGraphic?.ClearSeries();
            if (summaryText != null)
            {
                summaryText.text = "AWAITING RUN\n40 cycles • real-time fluorescence";
            }

            if (calloutText != null)
            {
                calloutText.text = "Threshold 0.22 ΔRn\nPositive samples cross the threshold; valid NTC wells remain flat.";
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

            var curves = new List<CurveSeries>();
            AddCurve(curves, positiveControl, "Positive control", new Color(0.21f, 0.94f, 0.74f, 1f));
            AddCurve(curves, representativeSample, "Representative positive", new Color(0.26f, 0.72f, 1f, 1f));
            AddCurve(curves, noTemplateControl, "No-template control", new Color(0.95f, 0.48f, 0.55f, 1f));
            curveGraphic?.SetSeries(curves);

            if (summaryText != null)
            {
                summaryText.text = $"RUN COMPLETE  •  CONTROLS VALID\n{positiveSamples:00} positive samples   {negativeSamples:00} negative samples   04 controls";
            }

            if (calloutText != null)
            {
                var cq = representativeSample == null ? "—" : representativeSample.Cq.ToString("0.0");
                calloutText.text = $"Representative sample Cq: {cq}\nNTC: no amplification detected\nEducational simulation — not for diagnostic use.";
            }
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
    }
}
