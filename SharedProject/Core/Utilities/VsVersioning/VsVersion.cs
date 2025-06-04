using System;
using System.ComponentModel.Composition;
using Microsoft;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IVsVersion))]
    internal class VsVersion : IVsVersion
    {
        private readonly IServiceProvider _serviceProvider;
        private string _semanticVersion;
        private string _releaseVersion;
        private string _displayVersion;
        private string _editionName;

        [ImportingConstructor]
        public VsVersion(
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider
        )
        {
            this.Is2022 = IsVs2022.Value;
            this._serviceProvider = serviceProvider;
        }

        public bool Is2022 { get; }

        public string GetSemanticVersion()
        {
            if (this._semanticVersion == null)
            {
                this._semanticVersion = this.GetAppIdStringProperty(-8642);
            }

            return this._semanticVersion;
        }

        public string GetReleaseVersion()
        {
            if (this._releaseVersion == null)
            {
                this._releaseVersion = this.GetAppIdStringProperty(-8597);
            }

            return this._releaseVersion;
        }

        public string GetDisplayVersion()
        {
            if (this._displayVersion == null)
            {
                this._displayVersion = this.GetAppIdStringProperty(-8641);
            }

            return this._displayVersion;
        }

        public string GetEditionName()
        {
            if (this._editionName == null)
            {
                this._editionName = this.GetAppIdStringProperty(-8620);
            }

            return this._editionName;
        }

        private string GetAppIdStringProperty(int propId)
        {
            var vsAppId = this._serviceProvider.GetService(typeof(SVsAppId)) as IVsAppId;
            Assumes.Present(vsAppId);
            _ = vsAppId.GetProperty(propId, out object v);
            return v as string;
        }
    }
}
