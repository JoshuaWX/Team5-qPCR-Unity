using NUnit.Framework;

namespace Team5.qPCR.Tests
{
    public sealed class ProtocolValidatorTests
    {
        [Test]
        public void DetailedTeamFiveProtocolPassesValidation()
        {
            Assert.IsTrue(ProtocolValidator.IsValid(new RunConfiguration()));
        }

        [Test]
        public void WrongCycleCountIsRejectedWithUsefulIssue()
        {
            var configuration = new RunConfiguration { Cycles = 40 };
            var issues = ProtocolValidator.Validate(configuration);

            Assert.IsFalse(ProtocolValidator.IsValid(configuration));
            Assert.That(issues, Has.Some.Matches<ValidationIssue>(issue => issue.Code == ValidationIssueCode.CycleCount));
        }

        [Test]
        public void WrongFluorophoreIsRejected()
        {
            var configuration = new RunConfiguration { Fluorophore = "None" };
            Assert.That(ProtocolValidator.Validate(configuration),
                Has.Some.Matches<ValidationIssue>(issue => issue.Code == ValidationIssueCode.Fluorophore));
        }
    }
}
