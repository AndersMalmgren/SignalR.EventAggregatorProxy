using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalR.EventAggregatorProxy.Constraint
{
	public class ConstraintContext
	{
		public ConstraintContext(string connectionId, string username)
		{
			ConnectionId = connectionId;
			Username = username;
		}

		public string ConnectionId { get; private set; }
		public string Username { get; private set; }
	}
}
