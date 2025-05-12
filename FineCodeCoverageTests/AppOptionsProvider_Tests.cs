using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMoq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Options;
using FineCodeCoverage.Output;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Threading;
using Moq;
using NUnit.Framework;

namespace FineCodeCoverageTests
{
    public class FakeAppOptions
    {
        public bool Property1 { get; set; }
        public static PropertyInfo PropertyInfo1 = typeof(FakeAppOptions).GetProperty(nameof(Property1));
    }

    class FakeObject
    {
        public static FakeObject Instance = new FakeObject();
    }

    class FakeException : Exception {
        private readonly string message;

        public FakeException(string message)
        {
            this.message = message;
        }
        public override string ToString()
        {
            return message;
        }
    }

    public class AppOptionsProvider_Tests
    {
        private AutoMoqer autoMocker;
        private AppOptionsProvider appOptionsProvider;
        private Mock<WritableSettingsStore> mockWritableSettingsStore;

        [SetUp]
        public void Setup()
        {
            autoMocker = new AutoMoqer();
            appOptionsProvider = autoMocker.Create<AppOptionsProvider>();
            mockWritableSettingsStore = new Mock<WritableSettingsStore>();
            var lazyMockWritableSettingsStore = new AsyncLazy<WritableSettingsStore>(() => Task.FromResult(mockWritableSettingsStore.Object),null);
            var mockWritableUserSettingsStoreProvider = autoMocker.GetMock<IWritableUserSettingsStoreProvider>();
            mockWritableUserSettingsStoreProvider.Setup(
                writableSettingsStoreProvider => writableSettingsStoreProvider.LazySettingsStore
            ).Returns(lazyMockWritableSettingsStore);
        }


        [Test]
        public void Should_Ensure_Store_When_LoadSettingsFromStorage()
        {
            appOptionsProvider.LoadSettingsFromStorage(new Mock<IAppOptions>().Object);
            mockWritableSettingsStore.Verify(writableSettingsStore => writableSettingsStore.CreateCollection("FineCodeCoverage"));
        }

        [Test]
        public void Should_Not_Create_Settings_Collection_If_Already_Exists()
        {
            mockWritableSettingsStore.Setup(writableSettingsStore => writableSettingsStore.CollectionExists("FineCodeCoverage")).Returns(true);
            appOptionsProvider.LoadSettingsFromStorage(new Mock<IAppOptions>().Object);
            mockWritableSettingsStore.Verify(writableSettingsStore => writableSettingsStore.CreateCollection("FineCodeCoverage"), Times.Never());
        }

        [Test]
        public void Should_Ensure_Store_When_SaveSettingsToStorage()
        {
            appOptionsProvider.SaveSettingsToStorage(new Mock<IAppOptions>().Object);
            mockWritableSettingsStore.Verify(writableSettingsStore => writableSettingsStore.CreateCollection("FineCodeCoverage"));
        }

        [Test]
        public void Should_Not_Create_Settings_Collection_If_Already_Exists_When_SaveSettingsToStorage()
        {
            mockWritableSettingsStore.Setup(writableSettingsStore => writableSettingsStore.CollectionExists("FineCodeCoverage")).Returns(true);
            appOptionsProvider.SaveSettingsToStorage(new Mock<IAppOptions>().Object);
            mockWritableSettingsStore.Verify(writableSettingsStore => writableSettingsStore.CreateCollection("FineCodeCoverage"), Times.Never());
        }

        [Test]
        public void IAppOptions_Should_Have_A_Getter_And_Setter_For_Each_Property()
        {
            var propertyInfos = typeof(IAppOptions).GetPublicProperties();
            Assert.True(propertyInfos.All(pi => pi.GetMethod != null && pi.SetMethod != null));
        }

        [Test]
        public void Should_Set_Defaults_When_Get()
        {
            var appOptions = appOptionsProvider.Get();
            autoMocker.Verify<IAppOptionsDefaults>(appOptionsDefaults => appOptionsDefaults.Set(appOptions));
        }

