using TMPro;
using UnityEngine;

namespace Team5.qPCR
{
    public sealed class DnaCycleVisualizer : MonoBehaviour
    {
        [SerializeField] private GameObject denaturationVisual;
        [SerializeField] private GameObject annealingVisual;
        [SerializeField] private GameObject extensionVisual;
        [SerializeField] private TMP_Text phaseLabel;

        public void Configure(GameObject denaturation, GameObject annealing, GameObject extension, TMP_Text label)
        {
            denaturationVisual = denaturation;
            annealingVisual = annealing;
            extensionVisual = extension;
            phaseLabel = label;
            ResetVisual();
        }

        public void SetPhase(float normalizedCyclePhase)
        {
            if (normalizedCyclePhase < 0.34f)
            {
                Show(denaturationVisual, "DENATURATION · 95°C\nDNA strands separate");
            }
            else if (normalizedCyclePhase < 0.67f)
            {
                Show(annealingVisual, "ANNEALING · 60°C\nPrimers attach · fluorescence read");
            }
            else
            {
                Show(extensionVisual, "EXTENSION · 72°C\nNew DNA strands are built");
            }
        }

        public void ResetVisual()
        {
            SetActive(denaturationVisual, false);
            SetActive(annealingVisual, false);
            SetActive(extensionVisual, false);
            if (phaseLabel != null)
            {
                phaseLabel.text = "THERMAL CYCLE VISUALIZER\nWaiting for run";
            }
        }

        private void Show(GameObject active, string label)
        {
            SetActive(denaturationVisual, active == denaturationVisual);
            SetActive(annealingVisual, active == annealingVisual);
            SetActive(extensionVisual, active == extensionVisual);
            if (phaseLabel != null)
            {
                phaseLabel.text = label;
            }
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
