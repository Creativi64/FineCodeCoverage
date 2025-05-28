using System;
using Markdig.Helpers;

namespace FineCodeCoverage.Readme
{
    internal static class MarkdigExtensionMethods
    {
        public static void AddIfNotAlready<TItem>(this OrderedList<TItem> list, Func<TItem> creator) where TItem : class
        {
            if (!list.Contains<TItem>())
            {
                list.Add(creator());
            }
        }
    }
}
