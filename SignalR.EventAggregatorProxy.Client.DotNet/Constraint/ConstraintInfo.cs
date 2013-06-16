using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalR.EventAggregatorProxy.Client.Constraint
{
    public interface IConstraintInfo
    {
        object GetConstraint();
    }

    public class ConstraintInfo<TEvent, TConstraint> : IConstraintInfo
    {
        public TConstraint Constraint { get; set; }

        public ConstraintInfo(TConstraint constraint)
        {
            Constraint = constraint;
        }

        public object GetConstraint()
        {
            return Constraint;
        }
    }
}
