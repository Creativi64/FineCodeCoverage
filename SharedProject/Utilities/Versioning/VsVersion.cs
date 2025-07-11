using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.VSAbstractions.Versioning;
using Microsoft;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IVsVersion))]
    internal sealed class VsVersion : IVsVersion
    {
        private readonly IServiceProvider _serviceProvider;
        private string _semanticVersion;
        private string _releaseVersion;
        private string _displayVersion;
        private string _editionName;

        [ImportingConstructor]
        public VsVersion(
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider)
        {
            Is2022 = IsVs2022.Value;
            _serviceProvider = serviceProvider;
        }

        public bool Is2022 { get; }

        public string GetSemanticVersion()
        {
            if (_semanticVersion == null)
            {
                _semanticVersion = GetAppIdStringProperty(-8642);
            }

            return _semanticVersion;
        }

        public string GetReleaseVersion()
        {
            if (_releaseVersion == null)
            {
                _releaseVersion = GetAppIdStringProperty(-8597);
            }

            return _releaseVersion;
        }

        public string GetDisplayVersion()
        {
            if (_displayVersion == null)
            {
                _displayVersion = GetAppIdStringProperty(-8641);
            }

            return _displayVersion;
        }

        public string GetEditionName()
        {
            if (_editionName == null)
            {
                _editionName = GetAppIdStringProperty(-8620);
            }

            return _editionName;
        }

        private string GetAppIdStringProperty(int propId)
        {
            var vsAppId = _serviceProvider.GetService(typeof(SVsAppId)) as IVsAppId;
            Assumes.Present(vsAppId);
            _ = vsAppId.GetProperty(propId, out object v);
            return v as string;
        }
    }
}
