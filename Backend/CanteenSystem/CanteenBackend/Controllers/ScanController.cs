using Microsoft.AspNetCore.Mvc;
using CanteenBackend.Services;
using CanteenBackend.Models;
using System.Text.Json;

namespace CanteenBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScanController : ControllerBase
    {
        private readonly ScanService _scanService;
        private readonly MealSessionState _mealSessionState;
        private readonly EventStream _eventStream;

        public ScanController(
            ScanService scanService,
            MealSessionState mealSessionState,
            EventStream eventStream)
        {
            _scanService = scanService;
            _mealSessionState = mealSessionState;
            _eventStream = eventStream;
        }

        [HttpPost]
        public async Task<IActionResult> Scan([FromBody] ScanRequest request)
        {
            var barcode = request.Barcode.Trim();

            // ------------------------------------------------------------
            // Duplicate detection (in-memory)
            // ------------------------------------------------------------
            if (_mealSessionState.IsDuplicate(barcode))
            {
                var payload = JsonSerializer.Serialize(new
                {
                    barcode,
                    meal = _mealSessionState.CurrentMeal.ToString(),
                    timestamp = DateTime.UtcNow
                });

                await _eventStream.BroadcastAsync(
                    new SseMessage("duplicate-scan", payload)
                );

                return Ok(new ScanResult(false, "Already scanned for this meal."));
            }

            // ------------------------------------------------------------
            // Process scan (SQL + business logic)
            // ------------------------------------------------------------
            var result = await _scanService.ProcessScanAsync(barcode);

            if (!result.Success)
            {
                // SQL rejected it (rare, but possible)
                return Ok(result);
            }

            // ------------------------------------------------------------
            // Add to in-memory list
            // ------------------------------------------------------------
            _mealSessionState.AddScan(barcode);

            // ------------------------------------------------------------
            // Broadcast success event
            // ------------------------------------------------------------
            var successPayload = JsonSerializer.Serialize(new
            {
                barcode,
                name = result.PersonName,
                message = result.Message,
                meal = _mealSessionState.CurrentMeal.ToString(),
                timestamp = DateTime.UtcNow
            });

            await _eventStream.BroadcastAsync(
                new SseMessage("scan-success", successPayload)
            );

            return Ok(result);
        }

    }
}
