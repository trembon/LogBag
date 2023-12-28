using LogBag.Services;
using LogBag.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace LogBag.Server.Controllers
{
    [ApiController]
    [Route("api/internal/pockets")]
    public class PocketsController(IPocketService pocketService, ILogsService logsService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> ListPocketNames()
        {
            var pocketNames = await pocketService.GetPocketNames();
            return Ok(pocketNames);
        }

        [HttpGet("{pocket}")]
        public async Task<IActionResult> GetPocketMetadata(string pocket, CancellationToken cancellationToken = default)
        {
            var columns = await pocketService.GetColumns(pocket, cancellationToken);
            var logCount = await logsService.GetLogCount(pocket, cancellationToken);

            return Ok(new PocketMetadataResponse
            {
                Name = pocket,
                Columns = columns,
                TotalLogCount = logCount
            });
        }
    }
}
