using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalR.EventAggregatorProxy.Client.DotNetCore.Constraint
{
    internal static class ConstraintInfoExtensions
    {
        public static IConstraintInfo GetConstraintInfo(this IEnumerable<IConstraintInfo> constraintInfos, Type eventType)
        {
            return constraintInfos.FirstOrDefault(ci => ci.GetType().GetGenericArguments()[0] == eventType);
        }

        public static int? GetConstraintId(this IConstraintInfo constraintInfo)
        {
            return constraintInfo != null ? (int?)constraintInfo.Id : null;
        }
    }
}