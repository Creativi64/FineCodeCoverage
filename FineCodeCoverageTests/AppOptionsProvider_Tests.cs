using System;
using System.Collections.Generic;
using System.Linq;
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
    
    internal class NullStringArrayDefaultValueProvider : LookupOrFallbackDefaultValueProvider
    {
        public NullStringArrayDefaultValueProvider()
        {
            base.Register(typeof(string[]), (_, __) => null);
        }
    }
}
