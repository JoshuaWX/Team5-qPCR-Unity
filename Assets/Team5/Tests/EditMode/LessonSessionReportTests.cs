using NUnit.Framework;

namespace Team5.qPCR.Tests
{
    public sealed class LessonSessionReportTests
    {
        [Test]
        public void GuidedSessionTracksElapsedMistakesHintsAndCompletion()
        {
            var report = new LessonSessionReport();
            report.Begin(LessonMode.GuidedTraining);
            report.Tick(12.5f);
            report.RecordMistake();
            report.RecordHint();
            report.Complete();
            report.Tick(5f);

            Assert.AreEqual(LessonMode.GuidedTraining, report.Mode);
            Assert.AreEqual(12.5f, report.ElapsedSeconds, .001f);
            Assert.AreEqual(1, report.Mistakes);
            Assert.AreEqual(1, report.HintsUsed);
            Assert.IsTrue(report.Completed);
            StringAssert.Contains("Guided Training", report.Summary);
        }

        [Test]
        public void BeginningAssessmentResetsPreviousSessionValues()
        {
            var report = new LessonSessionReport();
            report.Begin(LessonMode.GuidedTraining);
            report.Tick(5f);
            report.RecordMistake();
            report.RecordHint();
            report.Complete();

            report.Begin(LessonMode.Assessment);

            Assert.AreEqual(LessonMode.Assessment, report.Mode);
            Assert.AreEqual(0f, report.ElapsedSeconds);
            Assert.AreEqual(0, report.Mistakes);
            Assert.AreEqual(0, report.HintsUsed);
            Assert.IsFalse(report.Completed);
        }

        [Test]
        public void TrainingActionsContainFourIndependentInspectionCheckpoints()
        {
            Assert.AreNotEqual(TrainingAction.InspectPlateId, TrainingAction.InspectOpticalSeal);
            Assert.AreNotEqual(TrainingAction.InspectOpticalSeal, TrainingAction.InspectBubbles);
            Assert.AreNotEqual(TrainingAction.InspectBubbles, TrainingAction.InspectA1Marker);
        }
    }
}
