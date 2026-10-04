using System;
using System.Collections;
using UnityEngine;

namespace Team5.qPCR
{
    public sealed class PlateController : MonoBehaviour
    {
        [SerializeField] private ExperimentDefinition experiment;
        [SerializeField] private PlateHandoffData handoff;
        [SerializeField] private Transform wellsRoot;
        [SerializeField] private Renderer[] wellRenderers = Array.Empty<Renderer>();
        [SerializeField] private Renderer opticalSealRenderer;
        [SerializeField] private Transform orientationMarker;
        [SerializeField] private Color unusedWellColor = new Color(0.08f, 0.12f, 0.14f, 1f);

        private MaterialPropertyBlock propertyBlock;
        private Quaternion alignedRotation;

        public bool IsPrepared => handoff != null && handoff.IsReady && experiment != null && experiment.IsValid96WellPlate
            && experiment.ActiveReactionCount == handoff.ActiveReactionCount;
        public PlateHandoffData Handoff => handoff;
        public bool AllWellsLoaded => IsPrepared;
        public bool IsSealed => handoff != null && handoff.IsSealed;
        public bool IsInspected { get; private set; }
        public bool IsA1Aligned { get; private set; }
        public int WellCount => wellRenderers == null ? 0 : wellRenderers.Length;
        public int ActiveReactionCount => experiment == null ? 0 : experiment.ActiveReactionCount;

        public void Configure(
            ExperimentDefinition definition,
            Transform generatedWellsRoot,
            Renderer[] renderers,
            Renderer sealRenderer)
        {
            Configure(definition, null, generatedWellsRoot, renderers, sealRenderer, null);
        }

        public void Configure(
            ExperimentDefinition definition,
            PlateHandoffData handoffData,
            Transform generatedWellsRoot,
            Renderer[] renderers,
            Renderer sealRenderer,
            Transform marker)
        {
            experiment = definition;
            handoff = handoffData;
            wellsRoot = generatedWellsRoot;
            wellRenderers = renderers ?? Array.Empty<Renderer>();
            opticalSealRenderer = sealRenderer;
            orientationMarker = marker;
            alignedRotation = transform.localRotation;
        }

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
            alignedRotation = transform.localRotation;
            ResetPlate();
        }

        public bool InspectPlate()
        {
            if (!IsPrepared || !IsA1Aligned)
            {
                return false;
            }

            IsInspected = true;
            return true;
        }

        public void AlignA1()
        {
            IsA1Aligned = true;
            transform.localRotation = alignedRotation;
            if (orientationMarker != null)
            {
                orientationMarker.gameObject.SetActive(true);
            }
        }

        public void SetA1Aligned(bool aligned)
        {
            IsA1Aligned = aligned;
            transform.localRotation = aligned ? alignedRotation : alignedRotation * Quaternion.Euler(0f, 180f, 0f);
        }

        public void MarkOrientationFromPose()
        {
            IsA1Aligned = Quaternion.Angle(transform.localRotation, alignedRotation) <= 15f;
            if(!IsA1Aligned)IsInspected=false;
        }

        public void LoadAllWells(Action completed)
        {
            StopAllCoroutines();
            StartCoroutine(ShowPreparedPlateRoutine(completed));
        }

        public bool ApplySeal()
        {
            return IsSealed;
        }

        public void ResetPlate()
        {
            StopAllCoroutines();
            IsInspected = false;
            IsA1Aligned = false;
            transform.localRotation = alignedRotation * Quaternion.Euler(0f, 180f, 0f);

            if (opticalSealRenderer != null)
            {
                opticalSealRenderer.gameObject.SetActive(IsSealed);
            }

            if (orientationMarker != null)
            {
                orientationMarker.gameObject.SetActive(true);
            }

            ApplyPreparedWellColours();
        }

        private IEnumerator ShowPreparedPlateRoutine(Action completed)
        {
            ApplyPreparedWellColours();
            yield return new WaitForSeconds(0.25f);
            completed?.Invoke();
        }

        private void ApplyPreparedWellColours()
        {
            if (experiment == null || wellRenderers == null)
            {
                return;
            }

            for (var index = 0; index < wellRenderers.Length; index++)
            {
                var color = unusedWellColor;
                if (index < experiment.Wells.Count && experiment.Wells[index] != null)
                {
                    color = experiment.Wells[index].Type == WellType.Unused
                        ? unusedWellColor
                        : new Color(.76f, .86f, .90f, 1f);
                }

                SetWellColor(index, color);
            }
        }

        private void SetWellColor(int index, Color color)
        {
            if (wellRenderers == null || index < 0 || index >= wellRenderers.Length || wellRenderers[index] == null)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            propertyBlock.Clear();
            propertyBlock.SetColor("_BaseColor", color);
            propertyBlock.SetColor("_EmissionColor", Color.black);
            wellRenderers[index].SetPropertyBlock(propertyBlock);
        }
    }
}
