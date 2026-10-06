using System.Linq;
using TMPro;
using UnityEngine;

namespace Team5.qPCR
{
    public sealed class GuidanceCueController : MonoBehaviour
    {
        [SerializeField] private Transform arrow;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private LabObjectDescriptor[] descriptors;
        private LabObjectDescriptor current;

        public LabObjectDescriptor CurrentTarget => current;

        public void Configure(Transform arrowTransform, TMP_Text hint, LabObjectDescriptor[] availableDescriptors)
        {
            arrow = arrowTransform;
            hintText = hint;
            descriptors = availableDescriptors ?? System.Array.Empty<LabObjectDescriptor>();
            HideAll();
        }

        public void SetTarget(TrainingAction action, bool showImmediately)
        {
            if (current != null) current.SetCurrent(false);
            current = descriptors.FirstOrDefault(item => item != null && item.Action == action);
            if (current != null)
            {
                current.SetCurrent(showImmediately);
                if (!showImmediately) current.SetCurrent(false);
            }
            if (arrow != null) arrow.gameObject.SetActive(false);
            if (hintText != null) hintText.gameObject.SetActive(false);
        }

        public void MarkActionComplete(TrainingAction action)
        {
            var descriptor = descriptors.FirstOrDefault(item => item != null && item.Action == action);
            descriptor?.MarkComplete();
            if (descriptor == current) current = null;
            if (arrow != null) arrow.gameObject.SetActive(false);
            if (hintText != null) hintText.gameObject.SetActive(false);
        }

        public void UpdateCue(float stepElapsed, bool guided, bool forcedHint, string instruction)
        {
            if (current == null)
            {
                if (arrow != null) arrow.gameObject.SetActive(false);
                return;
            }

            var pulse = guided && stepElapsed >= 12f || forcedHint;
            var strong = guided && stepElapsed >= 20f || forcedHint;
            current.SetCurrent(pulse || strong);
            current.SetAttention(strong);
            if (arrow != null)
            {
                arrow.gameObject.SetActive(strong);
                if (strong)
                {
                    arrow.position = current.CueAnchor.position + Vector3.up * (0.22f + Mathf.Sin(Time.unscaledTime * 4f) * 0.025f);
                    arrow.rotation = Quaternion.Euler(0f, Time.unscaledTime * 25f, 180f);
                }
            }

            if (hintText != null)
            {
                hintText.gameObject.SetActive(strong);
                if (strong) hintText.text = "Hint: " + instruction;
            }
        }

        public void HideAll()
        {
            current = null;
            if (descriptors != null)
                foreach (var descriptor in descriptors)
                    if (descriptor != null) descriptor.SetCurrent(false);
            if (arrow != null) arrow.gameObject.SetActive(false);
            if (hintText != null) hintText.gameObject.SetActive(false);
        }
    }
}
