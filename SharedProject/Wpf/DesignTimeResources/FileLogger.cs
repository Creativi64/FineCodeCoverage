using System;

namespace FineCodeCoverage.Wpf
{
    public static class FileLogger
    {
        public static string LogPath = @"C:\Users\tonyh\Downloads\log.txt";
        public static void Clear() => System.IO.File.WriteAllText(LogPath, string.Empty);
        public static void Log(string message) => System.IO.File.AppendAllText(LogPath, message + Environment.NewLine);
    }
}
