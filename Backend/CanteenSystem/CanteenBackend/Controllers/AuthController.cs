using Microsoft.AspNetCore.Mvc;
using CanteenBackend.Services;
using CanteenBackend.Models.Auth;

namespace CanteenBackend.Controllers
{
    /// <summary>
    /// Provides authentication endpoints for admin login.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Authenticates an admin and returns a JWT token.
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var admin = _authService.ValidateCredentials(request.Username, request.Password);

            if (admin == null)
                return Unauthorized("Invalid username or password.");

            var token = _authService.GenerateJwt(admin);

            return Ok(new { token });
        }

        /// <summary>
        /// Creates a new admin account (optional).
        /// </summary>
        [HttpPost("create-admin")]
        public IActionResult CreateAdmin([FromBody] LoginRequest request)
        {
            var result = _authService.CreateAdmin(request.Username, request.Password);
            return Ok(result);
        }
    }
}
