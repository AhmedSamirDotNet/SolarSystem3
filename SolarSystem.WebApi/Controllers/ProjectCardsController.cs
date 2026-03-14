using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarSystem.DataAccess1.Repository.IRepository;
using SolarSystem.Models1.Models;
using SolarSystem.Models1.Dtos;
using System.Text.Json;
using System.IO;

namespace SolarSystem.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] // فك التشفير عنها لو عايز تقفل الـ API
    public class ProjectCardsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProjectCardsController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        // 1. عرض الكل (مع لغة معينة)
        [HttpGet]
        public IActionResult GetAll([FromQuery] string lang = "en")
        {
            var cards = _unitOfWork.ProjectCard.GetAll(includeProperties: "Translations");

            var result = cards.Select(c => new {
                c.Id,
                ImageUrl = ToAbsoluteUrl(c.ImageRelativePath), // بنعرض الصورة بـ Path كامل للـ Frontend
                Translation = c.Translations.FirstOrDefault(t => t.LanguageCode == lang)
                             ?? c.Translations.FirstOrDefault()
            });

            return Ok(result);
        }

        // 2. إنشاء كارد جديد
        [HttpPost]
        public IActionResult Create([FromForm] string? TranslationsJson, IFormFile? file)
        {
            var card = new ProjectHomePageCard();

            // رفع الصورة وحفظ الـ Relative Path
            if (file != null)
            {
                card.ImageRelativePath = UploadImage(file);
            }

            _unitOfWork.ProjectCard.Add(card);
            _unitOfWork.Save(); // بنحفظ عشان نأخد الـ Id

            // إضافة الترجمات من الـ JSON
            if (!string.IsNullOrEmpty(TranslationsJson))
            {
                var translations = JsonSerializer.Deserialize<List<ProjectCardTranslation>>(TranslationsJson);
                if (translations != null)
                {
                    foreach (var t in translations)
                    {
                        t.ProjectCardId = card.Id;
                        t.Id = 0; // نضمن إنها جديدة
                        _unitOfWork.ProjectCardTranslation.Add(t);
                    }
                    _unitOfWork.Save();
                }
            }

            return Ok(new { Message = "Done! 🚀", CardId = card.Id });
        }

        // 3. تعديل كارد
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromForm] string? TranslationsJson, IFormFile? file)
        {
            var card = _unitOfWork.ProjectCard.Get(c => c.Id == id, includeProperties: "Translations");
            if (card == null) return NotFound();

            // تحديث الصورة لو مبعوتة
            if (file != null)
            {
                DeleteImage(card.ImageRelativePath); // مسح القديمة
                card.ImageRelativePath = UploadImage(file);
            }

            // تحديث الترجمات (أسهل طريقة: امسح القديم وحط الجديد)
            if (!string.IsNullOrEmpty(TranslationsJson))
            {
                var oldTranslations = _unitOfWork.ProjectCardTranslation.GetAll(t => t.ProjectCardId == id);
                foreach (var old in oldTranslations) _unitOfWork.ProjectCardTranslation.Remove(old);

                var newTranslations = JsonSerializer.Deserialize<List<ProjectCardTranslation>>(TranslationsJson);
                if (newTranslations != null)
                {
                    foreach (var t in newTranslations)
                    {
                        t.ProjectCardId = id;
                        t.Id = 0;
                        _unitOfWork.ProjectCardTranslation.Add(t);
                    }
                }
            }

            _unitOfWork.ProjectCard.Update(card);
            _unitOfWork.Save();

            return Ok(new { Message = "Updated! ✅" });
        }

        // 4. حذف الكارد
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var card = _unitOfWork.ProjectCard.Get(c => c.Id == id);
            if (card == null) return NotFound();

            DeleteImage(card.ImageRelativePath);
            _unitOfWork.ProjectCard.Remove(card);
            _unitOfWork.Save();

            return Ok(new { Message = "Deleted! 🗑️" });
        }

        // ================= Helpers (الخدمات المساعدة) =================

        private string UploadImage(IFormFile file)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string projectsPath = Path.Combine(wwwRootPath, "images", "projects");

            if (!Directory.Exists(projectsPath)) Directory.CreateDirectory(projectsPath);

            using (var fileStream = new FileStream(Path.Combine(projectsPath, fileName), FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            return "/images/projects/" + fileName; // الـ Relative Path اللي هيتحفظ في الداتا بيز
        }

        private void DeleteImage(string? path)
        {
            if (string.IsNullOrEmpty(path)) return;
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, path.TrimStart('/'));
            if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
        }

        private string ToAbsoluteUrl(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return "";
            return $"{Request.Scheme}://{Request.Host}{relativePath}";
        }
    }
}