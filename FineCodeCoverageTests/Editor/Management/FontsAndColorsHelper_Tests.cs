using FineCodeCoverage.Editor.Management;
using FineCodeCoverage.VSAbstractions.OutputWindow;
using FineCodeCoverageTests.TestHelpers;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.TextManager.Interop;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace FineCodeCoverageTests.Editor.Management
{
    internal class FontsAndColorsHelper_Tests
    {
        [TestCase(true)]
        [TestCase(false)]
        public async Task Should_Use_IVsFontAndColorStorage_Async(bool isBold)
        {
            var serviceProvider = new Mock<IServiceProvider>();
            var mockVsFontAndColorStorage = new Mock<IVsFontAndColorStorage>();
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
            mockVsFontAndColorStorage.Setup(vsFontsAndColorStorage => vsFontsAndColorStorage.GetItem("name", It.IsAny<ColorableItemInfo[]>()))
                .Callback<string, ColorableItemInfo[]>((_, colorableItemsInfos) =>
                {
                    var colorableItemInfo = new ColorableItemInfo
                    {
                        crBackground = (uint)System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.AliceBlue),
                        crForeground = (uint)System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Aqua),
                        dwFontFlags = isBold ? (uint)FONTFLAGS.FF_BOLD : (uint)FONTFLAGS.FF_DEFAULT
                    };
                    colorableItemsInfos[0] = colorableItemInfo;
                });
            serviceProvider.Setup(x => x.GetService(typeof(IVsFontAndColorStorage))).Returns(mockVsFontAndColorStorage.Object);
            var fontsAndColorsHelper = new FontsAndColorsHelper(serviceProvider.Object, new TestThreadHelper(), Mock.Of<IEditorFormatMapService>(), Mock.Of<ILogger>());
            var categoryGuid = Guid.NewGuid();
            var infos = await fontsAndColorsHelper.GetInfosAsync(categoryGuid, new List<string> { "name" });
            var info = infos[0];
            Assert.That(info.IsBold, Is.EqualTo(isBold));
            Assert.That(info.ItemCoverageColours.Background, Is.EqualTo(System.Windows.Media.Colors.AliceBlue));
            Assert.That(info.ItemCoverageColours.Foreground, Is.EqualTo(System.Windows.Media.Colors.Aqua));
            var flags = (uint)(__FCSTORAGEFLAGS.FCSF_READONLY | __FCSTORAGEFLAGS.FCSF_LOADDEFAULTS | __FCSTORAGEFLAGS.FCSF_NOAUTOCOLORS | __FCSTORAGEFLAGS.FCSF_PROPAGATECHANGES);
            mockVsFontAndColorStorage.Verify(x => x.OpenCategory(ref categoryGuid, flags), Times.Once);
            mockVsFontAndColorStorage.Verify(x => x.CloseCategory(), Times.Once);
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
        }

        [Test]
        public async Task Should_Use_Editor_Formats_When_OpenCategory_Fails_Async()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            var mockVsFontAndColorStorage = new Mock<IVsFontAndColorStorage>();
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
            mockVsFontAndColorStorage.Setup(vsFontAndColorStorage => vsFontAndColorStorage.OpenCategory(ref It.Ref<Guid>.IsAny, It.IsAny<uint>()))
                .Returns(VSConstants.E_FAIL);
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
            serviceProvider.Setup(x => x.GetService(typeof(IVsFontAndColorStorage))).Returns(mockVsFontAndColorStorage.Object);

            var formatMapService = CreateFormatMapService("name", false);
            var logger = new Mock<ILogger>();
            var fontsAndColorsHelper = new FontsAndColorsHelper(serviceProvider.Object, new TestThreadHelper(), formatMapService.Object, logger.Object);
            var infos = await fontsAndColorsHelper.GetInfosAsync(Guid.Empty, new List<string> { "name" });

            Assert.That(infos.Count, Is.EqualTo(1));
            Assert.That(infos[0].ItemCoverageColours.Background, Is.EqualTo(Colors.Red));
            Assert.That(infos[0].ItemCoverageColours.Foreground, Is.EqualTo(Colors.White));
            Assert.That(infos[0].IsBold, Is.True);
            logger.Verify(log => log.LogFileAndForget(It.IsAny<string[]>()), Times.Once);
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
            mockVsFontAndColorStorage.Verify(storage => storage.CloseCategory(), Times.Never);
            mockVsFontAndColorStorage.Verify(storage => storage.GetItem(It.IsAny<string>(), It.IsAny<ColorableItemInfo[]>()), Times.Never);
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task Should_Preserve_Item_Positions_When_GetItem_Fails_Async(bool useBrushes)
        {
            var serviceProvider = new Mock<IServiceProvider>();
            var mockVsFontAndColorStorage = new Mock<IVsFontAndColorStorage>();
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
            mockVsFontAndColorStorage.Setup(vsFontAndColorStorage => vsFontAndColorStorage.GetItem("name", It.IsAny<ColorableItemInfo[]>()))
                .Returns(VSConstants.E_FAIL);
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
            serviceProvider.Setup(x => x.GetService(typeof(IVsFontAndColorStorage))).Returns(mockVsFontAndColorStorage.Object);

            var formatMapService = CreateFormatMapService("name", useBrushes);
            var logger = new Mock<ILogger>();
            var fontsAndColorsHelper = new FontsAndColorsHelper(serviceProvider.Object, new TestThreadHelper(), formatMapService.Object, logger.Object);
            var infos = await fontsAndColorsHelper.GetInfosAsync(Guid.Empty, new[] { "before", "name", "after" });

            Assert.That(infos.Count, Is.EqualTo(3));
            Assert.That(infos[0].ItemCoverageColours.Background, Is.EqualTo(Colors.Black));
            Assert.That(infos[1].ItemCoverageColours.Background, Is.EqualTo(Colors.Red));
            Assert.That(infos[1].ItemCoverageColours.Foreground, Is.EqualTo(Colors.White));
            Assert.That(infos[1].IsBold, Is.True);
            Assert.That(infos[2].ItemCoverageColours.Background, Is.EqualTo(Colors.Black));
            logger.Verify(log => log.LogFileAndForget(It.IsAny<string[]>()), Times.Once);
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
            mockVsFontAndColorStorage.Verify(storage => storage.CloseCategory(), Times.Once);
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
        }

        [Test]
        public void Should_Report_Missing_Editor_Colors_And_Close_The_Category()
        {
            var storage = new Mock<IVsFontAndColorStorage>();
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
            storage.Setup(s => s.GetItem("missing", It.IsAny<ColorableItemInfo[]>())).Returns(VSConstants.E_FAIL);
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(s => s.GetService(typeof(IVsFontAndColorStorage))).Returns(storage.Object);
            var formatMap = new Mock<IEditorFormatMap>();
            formatMap.Setup(map => map.GetProperties("missing")).Returns(new ResourceDictionary());
            var formatMapService = new Mock<IEditorFormatMapService>();
            formatMapService.Setup(service => service.GetEditorFormatMap("text")).Returns(formatMap.Object);
            var helper = new FontsAndColorsHelper(serviceProvider.Object, new TestThreadHelper(), formatMapService.Object, Mock.Of<ILogger>());

            var exception = Assert.ThrowsAsync<InvalidOperationException>(() => helper.GetInfosAsync(Guid.Empty, new[] { "missing" }));

            Assert.That(exception.Message, Does.Contain("missing"));
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
            storage.Verify(s => s.CloseCategory(), Times.Once);
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
        }

        [TestCase(CoverageEditorFormatDefinitions.CoveredEditorFormatDefinitionName, "Green", "Black")]
        [TestCase(CoverageEditorFormatDefinitions.NotCoveredEditorFormatDefinitionName, "Red", "White")]
        [TestCase(CoverageEditorFormatDefinitions.PartiallyCoveredEditorFormatDefinitionName, "Yellow", "Black")]
        [TestCase(CoverageEditorFormatDefinitions.NewLinesEditorFormatDefinitionName, "Yellow", "Black")]
        [TestCase(CoverageEditorFormatDefinitions.DirtyEditorFormatDefinitionName, "Brown", "White")]
        [TestCase(CoverageEditorFormatDefinitions.NotIncludedEditorFormatDefinitionName, "LightPink", "Black")]
        [TestCase(MarkerTypeNames.Covered, "Green", "Black")]
        [TestCase(MarkerTypeNames.NotCovered, "Red", "White")]
        [TestCase(MarkerTypeNames.PartiallyCovered, "Yellow", "Black")]
        public async Task Should_Use_Defaults_When_Storage_And_Editor_Colors_Are_Missing_Async(string name, string background, string foreground)
        {
            var storage = new Mock<IVsFontAndColorStorage>();
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
            storage.Setup(s => s.GetItem(name, It.IsAny<ColorableItemInfo[]>())).Returns(VSConstants.E_FAIL);
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(s => s.GetService(typeof(IVsFontAndColorStorage))).Returns(storage.Object);
            var formatMap = new Mock<IEditorFormatMap>();
            formatMap.Setup(map => map.GetProperties(name)).Returns(new ResourceDictionary());
            var formatMapService = new Mock<IEditorFormatMapService>();
            formatMapService.Setup(service => service.GetEditorFormatMap("text")).Returns(formatMap.Object);
            var logger = new Mock<ILogger>();
            var helper = new FontsAndColorsHelper(serviceProvider.Object, new TestThreadHelper(), formatMapService.Object, logger.Object);

            var infos = await helper.GetInfosAsync(Guid.Empty, new[] { name });

            Assert.That(infos.Count, Is.EqualTo(1));
            Assert.That(infos[0].ItemCoverageColours.Background, Is.EqualTo((Color)ColorConverter.ConvertFromString(background)));
            Assert.That(infos[0].ItemCoverageColours.Foreground, Is.EqualTo((Color)ColorConverter.ConvertFromString(foreground)));
            Assert.That(infos[0].IsBold, Is.False);
            logger.Verify(log => log.LogFileAndForget(It.Is<string[]>(messages =>
                messages[0].Contains("FCC defaults"))), Times.Once);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task Should_Default_Only_The_Missing_Component_And_Keep_Custom_Colors_And_Bold_Async(bool hasForeground)
        {
            const string name = CoverageEditorFormatDefinitions.NotCoveredEditorFormatDefinitionName;
            var storage = new Mock<IVsFontAndColorStorage>();
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
            storage.Setup(s => s.OpenCategory(ref It.Ref<Guid>.IsAny, It.IsAny<uint>())).Returns(VSConstants.E_FAIL);
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(s => s.GetService(typeof(IVsFontAndColorStorage))).Returns(storage.Object);
            var properties = new ResourceDictionary
            {
                [ClassificationFormatDefinition.IsBoldId] = true,
                [hasForeground ? EditorFormatDefinition.ForegroundColorId : EditorFormatDefinition.BackgroundColorId] = Colors.Blue
            };
            var formatMap = new Mock<IEditorFormatMap>();
            formatMap.Setup(map => map.GetProperties(name)).Returns(properties);
            var formatMapService = new Mock<IEditorFormatMapService>();
            formatMapService.Setup(service => service.GetEditorFormatMap("text")).Returns(formatMap.Object);
            var helper = new FontsAndColorsHelper(serviceProvider.Object, new TestThreadHelper(), formatMapService.Object, Mock.Of<ILogger>());

            var infos = await helper.GetInfosAsync(Guid.Empty, new[] { name });

            Assert.That(infos[0].ItemCoverageColours.Foreground, Is.EqualTo(hasForeground ? Colors.Blue : Colors.White));
            Assert.That(infos[0].ItemCoverageColours.Background, Is.EqualTo(hasForeground ? Colors.Red : Colors.Blue));
            Assert.That(infos[0].IsBold, Is.True);
        }

        private static Mock<IEditorFormatMapService> CreateFormatMapService(string name, bool useBrushes)
        {
            var properties = new ResourceDictionary
            {
                [ClassificationFormatDefinition.IsBoldId] = true
            };
            if (useBrushes)
            {
                properties[EditorFormatDefinition.ForegroundBrushId] = new SolidColorBrush(Colors.White);
                properties[EditorFormatDefinition.BackgroundBrushId] = new SolidColorBrush(Colors.Red);
            }
            else
            {
                properties[EditorFormatDefinition.ForegroundColorId] = Colors.White;
                properties[EditorFormatDefinition.BackgroundColorId] = Colors.Red;
            }

            var formatMap = new Mock<IEditorFormatMap>();
            formatMap.Setup(map => map.GetProperties(name)).Returns(properties);
            var service = new Mock<IEditorFormatMapService>();
            service.Setup(s => s.GetEditorFormatMap("text")).Returns(formatMap.Object);
            return service;
        }
    }
}