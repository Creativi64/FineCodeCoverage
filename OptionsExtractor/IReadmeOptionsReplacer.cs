using System;

namespace OptionsExtractor
{
    public interface IReadmeOptionsReplacer {
        string ReplaceReadMeMarkerWithOptionsTable(
            string markedReadme, string marker, Type packageType, Type coverageSettingsType
        );
    }
}
