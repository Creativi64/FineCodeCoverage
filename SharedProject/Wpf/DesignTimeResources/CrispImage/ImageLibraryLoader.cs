using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Threading;

namespace FineCodeCoverage.Wpf
{
    /// <summary>
    ///     Replicates image library loading functionality of CrispImage.
    /// </summary>
    public static class ImageLibraryLoader
    {
        private static readonly string[] s_separator = new string[] { "|" };
        private static ImageLibrary s_defaultImageLibrary;
        private static List<string> s_defaultDirectories;

        public static ImageLibrary Default => s_defaultImageLibrary ?? (s_defaultImageLibrary = GetImageLibrary(string.Empty));

        private static List<string> DefaultDirectories
        {
            get
            {
                if (s_defaultDirectories == null)
                {
                    s_defaultDirectories = new List<string>();
                    string installationPath = VsInstallationPath.GetAVsInstallationPath();
                    if (installationPath != null)
                    {
                        // Path.Combine(installationPath, "Common7\\IDE")
                        s_defaultDirectories.Add(Path.Combine(installationPath, "Common7\\IDE\\CommonExtensions\\Platform\\Shell"));
                    }
                }

                return s_defaultDirectories;
            }
        }

        private static ImageLibrary GetImageLibrary(string manifestsOrDirectories)
        {
            List<string> imageManifests = GetImageManifests(manifestsOrDirectories);
#pragma warning disable VSSDK005 // Avoid instantiating JoinableTaskContext
            JoinableTaskFactory fakeJoinableTaskFactory = new JoinableTaskContext().Factory;
#pragma warning restore VSSDK005 // Avoid instantiating JoinableTaskContext

            return ImageLibrary.Load(fakeJoinableTaskFactory, imageManifests, false, null);
        }

        private static List<string> GetImageManifests(string manifestsOrDirectories)
        {
            string[] manifests = manifestsOrDirectories.Split(s_separator, StringSplitOptions.RemoveEmptyEntries);
            var imageManifests = manifests.Where(m => m.EndsWith(".imagemanifest")).ToList();
            List<string> directories = string.IsNullOrEmpty(manifestsOrDirectories) ?
                DefaultDirectories : manifests.Except(imageManifests).ToList();
            AddImageManifests(directories, imageManifests);
            return imageManifests;
        }

        private static void AddImageManifests(List<string> directories, List<string> imageManifests)
        {
            foreach (string directory in directories)
            {
                try
                {
                    var directoryInfo = new DirectoryInfo(directory);
                    if (directoryInfo.Exists)
                    {
                        imageManifests.AddRange(GetImageManifestDescendants(directoryInfo.FullName));
                    }
                }
                catch
                {
                }
            }
        }

        private static IEnumerable<string> GetImageManifestDescendants(string path)
            => Directory.EnumerateFiles(path, "*.imagemanifest", SearchOption.AllDirectories);
    }
}
