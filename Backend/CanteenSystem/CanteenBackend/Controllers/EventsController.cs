using Microsoft.AspNetCore.Mvc;
using CanteenBackend.Services;

namespace CanteenBackend.Controllers
{
    /// <summary>
    /// Provides a Server-Sent Events (SSE) endpoint for real-time updates.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly EventStream _eventStream;

        public EventsController(EventStream eventStream)
        {
            _eventStream = eventStream;
        }

        /// <summary>
        /// Opens a persistent SSE connection for real-time updates.
        /// </summary>
        [HttpGet]
        public async Task Get()
        {
            Response.Headers.Add("Content-Type", "text/event-stream");

            using var writer = new StreamWriter(Response.Body);
            _eventStream.AddClient(writer);

            await writer.WriteAsync("event: connected\ndata: ok\n\n");
            await writer.FlushAsync();

            // Keep the connection open
            await Task.Delay(Timeout.Infinite);
        }
    }
}
