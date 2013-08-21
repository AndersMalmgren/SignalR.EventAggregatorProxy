using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalR.EventAggregatorProxy.Client.Constraint
{
    internal static class ConstraintInfoExtensions
    {
        public static object GetConstraint(this IEnumerable<IConstraintInfo> constraintInfos, Type eventType)
        {
            var constraintInfo = constraintInfos.GetConstraintInfo(eventType);
            return constraintInfo != null ? constraintInfo.GetConstraint() : null;
        }

        public static IConstraintInfo GetConstraintInfo(this IEnumerable<IConstraintInfo> constraintInfos, Type eventType)
        {
            return constraintInfos.FirstOrDefault(ci => ci.GetType().GetGenericArguments()[0] == eventType);
        }

        public static int? GetConstraintId(this IEnumerable<IConstraintInfo> constraintInfos, Type eventType)
        {
            var constraint = constraintInfos.GetConstraintInfo(eventType);
            return constraint.GetConstraintId();
        }

        public static int? GetConstraintId(this IConstraintInfo constraintInfo)
        {
            return constraintInfo != null ? (int?)constraintInfo.Id : null;
        }
    }
}