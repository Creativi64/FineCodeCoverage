using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.Engine;

namespace FineCodeCoverage.Collection.Runners
{
    internal interface ITUnitCoverage
    {
        bool Ready { get; }

        event EventHandler<bool> CollectingChangedEvent;

        event EventHandler ReadyEvent;

        void CollectCoverage();

        void Cancel();

        /// <summary>
        /// Called once the Test Explorer has built and is about to run the Microsoft.Testing.Platform
        /// test hosts.  Injects the coverage options into each host's testconfig.json so that the run
        /// Visual Studio is about to start collects coverage itself.
        /// </summary>
        /// <returns>
        /// The project file paths this collector owns, so the caller can keep them away from the VSTest
        /// collector - they do not run under VSTest.
        /// </returns>
        Task<IReadOnlyCollection<string>> PrepareForTestRunAsync();

        /// <summary>
        /// Called when the Test Explorer run has finished. Picks up only coverage produced by that run;
        /// never re-runs test hosts because the selected-test filter is unavailable.
        /// </summary>
        Task<CoverageCollectionResult> CollectAfterTestRunAsync();
    }
}
