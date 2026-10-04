using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Team5.qPCR
{
    [Serializable]
    public sealed class CurveSeries
    {
        public string Label;
        public Color Color;
        public List<float> Values = new List<float>();
    }

    public sealed class AmplificationCurveGraphic : MaskableGraphic
    {
        [SerializeField] private List<CurveSeries> series = new List<CurveSeries>();
        [SerializeField] private Color gridColor = new Color(0.25f, 0.43f, 0.48f, 0.28f);
        [SerializeField] private Color thresholdColor = new Color(0.96f, 0.77f, 0.25f, 0.75f);
        [SerializeField, Min(0.5f)] private float lineWidth = 2.5f;
        [SerializeField] private Vector4 padding = new Vector4(44f, 24f, 24f, 34f);
        [SerializeField] private int visiblePointCount = int.MaxValue;

        public void SetSeries(IEnumerable<CurveSeries> curves)
        {
            series = curves == null ? new List<CurveSeries>() : new List<CurveSeries>(curves);
            SetVerticesDirty();
        }

        public void SetVisiblePointCount(int count)
        {
            visiblePointCount = Mathf.Max(1, count);
            SetVerticesDirty();
        }

        public void ClearSeries()
        {
            series.Clear();
            visiblePointCount = int.MaxValue;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();
            var rect = rectTransform.rect;
            var plot = new Rect(
                rect.xMin + padding.x,
                rect.yMin + padding.w,
                Mathf.Max(1f, rect.width - padding.x - padding.z),
                Mathf.Max(1f, rect.height - padding.y - padding.w));

            for (var index = 0; index <= 8; index++)
            {
                var x = Mathf.Lerp(plot.xMin, plot.xMax, index / 8f);
                AddLine(vertexHelper, new Vector2(x, plot.yMin), new Vector2(x, plot.yMax), 1f, gridColor);
            }

            for (var index = 0; index <= 4; index++)
            {
                var y = Mathf.Lerp(plot.yMin, plot.yMax, index / 4f);
                AddLine(vertexHelper, new Vector2(plot.xMin, y), new Vector2(plot.xMax, y), 1f, gridColor);
            }

            var thresholdY = Mathf.Lerp(plot.yMin, plot.yMax, AssayResultGenerator.DetectionThreshold);
            AddLine(vertexHelper, new Vector2(plot.xMin, thresholdY), new Vector2(plot.xMax, thresholdY), 1.4f, thresholdColor);

            for (var seriesIndex = 0; seriesIndex < series.Count; seriesIndex++)
            {
                var curve = series[seriesIndex];
                if (curve == null || curve.Values == null || curve.Values.Count < 2)
                {
                    continue;
                }

                var pointsToDraw = Mathf.Min(curve.Values.Count, visiblePointCount);
                for (var pointIndex = 1; pointIndex < pointsToDraw; pointIndex++)
                {
                    var previous = new Vector2(
                        Mathf.Lerp(plot.xMin, plot.xMax, (pointIndex - 1f) / (curve.Values.Count - 1f)),
                        Mathf.Lerp(plot.yMin, plot.yMax, Mathf.Clamp01(curve.Values[pointIndex - 1])));
                    var current = new Vector2(
                        Mathf.Lerp(plot.xMin, plot.xMax, pointIndex / (curve.Values.Count - 1f)),
                        Mathf.Lerp(plot.yMin, plot.yMax, Mathf.Clamp01(curve.Values[pointIndex])));
                    AddLine(vertexHelper, previous, current, lineWidth, curve.Color);
                }
            }
        }

        private static void AddLine(VertexHelper helper, Vector2 start, Vector2 end, float width, Color color)
        {
            var direction = (end - start).normalized;
            var normal = new Vector2(-direction.y, direction.x) * (width * 0.5f);
            var vertexIndex = helper.currentVertCount;

            helper.AddVert(start - normal, color, Vector2.zero);
            helper.AddVert(start + normal, color, Vector2.zero);
            helper.AddVert(end + normal, color, Vector2.zero);
            helper.AddVert(end - normal, color, Vector2.zero);
            helper.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 2);
            helper.AddTriangle(vertexIndex, vertexIndex + 2, vertexIndex + 3);
        }
    }
}
