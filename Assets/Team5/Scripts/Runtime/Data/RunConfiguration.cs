using System;
using System.Collections.Generic;
using UnityEngine;

namespace Team5.qPCR
{
    [Serializable]
    public sealed class RunConfiguration
    {
        public float ReactionVolumeMicrolitres = 20f;
        public float LidTemperature = 105f;
        public string Fluorophore = "SYBR Green";
        public float InitialHoldTemperature = 95f;
        public int InitialHoldSeconds = 120;
        public int Cycles = 35;
        public float DenaturationTemperature = 95f;
        public int DenaturationSeconds = 15;
        public float AnnealingTemperature = 60f;
        public int AnnealingSeconds = 30;
        public float ExtensionTemperature = 72f;
        public int ExtensionSeconds = 30;
        public float AcquisitionTemperature = 60f;
        public float MeltCurveStartTemperature = 65f;
        public float MeltCurveEndTemperature = 95f;

        public RunConfiguration Clone()
        {
            return (RunConfiguration)MemberwiseClone();
        }

        public void CopyFrom(RunConfiguration source)
        {
            if (source == null)
            {
                return;
            }

            ReactionVolumeMicrolitres = source.ReactionVolumeMicrolitres;
            LidTemperature = source.LidTemperature;
            Fluorophore = source.Fluorophore;
            InitialHoldTemperature = source.InitialHoldTemperature;
            InitialHoldSeconds = source.InitialHoldSeconds;
            Cycles = source.Cycles;
            DenaturationTemperature = source.DenaturationTemperature;
            DenaturationSeconds = source.DenaturationSeconds;
            AnnealingTemperature = source.AnnealingTemperature;
            AnnealingSeconds = source.AnnealingSeconds;
            ExtensionTemperature = source.ExtensionTemperature;
            ExtensionSeconds = source.ExtensionSeconds;
            AcquisitionTemperature = source.AcquisitionTemperature;
            MeltCurveStartTemperature = source.MeltCurveStartTemperature;
            MeltCurveEndTemperature = source.MeltCurveEndTemperature;
        }
    }

    public enum ValidationIssueCode
    {
        ReactionVolume,
        LidTemperature,
        Fluorophore,
        InitialHold,
        CycleCount,
        Denaturation,
        Annealing,
        Extension,
        Acquisition,
        MeltCurve
    }

    public readonly struct ValidationIssue
    {
        public ValidationIssue(ValidationIssueCode code, string message)
        {
            Code = code;
            Message = message;
        }

        public ValidationIssueCode Code { get; }
        public string Message { get; }
    }

    public static class ProtocolValidator
    {
        private const float TemperatureTolerance = 0.11f;
        private const float VolumeTolerance = 0.11f;

        public static IReadOnlyList<ValidationIssue> Validate(RunConfiguration value)
        {
            var issues = new List<ValidationIssue>();
            if (value == null)
            {
                issues.Add(new ValidationIssue(ValidationIssueCode.CycleCount, "Enter the qPCR protocol before continuing."));
                return issues;
            }

            Check(issues, Nearly(value.ReactionVolumeMicrolitres, 20f, VolumeTolerance), ValidationIssueCode.ReactionVolume,
                "Reaction volume must be 20 µL for this teaching run.");
            Check(issues, Nearly(value.LidTemperature, 105f), ValidationIssueCode.LidTemperature,
                "Set the heated lid to 105°C to limit condensation.");
            Check(issues, string.Equals(value.Fluorophore?.Trim(), "SYBR Green", StringComparison.OrdinalIgnoreCase),
                ValidationIssueCode.Fluorophore, "Choose SYBR Green so fluorescence can be measured during amplification.");
            Check(issues, Nearly(value.InitialHoldTemperature, 95f) && value.InitialHoldSeconds == 120,
                ValidationIssueCode.InitialHold, "The initial hold is 95°C for 120 seconds.");
            Check(issues, value.Cycles == 35, ValidationIssueCode.CycleCount,
                "This protocol uses 35 cycles.");
            Check(issues, Nearly(value.DenaturationTemperature, 95f) && value.DenaturationSeconds == 15,
                ValidationIssueCode.Denaturation, "Denaturation is 95°C for 15 seconds.");
            Check(issues, Nearly(value.AnnealingTemperature, 60f) && value.AnnealingSeconds == 30,
                ValidationIssueCode.Annealing, "Annealing is 60°C for 30 seconds.");
            Check(issues, Nearly(value.ExtensionTemperature, 72f) && value.ExtensionSeconds == 30,
                ValidationIssueCode.Extension, "Extension is 72°C for 30 seconds.");
            Check(issues, Nearly(value.AcquisitionTemperature, 60f), ValidationIssueCode.Acquisition,
                "Read fluorescence at the 60°C annealing step.");
            Check(issues, Nearly(value.MeltCurveStartTemperature, 65f) && Nearly(value.MeltCurveEndTemperature, 95f),
                ValidationIssueCode.MeltCurve, "The melt curve should run from 65°C to 95°C.");
            return issues;
        }

        public static bool IsValid(RunConfiguration value)
        {
            return Validate(value).Count == 0;
        }

        private static void Check(List<ValidationIssue> issues, bool valid, ValidationIssueCode code, string message)
        {
            if (!valid)
            {
                issues.Add(new ValidationIssue(code, message));
            }
        }

        private static bool Nearly(float value, float expected, float tolerance = TemperatureTolerance)
        {
            return Mathf.Abs(value - expected) <= tolerance;
        }
    }
}
