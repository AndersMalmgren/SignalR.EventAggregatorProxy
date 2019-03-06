using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
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
        IConstraintinfoBuilder Add<TConstraint>(TConstraint constraint);
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

        public IConstraintinfoBuilder Add<TConstraint>(TConstraint constraint)
        {
            return constraintinfoBuilder.Add<TEvent, TConstraint>(constraint);
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
            return Constraint;
        }

        public int Id { get; internal set; }
    }
}
