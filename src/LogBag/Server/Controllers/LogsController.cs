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
    }
}
