using FineCodeCoverage.Collection.TestingPlatform.TUnit;
using NUnit.Framework;

namespace FineCodeCoverageTests
{
	public class TUnitProjectFactory_Tests
	{
		[TestCase(false, "true", "true")]
		[TestCase(false, "true", "TRUE")]
		[TestCase(true, "true", "true")]
		[TestCase(true, null, "true")]
		public void Should_Not_Exclude_VsTest_Compatibility_Projects_From_VsTest_Coverage(
			bool hasTUnit,
			string isTestingPlatformApplication,
			string disableTestingPlatformServerCapability)
		{
			Assert.That(TUnitProjectFactory.IsNativeTestingPlatformProject(
				hasTUnit,
				isTestingPlatformApplication,
				disableTestingPlatformServerCapability), Is.False);
		}

		[TestCase(false, "true", null)]
		[TestCase(false, "true", "")]
		[TestCase(false, "true", "false")]
		[TestCase(false, "TRUE", "FALSE")]
		[TestCase(true, null, null)]
		[TestCase(true, "false", "false")]
		public void Should_Keep_Native_Testing_Platform_Projects_In_The_Mtp_Collector(
			bool hasTUnit,
			string isTestingPlatformApplication,
			string disableTestingPlatformServerCapability)
		{
			Assert.That(TUnitProjectFactory.IsNativeTestingPlatformProject(
				hasTUnit,
				isTestingPlatformApplication,
				disableTestingPlatformServerCapability), Is.True);
		}

		[TestCase(null)]
		[TestCase("")]
		[TestCase("false")]
		public void Should_Keep_Ordinary_VsTest_Projects_Out_Of_The_Mtp_Collector(string isTestingPlatformApplication)
		{
			Assert.That(TUnitProjectFactory.IsNativeTestingPlatformProject(
				false,
				isTestingPlatformApplication,
				null), Is.False);
		}

		[Test]
		public void Should_Update_Routing_When_Compatibility_Mode_Changes_Without_Package_Changes()
		{
			Assert.Multiple(() =>
			{
				Assert.That(TUnitProjectFactory.IsNativeTestingPlatformProject(false, "true", null), Is.True);
				Assert.That(TUnitProjectFactory.IsNativeTestingPlatformProject(false, "true", "true"), Is.False);
				Assert.That(TUnitProjectFactory.IsNativeTestingPlatformProject(false, "true", "false"), Is.True);
			});
		}
	}
}
