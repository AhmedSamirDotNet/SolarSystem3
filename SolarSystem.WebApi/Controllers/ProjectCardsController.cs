using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarSystem.DataAccess1.Repository.IRepository;
using SolarSystem.Models1.Models;
using SolarSystem.Models1.Dtos;
using SolarSystem.Models1.Extensions;
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

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll([FromQuery] string lang = "en")
        {
            var cards = _unitOfWork.ProjectCard.GetAll(includeProperties: "Translations");
            var cardDtos = cards
                .Select(c => NormalizeProjectCardImageUrl(c.ToDto(lang)))
                .ToList();
            return Ok(cardDtos);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult Get(int id, [FromQuery] string lang = "en")
        {
            var card = _unitOfWork.ProjectCard.Get(c => c.Id == id, includeProperties: "Translations");
            if (card == null) return NotFound(new ErrorResponseDto { Message = "Project card not found" });
            return Ok(NormalizeProjectCardImageUrl(card.ToDto(lang)));
        }

        [HttpGet("full/{id}")]
        public IActionResult GetFull(int id)
        {
            var card = _unitOfWork.ProjectCard.Get(c => c.Id == id, includeProperties: "Translations");
            if (card == null) return NotFound(new ErrorResponseDto { Message = "Project card not found" });
            return Ok(NormalizeProjectCardDetailImageUrl(card.ToDetailDto()));
        }

        [HttpPost]
        [Authorize(Roles = "MasterAdmin,CreateDeleteAdmin")]
        public IActionResult Create([FromForm] CreateProjectHomePageCardDto createDto, [FromForm] string? TranslationsJson, IFormFile? file)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var card = createDto.ToModel();

            if (file != null)
            {
                card.ImageRelativePath = HandleImageUpload(file);
            }

            _unitOfWork.ProjectCard.Add(card);
            _unitOfWork.Save();

            if (!string.IsNullOrWhiteSpace(TranslationsJson))
            {
                try
                {
                    var translations = JsonSerializer.Deserialize<List<ProjectCardTranslationDto>>(TranslationsJson);
                    if (translations != null)
                    {
                        foreach (var trDto in translations)
                        {
                            var translation = trDto.ToModel();
                            translation.ProjectCardId = card.Id;
                            _unitOfWork.ProjectCardTranslation.Add(translation);
                        }
                        _unitOfWork.Save();
                    }
                }
                catch (JsonException) { /* Fallback to default if needed */ }
            }

            return Ok(new SuccessResponseDto { Message = "Project card created successfully", Data = card.ToDetailDto() });
        }

        [HttpPut("{id?}")]
        [Authorize(Roles = "MasterAdmin,CreateDeleteAdmin")]
        public IActionResult Update(int? id, [FromForm] UpdateProjectHomePageCardDto updateDto, [FromForm] string? TranslationsJson, IFormFile? file)
        {
            // --- بداية الكود الخاص باستخلاص الـ Id ---
            if (updateDto.Id <= 0 && id.HasValue && id.Value > 0)
            {
                updateDto.Id = id.Value;
                ModelState.Remove("Id");
                ModelState.Remove("id");
            }

            // If model binding missed Id in multipart form, recover it manually
            if (updateDto.Id <= 0)
            {
                var idRaw = Request.Form["Id"].FirstOrDefault()
                    ?? Request.Form["id"].FirstOrDefault()
                    ?? Request.Form["updateDto.Id"].FirstOrDefault()
                    ?? Request.Form["updateDto.id"].FirstOrDefault();

                if (int.TryParse(idRaw, out var parsedId) && parsedId > 0)
                {
                    updateDto.Id = parsedId;

                    // Clear stale model-state errors for Id after manual recovery
                    ModelState.Remove("Id");
                    ModelState.Remove("id");
                    ModelState.Remove("updateDto.Id");
                    ModelState.Remove("updateDto.id");
                }
            }
            // --- نهاية الكود الخاص باستخلاص الـ Id ---

            if (!ModelState.IsValid || updateDto.Id <= 0) return BadRequest(new ErrorResponseDto { Message = "Invalid data" });

            var card = _unitOfWork.ProjectCard.Get(c => c.Id == updateDto.Id, includeProperties: "Translations");
            if (card == null) return NotFound(new ErrorResponseDto { Message = "Project card not found" });

            // تحديث بيانات الكارد باستخدام الـ DTO الذي يحتوي الآن على بيانات العناوين
            card.UpdateFromDto(updateDto);

            if (file != null)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(card.ImageRelativePath))
                {
                    string oldPath = Path.Combine(_webHostEnvironment.WebRootPath, card.ImageRelativePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }
                card.ImageRelativePath = HandleImageUpload(file);
            }

            _unitOfWork.ProjectCard.Update(card);

            // --- معالجة الترجمات (إذا كنت لا تزال ترسلها) ---
            // ملاحظة: هذه الطريقة في إرسال الترجمات معقدة. 
            // الأفضل هو إرسالها في الـ DTO نفسه أو استخدام API منفصل للترجمات.
            if (!string.IsNullOrWhiteSpace(TranslationsJson))
            {
                try
                {
                    var translations = JsonSerializer.Deserialize<List<ProjectCardTranslationDto>>(TranslationsJson);
                    if (translations != null)
                    {
                        foreach (var trDto in translations)
                        {
                            var translation = trDto.ToModel();
                            translation.ProjectCardId = card.Id;
                            if (trDto.Id == 0)
                                _unitOfWork.ProjectCardTranslation.Add(translation);
                            else
                                _unitOfWork.ProjectCardTranslation.Update(translation);
                        }
                    }
                }
                catch (JsonException) { }
            }
            // --- نهاية معالجة الترجمات ---

            _unitOfWork.Save();
            return Ok(new SuccessResponseDto { Message = "Project card updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "MasterAdmin,CreateDeleteAdmin")]
        public IActionResult Delete(int id)
        {
            var card = _unitOfWork.ProjectCard.Get(c => c.Id == id);
            if (card == null) return NotFound(new ErrorResponseDto { Message = "Project card not found" });

            if (!string.IsNullOrEmpty(card.ImageRelativePath))
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, card.ImageRelativePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            }

            _unitOfWork.ProjectCard.Remove(card);
            _unitOfWork.Save();

            return Ok(new SuccessResponseDto { Message = "Project card deleted successfully" });
        }

        private string HandleImageUpload(IFormFile file)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string projectsPath = Path.Combine(wwwRootPath, "images", "projects");

            if (!Directory.Exists(projectsPath)) Directory.CreateDirectory(projectsPath);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string relativePath = "/images/projects/" + fileName;
            string fullPath = Path.Combine(projectsPath, fileName);

            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            return relativePath;
        }

        private ProjectHomePageCardDto NormalizeProjectCardImageUrl(ProjectHomePageCardDto dto)
        {
            dto.ImageRelativePath = ToAbsoluteImageUrl(dto.ImageRelativePath);
            return dto;
        }

        private ProjectCardDetailDto NormalizeProjectCardDetailImageUrl(ProjectCardDetailDto dto)
        {
            dto.ImageRelativePath = ToAbsoluteImageUrl(dto.ImageRelativePath);
            return dto;
        }

        private string ToAbsoluteImageUrl(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            var normalized = path.StartsWith("/") ? path : $"/{path}";
            return $"{Request.Scheme}://{Request.Host}{normalized}";
        }
    }
}