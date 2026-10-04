using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace Team5.qPCR
{
    public sealed class ProtocolSetupController : MonoBehaviour
    {
        private const int FieldCount = 14;

        [SerializeField] private RunProtocol expectedProtocol;
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_InputField[] fields = new TMP_InputField[FieldCount];
        [SerializeField] private TMP_Dropdown fluorophoreDropdown;
        [SerializeField] private TMP_Text validationText;
        [SerializeField] private GameObject secondaryPanel;
        [SerializeField] private TMP_InputField[] secondaryFields = new TMP_InputField[FieldCount];
        [SerializeField] private TMP_Dropdown secondaryFluorophoreDropdown;
        [SerializeField] private TMP_Text secondaryValidationText;

        public RunConfiguration CurrentConfiguration { get; private set; } = new RunConfiguration();
        public bool IsValid { get; private set; }

        public void Configure(
            RunProtocol protocol,
            GameObject setupPanel,
            TMP_InputField[] inputFields,
            TMP_Dropdown fluorophore,
            TMP_Text feedback)
        {
            expectedProtocol = protocol;
            panel = setupPanel;
            fields = inputFields ?? new TMP_InputField[FieldCount];
            fluorophoreDropdown = fluorophore;
            validationText = feedback;
            ResetConfiguration();
        }

        public void ConfigureSecondaryView(
            GameObject setupPanel,
            TMP_InputField[] inputFields,
            TMP_Dropdown fluorophore,
            TMP_Text feedback)
        {
            secondaryPanel = setupPanel;
            secondaryFields = inputFields ?? new TMP_InputField[FieldCount];
            secondaryFluorophoreDropdown = fluorophore;
            secondaryValidationText = feedback;
            WriteConfiguration(CurrentConfiguration);
        }

        public void SetVisible(bool visible)
        {
            if (panel != null)
            {
                panel.SetActive(visible);
            }
        }

        public bool ValidateAndReport()
        {
            if (!TryReadConfiguration(out var configuration, out var parsingError))
            {
                IsValid = false;
                SetFeedback(parsingError, false);
                return false;
            }

            CurrentConfiguration = configuration;
            var issues = ProtocolValidator.Validate(configuration);
            IsValid = issues.Count == 0;
            if (IsValid)
            {
                SetFeedback("Protocol valid — all temperatures, times, cycles, and fluorescence settings are ready.", true);
                return true;
            }

            SetFeedback(issues[0].Message, false);
            return false;
        }

        public void ResetConfiguration()
        {
            CurrentConfiguration = expectedProtocol == null
                ? new RunConfiguration()
                : expectedProtocol.ExpectedConfiguration;
            IsValid = false;
            WriteConfiguration(CurrentConfiguration);
            SetFeedback("Check each value, then validate the protocol.", true);
        }

        private bool TryReadConfiguration(out RunConfiguration value, out string error)
        {
            value = new RunConfiguration();
            error = null;
            var activeFields = secondaryPanel != null && secondaryPanel.activeInHierarchy ? secondaryFields : fields;
            var activeDropdown = secondaryPanel != null && secondaryPanel.activeInHierarchy
                ? secondaryFluorophoreDropdown
                : fluorophoreDropdown;
            if (activeFields == null || activeFields.Length < FieldCount)
            {
                error = "The protocol screen is incomplete. Reset the lesson and try again.";
                return false;
            }

            var numbers = new float[FieldCount];
            for (var index = 0; index < activeFields.Length && index < FieldCount; index++)
            {
                if (activeFields[index] == null || !float.TryParse(activeFields[index].text, NumberStyles.Float, CultureInfo.InvariantCulture, out numbers[index]))
                {
                    error = "Use numbers only in every temperature, time, volume, and cycle field.";
                    return false;
                }
            }

            value.ReactionVolumeMicrolitres = numbers[0];
            value.LidTemperature = numbers[1];
            value.InitialHoldTemperature = numbers[2];
            value.InitialHoldSeconds = Mathf.RoundToInt(numbers[3]);
            value.Cycles = Mathf.RoundToInt(numbers[4]);
            value.DenaturationTemperature = numbers[5];
            value.DenaturationSeconds = Mathf.RoundToInt(numbers[6]);
            value.AnnealingTemperature = numbers[7];
            value.AnnealingSeconds = Mathf.RoundToInt(numbers[8]);
            value.ExtensionTemperature = numbers[9];
            value.ExtensionSeconds = Mathf.RoundToInt(numbers[10]);
            value.AcquisitionTemperature = numbers[11];
            value.MeltCurveStartTemperature = numbers[12];
            value.MeltCurveEndTemperature = numbers[13];
            value.Fluorophore = activeDropdown == null || activeDropdown.options.Count == 0
                ? "SYBR Green"
                : activeDropdown.options[activeDropdown.value].text;
            return true;
        }

        private void WriteConfiguration(RunConfiguration value)
        {
            if (value == null || fields == null || fields.Length < FieldCount)
            {
                return;
            }

            var values = new[]
            {
                value.ReactionVolumeMicrolitres, value.LidTemperature, value.InitialHoldTemperature, value.InitialHoldSeconds,
                value.Cycles, value.DenaturationTemperature, value.DenaturationSeconds, value.AnnealingTemperature,
                value.AnnealingSeconds, value.ExtensionTemperature, value.ExtensionSeconds, value.AcquisitionTemperature,
                value.MeltCurveStartTemperature, value.MeltCurveEndTemperature
            };

            for (var index = 0; index < FieldCount; index++)
            {
                fields[index]?.SetTextWithoutNotify(values[index].ToString("0.##", CultureInfo.InvariantCulture));
                if (secondaryFields != null && index < secondaryFields.Length)
                {
                    secondaryFields[index]?.SetTextWithoutNotify(values[index].ToString("0.##", CultureInfo.InvariantCulture));
                }
            }

            ConfigureDropdown(fluorophoreDropdown);
            ConfigureDropdown(secondaryFluorophoreDropdown);
        }

        private static void ConfigureDropdown(TMP_Dropdown dropdown)
        {
            if (dropdown != null)
            {
                var options = new List<TMP_Dropdown.OptionData>
                {
                    new TMP_Dropdown.OptionData("SYBR Green"),
                    new TMP_Dropdown.OptionData("FAM probe"),
                    new TMP_Dropdown.OptionData("None")
                };
                dropdown.ClearOptions();
                dropdown.AddOptions(options);
                dropdown.SetValueWithoutNotify(0);
                dropdown.RefreshShownValue();
            }
        }

        private void SetFeedback(string message, bool positive)
        {
            if (validationText == null)
            {
                return;
            }

            validationText.text = message;
            validationText.color = positive
                ? new Color(0.43f, 0.93f, 0.78f)
                : new Color(1f, 0.48f, 0.48f);

            if (secondaryValidationText != null)
            {
                secondaryValidationText.text = message;
                secondaryValidationText.color = validationText.color;
            }
        }
    }
}
