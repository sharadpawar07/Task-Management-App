using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using TaskManagementApp.Models;

namespace TaskManagementApp.Controllers
{
    [Route("api/auth")]
    [ApiController] 
    public class AuthController : ControllerBase
    {
        private readonly TaskManagementDBContext _context;

        public AuthController(TaskManagementDBContext context)
        {
            _context = context;
        }

       
        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.Users.Any(u => u.Email == user.Email))
            {
                return BadRequest(new { message = "Email already registered." });
            }

           
            user.RoleId = 2;
            user.Role = null;

            try
            {
                _context.Users.Add(user);
                _context.SaveChanges();
               
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Database error: " + ex.Message });
            }

            return Ok(new { message = "User registered successfully." });
        }



        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == loginRequest.Email && u.UserPassword == loginRequest.Password);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("FLDSJLFNDNFjfdsFLKJDKJfldljfdfmasfmoer5476486446444^^$^$^%!");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.RoleId.ToString()) 
        }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = "http://localhost:5196",
                Audience = "http://localhost:5196",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);


            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("UserRole", user.RoleId.ToString());

            string redirectUrl;
            if (user.RoleId == 1)
            {
                redirectUrl = "/Dashboard/Index";
            }
            else
            {
                redirectUrl = "/TaskView/Index";
            }

            return Ok(new { message = "Login successful", token = jwtToken, userId = user.UserId, role = user.RoleId, redirectUrl = redirectUrl });
        }




        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out successfully." });
        }
    }

    
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
