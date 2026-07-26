using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ModernCRM.Crm.Api.UserSync;

public sealed class UserSyncSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "";
    public int CrmUserId { get; set; }
    public string TenantId { get; set; } = "";
    public string Operation { get; set; } = "";
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Error { get; set; }
}
