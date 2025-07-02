using FineCodeCoverage.Collection.CoverletOpenCover.Coverlet.DataCollector;
using NUnit.Framework;

namespace Test
{
    public class RunSettingsCoverletConfigurationFactory_Tests
    {
        [Test]
        public void Should_Create_An_Instance_Of_RunSettingsCoverletConfiguration()
        {
            var runSettingsCoverletConfigurationFactory = new RunSettingsCoverletConfigurationFactory();
            Assert.IsInstanceOf<RunSettingsCoverletConfiguration>(runSettingsCoverletConfigurationFactory.Create());
        }
    }
}