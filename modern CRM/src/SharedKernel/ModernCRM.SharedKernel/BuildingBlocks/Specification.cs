using System.Linq.Expressions;

namespace ModernCRM.SharedKernel.BuildingBlocks;

public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
}

public abstract class Specification<T> : ISpecification<T>
{
    public abstract Expression<Func<T, bool>> Criteria { get; }
}
