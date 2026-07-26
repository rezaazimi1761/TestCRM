namespace ModernCRM.SharedKernel.Application;

public interface ICommand<out TResult> { }
public interface IQuery<out TResult> { }
public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{ Task<TResult> Handle(TCommand command, CancellationToken ct); }
public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{ Task<TResult> Handle(TQuery query, CancellationToken ct); }
