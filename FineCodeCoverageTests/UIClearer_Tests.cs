using AutoMoq;
using FineCodeCoverage.Collection.Messages;
using FineCodeCoverage.Editor.DynamicCoverage.Messages;
using FineCodeCoverage.Options.Base;
using FineCodeCoverage.Options.Run;
using FineCodeCoverage.Output;
using FineCodeCoverage.Utilities.Events;
using FineCodeCoverage.Utilities.UI;
using FineCodeCoverage.Vs.Events;
using Moq;
using NUnit.Framework;
using System;

namespace FineCodeCoverageTests
{
    class UIClearer_Tests
    {
        private AutoMoqer mocker;
        private UIClearer uiClearer;

        [SetUp]
        public void SetUp()
        {
            mocker = new AutoMoqer();
            uiClearer = mocker.Create<UIClearer>();
        }

        [Test]
        public void Should_Send_Clear_Messages_When_Solution_Closes()
        {
            var mockSolutionEvents = mocker.GetMock<ISolutionEvents>();
            mockSolutionEvents.Raise(s => s.AfterClosing += null, EventArgs.Empty);

            AssertSendsClearMessages();
        }

        [Test]
        public void Should_Send_Clear_Messages_When_AppOptions_Changes_To_Disabled()
        {
            var runOptionsProvider = mocker.GetMock<IOptionsProvider<RunOptions>>();
            runOptionsProvider.Raise(appOptionsProvider => appOptionsProvider.OptionsChanged += null, 
                new RunOptions { Enabled = false });

            AssertSendsClearMessages();
        }

        [Test]
        public void Should_Send_Clear_Messages_When_ClearUI()
        {
            uiClearer.ClearUI();

            AssertSendsClearMessages();
        }

        [Test]
        public void Should_Register_As_A_Listener()
        {
            mocker.Verify<IEventAggregator>(ea => ea.AddListener(uiClearer, null));
        }

        [Test]
        public void Should_Send_ClearLinesMessage_When_CoverageStartingMessage_Received()
        {
            uiClearer.Handle(new CoverageStartingMessage());

            AssertSendsClearLinesMessage();
        }

        private void AssertSendsClearMessages()
        {
            mocker.Verify<IEventAggregator>(ea => ea.SendMessage(It.IsAny<ClearReportMessage>(),null));
            AssertSendsClearLinesMessage();
        }

        private void AssertSendsClearLinesMessage()
        {
            mocker.Verify<IEventAggregator>(ea => ea.SendMessage(It.IsAny<ClearLinesMessage>(), null));
        }

    }
}
