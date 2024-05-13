using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharpen;
using SignalR.EventAggregatorProxy.Constraint;
using SignalR.EventAggregatorProxy.Extensions;

namespace SignalR.EventAggregatorProxy.Tests.Server
{

    public abstract class MultipleConstraintHandlerTest : EventProxyTest
    {
        protected abstract Dictionary<Type, bool> GetResults();
        private static Dictionary<Type, bool> results;
        protected static Dictionary<Type, bool> called;

        protected override void ConfigureCollection(IServiceCollection serviceCollection)
        {
            called = new Dictionary<Type, bool>();
            results = GetResults();
            SetupProxy(serviceCollection, typeof(MySub), new[] { typeof(Handler), typeof(HandlerTwo) });
        }

        [TestInitialize]
        public async Task Context()
        {
            Subscribe();
            await handler(new MySub());
        }

        [TestMethod]
        public void It_should_only_fire_event_if_all_allow_methods_allow_it()
        {

            Assert.AreEqual(results.Values.All(result => result) ? 1 : 0, events.Count);
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
                called[GetType()] = true;
                return results[GetType()];
            }
        }

        public class HandlerTwo : EventConstraintHandler<MySub>
        {
            public override bool Allow(MySub message, ConstraintContext context, JsonElement constraint)
            {
                called[GetType()] = true;
                return results[GetType()];
            }
        }
    }

    [TestClass]
    public class When_having_multiple_constraint_handlers_for_an_even_type_which_disallows_event_on_second_handler : MultipleConstraintHandlerTest
    {
        protected override Dictionary<Type, bool> GetResults()
        {
            return new Dictionary<Type, bool>
            {
                {typeof(Handler), true },
                {typeof(HandlerTwo), false }
            };
        }

        [TestMethod]
        public void It_should_invoke_both_base_and_sub_implementaion_of_constraint_handers()
        {
            
            var git = NGit.Api.Git.Open(new FilePath("C:\\git\\SignalR.EventAggregatorProxy3"));
            var count = git.Log().Call().Count();
            Assert.AreEqual(2, called.Count);
        }
    }

    [TestClass]
    public class When_having_multiple_constraint_handlers_for_an_even_type_which_disallows_event_on_first_handler : MultipleConstraintHandlerTest
    {
        protected override Dictionary<Type, bool> GetResults()
        {
            return new Dictionary<Type, bool>
            {
                {typeof(Handler), false },
                {typeof(HandlerTwo), true }
            };
        }

        [TestMethod]
        public void It_should_only_invoke_first_allow_method()
        {
            Assert.AreEqual(1, called.Count);
        }
    }

    [TestClass]
    public class When_having_multiple_constraint_handlers_for_an_even_type_which_allows_event : MultipleConstraintHandlerTest
    {
        protected override Dictionary<Type, bool> GetResults()
        {
            return new Dictionary<Type, bool>
            {
                {typeof(Handler), true },
                {typeof(HandlerTwo), true }
            };
        }

        [TestMethod]
        public void It_should_only_invoke_both_allow_method()
        {
            Assert.AreEqual(2, called.Count);
        }
    }

    public class MyGenericEvent<T> 
    {
        public T Foo { get; set; }
    }

    [TestClass]
    public class When_subscribing_to_a_generic_event : EventProxyTest
    {
        protected override void ConfigureCollection(IServiceCollection serviceCollection)
        {
            SetupProxy(serviceCollection, typeof(MyGenericEvent<string>));
        }

        [TestInitialize]
        public Task Context()
        {
            Subscribe();
            return handler(new MyGenericEvent<string>{ Foo = "bar"});
        }

        [TestMethod]
        public void It_should_publish_correctly()
        {
            Assert.AreEqual(1, events.Count);
        }
    }
}
