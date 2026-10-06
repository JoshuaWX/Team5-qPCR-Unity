using System;
using UnityEngine;

namespace Team5.qPCR
{
    [Serializable]
    public sealed class LessonSessionReport
    {
        [SerializeField] private LessonMode mode;
        [SerializeField] private float elapsedSeconds;
        [SerializeField] private int mistakes;
        [SerializeField] private int hintsUsed;
        [SerializeField] private bool completed;

        public LessonMode Mode => mode;
        public float ElapsedSeconds => elapsedSeconds;
        public int Mistakes => mistakes;
        public int HintsUsed => hintsUsed;
        public bool Completed => completed;

        public void Begin(LessonMode selectedMode)
        {
            mode = selectedMode;
            elapsedSeconds = 0f;
            mistakes = 0;
            hintsUsed = 0;
            completed = false;
        }

        public void Tick(float deltaSeconds)
        {
            if (!completed)
            {
                elapsedSeconds += Mathf.Max(0f, deltaSeconds);
            }
        }

        public void RecordMistake() => mistakes++;
        public void RecordHint() => hintsUsed++;
        public void Complete() => completed = true;

        public string Summary =>
            $"Mode: {(mode == LessonMode.GuidedTraining ? "Guided Training" : "Assessment")}\n" +
            $"Time: {Mathf.FloorToInt(elapsedSeconds / 60f):00}:{Mathf.FloorToInt(elapsedSeconds % 60f):00}\n" +
            $"Mistakes: {mistakes}    Hints used: {hintsUsed}";
    }
}