        [Test]
        public void Should_Set_Each_Property_With_The_Deserialized_Value_From_The_Store()
        {
            var mockReflectionService = autoMocker.GetMock<IReflectionService>();
            mockReflectionService.Setup(reflectionService => reflectionService.GetPublicProperties(typeof(IAppOptions)))
                .Returns(new PropertyInfo[] { FakeAppOptions.PropertyInfo1 });
            mockWritableSettingsStore.Setup(store => store.PropertyExists("FineCodeCoverage", FakeAppOptions.PropertyInfo1.Name)).Returns(true);
            mockWritableSettingsStore.Setup(store => store.GetString("FineCodeCoverage", FakeAppOptions.PropertyInfo1.Name)).Returns("serialized");
            autoMocker.Setup<IJsonConvertService, object>(jsonConvertService => jsonConvertService.DeserializeObject("serialized", FakeAppOptions.PropertyInfo1.PropertyType))
                .Returns(FakeObject.Instance);
            
            var appOptions = appOptionsProvider.Get();

            mockReflectionService.Verify(reflectionService => reflectionService.SetPropertyValue(FakeAppOptions.PropertyInfo1, appOptions, FakeObject.Instance));
        }

        [Test]
        public void Should_Override_Defaults_From_The_Store()
        {
            var storeSet = false;
            autoMocker.GetMock<IAppOptionsDefaults>().Setup(appOptionsDefaults => appOptionsDefaults.Set(It.IsAny<IAppOptions>()))
                .Callback(() => Assert.That(storeSet, Is.False));
            var mockReflectionService = autoMocker.GetMock<IReflectionService>();
            mockReflectionService.Setup(reflectionService => reflectionService.SetPropertyValue(FakeAppOptions.PropertyInfo1, It.IsAny<IAppOptions>(), FakeObject.Instance))
                .Callback(() => storeSet = true);

            mockReflectionService.Setup(reflectionService => reflectionService.GetPublicProperties(typeof(IAppOptions)))
                .Returns(new PropertyInfo[] { FakeAppOptions.PropertyInfo1 });
            mockWritableSettingsStore.Setup(store => store.PropertyExists("FineCodeCoverage", FakeAppOptions.PropertyInfo1.Name)).Returns(true);
            mockWritableSettingsStore.Setup(store => store.GetString("FineCodeCoverage", FakeAppOptions.PropertyInfo1.Name)).Returns("serialized");
            autoMocker.Setup<IJsonConvertService, object>(jsonConvertService => jsonConvertService.DeserializeObject("serialized", FakeAppOptions.PropertyInfo1.PropertyType))
                .Returns(FakeObject.Instance);

            var appOptions = appOptionsProvider.Get();

            
        }

        [Test]
        public void Should_Not_Set_Property_If_Is_Not_In_The_Store()
        {
            var mockReflectionService = autoMocker.GetMock<IReflectionService>();
            mockReflectionService.Setup(reflectionService => reflectionService.GetPublicProperties(typeof(IAppOptions)))
                .Returns(new PropertyInfo[] { FakeAppOptions.PropertyInfo1 });
            mockWritableSettingsStore.Setup(store => store.PropertyExists("FineCodeCoverage", FakeAppOptions.PropertyInfo1.Name)).Returns(false);

            var appOptions = appOptionsProvider.Get();

            mockReflectionService.Verify(reflectionService => reflectionService.SetPropertyValue(FakeAppOptions.PropertyInfo1, appOptions, It.IsAny<object>()), Times.Never());
        }

        [TestCase((string)null)]
        [TestCase("")]
        [TestCase("  ")]
        public void Should_Not_Set_A_Null_Or_Whitespace_Serialized_Value(string serializedValue)
        {
            var mockReflectionService = autoMocker.GetMock<IReflectionService>();
            mockReflectionService.Setup(reflectionService => reflectionService.GetPublicProperties(typeof(IAppOptions)))
                .Returns(new PropertyInfo[] { FakeAppOptions.PropertyInfo1 });
            mockWritableSettingsStore.Setup(store => store.PropertyExists("FineCodeCoverage", FakeAppOptions.PropertyInfo1.Name)).Returns(true);
            mockWritableSettingsStore.Setup(store => store.GetString("FineCodeCoverage", FakeAppOptions.PropertyInfo1.Name)).Returns(serializedValue);

            autoMocker.Setup<IJsonConvertService, object>(jsonConvertService => jsonConvertService.DeserializeObject("serialized", FakeAppOptions.PropertyInfo1.PropertyType))
                .Returns(FakeObject.Instance);

            var appOptions = appOptionsProvider.Get();
            autoMocker.Verify<IJsonConvertService>(jsonConvertService => jsonConvertService.DeserializeObject(It.IsAny<string>(), FakeAppOptions.PropertyInfo1.PropertyType), Times.Never());
            mockReflectionService.Verify(reflectionService => reflectionService.SetPropertyValue(FakeAppOptions.PropertyInfo1, appOptions, It.IsAny<object>()), Times.Never());
        }

