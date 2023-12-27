using LogBag.Services;
using LogBag.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace LogBag.Server.Controllers
{
    [ApiController]
    [Route("api/internal/logs")]
    public class LogsController(ILogsService logsService) : Controller
    {
        [HttpGet("{pocket}")]
        public async Task<IActionResult> GetLogs(string pocket, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var logRows = await logsService.GetLogs(pocket, page, pageSize, cancellationToken);
            if(logRows is null || logRows.Count == 0)
                return NoContent();

            return Ok(logRows);
        }

        [HttpGet("{pocket}/details/{logId}")]
        public async Task<IActionResult> GetLogs(string pocket, string logId, CancellationToken cancellationToken = default)
        {
            var details = await logsService.GetDetails(pocket, logId, cancellationToken);
            if (details is null)
                return NotFound();

            return Ok(details);
        }
    }
}
