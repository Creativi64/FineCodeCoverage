using System;
using System.IO;

namespace FineCodeCoverage.Core.Utilities
{
	internal static class FileSystemInfoDeleteExtensions
	{
		public static void TryDelete(string path)
        {
            if (File.Exists(path))
            {
				new FileInfo(path).TryDelete();
            }
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
