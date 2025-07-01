using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using AutoMoq;
using FineCodeCoverage.Initialization;
using FineCodeCoverage.Output;
using Moq;
using NUnit.Framework;

namespace Test
{
    public class Initializer_Tests
    {
		[Test]
		public void Should_ImportMany_IInitializable()
		{
			var constructor = typeof(Initializer).GetConstructors().Single();
			var initializablesConstructorParameter = constructor.GetParameters().Single(p => p.ParameterType == typeof(IInitializable[]));
			var hasImportManyAttribute = initializablesConstructorParameter.GetCustomAttributes(typeof(ImportManyAttribute), false).Any();
			Assert.True(hasImportManyAttribute);
        }

		[Test]
		public void Should_Have_Initial_InitializeStatus_As_Initializing()
        {
			var initializer = new Initializer(null, null, null, null, null);
			Assert.AreEqual(InitializeStatus.Initializing, initializer.InitializeStatus);
        }

		[Test]
		public async Task Should_Log_Initializing_When_Initialize_Async()
        {
			var mockLogger = new Mock<ILogger>();
			var initializer = new Initializer(new Mock<IAppDataFolder>().Object, new IAppDataFolderPathDependent[0], mockLogger.Object, new Mock<IFirstTimeToolWindowOpener>().Object, new IInitializable[0]);
			await initializer.InitializeAsync(CancellationToken.None);
#pragma warning disable VSTHRD110 // Observe result of async calls
            mockLogger.Verify(l => l.LogAsync("Initializing"));
#pragma warning restore VSTHRD110 // Observe result of async calls
        }

		private async Task InitializeWithExceptionAsync(Action<Initializer,Exception,Mock<ILogger>> callback = null)
		{
            var mockLogger = new Mock<ILogger>();
			var mockAppDataFolder = new Mock<IAppDataFolder>();
            var initializeException = new Exception("initialize exception");
#pragma warning disable VSTHRD110 // Observe result of async calls
            mockAppDataFolder.Setup(appDataFolder => appDataFolder.InitializeAsync(It.IsAny<CancellationToken>()))
                .Throws(initializeException);
#pragma warning restore VSTHRD110 // Observe result of async calls
            var initializer = new Initializer(mockAppDataFolder.Object, new IAppDataFolderPathDependent[0], mockLogger.Object, new Mock<IFirstTimeToolWindowOpener>().Object, new IInitializable[0]);
			await initializer.InitializeAsync(CancellationToken.None);

			callback?.Invoke(initializer,initializeException,mockLogger);

		}
		[Test]
		public async Task Should_Set_InitializeStatus_To_Error_If_Exception_When_Initialize_Async()
		{
			Initializer initializer = null;
			await InitializeWithExceptionAsync((_initializer,_,__) =>
			{
				initializer = _initializer;
			});
			Assert.AreEqual(InitializeStatus.Error, initializer.InitializeStatus);
		}

		[Test]
		public async Task Should_Set_InitializeExceptionMessage_If_Exception_When_Initialize_Async()
		{
            Initializer initializer = null;
            await InitializeWithExceptionAsync((_initializer, _, __) =>
            {
                initializer = _initializer;
            });
            Assert.AreEqual("initialize exception", initializer.InitializeExceptionMessage);
        }

		[Test]
		public async Task Should_Log_Failed_Initialization_With_Exception_if_Exception_When_Initialize_Async()
        {
            Exception initializeException = null;
			Mock<ILogger> mockLogger = null;
            await InitializeWithExceptionAsync((_, exc, mLogger) =>
            {
				initializeException = exc;
				mockLogger = mLogger;
            });

#pragma warning disable VSTHRD110 // Observe result of async calls
            mockLogger.Verify(l => l.LogAsync("Failed Initialization", initializeException.ToString()));
#pragma warning restore VSTHRD110 // Observe result of async calls
		}

		[Test]
		public async Task Should_Set_InitializeStatus_To_Initialized_When_Successfully_Completed_Async()
		{
            var initializer = new Initializer(new Mock<IAppDataFolder>().Object, new IAppDataFolderPathDependent[0], new Mock<ILogger>().Object, new Mock<IFirstTimeToolWindowOpener>().Object, new IInitializable[0]);
            await initializer.InitializeAsync(CancellationToken.None);
            Assert.AreEqual(InitializeStatus.Initialized, initializer.InitializeStatus);
        }

		[Test]
		public async Task Should_Log_Initialized_When_Successfully_Completed_Async()
		{
			var mockLogger = new Mock<ILogger>();
            var initializer = new Initializer(new Mock<IAppDataFolder>().Object, new IAppDataFolderPathDependent[0], mockLogger.Object, new Mock<IFirstTimeToolWindowOpener>().Object, new IInitializable[0]);
            await initializer.InitializeAsync(CancellationToken.None);
#pragma warning disable VSTHRD110 // Observe result of async calls
            mockLogger.Verify(l => l.LogAsync("Initialized"));
#pragma warning restore VSTHRD110 // Observe result of async calls
		}

		[Test]
		public async Task Should_Initialize_AppDataFolder_Then_Dependents_In_Order_Then_Open_ToolWindow_If_First_Time_Async()
        {
			var mockAppDataFolder = new Mock<IAppDataFolder>();
			mockAppDataFolder.SetupGet(appDataFolder => appDataFolder.DirectoryPath).Returns("DirectoryPath");
			var mockAppDataFolderDependent = new Mock<IAppDataFolderPathDependent>();
			var mockFirstTimeToolWindowOpener = new Mock<IFirstTimeToolWindowOpener>();

            var initializer = new Initializer(mockAppDataFolder.Object, new IAppDataFolderPathDependent[] { mockAppDataFolderDependent.Object }, new Mock<ILogger>().Object, mockFirstTimeToolWindowOpener.Object, new IInitializable[0]);
            var cancellationToken = CancellationToken.None;
            List<int> callOrder = new List<int>();

			mockAppDataFolder.Setup(appDataFolder => appDataFolder.InitializeAsync(cancellationToken)).Callback(() =>
			{
				callOrder.Add(1);
			});
			mockAppDataFolderDependent.Setup(appDataFolderDependent => appDataFolderDependent.InitializeAsync("DirectoryPath", cancellationToken)).Callback(() =>
			{
                callOrder.Add(2);
            });
            mockFirstTimeToolWindowOpener.Setup(firstTimeToolWindowOpener => firstTimeToolWindowOpener.OpenIfFirstTimeAsync(cancellationToken)).Callback(() =>
			{
				callOrder.Add(3);
			});

			await initializer.InitializeAsync(cancellationToken);
			
			Assert.AreEqual(new List<int> { 1, 2, 3}, callOrder);
		}
	}
}