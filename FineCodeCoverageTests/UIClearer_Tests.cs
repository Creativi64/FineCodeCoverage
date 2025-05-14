using AutoMoq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Engine.Messages;
using FineCodeCoverage.Options;
using FineCodeCoverage.Output;
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
            var mockAppOptionsProvider = mocker.GetMock<IAppOptionsProvider>();
            var appOptions = new AppOptions { Enabled = false };
            mockAppOptionsProvider.Raise(appOptionsProvider => appOptionsProvider.OptionsChanged += null, appOptions);

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
