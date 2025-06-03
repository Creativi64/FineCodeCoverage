using System;

namespace FineCodeCoverage.Engine.Model
{
    internal interface ISettingsMergeLogic
    {
        bool CanMerge(Type type);
        object Merge(Type type, object first, object second);
    }
}