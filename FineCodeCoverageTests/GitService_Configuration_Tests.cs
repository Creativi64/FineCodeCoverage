using FineCodeCoverage.Output;
using NUnit.Framework;

namespace FineCodeCoverageTests
{
    public class GitService_Configuration_Tests
    {
        [Test]
        public void Supported_Visual_Studio_Build_Should_Include_Changeset_Integration()
        {
            var assembly = typeof(IGitService).Assembly;
            Assert.That(assembly.GetType("FineCodeCoverage.Output.GitService"), Is.Not.Null,
                "Both Debug and Release must compile the Git service for the supported Visual Studio host.");
            Assert.That(assembly.GetType("FineCodeCoverage.Output.GitService2019"), Is.Null,
                "The legacy fallback disables changeset views and must not be used in a VS 2026 build.");
        }
    }
}
