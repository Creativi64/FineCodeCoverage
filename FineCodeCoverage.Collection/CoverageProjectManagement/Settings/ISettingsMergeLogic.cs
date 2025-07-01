using System;

namespace FineCodeCoverage.Collection.CoverageProjectManagement.Settings
{
    internal interface ISettingsMergeLogic
    {
        bool CanMerge(Type type);

        object Merge(Type type, object first, object second);
    }
}
