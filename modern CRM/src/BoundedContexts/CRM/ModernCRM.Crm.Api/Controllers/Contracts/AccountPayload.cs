using System.ComponentModel.DataAnnotations; namespace ModernCRM.Crm.Api.Controllers;
public sealed record AccountPayload([Required,StringLength(255)]string? Name,[StringLength(100)]string? Industry,[Url,StringLength(500)]string? Website,[Phone,StringLength(30)]string? Phone,[StringLength(500)]string? Address,[StringLength(4000)]string? Notes);
