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
        [SerializeField] private Vector3 drawerOpenOffset = new Vector3(0f, 0f, -1.1f);

        private Vector3 drawerClosedPosition;

        public bool IsPlateInserted { get; private set; }

        public void Configure(Transform plate, Transform home, Transform instrumentAnchor, Transform instrumentDrawer)
        {
            plateTransform = plate;
            plateHomeAnchor = home;
            instrumentPlateAnchor = instrumentAnchor;
            drawer = instrumentDrawer;
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

        public void InsertPlate(Action completed)
        {
            if (IsPlateInserted || plateTransform == null || instrumentPlateAnchor == null)
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
            IsPlateInserted = false;

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
            var openPosition = drawerClosedPosition + drawerOpenOffset;
            if (drawer != null)
            {
                yield return MoveLocal(drawer, drawerClosedPosition, openPosition, 0.45f);
            }

            var startPosition = plateTransform.position;
            var startRotation = plateTransform.rotation;
            var elapsed = 0f;
            const float moveDuration = 0.75f;
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
                yield return MoveLocal(drawer, openPosition, drawerClosedPosition, 0.55f);
            }

            IsPlateInserted = true;
            completed?.Invoke();
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
