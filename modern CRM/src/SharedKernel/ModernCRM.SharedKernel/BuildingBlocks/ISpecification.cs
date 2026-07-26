using System.Linq.Expressions;

namespace ModernCRM.SharedKernel.BuildingBlocks;

public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
}
