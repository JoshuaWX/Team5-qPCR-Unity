using System;
using UnityEngine;

namespace Team5.qPCR
{
    [Serializable]
    public sealed class DialogueEntry
    {
        public WorkflowStage Stage;
        public string Title;
        [TextArea] public string Instruction;
        [TextArea] public string Why;
        public string Action;
        public CameraFocus Focus;
    }

    [CreateAssetMenu(fileName = "DialogueSequence", menuName = "Team 5/qPCR/Dialogue Sequence")]
    public sealed class DialogueSequence : ScriptableObject
    {
        [SerializeField] private DialogueEntry[] entries = Array.Empty<DialogueEntry>();

        public DialogueEntry For(WorkflowStage stage)
        {
            for (var index = 0; index < entries.Length; index++)
            {
                if (entries[index] != null && entries[index].Stage == stage)
                {
                    return entries[index];
                }
            }

            var fallback = WorkflowContent.For(stage);
            return new DialogueEntry
            {
                Stage = stage,
                Title = fallback.Title,
                Instruction = fallback.Instruction,
                Why = fallback.Why,
                Action = fallback.Action,
                Focus = LabCameraDirector.FocusForStage(stage)
            };
        }

        public void Configure(DialogueEntry[] values)
        {
            entries = values ?? Array.Empty<DialogueEntry>();
        }
    }
}
