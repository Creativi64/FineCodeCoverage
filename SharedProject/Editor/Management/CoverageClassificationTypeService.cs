using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FineCodeCoverage.Editor.DynamicCoverage;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace FineCodeCoverage.Editor.Management
{
    [Export(typeof(ICoverageTypeService))]
    [Export(typeof(ICoverageColoursEditorFormatMapNames))]
    [Export(typeof(ICoverageClassificationColourService))]
    internal class CoverageClassificationTypeService :
        ICoverageClassificationColourService, ICoverageColoursEditorFormatMapNames, ICoverageTypeService
    {
        public const string FCCCoveredClassificationTypeName = "FCCCovered";
        public const string FCCNotCoveredClassificationTypeName = "FCCNotCovered";
        public const string FCCPartiallyCoveredClassificationTypeName = "FCCPartial";
        public const string FCCDirtyClassificationTypeName = "FCCDirty";
        public const string FCCNewLineClassificationTypeName = "FCCNewLine";
        public const string FCCNotIncludedClassificationTypeName = "FCCNotIncluded";
        private readonly Dictionary<DynamicCoverageType, string> _editorFormatNames = new Dictionary<DynamicCoverageType, string>
        {
            {DynamicCoverageType.Partial, FCCPartiallyCoveredClassificationTypeName },
            {DynamicCoverageType.NotCovered, FCCNotCoveredClassificationTypeName },
            {DynamicCoverageType.Covered, FCCCoveredClassificationTypeName },
            {DynamicCoverageType.Dirty, FCCDirtyClassificationTypeName },
            {DynamicCoverageType.NewLine, FCCNewLineClassificationTypeName },
            {DynamicCoverageType.NotIncluded, FCCNotIncludedClassificationTypeName }
        };

        private readonly IClassificationFormatMap _classificationFormatMap;
        private readonly ReadOnlyDictionary<DynamicCoverageType, IClassificationType> _classificationTypes;
        private readonly IClassificationType _highestPriorityClassificationType;

        [ExcludeFromCodeCoverage]
        [Export]
        [Name(FCCNotCoveredClassificationTypeName)]
        public ClassificationTypeDefinition FCCNotCoveredTypeDefinition { get; set; }

        [ExcludeFromCodeCoverage]
        [Export]
        [Name(FCCCoveredClassificationTypeName)]
        public ClassificationTypeDefinition FCCCoveredTypeDefinition { get; set; }

        [ExcludeFromCodeCoverage]
        [Export]
        [Name(FCCPartiallyCoveredClassificationTypeName)]
        public ClassificationTypeDefinition FCCPartiallyCoveredTypeDefinition { get; set; }

        [ExcludeFromCodeCoverage]
        [Export]
        [Name(FCCDirtyClassificationTypeName)]
        public ClassificationTypeDefinition FCCDirtyTypeDefinition { get; set; }

        [ExcludeFromCodeCoverage]
        [Export]
        [Name(FCCNewLineClassificationTypeName)]
        public ClassificationTypeDefinition FCCNewLineTypeDefinition { get; set; }

        [ExcludeFromCodeCoverage]
        [Export]
        [Name(FCCNotIncludedClassificationTypeName)]
        public ClassificationTypeDefinition FCCNotIncludedTypeDefinition { get; set; }

        [ImportingConstructor]
        public CoverageClassificationTypeService(
            IClassificationFormatMapService classificationFormatMapService,
            IClassificationTypeRegistryService classificationTypeRegistryService
        )
        {
            this._classificationFormatMap = classificationFormatMapService.GetClassificationFormatMap("text");
            this._highestPriorityClassificationType = this._classificationFormatMap.CurrentPriorityOrder.Last(ct => ct != null);

            IClassificationType notCoveredClassificationType = classificationTypeRegistryService.GetClassificationType(FCCNotCoveredClassificationTypeName);
            IClassificationType coveredClassificationType = classificationTypeRegistryService.GetClassificationType(FCCCoveredClassificationTypeName);
            IClassificationType partiallyCoveredClassificationType = classificationTypeRegistryService.GetClassificationType(FCCPartiallyCoveredClassificationTypeName);
            IClassificationType dirtyClassificationType = classificationTypeRegistryService.GetClassificationType(FCCDirtyClassificationTypeName);
            IClassificationType newCodeClassificationType = classificationTypeRegistryService.GetClassificationType(FCCNewLineClassificationTypeName);
            IClassificationType notIncludedClassificationType = classificationTypeRegistryService.GetClassificationType(FCCNotIncludedClassificationTypeName);

            this._classificationTypes = new ReadOnlyDictionary<DynamicCoverageType, IClassificationType>(
                new Dictionary<DynamicCoverageType, IClassificationType>
                {
                    { DynamicCoverageType.Covered, coveredClassificationType },
                    { DynamicCoverageType.NotCovered, notCoveredClassificationType },
                    { DynamicCoverageType.Partial, partiallyCoveredClassificationType },
                    { DynamicCoverageType.Dirty, dirtyClassificationType },
                    { DynamicCoverageType.NewLine, newCodeClassificationType },
                    { DynamicCoverageType.NotIncluded, notIncludedClassificationType }
                });
        }

        private void BatchUpdateIfRequired(Action action)
        {
            if (this._classificationFormatMap.IsInBatchUpdate)
            {
                action();
            }
            else
            {
                this._classificationFormatMap.BeginBatchUpdate();
                action();
                this._classificationFormatMap.EndBatchUpdate();
            }
        }

        public string GetEditorFormatDefinitionName(DynamicCoverageType coverageType) => this._editorFormatNames[coverageType];

        public IClassificationType GetClassificationType(DynamicCoverageType coverageType) => this._classificationTypes[coverageType];

        public void SetCoverageColours(IEnumerable<ICoverageTypeColour> coverageTypeColours)
            => this.BatchUpdateIfRequired(() =>
            {
                foreach (ICoverageTypeColour coverageTypeColour in coverageTypeColours)
                {
                    this.SetCoverageColour(coverageTypeColour);
                }
            });

        private void SetCoverageColour(ICoverageTypeColour coverageTypeColour)
        {
            IClassificationType classificationType = this._classificationTypes[coverageTypeColour.CoverageType];
            this._classificationFormatMap.AddExplicitTextProperties(
                classificationType, coverageTypeColour.TextFormattingRunProperties, this._highestPriorityClassificationType);
        }
    }
}