        [Test]
        public void Should_Log_Exception_Loading()
        {
            var mockReflectionService = autoMocker.GetMock<IReflectionService>();
            mockReflectionService.Setup(reflectionService => reflectionService.GetPublicProperties(typeof(IAppOptions)))
                .Returns(new PropertyInfo[] { FakeAppOptions.PropertyInfo1 });
            mockWritableSettingsStore.Setup(store => store.PropertyExists("FineCodeCoverage", FakeAppOptions.PropertyInfo1.Name)).Returns(true);
            mockWritableSettingsStore.Setup(store => store.GetString("FineCodeCoverage", FakeAppOptions.PropertyInfo1.Name)).Returns("serialized");
            var exception = new FakeException("oh no");
            autoMocker.Setup<IJsonConvertService, object>(jsonConvertService => jsonConvertService.DeserializeObject("serialized", FakeAppOptions.PropertyInfo1.PropertyType))
                .Throws(exception);

            var appOptions = appOptionsProvider.Get();

            autoMocker.Verify<ILogger>(logger => logger.LogFileAndForget("Failed to load 'Property1' setting", "oh no"));

        }

        [Test]
        public void Should_Save_All_Serialized_Properties_To_The_Store()
        {
            var mockReflectionService = autoMocker.GetMock<IReflectionService>();
            mockReflectionService.Setup(reflectionService => reflectionService.GetPublicProperties(typeof(IAppOptions)))
                .Returns(new PropertyInfo[] { FakeAppOptions.PropertyInfo1 });
            var appOptions = new Mock<IAppOptions>().Object;
            mockReflectionService.Setup(reflectionService => reflectionService.GetPropertyValue(FakeAppOptions.PropertyInfo1, appOptions))
                .Returns(FakeObject.Instance);
            autoMocker.Setup<IJsonConvertService, object>(jsonConvertService => jsonConvertService.SerializeObject(FakeObject.Instance))
                .Returns("serialized");

            appOptionsProvider.SaveSettingsToStorage(appOptions);

            mockWritableSettingsStore.Verify(store => store.SetString("FineCodeCoverage", FakeAppOptions.PropertyInfo1.Name, "serialized"));
        }

        [Test]
        public void Should_Log_Exception_When_Saving_To_The_Store()
        {
            var mockReflectionService = autoMocker.GetMock<IReflectionService>();
            mockReflectionService.Setup(reflectionService => reflectionService.GetPublicProperties(typeof(IAppOptions)))
                .Returns(new PropertyInfo[] { FakeAppOptions.PropertyInfo1 });
            var appOptions = new Mock<IAppOptions>().Object;
            mockReflectionService.Setup(reflectionService => reflectionService.GetPropertyValue(FakeAppOptions.PropertyInfo1, appOptions))
                .Returns(FakeObject.Instance);
            autoMocker.Setup<IJsonConvertService, object>(jsonConvertService => jsonConvertService.SerializeObject(FakeObject.Instance))
                .Throws(new FakeException("oh no"));

            appOptionsProvider.SaveSettingsToStorage(appOptions);

            autoMocker.Verify<ILogger>(logger => logger.LogFileAndForget("Failed to save 'Property1' setting", "oh no"));
        }

        [Test]
        public void Should_Raise_Options_Changed_When_SaveSettingsToStorage()
        {
            var mockAppOptions = new Mock<IAppOptions>();
            var appOptions = mockAppOptions.Object;

            IAppOptions changedOptions = null;
            appOptionsProvider.OptionsChanged += (options) =>
            {
                changedOptions = options;
            };

            appOptionsProvider.SaveSettingsToStorage(appOptions);

            Assert.AreSame(appOptions, changedOptions);
        }
    }
}
