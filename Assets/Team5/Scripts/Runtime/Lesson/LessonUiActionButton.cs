using UnityEngine;
using UnityEngine.UI;

namespace Team5.qPCR
{
    public sealed class LessonUiActionButton : MonoBehaviour
    {
        [SerializeField] private GuidedLessonController lesson;
        [SerializeField] private TrainingAction action;
        [SerializeField] private Button button;

        public void Configure(GuidedLessonController controller, TrainingAction trainingAction, Button uiButton)
        {
            lesson = controller;
            action = trainingAction;
            button = uiButton;
        }

        private void OnEnable() => button?.onClick.AddListener(Activate);
        private void OnDisable() => button?.onClick.RemoveListener(Activate);
        private void Activate() => lesson?.TryPerformAction(action, null);
    }
}
