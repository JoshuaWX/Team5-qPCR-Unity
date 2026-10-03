using System;
using System.Collections;
using UnityEngine;

namespace Team5.qPCR
{
    public sealed class PlateController : MonoBehaviour
    {
        [SerializeField] private ExperimentDefinition experiment;
        [SerializeField] private Transform wellsRoot;
        [SerializeField] private Renderer[] wellRenderers = Array.Empty<Renderer>();
        [SerializeField] private Renderer opticalSealRenderer;
        [SerializeField] private Color emptyWellColor = new Color(0.12f, 0.18f, 0.22f, 1f);

        private MaterialPropertyBlock propertyBlock;

        public bool AllWellsLoaded { get; private set; }
        public bool IsSealed { get; private set; }
        public int WellCount => wellRenderers == null ? 0 : wellRenderers.Length;

        public void Configure(
            ExperimentDefinition definition,
            Transform generatedWellsRoot,
            Renderer[] renderers,
            Renderer sealRenderer)
        {
            experiment = definition;
            wellsRoot = generatedWellsRoot;
            wellRenderers = renderers ?? Array.Empty<Renderer>();
            opticalSealRenderer = sealRenderer;
        }

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
            ResetPlate();
        }

        public void LoadAllWells(Action completed)
        {
            StopAllCoroutines();
            StartCoroutine(LoadRoutine(completed));
        }

        public bool ApplySeal()
        {
            if (!AllWellsLoaded)
            {
                return false;
            }

            IsSealed = true;
            if (opticalSealRenderer != null)
            {
                opticalSealRenderer.gameObject.SetActive(true);
            }

            return true;
        }

        public void ResetPlate()
        {
            StopAllCoroutines();
            AllWellsLoaded = false;
            IsSealed = false;

            if (opticalSealRenderer != null)
            {
                opticalSealRenderer.gameObject.SetActive(false);
            }

            if (wellRenderers == null)
            {
                return;
            }

            for (var index = 0; index < wellRenderers.Length; index++)
            {
                SetWellColor(index, emptyWellColor);
            }
        }

        private IEnumerator LoadRoutine(Action completed)
        {
            if (experiment == null || !experiment.IsValid96WellPlate || wellRenderers.Length != 96)
            {
                Debug.LogError("[Team 5] A valid 96-well experiment and 96 renderers are required.");
                yield break;
            }

            for (var index = 0; index < wellRenderers.Length; index++)
            {
                SetWellColor(index, experiment.Wells[index].DisplayColor);
                if (index % 6 == 5)
                {
                    yield return new WaitForSeconds(0.045f);
                }
            }

            AllWellsLoaded = true;
            completed?.Invoke();
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
            propertyBlock.SetColor("_EmissionColor", color * 0.18f);
            wellRenderers[index].SetPropertyBlock(propertyBlock);
        }
    }
}
