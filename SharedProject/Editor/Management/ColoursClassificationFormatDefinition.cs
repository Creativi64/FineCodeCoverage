using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;

namespace FineCodeCoverage.Editor.Management
{
    internal sealed class ColoursClassificationFormatDefinition : ClassificationFormatDefinition
    {
        public ColoursClassificationFormatDefinition(string displayName, Color foregroundColor, Color backgroundColor)
        {
            DisplayName = displayName;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }
}
