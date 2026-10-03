using NUnit.Framework;

namespace Team5.qPCR.Tests
{
    public sealed class WorkflowStateMachineTests
    {
        [Test]
        public void WorkflowFollowsTheRequiredOrder()
        {
            var machine = new WorkflowStateMachine();
            Assert.AreEqual(WorkflowStage.Introduction, machine.CurrentStage);

            Assert.IsTrue(machine.TryAdvance());
            Assert.AreEqual(WorkflowStage.PlateReview, machine.CurrentStage);
            Assert.IsTrue(machine.TryAdvance());
            Assert.AreEqual(WorkflowStage.PlateLoading, machine.CurrentStage);
            Assert.IsTrue(machine.TryAdvance());
            Assert.AreEqual(WorkflowStage.PlateSealing, machine.CurrentStage);
            Assert.IsTrue(machine.TryAdvance());
            Assert.AreEqual(WorkflowStage.InstrumentLoading, machine.CurrentStage);
            Assert.IsTrue(machine.TryAdvance());
            Assert.AreEqual(WorkflowStage.RunSetup, machine.CurrentStage);
            Assert.IsTrue(machine.TryAdvance());
            Assert.AreEqual(WorkflowStage.Amplification, machine.CurrentStage);
            Assert.IsFalse(machine.TryAdvance(), "Amplification cannot be skipped by a user action.");
            Assert.IsTrue(machine.TryCompleteAmplification());
            Assert.AreEqual(WorkflowStage.Results, machine.CurrentStage);
            Assert.IsTrue(machine.TryAdvance());
            Assert.AreEqual(WorkflowStage.Complete, machine.CurrentStage);
        }

        [TestCase(false, true, true)]
        [TestCase(true, false, true)]
        [TestCase(true, true, false)]
        [TestCase(false, false, false)]
        public void RunIsBlockedWhenPreparationIsIncomplete(bool loaded, bool sealedPlate, bool inserted)
        {
            Assert.IsFalse(RunEligibility.CanStart(loaded, sealedPlate, inserted));
        }

        [Test]
        public void RunIsAllowedOnlyWhenAllChecksPass()
        {
            Assert.IsTrue(RunEligibility.CanStart(true, true, true));
        }
    }
}
