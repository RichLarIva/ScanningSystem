using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using CanteenBackend.Services;

namespace CanteenBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("FrontendPolicy")] // Required for SSE
    public class EventsController : ControllerBase
    {
        private readonly EventStream _eventStream;

        public EventsController(EventStream eventStream)
        {
            _eventStream = eventStream;
        }

        [HttpGet]
        public async Task Get()
        {
            // Required SSE headers
            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");
            Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:5173");
            Response.Headers.Add("Access-Control-Allow-Credentials", "true");

            // Disable response buffering
            HttpContext.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpResponseBodyFeature>()?
                .DisableBuffering();

            // Create writer
            var writer = new StreamWriter(Response.Body);
            _eventStream.AddClient(writer);

            // Initial handshake event
            await writer.WriteAsync("event: connected\ndata: ok\n\n");
            await writer.FlushAsync();

            // Keep the connection alive with heartbeats
            while (!HttpContext.RequestAborted.IsCancellationRequested)
            {
                await writer.WriteAsync(":\n\n"); // SSE comment = heartbeat
                await writer.FlushAsync();
                await Task.Delay(15000, HttpContext.RequestAborted);
            }

            // Client disconnected
            _eventStream.RemoveClient(writer);
        }
    }
}
