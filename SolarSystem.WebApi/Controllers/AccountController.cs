using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SolarSystem.DataAccess1.Repository.IRepository;
using SolarSystem.Models1.Models;
using SolarSystem.Models1.Dtos;
using SolarSystem.Models1.Extensions;
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
        private readonly ILogger<AccountController> _logger;

        public AccountController(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<AccountController> logger)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDto loginDto)
        {
            _logger.LogInformation($"🔐 Login attempt for username: {loginDto.Username}");
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("❌ Invalid login request model");
                return BadRequest(ModelState);
            }

            try
            {
                var admin = _unitOfWork.Admin.Get(u => u.Username == loginDto.Username);

                if (admin == null)
                {
                    _logger.LogWarning($"⚠️ Admin not found: {loginDto.Username}");
                    return Unauthorized(new ErrorResponseDto { Message = "اسم المستخدم أو كلمة المرور غير صحيحة" });
                }

                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, admin.PasswordHash))
                {
                    _logger.LogWarning($"⚠️ Invalid password for user: {loginDto.Username}");
                    return Unauthorized(new ErrorResponseDto { Message = "اسم المستخدم أو كلمة المرور غير صحيحة" });
                }

                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, admin.Username),
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

                _logger.LogInformation($"✅ Token generated for user: {admin.Username}");

                return Ok(new LoginResponseDto
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    Expiration = token.ValidTo,
                    Username = admin.Username,
                    Role = admin.Role.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Login error: {ex.Message}");
                _logger.LogError($"Stack: {ex.StackTrace}");
                return StatusCode(500, new ErrorResponseDto { Message = "خطأ في المصادقة" });
            }
        }
    }
}