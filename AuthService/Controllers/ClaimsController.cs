using AuthService.Services;
using Shared.Contracts.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[ApiController]
[Route("api/users/{userId:int}/claims")]
[Authorize]
public class ClaimsController : ControllerBase
{
    private readonly IClaimManagerService _claimMgr;
    public ClaimsController(IClaimManagerService claimMgr) => _claimMgr = claimMgr;

    [HttpGet]
    public async Task<IActionResult> GetAll(int userId, CancellationToken ct)
        => Ok(await _claimMgr.GetClaimsAsync(userId, ct));

    [HttpPost]
    [Authorize(Roles = "Admin,SuperUser")]
    public async Task<IActionResult> Add(int userId, [FromBody] AddClaimRequest req, CancellationToken ct)
    {
        await _claimMgr.AddClaimAsync(userId, req.Type, req.Value, ct);
        return NoContent();
    }

    [HttpDelete("{claimId:int}")]
    [Authorize(Roles = "Admin,SuperUser")]
    public async Task<IActionResult> Remove(int userId, int claimId, CancellationToken ct)
    {
        await _claimMgr.RemoveClaimAsync(claimId, ct);
        return NoContent();
    }

    [HttpPut]
    [Authorize(Roles = "Admin,SuperUser")]
    public async Task<IActionResult> Replace(int userId, [FromBody] ReplaceClaimsRequest req, CancellationToken ct)
    {
        await _claimMgr.ReplaceClaimsAsync(userId,
            req.Claims.Select(c => (c.Type, c.Value)), ct);
        return NoContent();
    }
}
