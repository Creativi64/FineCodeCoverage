using System.Collections.Generic;
using FineCodeCoverage.Editor.DynamicCoverage;

namespace FineCodeCoverage.Editor.Management
{
    internal class CoverageColours : ICoverageColours
    {
        private readonly Dictionary<DynamicCoverageType, IFontAndColorsInfo> _coverageTypeToFontAndColorsInfo;

        public CoverageColours(
            IFontAndColorsInfo coverageTouchedInfo,
            IFontAndColorsInfo coverageNotTouchedInfo,
            IFontAndColorsInfo coveragePartiallyTouchedInfo,
            IFontAndColorsInfo dirtyInfo,
            IFontAndColorsInfo newLineInfo,
            IFontAndColorsInfo notIncludedInfo
        ) => _coverageTypeToFontAndColorsInfo = new Dictionary<DynamicCoverageType, IFontAndColorsInfo>
            {
                { DynamicCoverageType.Covered, coverageTouchedInfo},
                { DynamicCoverageType.NotCovered, coverageNotTouchedInfo },
                { DynamicCoverageType.Partial, coveragePartiallyTouchedInfo},
                { DynamicCoverageType.Dirty, dirtyInfo},
                { DynamicCoverageType.NewLine, newLineInfo},
                { DynamicCoverageType.NotIncluded, notIncludedInfo}
            };

        internal Dictionary<DynamicCoverageType, IFontAndColorsInfo> GetChanges(CoverageColours lastCoverageColours)
            => lastCoverageColours == null
                ? _coverageTypeToFontAndColorsInfo
                : GetChanges(lastCoverageColours._coverageTypeToFontAndColorsInfo);

        private Dictionary<DynamicCoverageType, IFontAndColorsInfo> GetChanges(Dictionary<DynamicCoverageType, IFontAndColorsInfo> lastCoverageTypeToFontAndColorsInfo)
        {
            var changes = new Dictionary<DynamicCoverageType, IFontAndColorsInfo>();
            foreach (KeyValuePair<DynamicCoverageType, IFontAndColorsInfo> kvp in lastCoverageTypeToFontAndColorsInfo)
            {
                if (!_coverageTypeToFontAndColorsInfo[kvp.Key].Equals(kvp.Value))
                {
                    changes.Add(kvp.Key, _coverageTypeToFontAndColorsInfo[kvp.Key]);
                }
            }

            return changes;
        }

        public IItemCoverageColours GetColour(DynamicCoverageType coverageType)
            => _coverageTypeToFontAndColorsInfo[coverageType].ItemCoverageColours;
    }
}
