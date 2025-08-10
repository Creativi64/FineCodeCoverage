using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Editor.DynamicCoverage.Common;
using FineCodeCoverage.Editor.Tagging.Base;
using FineCodeCoverage.Editor.Tagging.Classification;
using FineCodeCoverage.Options.EditorCoverageColouring;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace FineCodeCoverageTests.Editor.Tagging.Base
{
    internal class CoverageTypeFilterExceptions : CoverageTypeFilterBase
    {
        public override string TypeIdentifier => "";

        protected override bool Enabled(EditorCoverageColouringOptions appOptions)
        {
            return true;
        }
        public Func<Dictionary<DynamicCoverageType, bool>> ShowLookup;
        protected override Dictionary<DynamicCoverageType, bool> GetShowLookup(EditorCoverageColouringOptions appOptions)
        {
            return ShowLookup?.Invoke();
        }
    }

    internal class CoverageTypeFilterBase_Exception_Tests
    {
        [Test]
        public void Should_Throw_If_ShowLookup_Null()
        {
            var coverageTypeFilterExceptions = new CoverageTypeFilterExceptions();

            Assert.Throws<InvalidOperationException>(() => coverageTypeFilterExceptions.Initialize(new EditorCoverageColouringOptions { ShowEditorCoverage = true}));

        }

        [Test]
        public void Should_Throw_If_Incomplete_ShowLookup()
        {
            var coverageTypeFilterExceptions = new CoverageTypeFilterExceptions();
            coverageTypeFilterExceptions.ShowLookup = () => new Dictionary<DynamicCoverageType, bool>
            {
                { DynamicCoverageType.Covered, true },
                { DynamicCoverageType.NotCovered, true }
            };

            Assert.Throws<InvalidOperationException>(() => coverageTypeFilterExceptions.Initialize(new EditorCoverageColouringOptions { ShowEditorCoverage = true}));

        }

        [Test]
        public void Should_Throw_When_Comparing_Different_ICoverageTypeFilter_For_Changes()
        {
            var coverageTypeFilterExceptions = new CoverageTypeFilterExceptions();
            coverageTypeFilterExceptions.ShowLookup = () => new Dictionary<DynamicCoverageType, bool>
            {
                { DynamicCoverageType.Covered, true },
                { DynamicCoverageType.NotCovered, true },
                { DynamicCoverageType.Partial,true },
                { DynamicCoverageType.Dirty, true },
                { DynamicCoverageType.NewLine,true },
                { DynamicCoverageType.NotIncluded,true },
            };

            var other = new CoverageClassificationFilter();
            Assert.Throws<ArgumentException>(() => coverageTypeFilterExceptions.Changed(other));
        }
    }
}