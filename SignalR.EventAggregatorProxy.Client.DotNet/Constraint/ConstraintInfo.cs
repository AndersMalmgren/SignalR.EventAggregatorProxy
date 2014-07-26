using System.Text;
using System.Threading.Tasks;
using SignalR.EventAggregatorProxy.Client.Bootstrap;
using SignalR.EventAggregatorProxy.Client.EventAggregation;
using SignalR.EventAggregatorProxy.Client.EventAggregation.ProxyEvents;

namespace SignalR.EventAggregatorProxy.Client.Constraint
{
    public interface IConstraintInfo
    {
        object GetConstraint();
        int Id { get; }
    }

    public class ConstraintInfo<TEvent, TConstraint> : IConstraintInfo
    {
        public TConstraint Constraint { get; set; }

        public ConstraintInfo(TConstraint constraint)
        {
            Constraint = constraint;
            Id = DependencyResolver.Global.Get<ISubscriptionStore>().GenerateConstraintId<TEvent>(this);
        }

        public object GetConstraint()
        {
            return Constraint;
        }

        public int Id { get; private set; }
    }
}
