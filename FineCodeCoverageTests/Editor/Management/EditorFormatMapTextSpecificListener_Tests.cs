using AutoMoq;
using FineCodeCoverage.Editor.Management;
using Microsoft.VisualStudio.Text.Classification;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FineCodeCoverageTests.Editor.Management
{
    internal class EditorFormatMapTextSpecificListener_Tests
    {
        [Test]
        public void Should_Ignore_Format_Registration_Changes_Before_Listening()
        {
            var formatMap = new Mock<IEditorFormatMap>();
            var service = new Mock<IEditorFormatMapService>();
            service.Setup(s => s.GetEditorFormatMap("text")).Returns(formatMap.Object);
            var listener = new EditorFormatMapTextSpecificListener(service.Object);

            Assert.DoesNotThrow(() => formatMap.Raise(map => map.FormatMappingChanged += null,
                new FormatItemsEventArgs(new ReadOnlyCollection<string>(new[] { "Coverage Touched Area FCC" }))));
        }

        [Test]
        public void Should_Resume_Listening_When_Updating_Colors_Throws()
        {
            var formatMap = new Mock<IEditorFormatMap>();
            var service = new Mock<IEditorFormatMapService>();
            service.Setup(s => s.GetEditorFormatMap("text")).Returns(formatMap.Object);
            var listener = new EditorFormatMapTextSpecificListener(service.Object);
            var invoked = false;
            listener.ListenFor(new List<string> { "Match" }, () => invoked = true);

            Assert.Throws<InvalidOperationException>(() => listener.PauseListeningWhenExecuting(
                () => throw new InvalidOperationException("Failed to update colors")));
            formatMap.Raise(map => map.FormatMappingChanged += null,
                new FormatItemsEventArgs(new ReadOnlyCollection<string>(new[] { "Match" })));

            Assert.That(invoked, Is.True);
        }

        [TestCase(new string[] { "This" }, new string[] { "That" }, false)]
        [TestCase(new string[] { "Other", "Match" }, new string[] { "NoMatch", "Match" }, true)]
        public void Should_Listen_For_Specific_FormatMappingChanged_Items(string[] listenFor, string[] changedItems, bool expectedInvocation)
        {
            var autoMoqer = new AutoMoqer();
            var mockEditorFormatMap = new Mock<IEditorFormatMap>();
            autoMoqer.Setup<IEditorFormatMapService, IEditorFormatMap>(editorFormatMapService => editorFormatMapService.GetEditorFormatMap("text"))
                .Returns(mockEditorFormatMap.Object);
            var editorFormatTextSpecificListener = autoMoqer.Create<EditorFormatMapTextSpecificListener>();
            var invoked = false;
            editorFormatTextSpecificListener.ListenFor(listenFor.ToList(), () =>
            {
                invoked = true;
            });
            mockEditorFormatMap.Raise(editorFormatMap => editorFormatMap.FormatMappingChanged += null, new FormatItemsEventArgs(new ReadOnlyCollection<string>(changedItems)));

            Assert.That(invoked, Is.EqualTo(expectedInvocation));
        }

        [Test]
        public void Should_Pause_Listening_When_Executing()
        {
            var autoMoqer = new AutoMoqer();
            var mockEditorFormatMap = new Mock<IEditorFormatMap>();
            autoMoqer.Setup<IEditorFormatMapService, IEditorFormatMap>(editorFormatMapService => editorFormatMapService.GetEditorFormatMap("text"))
                .Returns(mockEditorFormatMap.Object);
            var editorFormatTextSpecificListener = autoMoqer.Create<EditorFormatMapTextSpecificListener>();
            var invoked = false;
            editorFormatTextSpecificListener.ListenFor(new List<string> { "Match" }, () =>
            {
                invoked = true;
            });
            editorFormatTextSpecificListener.PauseListeningWhenExecuting(() =>
            {
                mockEditorFormatMap.Raise(editorFormatMap => editorFormatMap.FormatMappingChanged += null, new FormatItemsEventArgs(new ReadOnlyCollection<string>(new string[] { "Match" })));
            });

            Assert.That(invoked, Is.False);
        }

    }
}