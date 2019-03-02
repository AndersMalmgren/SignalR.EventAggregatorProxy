using System;
using System.Collections.Generic;
using System.Text;

namespace SignalR.EventAggregatorProxy.Tests.Server
{
    public abstract class TestEventBase
    {

    }

    public class NoMembersEvent : TestEventBase
    {

    }

    public class MembersEvent : TestEventBase
    {
        public string TestPropety { get; set; }
    }

}
