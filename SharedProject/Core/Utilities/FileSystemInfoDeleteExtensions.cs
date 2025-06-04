using System;
using System.IO;

namespace FineCodeCoverage.Core.Utilities
{
    internal static class FileSystemInfoDeleteExtensions
    {
#pragma warning disable RCS1224 // Make method an extension method
        public static void TryDelete(string path)
#pragma warning restore RCS1224 // Make method an extension method
        {
            if (!File.Exists(path))
            {
                return;
            }

            new FileInfo(path).TryDelete();
        }
        public static void TryDelete(this FileInfo fileInfo, Action<Exception> exceptionCallback = null)
        {
            try
            {
                fileInfo.Delete();
            }
            catch (Exception exc)
            {
                exceptionCallback?.Invoke(exc);
            }
        }

        public static void TryDelete(this DirectoryInfo directoryInfo, bool recursive = true, Action<Exception> exceptionCallback = null)
        {
            try
            {
                directoryInfo.Delete(recursive);
            }
            catch (Exception exc)
            {
                exceptionCallback?.Invoke(exc);
            }
        }
    }
}