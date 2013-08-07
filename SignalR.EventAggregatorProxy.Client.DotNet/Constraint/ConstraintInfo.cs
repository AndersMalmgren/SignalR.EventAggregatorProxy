using System.Text;
using System.Threading.Tasks;

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
        private static int counter;

        public ConstraintInfo(TConstraint constraint)
        {
            Id = counter++;
            Constraint = constraint;
        }

        public object GetConstraint()
        {
            return Constraint;
        }

        public int Id { get; private set; }
    }
}
