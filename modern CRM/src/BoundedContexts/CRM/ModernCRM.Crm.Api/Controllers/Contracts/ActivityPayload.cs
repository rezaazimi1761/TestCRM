using System.ComponentModel.DataAnnotations; using ModernCRM.Crm.Api.Validation; namespace ModernCRM.Crm.Api.Controllers;
public sealed record ActivityPayload([Required,StringLength(255)]string? Subject,[RegularExpression("^(Call|Email|Meeting|Task|Note)$")]string? Type,[StringLength(4000)]string? Description,[NotInPast]DateTime? DueDate,bool? IsCompleted,[Range(1,int.MaxValue)]int? ContactId);
