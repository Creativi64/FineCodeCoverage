using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FineCodeCoverage.Utilities.AssemblyBinding
{
    internal abstract class DirectoryBindingRedirector
    {
        private string _assembliesDirectory;
        private List<AssemblyName> _assemblyNames;

        public DirectoryBindingRedirector() => AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            EnsureInitialized();
            var requested = new AssemblyName(args.Name);
            AssemblyName requestAssemblyName = _assemblyNames.FirstOrDefault(x => x.FullName == requested.FullName);
            if (requestAssemblyName == null)
            {
                return null;
            }

            string path = GetPath(requestAssemblyName);
            if (File.Exists(path))
            {
                return Assembly.LoadFrom(path);
            }

            return null;
        }

        private void EnsureInitialized()
        {
            if (_assembliesDirectory == null)
            {
                _assembliesDirectory = GetDirectory();
                _assemblyNames = Directory.GetFiles(_assembliesDirectory, "*.dll")
                    .Select(file => AssemblyName.GetAssemblyName(file)).ToList();
            }
        }

        private string GetPath(AssemblyName assemblyName)
            => Path.Combine(
                _assembliesDirectory,
                $"{assemblyName.Name}.dll");

        protected abstract string GetDirectory();
    }

}
