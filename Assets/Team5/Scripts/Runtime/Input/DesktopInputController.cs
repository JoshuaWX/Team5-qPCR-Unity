using UnityEngine;
using UnityEngine.InputSystem;

namespace Team5.qPCR
{
    public sealed class DesktopInputController : MonoBehaviour
    {
        [SerializeField] private WorkflowController workflow;
        [SerializeField] private LabShellController shell;

        private void Start() => shell = FindFirstObjectByType<LabShellController>();

        public void Configure(WorkflowController controller)
        {
            workflow = controller;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || workflow == null || DesktopInputGuard.IsEditingText ||
                (shell != null && shell.CurrentMode == InteractionMode.XR))
            {
                return;
            }

            if (!DesktopInputGuard.HasSelectedControl &&
                (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame))
            {
                workflow.HandlePrimaryAction();
            }
            else if (keyboard.fKey.wasPressedThisFrame)
            {
                workflow.HandleSecondaryAction();
            }
            else if (keyboard.rKey.wasPressedThisFrame)
            {
                workflow.ResetExperience();
            }
        }
    }
}
