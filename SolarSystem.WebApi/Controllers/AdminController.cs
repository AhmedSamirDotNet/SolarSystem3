using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarSystem.DataAccess1.Repository.IRepository;
using SolarSystem.Models1.Models;

namespace SolarSystem.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "MasterAdmin,3")]
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
            return Ok(admins);
        }

        [HttpPost("Register")]
        public IActionResult RegisterAdmin([FromBody] Admin newAdmin)
        {
            if (ModelState.IsValid)
            {
                // تشفير كلمة المرور قبل الحفظ
                newAdmin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newAdmin.PasswordHash);

                _unitOfWork.Admin.Add(newAdmin);
                _unitOfWork.Save();
                return Ok(new { message = $"تم تسجيل {newAdmin.Username} بنجاح" });
            }
            return BadRequest();
        }

        [HttpPut("UpdateRole")]
        public IActionResult UpdateAdminRole([FromBody] Admin adminUpdate)
        {
            var adminFromDb = _unitOfWork.Admin.Get(u => u.Id == adminUpdate.Id);
            if (adminFromDb == null) return NotFound("الأدمن غير موجود");

            adminFromDb.Role = adminUpdate.Role;

            _unitOfWork.Admin.Update(adminFromDb);
            _unitOfWork.Save();

            return Ok(new { message = "تم تحديث الرتبة بنجاح" });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAdmin(int id)
        {
            var admin = _unitOfWork.Admin.Get(u => u.Id == id);
            if (admin == null) return NotFound();

            if (admin.Role == AdminRole.MasterAdmin)
                return BadRequest("لا يمكن حذف الماستر أدمن");

            _unitOfWork.Admin.Remove(admin);
            _unitOfWork.Save();
            return Ok(new { message = "تم الحذف بنجاح" });
        }
    }
}