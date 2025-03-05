using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FineCodeCoverage.Wpf
{
    public static class ImageLibraryLoader
    {
        private static ImageLibrary defaultImageLibrary;
        private static List<string> _defaultDirectories;

        public static ImageLibrary Default
        {
            get
            {
                return defaultImageLibrary ?? (defaultImageLibrary = GetImageLibrary(""));
            }
        }

        private static List<string> DefaultDirectories
        {
            get
            {
                if (_defaultDirectories == null)
                {
                    _defaultDirectories = new List<string>();
                    string installationPath = VsHelper.GetAVsInstallationPath();
                    if (installationPath != null)
                    {
                        //Path.Combine(installationPath, "Common7\\IDE")
                        _defaultDirectories.Add(Path.Combine(installationPath, "Common7\\IDE\\CommonExtensions\\Platform\\Shell"));
                    }
                }

                return _defaultDirectories;
            }
        }

        private static ImageLibrary GetImageLibrary(string manifestsOrDirectories)
        {
            var manifests = manifestsOrDirectories.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            var imageManifests = manifests.Where(m => m.EndsWith(".imagemanifest")).ToList();
            List<string> directories = null;
            if (String.IsNullOrEmpty(manifestsOrDirectories))
            {
                directories = DefaultDirectories;
            }
            else
            {
                directories = manifests.Except(imageManifests).ToList();
            }
            AddImageManifests(directories, imageManifests);
#pragma warning disable VSSDK005 // Avoid instantiating JoinableTaskContext
            var fakeJoinableTaskFactory = new JoinableTaskContext().Factory;
#pragma warning restore VSSDK005 // Avoid instantiating JoinableTaskContext

            return ImageLibrary.Load(fakeJoinableTaskFactory, imageManifests, false, null);
        }

        private static List<string> AddImageManifests(List<string> directories, List<string> imageManifests)
        {
            foreach (var directory in directories)
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
            return imageManifests;
        }

        private static IEnumerable<string> GetImageManifests(string path)
        {
            return Directory.EnumerateFiles(path, "*.imagemanifest", SearchOption.AllDirectories);
        }
    }

}
