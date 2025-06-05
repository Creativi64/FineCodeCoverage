using System;
using System.Diagnostics;
using System.IO;

namespace FineCodeCoverage.Wpf
{
    public static class VsHelper
    {
        public static string GetAVsInstallationPath()
        {
            string vswherePath = GetVsWherePath();
            if (!File.Exists(vswherePath))
            {
                return null;
            }

            string vs2022Path = GetInstallPath(vswherePath, "17");
            return vs2022Path ?? GetInstallPath(vswherePath, "16");
        }

        private static string GetVsWherePath() => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            "Microsoft Visual Studio",
            "Installer",
            "vswhere.exe");

        private static string GetInstallPath(string vswherePath, string version)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = vswherePath,
                Arguments = $"-latest -products * -requires Microsoft.Component.MSBuild -version [{version}.0,{version}.99] -property installationPath",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (var process = Process.Start(processStartInfo))
            {
                string result = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
                return result;
            }
        }
    }
}
