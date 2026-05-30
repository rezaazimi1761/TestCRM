using AuthService.Models;
using AuthService.Services;
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

    // GET api/users/5/claims
    [HttpGet]
    public async Task<IActionResult> GetAll(int userId, CancellationToken ct)
        => Ok(await _claimMgr.GetClaimsAsync(userId, ct));

    // POST api/users/5/claims
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Add(int userId, [FromBody] AddClaimRequest req, CancellationToken ct)
    {
        await _claimMgr.AddClaimAsync(userId, req.Type, req.Value, ct);
        return NoContent();
    }

    // DELETE api/users/5/claims/12
    [HttpDelete("{claimId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Remove(int userId, int claimId, CancellationToken ct)
    {
        await _claimMgr.RemoveClaimAsync(claimId, ct);
        return NoContent();
    }

    // PUT api/users/5/claims  (replace all)
    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Replace(int userId, [FromBody] ReplaceClaimsRequest req, CancellationToken ct)
    {
        await _claimMgr.ReplaceClaimsAsync(userId,
            req.Claims.Select(c => (c.Type, c.Value)), ct);
        return NoContent();
    }
}
