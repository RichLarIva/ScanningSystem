using Microsoft.AspNetCore.Mvc;
using CanteenBackend.Services;
using CanteenBackend.Models;

namespace CanteenBackend.Controllers
{
    /// <summary>
    /// Provides endpoints for processing barcode scans and triggering reset events.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ScanController : ControllerBase
    {
        private readonly ScanService _scanService;

        public ScanController(ScanService scanService)
        {
            _scanService = scanService;
        }

        /// <summary>
        /// Processes a barcode scan and returns the result.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ScanResult>> Scan([FromBody] string barcode)
        {
            var result = await _scanService.ProcessScanAsync(barcode);
            return Ok(result);
        }

        /// <summary>
        /// Broadcasts a reset event to all SSE clients.
        /// </summary>
        [HttpPost("reset")]
        public async Task<IActionResult> Reset()
        {
            await _scanService.BroadcastResetAsync();
            return Ok(new { Message = "Reset event broadcasted." });
        }
    }
}
