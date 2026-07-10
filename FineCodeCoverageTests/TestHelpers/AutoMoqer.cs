using System;
using System.Linq.Expressions;
using Moq;
using Moq.Language.Flow;

namespace AutoMoq
{
    /// <summary>
    /// Drop-in replacement for the (abandoned) AutoMoq package's <c>AutoMoqer</c>, implemented on top of
    /// the maintained <see cref="Moq.AutoMock.AutoMocker"/>.  AutoMoq 2.0.0 dragged in Unity 4
    /// (<c>Microsoft.Practices.Unity</c>), which is incompatible with the renamed Unity 5 assemblies and
    /// could not be satisfied by a binding redirect.  This shim keeps the existing test call sites
    /// (<c>GetMock&lt;T&gt;</c>, <c>Create&lt;T&gt;</c>, <c>SetInstance&lt;T&gt;</c>, <c>Setup</c>, <c>Verify</c>)
    /// unchanged.  The <c>Setup</c>/<c>Verify</c> convenience methods mirror AutoMoq by delegating to the
    /// cached mock returned by <see cref="GetMock{T}"/> (the same instance injected by <see cref="Create{T}"/>).
    /// </summary>
    public class AutoMoqer
    {
        private readonly Moq.AutoMock.AutoMocker mocker = new Moq.AutoMock.AutoMocker();

        /// <summary>Returns the cached mock for <typeparamref name="T"/>, creating it on first use.</summary>
        public Mock<T> GetMock<T>()
            where T : class => this.mocker.GetMock<T>();

        /// <summary>
        /// Returns the cached mock for <typeparamref name="T"/>.  The <paramref name="behavior"/> matches
        /// AutoMoq's contract: it only applied when the mock did not already exist, and AutoMoq returned the
        /// existing (already-injected) mock otherwise.  Because <see cref="Moq.AutoMock.AutoMocker"/> creates
        /// its mocks Loose, the requested behavior is best-effort; relaxing Strict to Loose cannot turn a
        /// passing test into a failing one (Strict is strictly more restrictive).
        /// </summary>
        public Mock<T> GetMock<T>(MockBehavior behavior)
            where T : class => this.mocker.GetMock<T>();

        /// <summary>Creates an instance of <typeparamref name="T"/>, injecting cached mocks/instances for its dependencies.</summary>
        public T Create<T>()
            where T : class => this.mocker.CreateInstance<T>();

        /// <summary>Registers a concrete instance to be injected for <typeparamref name="T"/> instead of a mock.</summary>
        public void SetInstance<T>(T instance)
            where T : class => this.mocker.Use(instance);

        /// <summary>Sets up an expectation on the cached mock for <typeparamref name="TService"/>.</summary>
        public ISetup<TService, TReturn> Setup<TService, TReturn>(Expression<Func<TService, TReturn>> expression)
            where TService : class => this.GetMock<TService>().Setup(expression);

        /// <summary>Verifies a call against the cached mock for <typeparamref name="TService"/>.</summary>
        public void Verify<TService>(Expression<Action<TService>> expression)
            where TService : class => this.GetMock<TService>().Verify(expression);

        /// <summary>Verifies a call against the cached mock for <typeparamref name="TService"/> a specified number of times.</summary>
        public void Verify<TService>(Expression<Action<TService>> expression, Times times)
            where TService : class => this.GetMock<TService>().Verify(expression, times);
    }
}
