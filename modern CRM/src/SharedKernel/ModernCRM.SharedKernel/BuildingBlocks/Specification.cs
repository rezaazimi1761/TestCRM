using System.Linq.Expressions;

namespace ModernCRM.SharedKernel.BuildingBlocks;

public abstract class Specification<T> : ISpecification<T>
{
    public abstract Expression<Func<T, bool>> Criteria { get; }
}
