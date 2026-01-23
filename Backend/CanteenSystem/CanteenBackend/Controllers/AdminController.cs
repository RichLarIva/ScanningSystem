using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CanteenBackend.Services;

namespace CanteenBackend.Controllers
{
    /// <summary>
    /// Provides administrative operations such as bulk importing people.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AdminService _adminService;

        public AdminController(AdminService adminService)
        {
            _adminService = adminService;
        }

        /// <summary>
        /// Imports people from a CSV file. Only accessible to admins.
        /// </summary>
        /// <param name="file">The CSV file containing people data.</param>
        [HttpPost("import")]
        [Consumes("multipart/form-data")]   // <-- REQUIRED for Swagger
        public async Task<IActionResult> ImportCsv([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var results = await _adminService.ImportPeopleFromCsvAsync(file);
            return Ok(results);
        }
    }
}
