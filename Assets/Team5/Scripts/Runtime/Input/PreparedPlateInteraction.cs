using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Team5.qPCR
{
    /// <summary>Selection feedback and XR preparation gate; scientific progress still belongs to WorkflowController.</summary>
    public sealed class PreparedPlateInteraction : MonoBehaviour
    {
        [SerializeField] private WorkflowController workflow;
        [SerializeField] private XRGrabInteractable grab;
        [SerializeField] private LineRenderer outline;
        private PlateController plate;
        private void Awake() => plate = GetComponent<PlateController>();
        public void Configure(WorkflowController flow, XRGrabInteractable interactable, LineRenderer highlight)
        { workflow=flow;grab=interactable;outline=highlight; }
        private void Update()
        {
            if(workflow==null||grab==null)return;
            var allowed=!workflow.IsBusy && (workflow.CurrentStage==WorkflowStage.PlateInspection || workflow.CurrentStage==WorkflowStage.InstrumentLoading);
            if(grab.enabled!=allowed)grab.enabled=allowed;
            var hover=grab.isHovered;
            if(!DesktopInputGuard.PointerOverUi && Mouse.current!=null && Camera.main!=null &&
                Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()),out var hit,25))
                hover|=hit.transform.IsChildOf(transform);
            if(outline!=null)outline.enabled=hover || workflow.CurrentStage==WorkflowStage.PlateInspection;
            // A grabbed plate must be put down with its A1 corner facing the prepared station.
            if(allowed && grab.isSelected && plate!=null)plate.MarkOrientationFromPose();
        }
    }
}
