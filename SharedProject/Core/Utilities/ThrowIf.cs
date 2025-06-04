using System;

namespace FineCodeCoverage.Core.Utilities
{
    internal static class ThrowIf
    {
        public static void Null<T>(T value, string name) where T : class
        {
            if (value != null)
            {
                return;
            }

            throw new ArgumentNullException(name);
        }
    }
}
