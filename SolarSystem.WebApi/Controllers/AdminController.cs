using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarSystem.DataAccess1.Repository.IRepository;
using SolarSystem.Models1.Models;

namespace SolarSystem.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "MasterAdmin")] 
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
                // نصيحة: لازم تشفر الباسورد قبل ما تخزنه (Hashing) 🔐
                // هنا بنضيفه مباشرة للتبسيط حالياً
                _unitOfWork.Admin.Add(newAdmin);
                _unitOfWork.Save();
                return Ok(new { message = $"تم تعيين {newAdmin.Username} كقائد جديد! " });
            }
            return BadRequest();
        }

        [HttpPut("UpdateRole")]
        public IActionResult UpdateAdminRole([FromBody] Admin adminUpdate)
        {
            var adminFromDb = _unitOfWork.Admin.Get(u => u.Id == adminUpdate.Id);
            if (adminFromDb == null) return NotFound("الأدمن ده مش موجود في السجلات ");

            adminFromDb.Role = adminUpdate.Role; // تغيير الرتبة (مثلاً من Editor لـ MasterAdmin)

            _unitOfWork.Admin.Update(adminFromDb);
            _unitOfWork.Save();

            return Ok(new { message = $"تم تغيير رتبة {adminFromDb.Username} بنجاح!" });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAdmin(int id)
        {
            var admin = _unitOfWork.Admin.Get(u => u.Id == id);
            if (admin == null) return NotFound();

            if (admin.Role == AdminRole.MasterAdmin)
                return BadRequest("مقدرش أخليك تمسح نفسك يا ماستر");

            _unitOfWork.Admin.Remove(admin);
            _unitOfWork.Save();
            return Ok(new { message = "تم طرد الأدمن بنجاح" });
        }
    }
}