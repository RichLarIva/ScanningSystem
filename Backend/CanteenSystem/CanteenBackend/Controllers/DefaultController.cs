using Microsoft.AspNetCore.Mvc;

namespace CanteenBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DefaultController : ControllerBase
    {
        // GET: api/default
        [HttpGet]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                status = "Backend is running",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
