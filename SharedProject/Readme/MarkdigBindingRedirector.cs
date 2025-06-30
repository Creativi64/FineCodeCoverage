using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using FineCodeCoverage.Core.Initialization;
using FineCodeCoverage.Utilities.AssemblyBinding;

namespace FineCodeCoverage.Readme
{
    [Export(typeof(IInitializable))]
    internal sealed class MarkdigBindingRedirector : DirectoryBindingRedirector, IInitializable
    {
        protected override string GetDirectory() => Path.Combine(
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "markdig-redirects");

    }
}
