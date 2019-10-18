using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SignalR.EventAggregatorProxy.Constraint;
using SignalR.EventAggregatorProxy.Event;

namespace SignalR.EventAggregatorProxy.Tests.Server
{
    public class TypeFinderTest<TEvent> : Test
    {
        protected override void ConfigureCollection(IServiceCollection serviceCollection)
        {
            serviceCollection.MockSingleton<IAssemblyLocator>(mock => mock.Setup(x => x.GetAssemblies()).Returns(new[] {Assembly.GetExecutingAssembly()}))
                .MockSingleton<IEventTypeFinder>(mock => mock.Setup(x => x.ListEventsTypes()).Returns(new[] {typeof(TEvent)}))
                .AddSingleton<TypeFinder>();
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

            var handlerType = Get<TypeFinder>().GetConstraintHandlerTypes(type).Single();
            var eventConstraintHandler = Activator.CreateInstance(handlerType) as IEventConstraintHandler;
            eventConstraintHandler.Allow(new OuterGeneric<EntityOne>(), null, new JsonElement());
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

            public override bool Allow(IOuterGeneric<EntityBase> message, ConstraintContext context, JsonElement constraint)
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
                var handlerType = Get<TypeFinder>().GetConstraintHandlerTypes(type).Single();
                var eventConstraintHandler = Activator.CreateInstance(handlerType) as IEventConstraintHandler;
                result = eventConstraintHandler.Allow(Activator.CreateInstance(type), null, new JsonElement());
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
            public override bool Allow(MyBase message, ConstraintContext context, JsonElement constraint)
            {
                return true;
            }
        }
    }

    [TestClass]
    public class When_trying_to_find_a_constraint_handler_for_a_base_class_with_multiple_constraint_handlers : TypeFinderTest<When_trying_to_find_a_constraint_handler_for_a_base_class_with_multiple_constraint_handlers.MyBase>
    {
        private IEnumerable<Type> handlerTypes;

        [TestInitialize]
        public void Context()
        {
            handlerTypes = Get<TypeFinder>().GetConstraintHandlerTypes(typeof(MySub)).Where(t => new[] { typeof(Handler), typeof(HandlerTwo) }.Contains(t));
        }

        [TestMethod]
        public void It_should_invoke_correct_handler()
        {
            Assert.AreEqual(2, handlerTypes.Count());
        }

        public abstract class MyBase
        {

        }

        public class MySub : MyBase
        {

        }

        public class Handler : EventConstraintHandler<MyBase>
        {
            public override bool Allow(MyBase message, ConstraintContext context, JsonElement constraint)
            {
                return true;
            }
        }

        public class HandlerTwo : EventConstraintHandler<MySub>
        {
            public override bool Allow(MySub message, ConstraintContext context, JsonElement constraint)
            {
                return true;
            }
        }
    }
}
