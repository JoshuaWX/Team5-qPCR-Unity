using System;
using System.Collections.Generic;
using UnityEngine;

namespace Team5.qPCR
{
    public enum WellType
    {
        Sample,
        PositiveControl,
        NoTemplateControl,
        Unused
    }

    [Serializable]
    public sealed class WellDefinition
    {
        public int Row;
        public int Column;
        public string WellId;
        public string SampleId;
        public WellType Type;
        public Color DisplayColor;
    }

    [CreateAssetMenu(fileName = "ExperimentDefinition", menuName = "Team 5/qPCR/Experiment Definition")]
    public sealed class ExperimentDefinition : ScriptableObject
    {
        [SerializeField] private string assayName = "Team 5 Educational qPCR Panel";
        [SerializeField, TextArea] private string educationalDisclaimer =
            "Educational simulation only. Curves and Cq values are representative and must not be used for diagnosis.";
        [SerializeField] private WellDefinition[] wells = Array.Empty<WellDefinition>();

        public string AssayName => assayName;
        public string EducationalDisclaimer => educationalDisclaimer;
        public IReadOnlyList<WellDefinition> Wells => wells;
        public bool IsValid96WellPlate => wells != null && wells.Length == 96 && HasUniqueCoordinates();
        public int ActiveReactionCount
        {
            get
            {
                if (wells == null)
                {
                    return 0;
                }

                var count = 0;
                for (var index = 0; index < wells.Length; index++)
                {
                    if (wells[index] != null && wells[index].Type != WellType.Unused)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public void Configure(string name, string disclaimer, WellDefinition[] definitions)
        {
            assayName = name;
            educationalDisclaimer = disclaimer;
            wells = definitions ?? Array.Empty<WellDefinition>();
        }

        public WellDefinition GetWell(int row, int column)
        {
            if (wells == null)
            {
                return null;
            }

            for (var index = 0; index < wells.Length; index++)
            {
                if (wells[index].Row == row && wells[index].Column == column)
                {
                    return wells[index];
                }
            }

            return null;
        }

        private bool HasUniqueCoordinates()
        {
            var coordinates = new HashSet<int>();
            for (var index = 0; index < wells.Length; index++)
            {
                var well = wells[index];
                if (well == null || well.Row < 0 || well.Row > 7 || well.Column < 0 || well.Column > 11)
                {
                    return false;
                }

                if (!coordinates.Add((well.Row * 12) + well.Column))
                {
                    return false;
                }
            }

            return coordinates.Count == 96;
        }
    }
}
