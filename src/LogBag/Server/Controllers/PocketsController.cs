using LogBag.Services;
using LogBag.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace LogBag.Server.Controllers
{
    [ApiController]
    [Route("api/internal/pockets")]
    public class PocketsController(IPocketService pocketService, ILogsService logsService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var pocketNames = await pocketService.GetPocketNames();
            return Ok(pocketNames);
        }

        [HttpGet("{pocket}/logs")]
        public async Task<IActionResult> GetLogs(string pocket, CancellationToken cancellationToken = default)
        {
            var columns = await pocketService.GetColumns(pocket);
            var logRows = await logsService.GetLogs(pocket, cancellationToken);

            return Ok(new LogRowsResponse
            {
                Columns = columns,
                Rows = logRows
            });
        }
    }
}
