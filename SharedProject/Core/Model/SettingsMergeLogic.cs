using System;
using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverage.Engine.Model
{
    public class SettingsMergeLogic : ISettingsMergeLogic
    {
        private interface ITypeMerger
        {
            object Merge(object first, object second);
        }

        private abstract class TypeMerger<T> : ITypeMerger
        {
            public abstract T Merge(T first, T second);

            public object Merge(object first, object second) => Merge((T)first, (T)second);
        }

        private sealed class StringArrayMerger : TypeMerger<string[]>
        {
            public override string[] Merge(string[] first, string[] second) => first.Concat(second).ToArray();
        }

        private readonly Dictionary<Type, ITypeMerger> _typeMergers;

        public SettingsMergeLogic() => _typeMergers = new Dictionary<Type, ITypeMerger>
            {
                { typeof(string[]), new StringArrayMerger() },
            };

        public bool CanMerge(Type type) => _typeMergers.ContainsKey(type);

        public object Merge(Type type, object first, object second) => _typeMergers[type].Merge(first, second);
    }
}
