using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using CanteenBackend.Data.Repositories;
using CanteenBackend.Models;

namespace CanteenBackend.Services
{
    /// <summary>
    /// Handles authentication logic including password hashing and JWT generation.
    /// </summary>
    public class AuthService
    {
        private readonly AdminRepository _adminRepo;
        private readonly IConfiguration _config;

        public AuthService(AdminRepository adminRepo, IConfiguration config)
        {
            _adminRepo = adminRepo;
            _config = config;
        }

        /// <summary>
        /// Validates an admin's username and password.
        /// </summary>
        public AdminRecord? ValidateCredentials(string username, string password)
        {
            var admin = _adminRepo.GetAdminByUsername(username);
            if (admin == null)
                return null;

            using var hmac = new Rfc2898DeriveBytes(password, admin.PasswordSalt, 100000, HashAlgorithmName.SHA256);
            var computedHash = hmac.GetBytes(32);

            if (!computedHash.SequenceEqual(admin.PasswordHash))
                return null;

            return admin;
        }

        /// <summary>
        /// Generates a JWT token for an authenticated admin.
        /// </summary>
        public string GenerateJwt(AdminRecord admin)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, admin.AdminId.ToString()),
                new Claim(ClaimTypes.Name, admin.Username),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Creates a new admin account with hashed password.
        /// </summary>
        public ScanResult CreateAdmin(string username, string password)
        {
            using var hmac = new Rfc2898DeriveBytes(password, 32, 100000, HashAlgorithmName.SHA256);

            var salt = hmac.Salt;
            var hash = hmac.GetBytes(32);

            return _adminRepo.CreateAdmin(username, hash, salt);
        }
    }
}
