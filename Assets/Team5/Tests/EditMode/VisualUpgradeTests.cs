using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Team5.qPCR.Tests
{
    public sealed class VisualUpgradeTests
    {
        [TestCase(WellType.Sample, 2f, 6.8f, 20f)]
        [TestCase(WellType.PositiveControl, 2f, 6.8f, 20f)]
        [TestCase(WellType.NoTemplateControl, 0f, 8.8f, 20f)]
        [TestCase(WellType.Unused, 0f, 0f, 0f)]
        public void EveryWellContainsTheCorrectCompleteReaction(WellType type,float dna,float water,float total)
        {
            var mix=ScriptableObject.CreateInstance<ReactionMixDefinition>();
            var contents=mix.ForWell(type);
            Assert.AreEqual(dna,contents.Template,.001f);Assert.AreEqual(water,contents.Water,.001f);Assert.AreEqual(total,contents.Total,.001f);
            Object.DestroyImmediate(mix);
        }
        [Test]
        public void TeachingMixIsOneXWithThreeHundredNanomolarPrimers()
        {
            var mix=ScriptableObject.CreateInstance<ReactionMixDefinition>();
            Assert.IsTrue(mix.Validate(20,"SYBR Green",out var issue),issue);
            Assert.AreEqual(1,mix.FinalSupermixConcentration,.001f);Assert.AreEqual(300,mix.ForwardPrimerNanomolar,.01f);
            Assert.AreEqual(300,mix.ReversePrimerNanomolar,.01f);Object.DestroyImmediate(mix);
        }
        [Test]
        public void MissingCompositionOrWrongChemistryBlocksTheHandoff()
        {
            var handoff=ScriptableObject.CreateInstance<PlateHandoffData>();var mix=ScriptableObject.CreateInstance<ReactionMixDefinition>();
            Assert.IsFalse(handoff.IsReady);handoff.SetReactionMix(mix);Assert.IsTrue(handoff.IsReady);
            Assert.IsFalse(mix.Validate(20,"FAM probe",out _));Assert.IsFalse(mix.Validate(10,"SYBR Green",out _));
            Object.DestroyImmediate(handoff);Object.DestroyImmediate(mix);
        }
        [TestCase("waterVolume",-1f)]
        [TestCase("waterVolume",7f)]
        [TestCase("forwardPrimerVolume",0f)]
        [TestCase("templateVolume",float.NaN)]
        [TestCase("supermixConcentration",3f)]
        public void InvalidCompositionIsRejected(string field,float invalid)
        {
            var mix=ScriptableObject.CreateInstance<ReactionMixDefinition>();var so=new SerializedObject(mix);
            so.FindProperty(field).floatValue=invalid;so.ApplyModifiedPropertiesWithoutUndo();
            Assert.IsFalse(mix.Validate(20,"SYBR Green",out var issue));Assert.IsNotEmpty(issue);Object.DestroyImmediate(mix);
        }
        [Test]
        public void NormalisedWheelNotchProducesUsefulProportionalZoom()
        {
            Assert.AreEqual(.88f,LabCameraDirector.ZoomDistance(1,1,1,.18f,18),.001f);
            Assert.AreEqual(1,LabCameraDirector.ZoomDistance(.88f,-1,1,.18f,18),.001f);
            Assert.AreEqual(.18f,LabCameraDirector.ZoomDistance(.18f,5,2,.18f,18),.001f);
        }
        [TestCase(WorkflowStage.PlateInspection,CameraFocus.Plate)]
        [TestCase(WorkflowStage.ProtocolSetup,CameraFocus.Touchscreen)]
        [TestCase(WorkflowStage.ResultsInterpretation,CameraFocus.Monitor)]
        public void GuidedFocusMatchesTheLesson(WorkflowStage stage,CameraFocus expected)
            =>Assert.AreEqual(expected,LabCameraDirector.FocusForStage(stage));
    }
}
