using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;
using Microsoft.VisualStudio.Settings;
using FineCodeCoverage.Options;
using FineCodeCoverage.Output;
using FineCodeCoverage.Core.Utilities;
using Moq;
using AutoMoq;
using Microsoft.VisualStudio.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverageTests
{
    public class Options_Tests
    {
        public class DerivationAndOptionTypes
        {
            public DerivationAndOptionTypes(Type derivationType, Type optionType)
            {
                DerivationType = derivationType;
                OptionType = optionType;
            }

            public Type DerivationType { get; }
            public Type OptionType { get; }
        }
        public static class ReflectionHelper
        {
            public static IEnumerable<DerivationAndOptionTypes> GetDerivedOptionsProviders()
            {
                var baseTypeDefinition = typeof(OptionsProviderBase<>);

                return baseTypeDefinition.Assembly
                    .GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && InheritsFromGeneric(t, baseTypeDefinition))
                    .Select(t => (DerivedType: t, GenericArgument: GetGenericArgument(t, baseTypeDefinition)))
                    .Where(result => result.GenericArgument != null)
                    .Select(result => new DerivationAndOptionTypes(result.DerivedType, result.GenericArgument));
            }

            private static bool InheritsFromGeneric(Type type, Type genericBase)
            {
                while (type != null && type != typeof(object))
                {
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == genericBase)
                        return true;
                    type = type.BaseType;
                }
                return false;
            }

            private static Type GetGenericArgument(Type type, Type genericBase)
            {
                while (type != null && type != typeof(object))
                {
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == genericBase)
                        return type.GetGenericArguments()[0];
                    type = type.BaseType;
                }
                return null;
            }
        }
        [Test]
        public void Should_Not_Have_Different_Options_With_The_Same_Property_Name()
        {
            var allDerivationAndOptionTypes = ReflectionHelper.GetDerivedOptionsProviders().ToList();
            var allOptionPropertyNames = allDerivationAndOptionTypes.Select(d => d.OptionType).SelectMany(optionType => TypeDescriptor.GetProperties(optionType).Cast<PropertyDescriptor>().Select(p => p.Name));
            var grs = allOptionPropertyNames.GroupBy(n => n).Where(g => g.Count() > 1);
            Assert.That(allOptionPropertyNames.Count, Is.EqualTo(allOptionPropertyNames.Distinct().Count()));
        }
    }

    enum AnEnum { One, Two}

    class TestOptions
    {
        public bool DefaultFalse { get; set; }
        public bool DefaultTrueNotInStore { get; set; }
        public string[] ArrayProperty { get; set; }
        public AnEnum EnumProperty { get; set; }
        public int IntProperty { get; set; }
        public double? NullableDoubleProperty { get; set; }
        public string StringProperty { get; set; }
    }

    class TestOptionsProvider : OptionsProviderBase<TestOptions>
    {
        public TestOptionsProvider(
            ILogger logger, 
            IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider, 
            IJsonConvertService jsonConvertService, 
            IDefaultOptionsSetter<TestOptions> defaultOptionsSetter) 
            : base(logger, writableUserSettingsStoreProvider, jsonConvertService, defaultOptionsSetter)
        {
        }
    }

    class TestOptionsDefaultsSetter : IDefaultOptionsSetter<TestOptions>
    {
        public void Set(TestOptions option)
        {
            option.DefaultTrueNotInStore = true;
        }
    }

    class FakeException : Exception
    {
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

    public class OptionsProviderTests
    {
        private Mock<WritableSettingsStore> mockWritableSettingsStore;
        private AutoMoqer autoMocker;
        private TestOptionsProvider testOptionsProvider;

        [SetUp]
        public void Setup()
        {
            autoMocker = new AutoMoqer();
            autoMocker.SetInstance<IDefaultOptionsSetter<TestOptions>>(new TestOptionsDefaultsSetter());
            testOptionsProvider = autoMocker.Create<TestOptionsProvider>();
            mockWritableSettingsStore = new Mock<WritableSettingsStore>();
            var lazyMockWritableSettingsStore = new AsyncLazy<WritableSettingsStore>(() => Task.FromResult(mockWritableSettingsStore.Object), null);
            var mockWritableUserSettingsStoreProvider = autoMocker.GetMock<IWritableUserSettingsStoreProvider>();
            mockWritableUserSettingsStoreProvider.Setup(
                writableSettingsStoreProvider => writableSettingsStoreProvider.LazySettingsStore
            ).Returns(lazyMockWritableSettingsStore);
            autoMocker.Setup<IJsonConvertService, object>(jsonConvertService => jsonConvertService.DeserializeObject("Serialized", typeof(bool))).Returns(true);
        }

        private void SetUpPropertyInStore(string value)
        {
            mockWritableSettingsStore.Setup(store => store.PropertyExists("FineCodeCoverage", nameof(TestOptions.DefaultFalse))).Returns(true);
            mockWritableSettingsStore.Setup(store => store.GetString("FineCodeCoverage", nameof(TestOptions.DefaultFalse))).Returns(value);
        }

        private void DialogPageBase_LoadSettingsFromStorage()
            => (testOptionsProvider as IDialogPageOptionsProvider<TestOptions>).LoadSettingsFromStorage();

        private TestOptions DialogPageBase_LoadSettingsFromStorage_Then_Get()
        {
            DialogPageBase_LoadSettingsFromStorage();
            return testOptionsProvider.Get();
        }

        [Test]
        public void Should_Have_Newed_Options_Loaded_From_Storage_With_Defaults_When_Get()
        {
            SetUpPropertyInStore("Serialized");

            var testOptions = DialogPageBase_LoadSettingsFromStorage_Then_Get();
            
            Assert.That(testOptions.DefaultFalse, Is.True);
            Assert.That(testOptions.DefaultTrueNotInStore, Is.True);
            Assert.That(testOptions.ArrayProperty, Is.Null);
        }

        [Test]
        public void Should_Return_Options_When_IProfileOptionsProvider_LoadSettingsFromStorage()
        {
            SetUpPropertyInStore("Serialized");

            DialogPageBase_LoadSettingsFromStorage();

            var testOptions = (testOptionsProvider as IProfileOptionsProvider).Options as TestOptions;
            Assert.That(testOptions.DefaultFalse, Is.True);
            Assert.That(testOptions.DefaultTrueNotInStore, Is.True);
        }

        [TestCase((string)null)]
        [TestCase("  ")]
        public void Should_Not_Deserialize_Null_Or_Whitespace_Store_Values(string storePropertyValue)
        {
            SetUpPropertyInStore(storePropertyValue);

            var testOptions = DialogPageBase_LoadSettingsFromStorage_Then_Get();

            autoMocker.Verify<IJsonConvertService>(jsonConvertService => jsonConvertService.DeserializeObject(It.IsAny<string>(), typeof(bool)), Times.Never());
            
            Assert.That(testOptions.DefaultFalse, Is.False);
            Assert.That(testOptions.DefaultTrueNotInStore, Is.True);
        }

        [Test]
        public void Should_Ensure_Store_When_LoadSettingsFromStorage()
        {
            DialogPageBase_LoadSettingsFromStorage();

            mockWritableSettingsStore.Verify(writableSettingsStore => writableSettingsStore.CreateCollection("FineCodeCoverage"));
        }

        [Test]
        public void Should_Not_Create_Settings_Collection_If_Already_Exists()
        {
            mockWritableSettingsStore.Setup(writableSettingsStore => writableSettingsStore.CollectionExists("FineCodeCoverage")).Returns(true);

            DialogPageBase_LoadSettingsFromStorage();

            mockWritableSettingsStore.Verify(writableSettingsStore => writableSettingsStore.CreateCollection("FineCodeCoverage"), Times.Never());
        }

        [Test]
        public void Should_Log_Exceptions_When_LoadSettingsFromStorage()
        {
            SetUpPropertyInStore("...");
            autoMocker.Setup<IJsonConvertService, object>(jsonConvertService => jsonConvertService.DeserializeObject("...", typeof(bool)))
                .Throws(new FakeException("oh no"));

            DialogPageBase_LoadSettingsFromStorage();

            autoMocker.Verify<ILogger>(logger => logger.LogFileAndForget($"Failed to load '{nameof(TestOptions.DefaultFalse)}' setting", "oh no"));
        }

        [Test]
        public void Should_Be_Initializing_When_LoadSettingsFromStorage()
        {
            SetUpPropertyInStore("...");
            var initializing = false;
            autoMocker.Setup<IJsonConvertService, object>(jsonConvertService => jsonConvertService.DeserializeObject("...", typeof(bool)))
                .Callback(() => initializing = testOptionsProvider.Initializing);

            Assert.That(testOptionsProvider.Initializing, Is.False);
            DialogPageBase_LoadSettingsFromStorage();

            Assert.That(initializing, Is.True);
            Assert.That(testOptionsProvider.Initializing, Is.False);

        }

        [Test]
        public void Should_Not_Save_SettingsToStorage_When_Initializing()
        {
            testOptionsProvider.Initializing = true;

            (testOptionsProvider as IDialogPageOptionsProvider<TestOptions>).SaveSettingsToStorage();

            autoMocker.Verify<IJsonConvertService>(jsonConvertService => jsonConvertService.SerializeObject(It.IsAny<object>()), Times.Never());
            Assert.True(testOptionsProvider.Initializing);
        }

        [Test]
        public void Should_Ensure_Store_When_SaveSettingsToStorage()
        {
            (testOptionsProvider as IDialogPageOptionsProvider<TestOptions>).SaveSettingsToStorage();
            
            mockWritableSettingsStore.Verify(writableSettingsStore => writableSettingsStore.CreateCollection("FineCodeCoverage"));
        }

        [Test]
        public void Should_Not_Create_Settings_Collection_If_Already_Exists_When_SaveSettingsToStorage()
        {
            mockWritableSettingsStore.Setup(writableSettingsStore => writableSettingsStore.CollectionExists("FineCodeCoverage")).Returns(true);
            
            (testOptionsProvider as IDialogPageOptionsProvider<TestOptions>).SaveSettingsToStorage();
            
            mockWritableSettingsStore.Verify(writableSettingsStore => writableSettingsStore.CreateCollection("FineCodeCoverage"), Times.Never());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Save_Json_Serialized_Property_Values_ToStore_When_SaveSettingsToStorage(bool viaIProfileOptionsProvider)
        {
            var options = (testOptionsProvider as IDialogPageOptionsProvider<TestOptions>).Options;
            Assert.That(options.DefaultTrueNotInStore, Is.True);

            options.DefaultTrueNotInStore = false;
            options.DefaultFalse = true;

            var mockJsonConvertService = autoMocker.GetMock<IJsonConvertService>();
            mockJsonConvertService.Setup(jsonConvertService => jsonConvertService.SerializeObject(true)).Returns("true");
            mockJsonConvertService.Setup(jsonConvertService => jsonConvertService.SerializeObject(false)).Returns("false");

            if (viaIProfileOptionsProvider)
            {
                (testOptionsProvider as IProfileOptionsProvider).SaveSettingsToStorage();
            }
            else
            {
                (testOptionsProvider as IDialogPageOptionsProvider<TestOptions>).SaveSettingsToStorage();
            }
               
            mockWritableSettingsStore.Verify(store => store.SetString("FineCodeCoverage", nameof(TestOptions.DefaultTrueNotInStore), "false"));
            mockWritableSettingsStore.Verify(store => store.SetString("FineCodeCoverage", nameof(TestOptions.DefaultFalse), "true"));
        }

        [Test]
        public void Should_Raise_Options_Changed_When_SaveSettingsToStorage()
        {
            TestOptions handlerOptions = null;
            Action<TestOptions> optionsChanged = (changedOptions) =>
            {
                handlerOptions = changedOptions;
            };
            testOptionsProvider.OptionsChanged += optionsChanged;

            (testOptionsProvider as IDialogPageOptionsProvider<TestOptions>).SaveSettingsToStorage();

            Assert.That(handlerOptions, Is.SameAs((testOptionsProvider as IDialogPageOptionsProvider<TestOptions>).Options));
        }

        [Test]
        public void Should_Log_Exception_When_SaveSettingsToStorage()
        {
            var mockJsonConvertService = autoMocker.GetMock<IJsonConvertService>();
            mockJsonConvertService.Setup(jsonConvertService => jsonConvertService.SerializeObject(It.IsAny<object>())).
                Throws(new FakeException("oh no"));

            (testOptionsProvider as IDialogPageOptionsProvider<TestOptions>).SaveSettingsToStorage();

            autoMocker.Verify<ILogger>(logger => logger.LogFileAndForget($"Failed to save '{nameof(TestOptions.DefaultFalse)}' setting", "oh no"));
        }

        [Test]
        public void Should_Set_Each_Option_Property_To_Default_For_Property_Type_Apply_Defaults_And_SaveSettingsToStorage_When_Reset()
        {
            var mockJsonConvertService = autoMocker.GetMock<IJsonConvertService>();
            mockJsonConvertService.Setup(jsonConvertService => jsonConvertService.SerializeObject(true)).Returns("true");
            mockJsonConvertService.Setup(jsonConvertService => jsonConvertService.SerializeObject(false)).Returns("false");
            mockJsonConvertService.Setup(jsonConvertService => jsonConvertService.SerializeObject(null)).Returns("null");

            var options = testOptionsProvider.Get();
            options.DefaultFalse = true;
            options.DefaultTrueNotInStore = false;
            options.ArrayProperty = new[] { "1", "2" };
            options.StringProperty = "set";
            options.EnumProperty = AnEnum.Two;
            options.IntProperty = 1;
            options.NullableDoubleProperty = 1.0;

            testOptionsProvider.Reset();

            Assert.That(options.DefaultFalse, Is.False);
            Assert.That(options.DefaultTrueNotInStore, Is.True);
            Assert.That(options.ArrayProperty, Is.Null);
            Assert.That(options.StringProperty, Is.Null);
            Assert.That(options.EnumProperty, Is.EqualTo(AnEnum.One));
            Assert.That(options.IntProperty, Is.EqualTo(0));
            Assert.That(options.NullableDoubleProperty, Is.Null);

            mockWritableSettingsStore.Verify(store => store.SetString("FineCodeCoverage", nameof(TestOptions.DefaultTrueNotInStore), "true"));
            mockWritableSettingsStore.Verify(store => store.SetString("FineCodeCoverage", nameof(TestOptions.DefaultFalse), "false"));
            mockWritableSettingsStore.Verify(store => store.SetString("FineCodeCoverage", nameof(TestOptions.ArrayProperty), "null"));
        }
    }
}
