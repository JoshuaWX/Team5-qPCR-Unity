using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Team5.qPCR
{
    public enum CameraMode { Guided, FirstPerson, ThirdPerson }
    public enum CameraFocus { Overview, Touchscreen, Plate, Drawer, Monitor }

    /// <summary>Only this component owns the desktop camera. It never writes an XR pose.</summary>
    public sealed class LabCameraDirector : MonoBehaviour
    {
        [SerializeField] private WorkflowController workflow;
        [SerializeField] private DialogueSequence dialogueSequence;
        [SerializeField] private LabShellController shell;
        [SerializeField] private Camera desktopCamera;
        [SerializeField] private CharacterController character;
        [SerializeField] private GameObject scientistVisual;
        [SerializeField] private Animation scientistAnimation;
        [SerializeField] private Transform[] focusTargets = new Transform[5];
        [SerializeField] private CanvasGroup transitionFade;
        [SerializeField] private bool reducedMotion;
        [SerializeField, Range(0.5f, 2f)] private float zoomSensitivity = 1f;
        private Vector3 spawn, focus, startPosition;
        private Quaternion spawnRotation, startRotation;
        private float yaw, pitch, radius, desiredRadius, blendTime, gravity;
        private float walkYaw, walkPitch = 12f, thirdPersonRadius = 2.4f;
        private bool transitioning, dialogueOpen = true;
        private WorkflowStage lastStage = (WorkflowStage)(-1);
        private CameraFocus currentFocus;
        private string currentAnimation;
        public event Action<CameraMode> ModeChanged;
        public CameraMode Mode { get; private set; }
        public bool IsTransitioning => transitioning;
        public int TransitionCount { get; private set; }
        public float Distance => desiredRadius;
        public bool ReducedMotion { get => reducedMotion; set => reducedMotion = value; }
        public float ZoomSensitivity { get => zoomSensitivity; set => zoomSensitivity = Mathf.Clamp(value, .5f, 2f); }
        public bool IsOverview => Mode == CameraMode.Guided && currentFocus == CameraFocus.Overview;
        public bool ModalOpen { get; set; }
        public void SetDialogueSequence(DialogueSequence sequence) => dialogueSequence = sequence;

        public void Configure(WorkflowController flow, LabShellController labShell, Camera camera,
            CharacterController player, GameObject visual, Animation animations, Transform[] targets, CanvasGroup fade)
        {
            workflow = flow; shell = labShell; desktopCamera = camera; character = player;
            scientistVisual = visual; scientistAnimation = animations; focusTargets = targets; transitionFade = fade;
        }

        private void Awake()
        {
            if (character != null) { spawn = character.transform.position; spawnRotation = character.transform.rotation; }
            var oldOrbit = GetComponent<CameraOrbitController>();
            if (oldOrbit != null) oldOrbit.enabled = false;
        }
        private void OnEnable()
        {
            if (workflow != null) { workflow.StageChanged += OnStage; workflow.ExperienceReset += ResetView; }
        }
        private void OnDisable()
        {
            if (workflow != null) { workflow.StageChanged -= OnStage; workflow.ExperienceReset -= ResetView; }
            ReleaseCursor();
        }
        private void Start() => ResetView();

        public void ResetView()
        {
            if (character != null)
            {
                character.enabled = false;
                character.transform.SetPositionAndRotation(spawn, spawnRotation);
                character.enabled = true;
            }
            walkYaw = spawnRotation.eulerAngles.y; walkPitch = 12f; gravity = 0f; thirdPersonRadius = 2.4f;
            lastStage = (WorkflowStage)(-1); dialogueOpen = true; ModalOpen = false;
            SetMode(CameraMode.Guided);
            FocusStage(WorkflowStage.Introduction, true);
            transitioning = false; ApplyGuided(1f);
            if (transitionFade != null) transitionFade.alpha = 0;
        }

        public void SetMode(CameraMode mode)
        {
            Mode = mode; transitioning = false; dialogueOpen = true; ReleaseCursor();
            if (scientistVisual != null) scientistVisual.SetActive(mode == CameraMode.ThirdPerson);
            if (mode == CameraMode.Guided) FocusStage(workflow == null ? WorkflowStage.Introduction : workflow.CurrentStage, true);
            else { lastStage = (WorkflowStage)(-1); shell?.SetDesktopOverview(false); }
            ModeChanged?.Invoke(mode);
        }
        public void Guided() => SetMode(CameraMode.Guided);
        public void FirstPerson() => SetMode(CameraMode.FirstPerson);
        public void ThirdPerson() => SetMode(CameraMode.ThirdPerson);
        public void ReturnToStep() { SetMode(CameraMode.Guided); }
        public void ToggleReducedMotion() => ReducedMotion = !ReducedMotion;
        public void SetZoomSensitivity(float value) => ZoomSensitivity = value;
        public void SetDialogueOpen(bool open) { dialogueOpen = open; if (open) ReleaseCursor(); }

        public void FocusStage(WorkflowStage stage, bool force = false)
        {
            if (!force && stage == lastStage) return;
            lastStage = stage;
            if (Mode != CameraMode.Guided) return;
            var target = dialogueSequence != null ? dialogueSequence.For(stage).Focus : FocusForStage(stage);
            FocusOn(target);
        }

        public static CameraFocus FocusForStage(WorkflowStage stage)
        {
            switch (stage)
            {
                case WorkflowStage.Introduction: case WorkflowStage.Complete: return CameraFocus.Overview;
                case WorkflowStage.HandoffReview: case WorkflowStage.PlateInspection: return CameraFocus.Plate;
                case WorkflowStage.PowerOn: case WorkflowStage.ProtocolSetup: return CameraFocus.Touchscreen;
                case WorkflowStage.InstrumentLoading: case WorkflowStage.RunValidation: return CameraFocus.Drawer;
                default: return CameraFocus.Monitor;
            }
        }

        public void FocusOn(CameraFocus target)
        {
            if (Mode != CameraMode.Guided || desktopCamera == null) return;
            currentFocus = target;
            focus = focusTargets != null && (int)target < focusTargets.Length && focusTargets[(int)target] != null
                ? focusTargets[(int)target].position : new Vector3(0, 1, 0);
            yaw = target == CameraFocus.Overview ? -12 : -8;
            pitch = target == CameraFocus.Overview ? 34 : target == CameraFocus.Plate ? 52 : target == CameraFocus.Drawer ? 27 : 12;
            desiredRadius = target == CameraFocus.Overview ? 14 : target == CameraFocus.Plate ? .38f : target == CameraFocus.Drawer ? 1.05f : .9f;
            radius = desiredRadius;
            startPosition = desktopCamera.transform.position; startRotation = desktopCamera.transform.rotation;
            blendTime = 0; transitioning = true; TransitionCount++;
            shell?.SetDesktopOverview(IsOverview);
        }

        private void OnStage(WorkflowStage stage) => FocusStage(stage);
        public static float ZoomDistance(float distance, float notches, float sensitivity, float min, float max)
            => Mathf.Clamp(distance * Mathf.Pow(.88f, Mathf.Clamp(notches, -10, 10) * sensitivity), min, max);

        private void Update()
        {
            if (shell != null && shell.CurrentMode == InteractionMode.XR) return;
            var keyboard = Keyboard.current;
            if (keyboard == null || DesktopInputGuard.IsEditingText) return;
            if (keyboard.escapeKey.wasPressedThisFrame) { ReleaseCursor(); dialogueOpen = true; }
            if (keyboard.digit1Key.wasPressedThisFrame) Guided();
            if (keyboard.digit2Key.wasPressedThisFrame) FirstPerson();
            if (keyboard.digit3Key.wasPressedThisFrame) ThirdPerson();
            if (keyboard.tabKey.wasPressedThisFrame && Mode != CameraMode.Guided && !ModalOpen)
            {
                dialogueOpen = !dialogueOpen;
                Cursor.lockState = dialogueOpen ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = dialogueOpen;
                if (!dialogueOpen)
                    foreach (var mentor in FindObjectsByType<MentorPanelController>(FindObjectsSortMode.None)) mentor.SetMinimized(true);
            }
        }

        private void LateUpdate()
        {
            if (desktopCamera == null || (shell != null && shell.CurrentMode == InteractionMode.XR)) return;
            var mouse = Mouse.current;
            if (Mode == CameraMode.Guided)
            {
                if (mouse != null && !DesktopInputGuard.PointerOverUi && !DesktopInputGuard.IsEditingText)
                {
                    var delta = mouse.delta.ReadValue();
                    var scroll = mouse.scroll.ReadValue().y;
                    // UniformAcrossAllPlatforms yields one unit per notch; do not divide by 120.
                    if (mouse.rightButton.isPressed && delta.sqrMagnitude > 0)
                    { InterruptTravel(); yaw += delta.x * .14f; pitch = Mathf.Clamp(pitch - delta.y * .14f, -15, 80); }
                    if (Mathf.Abs(scroll) > .001f)
                    { InterruptTravel(); desiredRadius = ZoomDistance(desiredRadius, scroll, zoomSensitivity, currentFocus == CameraFocus.Overview ? 2f : .18f, 18f); }
                }
                radius = Mathf.Lerp(radius, desiredRadius, 1 - Mathf.Exp(-Time.unscaledDeltaTime * 20));
                ApplyGuided(Time.unscaledDeltaTime);
                AnimateScientist(false);
            }
            else Walk(mouse);
        }

        public void InterruptTravel()
        {
            if (!transitioning || desktopCamera == null) return;
            // Adopt the current pose instead of snapping to the unfinished shot's endpoint.
            var offset=focus-desktopCamera.transform.position;
            radius=desiredRadius=Mathf.Max(.18f,offset.magnitude);
            var angles=Quaternion.LookRotation(offset.normalized).eulerAngles;
            yaw=angles.y;pitch=angles.x>180?angles.x-360:angles.x;
            transitioning=false;if(transitionFade!=null)transitionFade.alpha=0;
        }

        private void ApplyGuided(float delta)
        {
            var rotation = Quaternion.Euler(pitch, yaw, 0);
            var position = focus - rotation * Vector3.forward * radius;
            if (transitioning)
            {
                blendTime += delta;
                var length = reducedMotion ? .24f : 1.2f;
                var t = Mathf.Clamp01(blendTime / length);
                if (reducedMotion)
                {
                    if (transitionFade != null) transitionFade.alpha = 1 - Mathf.Abs(2 * t - 1);
                    if (t >= .5f) desktopCamera.transform.SetPositionAndRotation(position, rotation);
                }
                else desktopCamera.transform.SetPositionAndRotation(Vector3.Lerp(startPosition, position, Mathf.SmoothStep(0, 1, t)),
                    Quaternion.Slerp(startRotation, rotation, Mathf.SmoothStep(0, 1, t)));
                if (t >= 1) transitioning = false;
            }
            else
            {
                desktopCamera.transform.SetPositionAndRotation(position, rotation);
                if (transitionFade != null) transitionFade.alpha = 0;
            }
        }

        private void Walk(Mouse mouse)
        {
            if (character == null) return;
            var canWalk = !dialogueOpen && !ModalOpen && !DesktopInputGuard.IsEditingText && Cursor.lockState == CursorLockMode.Locked;
            if (canWalk && mouse != null && Cursor.lockState == CursorLockMode.Locked)
            {
                var delta = mouse.delta.ReadValue();
                walkYaw += delta.x * .12f; walkPitch = Mathf.Clamp(walkPitch - delta.y * .12f, -60, 65);
                if (Mode == CameraMode.ThirdPerson)
                    thirdPersonRadius = ZoomDistance(thirdPersonRadius, mouse.scroll.ReadValue().y, zoomSensitivity, 1.2f, 4f);
            }
            var input = Vector2.zero;
            var keyboard = Keyboard.current;
            if (canWalk && keyboard != null)
                input = new Vector2((keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0),
                    (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0)).normalized;
            var facing = Quaternion.Euler(0, walkYaw, 0);
            var movement = facing * new Vector3(input.x, 0, input.y) * 1.65f;
            gravity = character.isGrounded ? -1 : Mathf.Max(gravity - 9.81f * Time.deltaTime, -20);
            if (character.enabled) character.Move((movement + Vector3.up * gravity) * Time.deltaTime);
            if (movement.sqrMagnitude > .01f) character.transform.rotation = Quaternion.Slerp(character.transform.rotation,
                Quaternion.LookRotation(movement), 1 - Mathf.Exp(-Time.deltaTime * 12));
            AnimateScientist(movement.sqrMagnitude > .01f);
            var rotation = Quaternion.Euler(walkPitch, walkYaw, 0);
            var eye = character.transform.position + Vector3.up * 1.64f;
            if (Mode == CameraMode.FirstPerson) desktopCamera.transform.SetPositionAndRotation(eye, rotation);
            else
            {
                var desired = eye + rotation * new Vector3(.3f, .08f, -thirdPersonRadius);
                var direction = desired - eye;
                var distance = direction.magnitude;
                foreach (var hit in Physics.SphereCastAll(eye, .14f, direction.normalized, distance, ~0, QueryTriggerInteraction.Ignore))
                    if (!hit.transform.IsChildOf(character.transform)) distance = Mathf.Min(distance, Mathf.Max(.15f, hit.distance - .03f));
                desktopCamera.transform.SetPositionAndRotation(eye + direction.normalized * distance, rotation);
            }
        }

        private void AnimateScientist(bool walking)
        {
            if (scientistAnimation == null || !scientistAnimation.gameObject.activeInHierarchy) return;
            var clip = workflow != null && workflow.IsBusy && workflow.CurrentStage == WorkflowStage.InstrumentLoading
                ? "Reach" : walking ? "Walk" : "Idle";
            if (clip == currentAnimation || scientistAnimation[clip] == null) return;
            scientistAnimation.CrossFade(clip, .18f); currentAnimation = clip;
        }
        private static void ReleaseCursor() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
    }
}
