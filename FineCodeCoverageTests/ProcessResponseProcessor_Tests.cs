using System;
using System.Threading.Tasks;
using AutoMoq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output;
using NUnit.Framework;

namespace Test
{
    public class ProcessResponseProcessor_Tests
    {
        private AutoMoqer mocker;
        private ProcessResponseProcessor processor;
        private Action successCallback;
        private bool successCallbackCalled;

        [SetUp]
        public void SetUp()
        {
            mocker = new AutoMoqer();
            processor = mocker.Create<ProcessResponseProcessor>();
            successCallbackCalled = false;
            successCallback = () =>
            {
                successCallbackCalled = true;
            };
            
        }

        [Test]
        public void Should_Throw_Exception_If_Non_Success_ExitCode_And_Throw_Error_True()
        {
            var executeResponse = new ExecuteResponse
            {
                ExitCode = 999,
                Output = "This will be exception message"
            };
            var callbackExitCode = 0;
            Assert.ThrowsAsync<ProcessResponseException>(async () =>
            {
                await processor.ProcessAsync(executeResponse, exitCode =>
                {
                    callbackExitCode = exitCode;
                    return false;
                }, true, null, null);
            }, executeResponse.Output);
            Assert.AreEqual(executeResponse.ExitCode, callbackExitCode);
        }

        [Test]
        public async Task Should_Log_Response_Output_With_Error_Title_If_Non_Success_ExitCode_And_Throw_Error_False_Async()
        {
            var executeResponse = new ExecuteResponse
            {
                ExitCode = 999,
                Output = "This will be logged"
            };
            Assert.False(await processor.ProcessAsync(executeResponse, exitCode =>
                {
                    return false;
                }, false, "title", successCallback));

            Assert.IsFalse(successCallbackCalled);
#pragma warning disable VSTHRD110 // Observe result of async calls
            mocker.Verify<ILogger>(l => l.LogAsync("title Error", "This will be logged"));
#pragma warning restore VSTHRD110 // Observe result of async calls

        }

        [Test]
        public async Task Should_Log_Response_Output_With_Title_If_Success_ExitCode_And_Call_Callback_Async()
        {
            var executeResponse = new ExecuteResponse
            {
                ExitCode = 0,
                Output = "This will be logged"
            };
            Assert.True(await processor.ProcessAsync(executeResponse, exitCode =>
            {
                return true;
            }, true, "title", successCallback));

            Assert.IsTrue(successCallbackCalled);
#pragma warning disable VSTHRD110 // Observe result of async calls
            mocker.Verify<ILogger>(l => l.LogAsync("title", "This will be logged"));
#pragma warning restore VSTHRD110 // Observe result of async calls
        }
    }
}