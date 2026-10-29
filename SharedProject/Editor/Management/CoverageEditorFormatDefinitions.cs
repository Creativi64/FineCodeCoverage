using System;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace FineCodeCoverage.Editor.Management
{
    internal sealed class CoverageEditorFormatDefinitions
    {
        public const string PartiallyCoveredEditorFormatDefinitionName = "Coverage Partially Touched Area FCC";
        public const string NotCoveredEditorFormatDefinitionName = "Coverage Not Touched Area FCC";
        public const string CoveredEditorFormatDefinitionName = "Coverage Touched Area FCC";
        public const string NewLinesEditorFormatDefinitionName = "Coverage New Lines Area FCC";
        public const string DirtyEditorFormatDefinitionName = "Coverage Dirty Area FCC";
        public const string NotIncludedEditorFormatDefinitionName = "Coverage Not Included Area FCC";

        [Export]
        [Name(NotIncludedEditorFormatDefinitionName)]
        [ClassificationType(ClassificationTypeNames = CoverageClassificationTypeService.FCCNotIncludedClassificationTypeName)]
        [UserVisible(true)]
        public EditorFormatDefinition NotIncludedEditorFormatDefinition { get; } = CreateDefinition(NotIncludedEditorFormatDefinitionName);

        [Export]
        [Name(NewLinesEditorFormatDefinitionName)]
        [ClassificationType(ClassificationTypeNames = CoverageClassificationTypeService.FCCNewLineClassificationTypeName)]
        [UserVisible(true)]
        public EditorFormatDefinition NewLinesEditorFormatDefinition { get; } = CreateDefinition(NewLinesEditorFormatDefinitionName);

        [Export]
        [Name(DirtyEditorFormatDefinitionName)]
        [ClassificationType(ClassificationTypeNames = CoverageClassificationTypeService.FCCDirtyClassificationTypeName)]
        [UserVisible(true)]
        public EditorFormatDefinition DirtyEditorFormatDefinition { get; } = CreateDefinition(DirtyEditorFormatDefinitionName);

        [Export]
        [Name(CoveredEditorFormatDefinitionName)]
        [ClassificationType(ClassificationTypeNames = CoverageClassificationTypeService.FCCCoveredClassificationTypeName)]
        [UserVisible(true)]
        public EditorFormatDefinition CoveredEditorFormatDefinition { get; } = CreateDefinition(CoveredEditorFormatDefinitionName);

        [Export]
        [Name(NotCoveredEditorFormatDefinitionName)]
        [ClassificationType(ClassificationTypeNames = CoverageClassificationTypeService.FCCNotCoveredClassificationTypeName)]
        [UserVisible(true)]
        public EditorFormatDefinition NotCoveredEditorFormatDefinition { get; } = CreateDefinition(NotCoveredEditorFormatDefinitionName);

        [Export]
        [Name(PartiallyCoveredEditorFormatDefinitionName)]
        [ClassificationType(ClassificationTypeNames = CoverageClassificationTypeService.FCCPartiallyCoveredClassificationTypeName)]
        [UserVisible(true)]
        public EditorFormatDefinition PartiallyCoveredEditorFormatDefinition { get; } = CreateDefinition(PartiallyCoveredEditorFormatDefinitionName);

        private static EditorFormatDefinition CreateDefinition(string name)
        {
            IItemCoverageColours colours = GetDefaultInfo(name).ItemCoverageColours;
            return new ColoursClassificationFormatDefinition(name, colours.Foreground, colours.Background);
        }

        internal static IFontAndColorsInfo GetDefaultInfo(string name)
        {
            Color foreground;
            Color background;
            switch (name)
            {
                case CoveredEditorFormatDefinitionName:
                case MarkerTypeNames.Covered:
                    foreground = Colors.Black;
                    background = Colors.Green;
                    break;
                case NotCoveredEditorFormatDefinitionName:
                case MarkerTypeNames.NotCovered:
                    foreground = Colors.White;
                    background = Colors.Red;
                    break;
                case PartiallyCoveredEditorFormatDefinitionName:
                case MarkerTypeNames.PartiallyCovered:
                case NewLinesEditorFormatDefinitionName:
                    foreground = Colors.Black;
                    background = Colors.Yellow;
                    break;
                case DirtyEditorFormatDefinitionName:
                    foreground = Colors.White;
                    background = Colors.Brown;
                    break;
                case NotIncludedEditorFormatDefinitionName:
                    foreground = Colors.Black;
                    background = Colors.LightPink;
                    break;
                default:
                    throw new InvalidOperationException($"Unknown coverage editor format '{name}'.");
            }

            return new FontAndColorsInfo(new ItemCoverageColours(foreground, background), false);
        }
    }
}
