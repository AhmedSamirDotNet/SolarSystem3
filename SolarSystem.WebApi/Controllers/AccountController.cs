using BCrypt.Net; // أضف هذا السطر يدوياً
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SolarSystem.DataAccess1.Repository.IRepository;
using SolarSystem.Models1.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SolarSystem.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AccountController(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest login)
        {
            var admin = _unitOfWork.Admin.Get(u => u.Username == login.Username);

            // استخدام BCrypt.Verify للمقارنة بين الباسورد العادي والهاش
            if (admin == null || !BCrypt.Net.BCrypt.Verify(login.Password, admin.PasswordHash))
            {
                return Unauthorized(new { message = "اسم المستخدم أو كلمة المرور غير صحيحة" });
            }

            var claims = new[]
{
    new Claim(ClaimTypes.Name, admin.Username),
    // هنا بنقوله: لو ملقتش اسم للرتبة، حط الرقم بتاعها كـ String عشان البرنامج ميفصلش
    new Claim(ClaimTypes.Role, Enum.GetName(typeof(AdminRole), admin.Role) ?? admin.Role.ToString()),
    new Claim("AdminId", admin.Id.ToString())
};

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(100),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                username = admin.Username,
                role = admin.Role.ToString()
            });
        }
    }
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}