using System;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalR.EventAggregatorProxy.Constraint;
using SignalR.EventAggregatorProxy.Event;

namespace SignalR.EventAggregatorProxy.Tests.Server
{
    public class TypeFinderTest<TEvent> : ServerTest
    {
        protected TypeFinder<TEvent> typeFinder;

        public TypeFinderTest()
        {
            WhenCalling<IAssemblyLocator>(x => x.GetAssemblies()).Return(new[] { Assembly.GetExecutingAssembly() });
            typeFinder = new TypeFinder<TEvent>();
        }
    }

    [TestClass]
    public class When_trying_to_find_a_constraint_handler_for_a_generic_event : TypeFinderTest<TestEventBase>
    {
        [TestInitialize]
        public void Context()
        {
            GenericEventConstraintHandler.Called = false;
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

            public override bool Allow(IOuterGeneric<EntityBase> message, ConstraintContext context, dynamic constraint)
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

    [TestClass]
    public class When_trying_to_find_a_constraint_handler_for_a_base_class_type_Issue_17 : TypeFinderTest<When_trying_to_find_a_constraint_handler_for_a_base_class_type_Issue_17.MyBase>
    {
        private bool result;

        [TestInitialize]
        public void Context()
        {
            var types = new List<Type> {typeof (MySub), typeof(MySubTwo)};
            foreach (var type in types)
            {
                var handlerType = typeFinder.GetConstraintHandlerType(type);
                var eventConstraintHandler = Activator.CreateInstance(handlerType) as IEventConstraintHandler;
                result = eventConstraintHandler.Allow(Activator.CreateInstance(type), null, null);
            }
        }

        [TestMethod]
        public void It_should_invoke_correct_handler()
        {
            Assert.IsTrue(result);
        }

        public abstract class MyBase 
        {
            
        }

        public class MySub : MyBase
        {
            
        }

        public class MySubTwo : MySub
        {

        }

        public class Handler : EventConstraintHandler<MyBase>
        {
            public override bool Allow(MyBase message, ConstraintContext context, dynamic constraint)
            {
                return true;
            }
        }
    }
}
