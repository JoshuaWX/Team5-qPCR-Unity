using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Team5.qPCR.Tests
{
    public sealed class AssayResultGeneratorTests
    {
        private ExperimentDefinition experiment;

        [SetUp]
        public void SetUp()
        {
            experiment = ScriptableObject.CreateInstance<ExperimentDefinition>();
            var wells = new WellDefinition[96];
            for (var row = 0; row < 8; row++)
            {
                for (var column = 0; column < 12; column++)
                {
                    var index = (row * 12) + column;
                    var type = index == 0
                        ? WellType.PositiveControl
                        : index == 27
                            ? WellType.NoTemplateControl
                            : index < 27 ? WellType.Sample : WellType.Unused;
                    wells[index] = new WellDefinition
                    {
                        Row = row,
                        Column = column,
                        WellId = $"{(char)('A' + row)}{column + 1}",
                        SampleId = $"W-{index:000}",
                        Type = type,
                        DisplayColor = Color.cyan
                    };
                }
            }

            experiment.Configure("Test", "Educational only", wells);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(experiment);
        }

        [Test]
        public void ExperimentContainsExactlyNinetySixUniqueWells()
        {
            Assert.IsTrue(experiment.IsValid96WellPlate);
            Assert.AreEqual(96, experiment.Wells.Count);
            Assert.AreEqual(96, experiment.Wells.Select(well => well.WellId).Distinct().Count());
            Assert.AreEqual(28, experiment.ActiveReactionCount);
        }

        [Test]
        public void RepresentativeResultsAreDeterministicAndControlsRemainValid()
        {
            var first = AssayResultGenerator.Generate(experiment, 42);
            var second = AssayResultGenerator.Generate(experiment, 42);

            Assert.AreEqual(28, first.Count);
            CollectionAssert.AreEqual(first.Select(result => result.Cq), second.Select(result => result.Cq));
            Assert.IsTrue(first.Where(result => result.WellType == WellType.PositiveControl).All(result => result.IsAmplified));
            Assert.IsTrue(first.Where(result => result.WellType == WellType.NoTemplateControl).All(result => !result.IsAmplified));
            Assert.IsTrue(first.All(result => result.Fluorescence.Count == 36));
        }
    }
}
