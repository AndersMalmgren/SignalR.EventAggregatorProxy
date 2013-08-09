using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNet.SignalR;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;
using SignalR.EventAggregatorProxy.Event;

namespace SignalR.EventAggregatorProxy.Tests
{
    public abstract class Test
    {
        public IMethodOptions<object> WhenCalling<T>(Action<T> action) where T : class
        {
            return GetOrMock<T>().Stub(action);
        }

        protected IMethodOptions<TResult> WhenAccessing<T, TResult>(Function<T, TResult> action) where T : class
        {
            var stub = GetOrMock<T>();
            return stub.Expect(action);
        }

        public T Get<T>() where T : class
        {
            return GlobalHost.DependencyResolver.GetService(typeof (T)) as T;
        }

        public T Mock<T>() where T : class
        {
            var stub = MockRepository.GenerateMock<T>();
            GlobalHost.DependencyResolver.Register(typeof(T), () => stub);
            return stub;
        }

        public T GetOrMock<T>() where T : class
        {
            var stub = Get<T>() ?? Mock<T>();
            return stub;
        }
    }
}
