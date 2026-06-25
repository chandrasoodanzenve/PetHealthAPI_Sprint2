using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PetHealthAPI.Data;
using PetHealthAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PetHealthAPI.Controllers
{
        /// <summary>
        /// Controller for handling authentication-related operations.
        /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        /// <summary> Registers a new user account in the system. </summary>
        /// <param name="user">The user object containing the username, password, and role for the new account.</param>
        /// <returns>Returns a success message if the user is registered successfully; otherwise, returns a 400 Bad Request response if the username already exists.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                return BadRequest("User already exists!");
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok("User registered successfully!");
        }
        /// <summary> Authenticates a user and generates a JWT token for valid credentials. </summary>
        /// <param name="request">The login request containing the username and password.</param>
        /// <returns>Returns a JWT token if the credentials are valid; otherwise, returns an unauthorized response.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password);
            if (user == null) return Unauthorized("Invalid credentials!");
            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7); 
            await _context.SaveChangesAsync();

            return Ok(new { Token = token, RefreshToken = refreshToken });
        }
        ///<summary> 
        /// Refreshes the JWT token using a valid refresh token.
        ///</summary>
        [HttpPost("refresh")]
public async Task<IActionResult> Refresh(string refreshToken)
{
    var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
    
    if (user == null || user.RefreshTokenExpiryTime <= DateTime.Now)
        return BadRequest("Invalid or expired refresh token!");

    var newJwtToken = GenerateJwtToken(user);
    var newRefreshToken = GenerateRefreshToken();

    // Rotation
    user.RefreshToken = newRefreshToken;
    await _context.SaveChangesAsync();

    return Ok(new { Token = newJwtToken, RefreshToken = newRefreshToken });
}
/// <summary>
/// Revokes the current user's refresh token, effectively logging them out of the system.
/// </summary>
/// <returns></returns>
[HttpPost("revoke")]
[Authorize]
public async Task<IActionResult> Revoke()
{
    var username = User.Identity?.Name;
    var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    
    if (user == null) return BadRequest();

    // Revocation
    user.RefreshToken = null;
    await _context.SaveChangesAsync();

    return Ok("Token revoked successfully (Logged out)!");
}

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[] { new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, user.Role) };
            var token = new JwtSecurityToken(issuer: jwtSettings["Issuer"], audience: jwtSettings["Audience"], claims: claims, expires: DateTime.Now.AddHours(2), signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}