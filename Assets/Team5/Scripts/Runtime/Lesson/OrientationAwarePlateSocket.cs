using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Team5.qPCR
{
    public sealed class OrientationAwarePlateSocket : XRSocketInteractor
    {
        [SerializeField] private PlateController plate;
        [SerializeField] private InstrumentController instrument;
        [SerializeField] private GuidedLessonController lesson;
        [SerializeField, Range(5f, 45f)] private float orientationTolerance = 18f;
        [SerializeField] private LabObjectDescriptor descriptor;

        public float OrientationTolerance => orientationTolerance;
        public bool IsPlateSeated => hasSelection && instrument != null && instrument.IsPlateInserted;

        public void Configure(PlateController plateController, InstrumentController instrumentController,
            GuidedLessonController lessonController, Transform socketAttach, LabObjectDescriptor targetDescriptor)
        {
            plate = plateController;
            instrument = instrumentController;
            lesson = lessonController;
            attachTransform = socketAttach;
            descriptor = targetDescriptor;
            socketScaleMode = SocketScaleMode.None;
            showInteractableHoverMeshes = true;
            recycleDelayTime = 0.2f;
            keepSelectedTargetValid = true;
        }

        public override bool CanSelect(IXRSelectInteractable interactable)
        {
            // Loading advances the lesson immediately. Do not invalidate the existing socket
            // selection merely because the learner is now closing the drawer or running PCR.
            if (IsPlateSeated && firstInteractableSelected == interactable)
                return base.CanSelect(interactable);
            return base.CanSelect(interactable) && IsExpectedPlate(interactable) && IsReady() && IsOrientationValid(interactable.transform);
        }

        protected override void OnHoverEntered(HoverEnterEventArgs args)
        {
            base.OnHoverEntered(args);
            if (!IsExpectedPlate(args.interactableObject) || lesson == null || lesson.CurrentAction != TrainingAction.SeatPlate)
                return;
            if (!IsReady() || !IsOrientationValid(args.interactableObject.transform))
            {
                descriptor?.FlashError();
                lesson.ReportRejectedAction(TrainingAction.SeatPlate);
                (args.interactorObject as XRBaseInputInteractor)?.SendHapticImpulse(0.78f, 0.14f);
            }
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            if (!IsExpectedPlate(args.interactableObject) || instrument == null) return;
            plate.SetA1Aligned(true);
            if (instrument.ConfirmPlateSocketed(args.interactableObject.transform))
            {
                lesson?.TryPerformAction(TrainingAction.SeatPlate, descriptor);
                (args.interactorObject as XRBaseInputInteractor)?.SendHapticImpulse(0.48f, 0.10f);
            }
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);
            if (instrument != null && !instrument.IsDrawerClosed)
                instrument.MarkPlateRemoved();
        }

        public bool IsOrientationValid(Transform candidate)
        {
            if (candidate == null || attachTransform == null) return false;
            return Quaternion.Angle(candidate.rotation, attachTransform.rotation) <= orientationTolerance;
        }

        public void ResetSocket()
        {
            if (interactionManager != null && hasSelection && firstInteractableSelected != null)
                interactionManager.SelectExit(this, firstInteractableSelected);
            socketActive = true;
        }

        private bool IsExpectedPlate(IXRInteractable interactable)
        {
            return plate != null && interactable != null && interactable.transform == plate.transform;
        }

        private bool IsReady()
        {
            return instrument != null && instrument.IsPowered && instrument.IsDrawerOpen && !instrument.IsMoving &&
                lesson != null && lesson.CurrentAction == TrainingAction.SeatPlate;
        }
    }
}
