using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Options;
using Microsoft.VisualStudio.Text;
using Moq;
using NUnit.Framework;
using System;

namespace FineCodeCoverageTests.Editor.DynamicCoverage.BufferLineCoverageTests
{
    internal class BufferLineCoverage_Event_Subscription_Tests
    {
        [Test]
        public void Should_Add_Itself_As_An_EventAggregator_Listener()
        {
            var setup = SimpleSetup.Setup();

            setup.AutoMoqer.Verify<IEventAggregator>(eventAggregator => eventAggregator.AddListener(setup.BufferLineCoverage, null));
        }

        [Test]
        public void Should_Remove_Listeners_When_TextView_Closed()
        {
            var setup = SimpleSetup.Setup();
            var textInfoMocks = setup.TextInfoMocks;
            var autoMoqer = setup.AutoMoqer;
            var mockTextView = textInfoMocks.TextView;
            
            mockTextView.Raise(textView => textView.Closed += null, EventArgs.Empty);

            autoMoqer.Verify<IEventAggregator>(eventAggregator => eventAggregator.RemoveListener(setup.BufferLineCoverage));
            mockTextView.VerifyRemove(textView => textView.Closed -= It.IsAny<EventHandler>(), Times.Once);
            textInfoMocks.TextBuffer.VerifyRemove(textBuffer => textBuffer.ChangedOnBackground -= It.IsAny<EventHandler<TextContentChangedEventArgs>>(), Times.Once);
            var mockEditorCoverageColouringOptionsProvider = autoMoqer.GetMock<IOptionsProvider<EditorCoverageColouringOptions>>();
            mockEditorCoverageColouringOptionsProvider.VerifyRemove(
                optionsProvider => optionsProvider.OptionsChanged -= It.IsAny<Action<EditorCoverageColouringOptions>>(), 
                Times.Once);
        }
    }
}
