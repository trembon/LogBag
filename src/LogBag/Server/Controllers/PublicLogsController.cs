using Amazon.Runtime.Internal;
using LogBag.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace LogBag.Server.Controllers
{
    [ApiController]
    [Route("api/logs")]
    public class PublicLogsController : ControllerBase
    {
        private readonly IAddLogsService _addLogsService;

        public PublicLogsController(IAddLogsService addLogsService)
        {
            _addLogsService = addLogsService;
        }

        [HttpPost("{pocket?}")]
        public async Task<IActionResult> AddLog(string pocket = "default", CancellationToken cancellationToken = default)
        {
            using var reader = new StreamReader(Request.Body);
            string item = await reader.ReadToEndAsync(cancellationToken);

            var objectId = await _addLogsService.AddLogItem(pocket, item, cancellationToken);
            if(objectId is not null)
            {
                return Created($"/logs/{pocket}/{objectId}", objectId);
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
