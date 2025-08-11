using System;
using Markdig.Helpers;

namespace FineCodeCoverage.Readme
{
    internal static class MarkdigExtensionMethods
    {
        public static void AddIfNotAlready<TItem, TCreated>(
            this OrderedList<TItem> list,
            Func<TCreated> creator)
            where TItem : class
            where TCreated : class, TItem
        {
            if (list.Contains<TCreated>())
            {
                return;
            }

            list.Add(creator());
        }
    }
}
