using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ViamaticaApi.Api.Filters;
using ViamaticaApi.Application.Interfaces;

namespace ViamaticaApi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [RequireJwt]
        [HttpGet("clientes-contratos")]
        public async Task<IActionResult> GetClientesConContratosVigentesAsync()
        {
            var html = await _reportService.GetClientesConContratosVigentesAsync();
            return Content(html, "text/html");
        }
    }
}
