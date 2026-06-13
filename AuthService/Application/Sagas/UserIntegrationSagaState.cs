using MassTransit;

namespace AuthService.Application.Sagas;

public class UserIntegrationSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;

    public int AuthUserId { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? AppliedAt { get; set; }
    public DateTime? FaultedAt { get; set; }
    public string? FaultReason { get; set; }
}
