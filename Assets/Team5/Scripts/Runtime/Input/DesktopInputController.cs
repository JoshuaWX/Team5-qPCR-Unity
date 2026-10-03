using UnityEngine;
using UnityEngine.InputSystem;

namespace Team5.qPCR
{
    public sealed class DesktopInputController : MonoBehaviour
    {
        [SerializeField] private WorkflowController workflow;

        public void Configure(WorkflowController controller)
        {
            workflow = controller;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || workflow == null)
            {
                return;
            }

            if (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame)
            {
                workflow.HandlePrimaryAction();
            }
            else if (keyboard.rKey.wasPressedThisFrame)
            {
                workflow.ResetExperience();
            }
        }
    }
}
