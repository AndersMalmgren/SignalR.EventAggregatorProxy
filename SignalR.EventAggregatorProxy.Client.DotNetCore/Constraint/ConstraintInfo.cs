using System.Collections.Generic;
using SignalR.EventAggregatorProxy.Client.DotNetCore.EventAggregation.ProxyEvents;

namespace SignalR.EventAggregatorProxy.Client.DotNetCore.Constraint
{
    public interface IConstraintInfo
    {
        object GetConstraint();
        int Id { get; }
    }

    public interface IConstraintinfoBuilderResult<TEvent>
    {
        IConstraintinfoBuilderResult<TEvent> Add<TConstraint>(TConstraint constraint);
        IConstraintinfoBuilderResult<TNewEvent> For<TNewEvent>();
    }

    public interface IConstraintinfoBuilder
    {
        IConstraintinfoBuilder Add<TEvent, TConstraint>(TConstraint constraint);
        IConstraintinfoBuilderResult<TEvent> For<TEvent>();
    }

    public class ConstraintinfoBuilder : IConstraintinfoBuilder
    {
        private readonly ISubscriptionStore subscriptionStore;
        private readonly List<IConstraintInfo> constraintInfos;
        public ConstraintinfoBuilder(List<IConstraintInfo> constraintInfos, ISubscriptionStore subscriptionStore)
        {
            this.constraintInfos = constraintInfos;
            this.subscriptionStore = subscriptionStore;
        }

        public IConstraintinfoBuilder Add<TEvent, TConstraint>(TConstraint constraint)
        {
            var constraintInfo = new ConstraintInfo<TEvent, TConstraint>(constraint);
            subscriptionStore.AddConstraint(constraintInfo);
            constraintInfos.Add(constraintInfo);
            return this;
        }

        public IConstraintinfoBuilderResult<TEvent> For<TEvent>() 
        {
            return new ConstraintinfoBuilderResult<TEvent>(this);
        }
    }

    public class ConstraintinfoBuilderResult<TEvent> : IConstraintinfoBuilderResult<TEvent>
    {
        private readonly ConstraintinfoBuilder constraintinfoBuilder;

        public ConstraintinfoBuilderResult(ConstraintinfoBuilder constraintinfoBuilder)
        {
            this.constraintinfoBuilder = constraintinfoBuilder;
        }

        public IConstraintinfoBuilderResult<TEvent> Add<TConstraint>(TConstraint constraint)
        {
            constraintinfoBuilder.Add<TEvent, TConstraint>(constraint);
            return this;
        }

        public IConstraintinfoBuilderResult<TNewEvent> For<TNewEvent>()
        {
            return new ConstraintinfoBuilderResult<TNewEvent>(constraintinfoBuilder);
        }
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
            return Constraint!;
        }

        public int Id { get; internal set; }
    }
}
