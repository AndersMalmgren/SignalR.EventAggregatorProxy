using System;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using SignalR.EventAggregatorProxy.Constraint;
using SignalR.EventAggregatorProxy.Event;

namespace SignalR.EventAggregatorProxy.Tests
{
    [TestClass]
    public class When_trying_to_find_a_constraint_handler_for_a_generic_event : Test
    {
        [TestInitialize]
        public void Context()
        {
            GenericEventConstraintHandler.Called = false;

            WhenCalling<IAssemblyLocator>(x => x.GetAssemblies()).Return(new[] {Assembly.GetExecutingAssembly()});

            var typeFinder = new TypeFinder<TestEventBase>();
            var type = typeof(OuterGeneric<EntityOne>);

            var handlerType = typeFinder.GetConstraintHandlerType(type);
            var eventConstraintHandler = Activator.CreateInstance(handlerType) as IEventConstraintHandler;
            eventConstraintHandler.Allow(new OuterGeneric<EntityOne>(), null, null);
        }

        [TestMethod]
        public void It_should_invoke_correct_handler()
        {
            var result = GenericEventConstraintHandler.Called;
            GenericEventConstraintHandler.Called = false;

            Assert.IsTrue(result, "Generic constraint handler was not found");
        }

        public interface ITestHandler<in T>
        {
            bool Allow(T message, string username, dynamic constraint);
        }

        public class GenericEventConstraintHandler : EventConstraintHandler<IOuterGeneric<EntityBase>>
        {
            public static bool Called { get; set; }

            public override bool Allow(IOuterGeneric<EntityBase> message, string username, dynamic constraint)
            {
                Called = true;
                return true;
            }
        }

        public interface IOuterGeneric<out T> : IInnerGeneric<IEnumerable<T>> where T : EntityBase
        {

        }

        public class OuterGeneric<T> : InnerGeneric<IEnumerable<T>>, IOuterGeneric<T> where T : EntityBase
        {
        }

        public interface IInnerGeneric<out T>
        {
            string Username { get; set; }
            T Data { get;}
        }

        public class InnerGeneric<T> : TestEventBase, IInnerGeneric<T> 
        {
            public string Username { get; set; }
            public T Data { get; set; }
        }

        public abstract class EntityBase
        {
        }

        public class EntityOne : EntityBase
        {
        }

        public class EntityTwo : EntityBase
        {
        }


    }
}
