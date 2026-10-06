using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Team5.qPCR
{
    public sealed class PhysicalControlInteractable : MonoBehaviour
    {
        [SerializeField] private TrainingAction action;
        [SerializeField] private GuidedLessonController lesson;
        [SerializeField] private XRSimpleInteractable interactable;
        [SerializeField] private Transform movingPart;
        [SerializeField] private LabObjectDescriptor descriptor;
        [SerializeField] private float pressDistance = 0.006f;
        private Vector3 restPosition;

        public TrainingAction Action => action;

        public void Configure(TrainingAction trainingAction, GuidedLessonController controller,
            XRSimpleInteractable simpleInteractable, Transform animatedPart)
        {
            Configure(trainingAction, controller, simpleInteractable, animatedPart, null);
        }

        public void Configure(TrainingAction trainingAction, GuidedLessonController controller,
            XRSimpleInteractable simpleInteractable, Transform animatedPart, LabObjectDescriptor objectDescriptor)
        {
            action = trainingAction;
            lesson = controller;
            interactable = simpleInteractable;
            movingPart = animatedPart == null ? transform : animatedPart;
            descriptor = objectDescriptor;
            restPosition = movingPart.localPosition;
        }

        private void Awake()
        {
            if (interactable == null) interactable = GetComponent<XRSimpleInteractable>();
            if (movingPart == null) movingPart = transform;
            restPosition = movingPart.localPosition;
        }

        private void OnEnable()
        {
            interactable?.selectEntered.AddListener(OnSelected);
        }

        private void OnDisable()
        {
            interactable?.selectEntered.RemoveListener(OnSelected);
        }

        private void OnMouseDown()
        {
            if (!DesktopInputGuard.PointerOverUi) Activate(null);
        }

        private void OnSelected(SelectEnterEventArgs args) => Activate(args.interactorObject as XRBaseInputInteractor);

        private void Activate(XRBaseInputInteractor inputInteractor)
        {
            var accepted = lesson != null && lesson.TryPerformAction(action, null);
            if (!accepted) descriptor?.FlashError();
            inputInteractor?.SendHapticImpulse(accepted ? 0.4f : 0.75f, accepted ? 0.07f : 0.13f);
            StopAllCoroutines();
            StartCoroutine(PressAnimation(accepted));
        }

        private IEnumerator PressAnimation(bool accepted)
        {
            var down = restPosition + Vector3.back * pressDistance;
            var duration = accepted ? 0.08f : 0.16f;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                movingPart.localPosition = Vector3.Lerp(restPosition, down, Mathf.Sin(Mathf.Clamp01(elapsed / duration) * Mathf.PI));
                yield return null;
            }
            movingPart.localPosition = restPosition;
        }
    }
}
