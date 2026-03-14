using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SolarSystem.DataAccess1.Repository.IRepository;
using SolarSystem.Models1.Dtos;
using SolarSystem.Models1.Extensions;
using SolarSystem.Models1.Models;
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
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponseDto { Message = "Invalid data", Errors = GetModelStateErrors(ModelState) });

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
            // ========== هذا الجزء كان ناقص ==========
            // استخلاص الـ ID من عدة مصادر
            if (updateDto.Id <= 0 && id.HasValue && id.Value > 0)
            {
                updateDto.Id = id.Value;
            }

            // محاولة استخلاص الـ ID من الـ Form لو لسه مش موجود
            if (updateDto.Id <= 0)
            {
                var idRaw = Request.Form["Id"].FirstOrDefault()
                    ?? Request.Form["id"].FirstOrDefault()
                    ?? Request.Form["updateDto.Id"].FirstOrDefault()
                    ?? Request.Form["updateDto.id"].FirstOrDefault();

                if (int.TryParse(idRaw, out var parsedId) && parsedId > 0)
                {
                    updateDto.Id = parsedId;
                }
            }

            // التحقق من صحة البيانات
            if (updateDto.Id <= 0)
            {
                return BadRequest(new ErrorResponseDto { Message = "Invalid ID" });
            }
            // ========================================

            var card = _unitOfWork.ProjectCard.Get(c => c.Id == updateDto.Id, includeProperties: "Translations");
            if (card == null)
                return NotFound(new ErrorResponseDto { Message = "Project card not found" });

            bool hasUpdates = false;

            // تحديث الصورة فقط (لأنها الخاصية الوحيدة في الـ Model)
            if (file != null)
            {
                if (!string.IsNullOrEmpty(card.ImageRelativePath))
                {
                    string oldPath = Path.Combine(_webHostEnvironment.WebRootPath, card.ImageRelativePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }
                card.ImageRelativePath = HandleImageUpload(file);
                hasUpdates = true;
            }

            // تحديث الترجمات من updateDto
            if (!string.IsNullOrEmpty(updateDto.TitleEn) || !string.IsNullOrEmpty(updateDto.TitleAr) ||
                updateDto.LocationEn != null || updateDto.LocationAr != null)
            {
                var translations = card.Translations.ToList();

                // تحديث الترجمة الإنجليزية
                var enTranslation = translations.FirstOrDefault(t => t.LanguageCode == "en");
                if (enTranslation != null)
                {
                    if (!string.IsNullOrEmpty(updateDto.TitleEn))
                        enTranslation.Title = updateDto.TitleEn;
                    if (updateDto.LocationEn != null)
                        enTranslation.LocationText = updateDto.LocationEn;
                    _unitOfWork.ProjectCardTranslation.Update(enTranslation);
                }
                else if (!string.IsNullOrEmpty(updateDto.TitleEn))
                {
                    // إنشاء ترجمة جديدة لو مش موجودة
                    enTranslation = new ProjectCardTranslation
                    {
                        LanguageCode = "en",
                        Title = updateDto.TitleEn,
                        LocationText = updateDto.LocationEn ?? "",
                        ProjectCardId = card.Id
                    };
                    _unitOfWork.ProjectCardTranslation.Add(enTranslation);
                }

                // تحديث الترجمة العربية
                var arTranslation = translations.FirstOrDefault(t => t.LanguageCode == "ar");
                if (arTranslation != null)
                {
                    if (!string.IsNullOrEmpty(updateDto.TitleAr))
                        arTranslation.Title = updateDto.TitleAr;
                    if (updateDto.LocationAr != null)
                        arTranslation.LocationText = updateDto.LocationAr;
                    _unitOfWork.ProjectCardTranslation.Update(arTranslation);
                }
                else if (!string.IsNullOrEmpty(updateDto.TitleAr))
                {
                    // إنشاء ترجمة جديدة لو مش موجودة
                    arTranslation = new ProjectCardTranslation
                    {
                        LanguageCode = "ar",
                        Title = updateDto.TitleAr,
                        LocationText = updateDto.LocationAr ?? "",
                        ProjectCardId = card.Id
                    };
                    _unitOfWork.ProjectCardTranslation.Add(arTranslation);
                }

                hasUpdates = true;
            }

            // التعامل مع TranslationsJson للتوافق مع الإصدارات القديمة
            if (!hasUpdates && !string.IsNullOrWhiteSpace(TranslationsJson))
            {
                try
                {
                    var translations = JsonSerializer.Deserialize<List<ProjectCardTranslationDto>>(TranslationsJson);
                    if (translations != null && translations.Any())
                    {
                        foreach (var trDto in translations)
                        {
                            var existingTranslation = card.Translations
                                .FirstOrDefault(t => t.LanguageCode == trDto.LanguageCode);

                            if (existingTranslation != null)
                            {
                                existingTranslation.Title = trDto.Title;
                                existingTranslation.LocationText = trDto.LocationText ?? "";
                                _unitOfWork.ProjectCardTranslation.Update(existingTranslation);
                            }
                            else
                            {
                                var translation = trDto.ToModel();
                                translation.ProjectCardId = card.Id;
                                _unitOfWork.ProjectCardTranslation.Add(translation);
                            }
                        }
                        hasUpdates = true;
                    }
                }
                catch (JsonException) { }
            }

            if (!hasUpdates && file == null)
            {
                return BadRequest(new ErrorResponseDto { Message = "No data provided for update" });
            }

            _unitOfWork.ProjectCard.Update(card);
            _unitOfWork.Save();

            return Ok(new SuccessResponseDto { Message = "Project card updated successfully", Data = card.ToDetailDto() });
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

            if (!Directory.Exists(projectsPath))
                Directory.CreateDirectory(projectsPath);

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
                return string.Empty;

            if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            var normalized = path.StartsWith("/") ? path : $"/{path}";
            return $"{Request.Scheme}://{Request.Host}{normalized}";
        }

        // دالة مساعدة لعرض أخطاء ModelState
        private Dictionary<string, string[]> GetModelStateErrors(ModelStateDictionary modelState)
        {
            return modelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );
        }
    }
}