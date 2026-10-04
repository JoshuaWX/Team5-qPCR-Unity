using System;
using System.Collections;
using UnityEngine;

namespace Team5.qPCR
{
    public sealed class InstrumentController : MonoBehaviour
    {
        [SerializeField] private Transform plateTransform;
        [SerializeField] private Transform plateHomeAnchor;
        [SerializeField] private Transform instrumentPlateAnchor;
        [SerializeField] private Transform drawer;
        [SerializeField] private Renderer powerIndicator;
        [SerializeField] private Vector3 drawerOpenOffset = new Vector3(0f, 0f, -1.1f);

        private Vector3 drawerClosedPosition;

        public bool IsPowered { get; private set; }
        public bool IsPlateInserted { get; private set; }
        public bool IsDrawerClosed { get; private set; } = true;

        public void Configure(Transform plate, Transform home, Transform instrumentAnchor, Transform instrumentDrawer)
        {
            Configure(plate, home, instrumentAnchor, instrumentDrawer, null);
        }

        public void Configure(Transform plate, Transform home, Transform instrumentAnchor, Transform instrumentDrawer, Renderer indicator)
        {
            plateTransform = plate;
            plateHomeAnchor = home;
            instrumentPlateAnchor = instrumentAnchor;
            drawer = instrumentDrawer;
            powerIndicator = indicator;
            if (drawer != null)
            {
                drawerClosedPosition = drawer.localPosition;
            }
        }

        private void Awake()
        {
            if (drawer != null)
            {
                drawerClosedPosition = drawer.localPosition;
            }

            ResetInstrument();
        }

        public void PowerOn()
        {
            IsPowered = true;
            SetIndicator(new Color(0.12f, 1f, 0.72f));
        }

        public void InsertPlate(Action completed)
        {
            if (!IsPowered || IsPlateInserted || plateTransform == null || instrumentPlateAnchor == null)
            {
                completed?.Invoke();
                return;
            }

            StopAllCoroutines();
            StartCoroutine(InsertRoutine(completed));
        }

        public void ResetInstrument()
        {
            StopAllCoroutines();
            IsPowered = false;
            IsPlateInserted = false;
            IsDrawerClosed = true;
            SetIndicator(new Color(0.12f, 0.18f, 0.2f));

            if (drawer != null)
            {
                drawer.localPosition = drawerClosedPosition;
            }

            if (plateTransform != null && plateHomeAnchor != null)
            {
                plateTransform.SetParent(plateHomeAnchor, false);
                plateTransform.localPosition = Vector3.zero;
                plateTransform.localRotation = Quaternion.identity;
            }
        }

        private IEnumerator InsertRoutine(Action completed)
        {
            IsDrawerClosed = false;
            var openPosition = drawerClosedPosition + drawerOpenOffset;
            if (drawer != null)
            {
                yield return MoveLocal(drawer, drawerClosedPosition, openPosition, 0.55f);
            }

            var startPosition = plateTransform.position;
            var startRotation = plateTransform.rotation;
            var elapsed = 0f;
            const float moveDuration = 0.9f;
            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                var amount = Mathf.SmoothStep(0f, 1f, elapsed / moveDuration);
                plateTransform.position = Vector3.Lerp(startPosition, instrumentPlateAnchor.position, amount);
                plateTransform.rotation = Quaternion.Slerp(startRotation, instrumentPlateAnchor.rotation, amount);
                yield return null;
            }

            plateTransform.SetParent(instrumentPlateAnchor, true);
            plateTransform.position = instrumentPlateAnchor.position;
            plateTransform.rotation = instrumentPlateAnchor.rotation;

            if (drawer != null)
            {
                yield return MoveLocal(drawer, openPosition, drawerClosedPosition, 0.65f);
            }

            IsPlateInserted = true;
            IsDrawerClosed = true;
            completed?.Invoke();
        }

        private void SetIndicator(Color color)
        {
            if (powerIndicator == null)
            {
                return;
            }

            var block = new MaterialPropertyBlock();
            powerIndicator.GetPropertyBlock(block);
            block.SetColor("_BaseColor", color);
            block.SetColor("_EmissionColor", color * 0.6f);
            powerIndicator.SetPropertyBlock(block);
        }

        private static IEnumerator MoveLocal(Transform target, Vector3 from, Vector3 to, float duration)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                target.localPosition = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, elapsed / duration));
                yield return null;
            }

            target.localPosition = to;
        }
    }
}
