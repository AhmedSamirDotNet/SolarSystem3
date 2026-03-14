using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarSystem.DataAccess1.Repository.IRepository;
using SolarSystem.Models1.Models;
using SolarSystem.Models1.Dtos;
using System.Text.Json;

namespace SolarSystem.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "MasterAdmin,CreateDeleteAdmin,ViewAdmin")]
    public class ProjectCardsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProjectCardsController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        // ==========================================
        // 1. GET ALL CARDS (بناءً على اللغة)
        // ==========================================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll([FromQuery] string lang = "en")
        {
            var cards = _unitOfWork.ProjectCard.GetAll(includeProperties: "Translations");

            var cardDtos = cards.Select(c => {
                // المابينج اليدوي المباشر عشان نضمن الداتا
                var translation = c.Translations?.FirstOrDefault(t => t.LanguageCode == lang)
                                  ?? c.Translations?.FirstOrDefault();

                var dto = new ProjectHomePageCardDto
                {
                    Id = c.Id,
                    ImageRelativePath = ToAbsoluteImageUrl(c.ImageRelativePath),
                    Title = translation?.Title ?? "No Title",
                    LocationText = translation?.LocationText ?? ""
                };
                return dto;
            }).ToList();

            return Ok(cardDtos);
        }

        // ==========================================
        // 2. GET SINGLE CARD (بناءً على اللغة)
        // ==========================================
        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult Get(int id, [FromQuery] string lang = "en")
        {
            var card = _unitOfWork.ProjectCard.Get(c => c.Id == id, includeProperties: "Translations");
            if (card == null) return NotFound(new ErrorResponseDto { Message = "Project card not found" });

            var translation = card.Translations?.FirstOrDefault(t => t.LanguageCode == lang)
                              ?? card.Translations?.FirstOrDefault();

            var dto = new ProjectHomePageCardDto
            {
                Id = card.Id,
                ImageRelativePath = ToAbsoluteImageUrl(card.ImageRelativePath),
                Title = translation?.Title ?? "No Title",
                LocationText = translation?.LocationText ?? ""
            };

            return Ok(dto);
        }

        // ==========================================
        // 3. CREATE CARD (مع الترجمات والصورة)
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "MasterAdmin,CreateDeleteAdmin")]
        public IActionResult Create([FromForm] CreateProjectHomePageCardDto createDto, [FromForm] string? TranslationsJson, IFormFile? file)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var card = new ProjectHomePageCard();

            if (file != null)
                card.ImageRelativePath = HandleImageUpload(file);

            _unitOfWork.ProjectCard.Add(card);
            _unitOfWork.Save(); // حفظ الكارد لجلب الـ ID

            if (!string.IsNullOrWhiteSpace(TranslationsJson))
            {
                try
                {
                    var translations = JsonSerializer.Deserialize<List<ProjectCardTranslationDto>>(TranslationsJson);
                    if (translations != null)
                    {
                        foreach (var trDto in translations)
                        {
                            _unitOfWork.ProjectCardTranslation.Add(new ProjectCardTranslation
                            {
                                ProjectCardId = card.Id,
                                LanguageCode = trDto.LanguageCode,
                                Title = trDto.Title,
                                LocationText = trDto.LocationText ?? ""
                            });
                        }
                        _unitOfWork.Save();
                    }
                }
                catch { /* Handle JSON Error */ }
            }

            return Ok(new SuccessResponseDto { Message = "Project card created successfully ✅", Data = new { id = card.Id } });
        }

        // ==========================================
        // 4. UPDATE CARD
        // ==========================================
        [HttpPut("{id}")]
        [Authorize(Roles = "MasterAdmin,CreateDeleteAdmin")]
        public IActionResult Update(int id, [FromForm] UpdateProjectHomePageCardDto updateDto, [FromForm] string? TranslationsJson, IFormFile? file)
        {
            var card = _unitOfWork.ProjectCard.Get(c => c.Id == id, includeProperties: "Translations");
            if (card == null) return NotFound(new ErrorResponseDto { Message = "Project card not found" });

            if (file != null)
            {
                DeleteOldImage(card.ImageRelativePath);
                card.ImageRelativePath = HandleImageUpload(file);
            }

            // تحديث الترجمات (الحذف والإضافة أضمن طريقة للـ Sync)
            if (!string.IsNullOrWhiteSpace(TranslationsJson))
            {
                var oldTranslations = _unitOfWork.ProjectCardTranslation.GetAll(t => t.ProjectCardId == id);
                foreach (var old in oldTranslations) _unitOfWork.ProjectCardTranslation.Remove(old);

                var newTranslations = JsonSerializer.Deserialize<List<ProjectCardTranslationDto>>(TranslationsJson);
                if (newTranslations != null)
                {
                    foreach (var trDto in newTranslations)
                    {
                        _unitOfWork.ProjectCardTranslation.Add(new ProjectCardTranslation
                        {
                            ProjectCardId = id,
                            LanguageCode = trDto.LanguageCode,
                            Title = trDto.Title,
                            LocationText = trDto.LocationText ?? ""
                        });
                    }
                }
            }

            _unitOfWork.ProjectCard.Update(card);
            _unitOfWork.Save();

            return Ok(new SuccessResponseDto { Message = "Project card updated successfully ✅" });
        }

        // ==========================================
        // 5. DELETE CARD
        // ==========================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "MasterAdmin,CreateDeleteAdmin")]
        public IActionResult Delete(int id)
        {
            var card = _unitOfWork.ProjectCard.Get(c => c.Id == id);
            if (card == null) return NotFound(new ErrorResponseDto { Message = "Project card not found" });

            DeleteOldImage(card.ImageRelativePath);
            _unitOfWork.ProjectCard.Remove(card);
            _unitOfWork.Save();

            return Ok(new SuccessResponseDto { Message = "Project card deleted successfully 🗑️" });
        }

        // ==========================================
        // 🛠️ HELPER METHODS
        // ==========================================

        private string HandleImageUpload(IFormFile file)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string projectsPath = Path.Combine(wwwRootPath, "images", "projects");
            if (!Directory.Exists(projectsPath)) Directory.CreateDirectory(projectsPath);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string fullPath = Path.Combine(projectsPath, fileName);

            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }
            return "/images/projects/" + fileName;
        }

        private void DeleteOldImage(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;
            string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
        }

        private string ToAbsoluteImageUrl(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;
            if (path.StartsWith("http")) return path;
            var normalized = path.StartsWith("/") ? path : $"/{path}";
            return $"{Request.Scheme}://{Request.Host}{normalized}";
        }
    }
}