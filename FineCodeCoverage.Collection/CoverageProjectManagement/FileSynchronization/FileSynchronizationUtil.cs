using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace FineCodeCoverage.Collection.CoverageProjectManagement.FileSynchronization
{
    [Export(typeof(IFileSynchronizationUtil))]
    internal sealed class FileSynchronizationUtil : IFileSynchronizationUtil
    {
        public List<string> Synchronize(string sourceFolder, string destinationFolder, string fineCodeCoverageFolderName)
        {
            var logs = new List<string>();
            var srceDir = new DirectoryInfo(Path.GetFullPath(sourceFolder) + '\\');
            var destDir = new DirectoryInfo(Path.GetFullPath(destinationFolder) + '\\');

            // file lists
            IEnumerable<FileInfo> sourceFileInfos = srceDir.GetFiles().Concat(srceDir.GetDirectories().Where(d => d.Name != fineCodeCoverageFolderName).SelectMany(d => d.GetFiles("*", SearchOption.AllDirectories)));
            ParallelQuery<ComparableFile> srceFiles = sourceFileInfos.AsParallel().Select(fi => new ComparableFile(fi, fi.FullName.Substring(srceDir.FullName.Length)));
            ParallelQuery<ComparableFile> DestFiles() => destDir.GetFiles("*", SearchOption.AllDirectories).AsParallel().Select(fi => new ComparableFile(fi, fi.FullName.Substring(destDir.FullName.Length)));

            // copy to dest
            foreach (ComparableFile fileToCopy in srceFiles.Except(DestFiles(), FileComparer.Singleton))
            {
                var to = new FileInfo(fileToCopy.FileInfo.FullName.Replace(srceDir.FullName, destDir.FullName));

                if (!to.Directory.Exists)
                {
                    try
                    {
                        _ = Directory.CreateDirectory(to.DirectoryName);
                        logs.Add($"Create : {to.DirectoryName}");
                    }
                    catch (Exception exception)
                    {
                        logs.Add($"Create : {to.DirectoryName} : {exception.Message}");
                        continue;
                    }
                }

                try
                {
                    File.Copy(fileToCopy.FileInfo.FullName, to.FullName, true);
                    logs.Add($"Copy : {fileToCopy.FileInfo.FullName} -> {to.FullName}");
                }
                catch (Exception exception)
                {
                    logs.Add($"Copy : {fileToCopy.FileInfo.FullName} -> {to.FullName} : {exception.Message}");
                }
            }

            // delete from dest
            foreach (ComparableFile fileToDelete in DestFiles().Except(srceFiles, FileComparer.Singleton))
            {
                try
                {
                    File.Delete(fileToDelete.FileInfo.FullName);
                    logs.Add($"Delete : {fileToDelete.FileInfo.FullName}");
                }
                catch (Exception exception)
                {
                    logs.Add($"Delete : {fileToDelete.FileInfo.FullName} : {exception.Message}");
                }
            }

            return logs;
        }
    }
}
