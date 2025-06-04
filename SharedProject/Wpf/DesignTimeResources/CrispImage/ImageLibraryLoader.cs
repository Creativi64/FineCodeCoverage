using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Threading;

namespace FineCodeCoverage.Wpf
{
    public static class ImageLibraryLoader
    {
        private static ImageLibrary defaultImageLibrary;
        private static List<string> defaultDirectories;

        public static ImageLibrary Default => defaultImageLibrary ?? (defaultImageLibrary = GetImageLibrary(""));

        private static List<string> DefaultDirectories
        {
            get
            {
                if (defaultDirectories == null)
                {
                    defaultDirectories = new List<string>();
                    string installationPath = VsHelper.GetAVsInstallationPath();
                    if (installationPath != null)
                    {
                        //Path.Combine(installationPath, "Common7\\IDE")
                        defaultDirectories.Add(Path.Combine(installationPath, "Common7\\IDE\\CommonExtensions\\Platform\\Shell"));
                    }
                }

                return defaultDirectories;
            }
        }

        private static ImageLibrary GetImageLibrary(string manifestsOrDirectories)
        {
            string[] manifests = manifestsOrDirectories.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            var imageManifests = manifests.Where(m => m.EndsWith(".imagemanifest")).ToList();
            List<string> directories = string.IsNullOrEmpty(manifestsOrDirectories) ?
                DefaultDirectories : manifests.Except(imageManifests).ToList();
            AddImageManifests(directories, imageManifests);
#pragma warning disable VSSDK005 // Avoid instantiating JoinableTaskContext
            JoinableTaskFactory fakeJoinableTaskFactory = new JoinableTaskContext().Factory;
#pragma warning restore VSSDK005 // Avoid instantiating JoinableTaskContext

            return ImageLibrary.Load(fakeJoinableTaskFactory, imageManifests, false, null);
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
                        imageManifests.AddRange(GetImageManifests(directoryInfo.FullName));
                    }
                }
                catch
                {

                }
            }
        }

        private static IEnumerable<string> GetImageManifests(string path)
            => Directory.EnumerateFiles(path, "*.imagemanifest", SearchOption.AllDirectories);
    }
}