namespace ModernCRM.SharedKernel.Application;

public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{ Task<TResult> Handle(TCommand command, CancellationToken ct); }
