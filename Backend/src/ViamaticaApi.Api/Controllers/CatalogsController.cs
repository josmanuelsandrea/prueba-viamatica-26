using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViamaticaApi.Infrastructure.Data;

namespace ViamaticaApi.Api.Controllers;

[ApiController]
[Route("api/catalogs")]
[Authorize]
public class CatalogsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CatalogsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("attention-types")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAttentionTypes()
    {
        var types = await _context.AttentionTypes
            .OrderBy(t => t.Description)
            .Select(t => new { id = t.Attentiontypeid, description = t.Description, prefix = t.Prefix })
            .ToListAsync();

        return Ok(types);
    }

    [HttpGet("method-payments")]
    public async Task<IActionResult> GetMethodPayments()
    {
        var methods = await _context.MethodPayments
            .OrderBy(m => m.Description)
            .Select(m => new { id = m.Methodpaymentid, description = m.Description })
            .ToListAsync();

        return Ok(methods);
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _context.Roles
            .OrderBy(r => r.Rolid)
            .Select(r => new { id = r.Rolid, name = r.Rolname })
            .ToListAsync();

        return Ok(roles);
    }

    [HttpGet("status-contracts")]
    public async Task<IActionResult> GetStatusContracts()
    {
        var statuses = await _context.StatusContracts
            .OrderBy(s => s.Statusid)
            .Select(s => new { id = s.Statusid, description = s.Description })
            .ToListAsync();

        return Ok(statuses);
    }
}
