using System;
using System.Collections.Generic;
using System.Text;

namespace SignalR.EventAggregatorProxy.Extensions
{
    public static class DateExtensions
    {
        public static DateTime StripMilliseconds(this DateTime source)
        {
            return source.AddTicks(-(source.Ticks % TimeSpan.TicksPerSecond));
        }
    } 
}
