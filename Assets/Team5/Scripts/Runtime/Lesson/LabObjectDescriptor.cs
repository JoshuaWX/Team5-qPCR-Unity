using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Team5.qPCR
{
    public enum GuidanceVisualState
    {
        Idle,
        Current,
        Complete,
        Attention,
        Error
    }

    public sealed class LabObjectDescriptor : MonoBehaviour
    {
        private static LabObjectDescriptor hoveredDescriptor;

        [SerializeField] private string displayName;
        [SerializeField, TextArea(2, 4)] private string description;
        [SerializeField] private TrainingAction action;
        [SerializeField] private GuidedLessonController lesson;
        [SerializeField] private XRSimpleInteractable interactable;
        [SerializeField] private LineRenderer outline;
        [SerializeField] private Transform cueAnchor;
        [SerializeField] private CanvasGroup labelRoot;
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private bool activatesLessonAction = true;

        private bool isCurrent;
        private GuidanceVisualState visualState;

        public string DisplayName => displayName;
        public string Description => description;
        public TrainingAction Action => action;
        public Transform CueAnchor => cueAnchor == null ? transform : cueAnchor;
        public bool IsCurrent => isCurrent;

        public void Configure(string objectName, string objectDescription, TrainingAction trainingAction,
            GuidedLessonController controller, XRSimpleInteractable simpleInteractable, LineRenderer highlight,
            Transform anchor, CanvasGroup label, TMP_Text text)
        {
            Configure(objectName, objectDescription, trainingAction, controller, simpleInteractable,
                highlight, anchor, label, text, true);
        }

        public void Configure(string objectName, string objectDescription, TrainingAction trainingAction,
            GuidedLessonController controller, XRSimpleInteractable simpleInteractable, LineRenderer highlight,
            Transform anchor, CanvasGroup label, TMP_Text text, bool canActivateLessonAction)
        {
            displayName = objectName;
            description = objectDescription;
            action = trainingAction;
            lesson = controller;
            interactable = simpleInteractable;
            outline = highlight;
            cueAnchor = anchor;
            labelRoot = label;
            labelText = text;
            activatesLessonAction = canActivateLessonAction;
            if (labelText != null) labelText.text = displayName;
            SetVisualState(GuidanceVisualState.Idle);
        }

        private void OnEnable()
        {
            if (interactable == null) interactable = GetComponent<XRSimpleInteractable>();
            if (interactable == null) return;
            interactable.selectEntered.AddListener(OnSelected);
            interactable.hoverEntered.AddListener(OnHoverEntered);
            interactable.hoverExited.AddListener(OnHoverExited);
        }

        private void OnDisable()
        {
            if (interactable == null) return;
            interactable.selectEntered.RemoveListener(OnSelected);
            interactable.hoverEntered.RemoveListener(OnHoverEntered);
            interactable.hoverExited.RemoveListener(OnHoverExited);
        }

        private void LateUpdate()
        {
            if (labelRoot == null || !labelRoot.gameObject.activeSelf) return;
            var camera = Camera.main;
            if (camera == null) return;
            var labelTransform = labelRoot.transform;
            labelTransform.rotation = Quaternion.LookRotation(labelTransform.position - camera.transform.position, Vector3.up);
            var distance = Vector3.Distance(camera.transform.position, labelTransform.position);
            labelRoot.alpha = Mathf.InverseLerp(5.5f, 1.2f, distance);
        }

        private void OnMouseDown()
        {
            if (DesktopInputGuard.PointerOverUi) return;
            Activate(null);
        }

        private void OnSelected(SelectEnterEventArgs args) => Activate(args.interactorObject as XRBaseInputInteractor);

        private void Activate(XRBaseInputInteractor inputInteractor)
        {
            if (!activatesLessonAction) return;
            var accepted = lesson != null && lesson.TryPerformAction(action, this);
            inputInteractor?.SendHapticImpulse(accepted ? 0.32f : 0.72f, accepted ? 0.06f : 0.12f);
            if (!accepted) FlashError();
        }

        private void OnHoverEntered(HoverEnterEventArgs args)
        {
            if (hoveredDescriptor != null && hoveredDescriptor != this && !hoveredDescriptor.isCurrent)
                hoveredDescriptor.ShowLabel(false);
            hoveredDescriptor = this;
            ShowLabel(true);
            (args.interactorObject as XRBaseInputInteractor)?.SendHapticImpulse(0.08f, 0.025f);
        }

        private void OnHoverExited(HoverExitEventArgs args)
        {
            if (hoveredDescriptor == this) hoveredDescriptor = null;
            if (!isCurrent) ShowLabel(false);
        }

        public void SetCurrent(bool value)
        {
            isCurrent = value;
            if (value)
            {
                SetVisualState(GuidanceVisualState.Current);
                ShowLabel(true);
            }
            else if (visualState == GuidanceVisualState.Current || visualState == GuidanceVisualState.Attention)
            {
                SetVisualState(GuidanceVisualState.Idle);
                ShowLabel(hoveredDescriptor == this);
            }
        }

        public void MarkComplete()
        {
            isCurrent = false;
            SetVisualState(GuidanceVisualState.Complete);
            ShowLabel(false);
        }

        public void SetAttention(bool value)
        {
            if (!isCurrent) return;
            SetVisualState(value ? GuidanceVisualState.Attention : GuidanceVisualState.Current);
        }

        public void FlashError()
        {
            SetVisualState(GuidanceVisualState.Error);
            CancelInvoke(nameof(RestoreAfterError));
            Invoke(nameof(RestoreAfterError), 0.65f);
        }

        private void RestoreAfterError() => SetVisualState(isCurrent ? GuidanceVisualState.Current : GuidanceVisualState.Idle);

        private void ShowLabel(bool visible)
        {
            if (labelRoot == null) return;
            labelRoot.gameObject.SetActive(visible);
            labelRoot.alpha = visible ? 1f : 0f;
        }

        private void SetVisualState(GuidanceVisualState value)
        {
            visualState = value;
            if (outline == null) return;
            outline.enabled = value != GuidanceVisualState.Idle;
            var color = value switch
            {
                GuidanceVisualState.Complete => new Color(0.16f, 0.72f, 0.38f, 1f),
                GuidanceVisualState.Attention => new Color(1f, 0.66f, 0.12f, 1f),
                GuidanceVisualState.Error => new Color(0.94f, 0.18f, 0.22f, 1f),
                _ => new Color(0.12f, 0.42f, 0.92f, 1f)
            };
            outline.startColor = color;
            outline.endColor = color;
        }
    }
}
