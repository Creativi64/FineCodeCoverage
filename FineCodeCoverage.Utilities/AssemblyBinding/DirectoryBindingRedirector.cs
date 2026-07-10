using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FineCodeCoverage.Utilities.AssemblyBinding
{
    /*
        Not used as ProvideBindingPathAttribute SubPath works
    */
    internal abstract class DirectoryBindingRedirector
    {
        private string _assembliesDirectory;
        private List<AssemblyName> _assemblyNames;

        protected DirectoryBindingRedirector() => AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

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
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        }

        private void EnsureInitialized()
        {
            if (_assembliesDirectory != null)
            {
                return;
            }

            _assembliesDirectory = GetDirectory();
            _assemblyNames = Directory.GetFiles(_assembliesDirectory, "*.dll")
                .Select(file => AssemblyName.GetAssemblyName(file)).ToList();
        }

        private string GetPath(AssemblyName assemblyName)
            => Path.Combine(
                _assembliesDirectory,
                $"{assemblyName.Name}.dll");

        protected abstract string GetDirectory();
    }
}
