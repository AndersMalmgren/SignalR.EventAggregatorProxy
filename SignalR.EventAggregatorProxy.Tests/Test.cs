using System;
using System.Collections.Generic;
using System.Text;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace SignalR.EventAggregatorProxy.Tests
{
    public abstract class Test
    {
        protected Test()
        {
            Reset();
        }

        protected abstract void Reset();
        
        public IMethodOptions<object> WhenCalling<T>(Action<T> action) where T : class
        {
            return GetOrMock<T>().Stub(action);
        }

        protected IMethodOptions<TResult> WhenAccessing<T, TResult>(Function<T, TResult> action) where T : class
        {
            var stub = GetOrMock<T>();
            return stub.Expect(action);
        }

        public abstract T Get<T>() where T : class;

        public abstract void Register<T>(T stub);

        public T Mock<T>() where T : class
        {
            var stub = MockRepository.GenerateMock<T>();
            Register(stub);
            return stub;
        }

        public T GetOrMock<T>() where T : class
        {
            var stub = Get<T>() ?? Mock<T>();
            return stub;
        }
    }
}
