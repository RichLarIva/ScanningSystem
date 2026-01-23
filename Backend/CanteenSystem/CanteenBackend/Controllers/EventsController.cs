using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using CanteenBackend.Services;

namespace CanteenBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("FrontendPolicy")]
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
            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:5173");
            Response.Headers.Add("Access-Control-Allow-Credentials", "true");

            HttpContext.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpResponseBodyFeature>()?
                .DisableBuffering();

            var writer = new StreamWriter(Response.Body);
            _eventStream.AddClient(writer);

            await writer.WriteAsync("event: connected\ndata: ok\n\n");
            await writer.FlushAsync();

            while (!HttpContext.RequestAborted.IsCancellationRequested)
            {
                await writer.WriteAsync(":\n\n"); // heartbeat
                await writer.FlushAsync();
                await Task.Delay(15000, HttpContext.RequestAborted);
            }

            _eventStream.RemoveClient(writer);
        }
    }
}
