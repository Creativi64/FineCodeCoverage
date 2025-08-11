using System;
using System.Diagnostics;

namespace StylableFindFlowDocumentReader
{
    internal static class StackTraceExtensions
    {
        public static bool TypeIsACaller<T>(this StackTrace stackTrace)
        {
            foreach (StackFrame frame in stackTrace.GetFrames() ?? Array.Empty<StackFrame>())
            {
                if (frame.GetMethod()?.DeclaringType == typeof(T))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
