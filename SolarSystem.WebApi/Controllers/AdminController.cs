using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarSystem.DataAccess1.Repository.IRepository;
using SolarSystem.Models1.Models;
using SolarSystem.Models1.Dtos;
using SolarSystem.Models1.Extensions;

namespace SolarSystem.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "MasterAdmin,CreateDeleteAdmin,ViewAdmin")]
    public class AdminController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult GetAllAdmins()
        {
            var admins = _unitOfWork.Admin.GetAll();
            var adminDtos = admins.Select(a => a.ToDto()).ToList();
            return Ok(adminDtos);
        }

        [HttpPost("Register")]
        [AllowAnonymous]
        public IActionResult RegisterAdmin([FromBody] CreateAdminDto createDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Check if username already exists
            var existingAdmin = _unitOfWork.Admin.Get(u => u.Username == createDto.Username);
            if (existingAdmin != null)
                return BadRequest(new ErrorResponseDto { Message = "اسم المستخدم موجود بالفعل" });

            var newAdmin = new Admin
            {
                Username = createDto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(createDto.Password),
                Role = createDto.Role ?? AdminRole.ViewAdmin
            };

            _unitOfWork.Admin.Add(newAdmin);
            _unitOfWork.Save();
            return Ok(new SuccessResponseDto { Message = $"تم تسجيل {newAdmin.Username} بنجاح", Data = new { adminId = newAdmin.Id } });
        }

        [HttpPut("UpdateRole")]
        [Authorize(Roles = "MasterAdmin,CreateDeleteAdmin")]
        public IActionResult UpdateAdminRole([FromBody] UpdateAdminRoleDto updateDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var adminFromDb = _unitOfWork.Admin.Get(u => u.Id == updateDto.Id);
            if (adminFromDb == null) return NotFound(new ErrorResponseDto { Message = "الأدمن غير موجود" });

            // Parse role name to enum
            if (Enum.TryParse<AdminRole>(updateDto.Role, out var role))
            {
                adminFromDb.Role = role;
            }
            else
            {
                return BadRequest(new ErrorResponseDto { Message = "Invalid role" });
            }

            _unitOfWork.Admin.Update(adminFromDb);
            _unitOfWork.Save();

            return Ok(new SuccessResponseDto { Message = "تم تحديث الرتبة بنجاح" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "MasterAdmin,CreateDeleteAdmin")]
        public IActionResult DeleteAdmin(int id)
        {
            var admin = _unitOfWork.Admin.Get(u => u.Id == id);
            if (admin == null) return NotFound(new ErrorResponseDto { Message = "Admin not found" });

            if (admin.Role == AdminRole.MasterAdmin)
                return BadRequest(new ErrorResponseDto { Message = "لا يمكن حذف الماستر أدمن" });

            _unitOfWork.Admin.Remove(admin);
            _unitOfWork.Save();
            return Ok(new SuccessResponseDto { Message = "تم الحذف بنجاح" });
        }
    }
}