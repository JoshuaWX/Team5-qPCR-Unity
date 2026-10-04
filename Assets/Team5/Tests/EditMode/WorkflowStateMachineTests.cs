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
            Assert.AreEqual(WorkflowStage.HandoffReview, machine.CurrentStage);
            Assert.IsTrue(machine.TryAdvance());
            Assert.AreEqual(WorkflowStage.PowerOn, machine.CurrentStage);
            Assert.IsTrue(machine.TryAdvance());
            Assert.AreEqual(WorkflowStage.ProtocolSetup, machine.CurrentStage);
            Assert.IsTrue(machine.TryAdvance());
            Assert.AreEqual(WorkflowStage.PlateInspection, machine.CurrentStage);
            Assert.IsTrue(machine.TryAdvance());
            Assert.AreEqual(WorkflowStage.InstrumentLoading, machine.CurrentStage);
            Assert.IsTrue(machine.TryAdvance());
            Assert.AreEqual(WorkflowStage.RunValidation, machine.CurrentStage);
            Assert.IsTrue(machine.TryAdvance());
            Assert.AreEqual(WorkflowStage.Amplification, machine.CurrentStage);
            Assert.IsFalse(machine.TryAdvance(), "Amplification cannot be skipped by a user action.");
            Assert.IsTrue(machine.TryCompleteAmplification());
            Assert.AreEqual(WorkflowStage.ResultsInterpretation, machine.CurrentStage);
            Assert.IsTrue(machine.TryAdvance());
            Assert.AreEqual(WorkflowStage.Complete, machine.CurrentStage);
        }

        [TestCase(false, true, true, true, true)]
        [TestCase(true, false, true, true, true)]
        [TestCase(true, true, false, true, true)]
        [TestCase(true, true, true, false, true)]
        [TestCase(true, true, true, true, false)]
        public void RunIsBlockedWhenPreparationIsIncomplete(bool prepared, bool inspected, bool inserted, bool closed, bool valid)
        {
            Assert.IsFalse(RunEligibility.CanStart(prepared, inspected, inserted, closed, valid));
        }

        [Test]
        public void RunIsAllowedOnlyWhenAllChecksPass()
        {
            Assert.IsTrue(RunEligibility.CanStart(true, true, true, true, true));
        }
    }
}
