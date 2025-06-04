using System;
using System.Collections.Generic;
using System.Linq;
using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Options;

namespace FineCodeCoverage.Editor.Tagging.Base
{
    internal abstract class CoverageTypeFilterBase : ICoverageTypeFilter
    {
        private static readonly Dictionary<DynamicCoverageType, bool> s_doNotShowLookup = new Dictionary<DynamicCoverageType, bool>()
        {
            { DynamicCoverageType.Covered, false },
            { DynamicCoverageType.Partial, false },
            { DynamicCoverageType.NotCovered, false },
            { DynamicCoverageType.Dirty, false },
            { DynamicCoverageType.NewLine, false },
            { DynamicCoverageType.NotIncluded, false }
        };
        private Dictionary<DynamicCoverageType, bool> _showLookup = s_doNotShowLookup;

        public void Initialize(EditorCoverageColouringOptions editorCoverageColouringOptions)
        {
            if (!this.ShouldGetShowLookup(editorCoverageColouringOptions))
            {
                return;
            }

            this._showLookup = this.GetShowLookup(editorCoverageColouringOptions);
            this.ThrowIfInvalidShowLookup();
        }

        private bool ShouldGetShowLookup(EditorCoverageColouringOptions editorCoverageColouringOptions) => editorCoverageColouringOptions.ShowEditorCoverage && this.EnabledPrivate(editorCoverageColouringOptions);

        private void ThrowIfInvalidShowLookup()
        {
            if (this._showLookup?.Count == 6)
            {
                return;
            }

            throw new InvalidOperationException("Invalid showLookup");
        }

        private bool EnabledPrivate(EditorCoverageColouringOptions editorCoverageColouringOptions)
        {
            bool enabled = this.Enabled(editorCoverageColouringOptions);
            this.Disabled = !enabled;
            return enabled;
        }

        protected abstract bool Enabled(EditorCoverageColouringOptions editorCoverageColouringOptions);
        protected abstract Dictionary<DynamicCoverageType, bool> GetShowLookup(EditorCoverageColouringOptions editorCoverageColouringOptions);

        public abstract string TypeIdentifier { get; }

        public bool Disabled { get; set; } = true;

        public bool Show(DynamicCoverageType coverageType) => this._showLookup[coverageType];

        public bool Changed(ICoverageTypeFilter other)
        {
            this.ThrowIfIncorrectCoverageTypeFilter(other);

            return this.CompareLookups(((CoverageTypeFilterBase)other)._showLookup);
        }

        private bool CompareLookups(Dictionary<DynamicCoverageType, bool> otherShowLookup)
            => Enum.GetValues(typeof(DynamicCoverageType)).Cast<DynamicCoverageType>()
                .Any(coverageType => this._showLookup[coverageType] != otherShowLookup[coverageType]);

        private void ThrowIfIncorrectCoverageTypeFilter(ICoverageTypeFilter other)
        {
            if (other.TypeIdentifier == this.TypeIdentifier)
            {
                return;
            }

            throw new ArgumentException("Argument of incorrect type", nameof(other));
        }
    }
}
