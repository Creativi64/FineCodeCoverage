using FineCodeCoverage.Collection.TestingPlatform;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace FineCodeCoverageTests
{
	public class TestingPlatformCompatibility_Tests
	{
		private static JObject CreateAssets(
			string platformVersion = "1.9.1",
			string adapter = "MSTest.TestAdapter",
			bool includeTestSdk = true)
		{
			var libraries = new JObject();
			if (platformVersion != null)
			{
				libraries["Microsoft.Testing.Platform/" + platformVersion] = new JObject();
			}

			if (adapter != null)
			{
				libraries[adapter + "/3.11.1"] = new JObject();
			}

			if (includeTestSdk)
			{
				libraries["Microsoft.NET.Test.Sdk/17.13.0"] = new JObject();
			}

			return new JObject
			{
				["libraries"] = libraries,
				["project"] = new JObject
				{
					["frameworks"] = new JObject
					{
						["net48"] = new JObject
						{
							["dependencies"] = new JObject
							{
								["HeadOnTestInfrastructure"] = new JObject { ["version"] = "[2.4.0, )" },
							},
						},
					},
				},
			};
		}

		[TestCase("1.9.1", "MSTest.TestAdapter")]
		[TestCase("2.0.2", "MSTest.TestAdapter")]
		[TestCase("2.2.3", "NUnit3TestAdapter")]
		[TestCase("1.9.1", "xunit.runner.visualstudio")]
		[TestCase("1.9.1", "mstest.testadapter")]
		[TestCase("2.2.3-preview.1", "MSTest.TestAdapter")]
		public void Should_Select_VsTest_For_Legacy_Mtp_With_Transitive_Adapter(string version, string adapter)
		{
			Assert.That(TestingPlatformCompatibility.RequiresVsTest(CreateAssets(version, adapter).ToString()), Is.True);
		}

		[TestCase("2.3.0")]
		[TestCase("2.3.3")]
		[TestCase("3.0.0")]
		public void Should_Keep_Supported_Mtp_Versions_Native(string version)
		{
			Assert.That(TestingPlatformCompatibility.RequiresVsTest(CreateAssets(version).ToString()), Is.False);
		}

		[TestCase(null)]
		[TestCase("not-a-version")]
		public void Should_Not_Change_Routing_When_The_Mtp_Version_Is_Unknown(string version)
		{
			Assert.That(TestingPlatformCompatibility.RequiresVsTest(CreateAssets(version).ToString()), Is.False);
		}

		[TestCase(null)]
		[TestCase("SomeOther.TestAdapter")]
		public void Should_Not_Select_VsTest_Without_A_Known_Adapter(string adapter)
		{
			Assert.That(TestingPlatformCompatibility.RequiresVsTest(CreateAssets(adapter: adapter).ToString()), Is.False);
		}

		[Test]
		public void Should_Not_Select_VsTest_Without_The_Test_Sdk()
		{
			Assert.That(TestingPlatformCompatibility.RequiresVsTest(CreateAssets(includeTestSdk: false).ToString()), Is.False);
		}

		[TestCase("TUnit")]
		[TestCase("TUnit.Engine")]
		public void Should_Never_Automatically_Force_TUnit_Through_VsTest(string packageId)
		{
			JObject assets = CreateAssets();
			assets["libraries"][packageId + "/1.0.0"] = new JObject();

			Assert.That(TestingPlatformCompatibility.RequiresVsTest(assets.ToString()), Is.False);
		}

		[TestCase(null)]
		[TestCase("")]
		[TestCase("{}")]
		[TestCase("{\"libraries\":[]}")]
		public void Should_Not_Change_Routing_Without_Resolved_Package_Data(string assets)
		{
			Assert.That(TestingPlatformCompatibility.RequiresVsTest(assets), Is.False);
		}

		[Test]
		public void Should_Surface_Invalid_Assets_To_The_Callers_Diagnostic_Handler()
		{
			Assert.Throws<JsonReaderException>(() => TestingPlatformCompatibility.RequiresVsTest("not json"));
		}

		[Test]
		public void Should_Make_Independent_Decisions_For_Projects_With_Different_Versions()
		{
			Assert.Multiple(() =>
			{
				Assert.That(TestingPlatformCompatibility.RequiresVsTest(CreateAssets().ToString()), Is.True);
				Assert.That(TestingPlatformCompatibility.RequiresVsTest(CreateAssets("2.3.3").ToString()), Is.False);
				Assert.That(TestingPlatformCompatibility.RequiresVsTest(CreateAssets().ToString()), Is.True);
			});
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Should_Not_Force_Multi_Targeted_Projects_With_A_Modern_Mtp_Version(bool modernFirst)
		{
			JObject assets = CreateAssets();
			var libraries = (JObject)assets["libraries"];
			var modernLibrary = new JProperty("Microsoft.Testing.Platform/2.3.3", new JObject());
			if (modernFirst)
			{
				libraries.AddFirst(modernLibrary);
			}
			else
			{
				libraries.Add(modernLibrary);
			}

			Assert.That(TestingPlatformCompatibility.RequiresVsTest(assets.ToString()), Is.False);
		}
	}
}
