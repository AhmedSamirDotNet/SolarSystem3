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

        // ==================== GET METHODS ====================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll([FromQuery] string lang = "en")
        {
            try
            {
                var cards = _unitOfWork.ProjectCard.GetAll(includeProperties: "Translations");

                var result = cards.Select(c => new
                {
                    id = c.Id,
                    imageRelativePath = GetImageUrl(c.ImageRelativePath),
                    title = GetLocalizedValue(c.Translations, lang, "Title"),
                    locationText = GetLocalizedValue(c.Translations, lang, "LocationText")
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult Get(int id, [FromQuery] string lang = "en")
        {
            try
            {
                var card = _unitOfWork.ProjectCard.Get(c => c.Id == id, includeProperties: "Translations");
                if (card == null)
                    return NotFound(new { success = false, message = "Project card not found" });

                var result = new
                {
                    id = card.Id,
                    imageRelativePath = GetImageUrl(card.ImageRelativePath),
                    title = GetLocalizedValue(card.Translations, lang, "Title"),
                    locationText = GetLocalizedValue(card.Translations, lang, "LocationText")
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("full/{id}")]
        public IActionResult GetFull(int id)
        {
            try
            {
                var card = _unitOfWork.ProjectCard.Get(c => c.Id == id, includeProperties: "Translations");
                if (card == null)
                    return NotFound(new { success = false, message = "Project card not found" });

                var translations = card.Translations.Select(t => new
                {
                    id = t.Id,
                    languageCode = t.LanguageCode,
                    title = t.Title,
                    locationText = t.LocationText
                });

                var result = new
                {
                    id = card.Id,
                    imageRelativePath = GetImageUrl(card.ImageRelativePath),
                    translations = translations
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ==================== CREATE METHOD ====================

        [HttpPost]
        [Authorize(Roles = "MasterAdmin,CreateDeleteAdmin")]
        public IActionResult Create()
        {
            try
            {
                // قراءة البيانات من Form
                var titleEn = Request.Form["TitleEn"].FirstOrDefault() ??
                              Request.Form["titleEn"].FirstOrDefault() ??
                              Request.Form["Titleen"].FirstOrDefault();

                var titleAr = Request.Form["TitleAr"].FirstOrDefault() ??
                              Request.Form["titleAr"].FirstOrDefault() ??
                              Request.Form["Titlear"].FirstOrDefault();

                var locationEn = Request.Form["LocationEn"].FirstOrDefault() ??
                                 Request.Form["locationEn"].FirstOrDefault();

                var locationAr = Request.Form["LocationAr"].FirstOrDefault() ??
                                 Request.Form["locationAr"].FirstOrDefault();

                var file = Request.Form.Files.FirstOrDefault();

                // التحقق من البيانات المطلوبة
                var errors = new Dictionary<string, string[]>();

                if (string.IsNullOrWhiteSpace(titleEn))
                    errors["TitleEn"] = new[] { "English title is required" };

                if (string.IsNullOrWhiteSpace(titleAr))
                    errors["TitleAr"] = new[] { "Arabic title is required" };

                if (errors.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Validation failed",
                        errors = errors
                    });
                }

                // إنشاء الكارد
                var card = new ProjectHomePageCard();

                // رفع الصورة إذا وجدت
                if (file != null && file.Length > 0)
                {
                    card.ImageRelativePath = HandleImageUpload(file);
                }

                _unitOfWork.ProjectCard.Add(card);
                _unitOfWork.Save();

                // إضافة الترجمات
                var translations = new List<ProjectCardTranslation>();

                // الترجمة الإنجليزية
                if (!string.IsNullOrWhiteSpace(titleEn))
                {
                    translations.Add(new ProjectCardTranslation
                    {
                        ProjectCardId = card.Id,
                        LanguageCode = "en",
                        Title = titleEn,
                        LocationText = locationEn ?? ""
                    });
                }

                // الترجمة العربية
                if (!string.IsNullOrWhiteSpace(titleAr))
                {
                    translations.Add(new ProjectCardTranslation
                    {
                        ProjectCardId = card.Id,
                        LanguageCode = "ar",
                        Title = titleAr,
                        LocationText = locationAr ?? ""
                    });
                }

                // معالجة TranslationsJson إذا وجد (للتوافق)
                var translationsJson = Request.Form["TranslationsJson"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(translationsJson))
                {
                    try
                    {
                        var jsonTranslations = JsonSerializer.Deserialize<List<ProjectCardTranslationDto>>(translationsJson);
                        if (jsonTranslations != null)
                        {
                            foreach (var tr in jsonTranslations)
                            {
                                if (!translations.Any(t => t.LanguageCode == tr.LanguageCode))
                                {
                                    translations.Add(new ProjectCardTranslation
                                    {
                                        ProjectCardId = card.Id,
                                        LanguageCode = tr.LanguageCode,
                                        Title = tr.Title,
                                        LocationText = tr.LocationText ?? ""
                                    });
                                }
                            }
                        }
                    }
                    catch { }
                }

                // حفظ الترجمات
                foreach (var trans in translations)
                {
                    _unitOfWork.ProjectCardTranslation.Add(trans);
                }
                _unitOfWork.Save();

                // تجهيز الرد
                var result = new
                {
                    id = card.Id,
                    imageRelativePath = GetImageUrl(card.ImageRelativePath),
                    translations = translations.Select(t => new
                    {
                        id = t.Id,
                        languageCode = t.LanguageCode,
                        title = t.Title,
                        locationText = t.LocationText
                    })
                };

                return Ok(new
                {
                    success = true,
                    message = "Project card created successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ==================== UPDATE METHOD ====================

        [HttpPut("{id?}")]
        [Authorize(Roles = "MasterAdmin,CreateDeleteAdmin")]
        public IActionResult Update(int? id)
        {
            try
            {
                // استخلاص الـ ID
                int targetId = id ?? 0;
                if (targetId == 0)
                {
                    int.TryParse(Request.Form["Id"].FirstOrDefault(), out targetId);
                }
                if (targetId == 0)
                {
                    int.TryParse(Request.Form["id"].FirstOrDefault(), out targetId);
                }

                if (targetId == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid ID"
                    });
                }

                // قراءة البيانات من Form
                var titleEn = Request.Form["TitleEn"].FirstOrDefault();
                var titleAr = Request.Form["TitleAr"].FirstOrDefault();
                var locationEn = Request.Form["LocationEn"].FirstOrDefault();
                var locationAr = Request.Form["LocationAr"].FirstOrDefault();
                var file = Request.Form.Files.FirstOrDefault();

                // جلب الكارد
                var card = _unitOfWork.ProjectCard.Get(c => c.Id == targetId, includeProperties: "Translations");
                if (card == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Project card not found"
                    });
                }

                bool hasUpdates = false;

                // تحديث الصورة
                if (file != null && file.Length > 0)
                {
                    // حذف الصورة القديمة
                    if (!string.IsNullOrEmpty(card.ImageRelativePath))
                    {
                        string oldPath = Path.Combine(_webHostEnvironment.WebRootPath, card.ImageRelativePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    // رفع الصورة الجديدة
                    card.ImageRelativePath = HandleImageUpload(file);
                    hasUpdates = true;
                }

                var translations = card.Translations.ToList();

                // تحديث الترجمة الإنجليزية
                if (!string.IsNullOrEmpty(titleEn) || locationEn != null)
                {
                    var enTrans = translations.FirstOrDefault(t => t.LanguageCode == "en");
                    if (enTrans != null)
                    {
                        if (!string.IsNullOrEmpty(titleEn))
                            enTrans.Title = titleEn;
                        if (locationEn != null)
                            enTrans.LocationText = locationEn;
                        _unitOfWork.ProjectCardTranslation.Update(enTrans);
                        hasUpdates = true;
                    }
                    else if (!string.IsNullOrEmpty(titleEn))
                    {
                        // إنشاء ترجمة جديدة
                        enTrans = new ProjectCardTranslation
                        {
                            ProjectCardId = card.Id,
                            LanguageCode = "en",
                            Title = titleEn,
                            LocationText = locationEn ?? ""
                        };
                        _unitOfWork.ProjectCardTranslation.Add(enTrans);
                        hasUpdates = true;
                    }
                }

                // تحديث الترجمة العربية
                if (!string.IsNullOrEmpty(titleAr) || locationAr != null)
                {
                    var arTrans = translations.FirstOrDefault(t => t.LanguageCode == "ar");
                    if (arTrans != null)
                    {
                        if (!string.IsNullOrEmpty(titleAr))
                            arTrans.Title = titleAr;
                        if (locationAr != null)
                            arTrans.LocationText = locationAr;
                        _unitOfWork.ProjectCardTranslation.Update(arTrans);
                        hasUpdates = true;
                    }
                    else if (!string.IsNullOrEmpty(titleAr))
                    {
                        // إنشاء ترجمة جديدة
                        arTrans = new ProjectCardTranslation
                        {
                            ProjectCardId = card.Id,
                            LanguageCode = "ar",
                            Title = titleAr,
                            LocationText = locationAr ?? ""
                        };
                        _unitOfWork.ProjectCardTranslation.Add(arTrans);
                        hasUpdates = true;
                    }
                }

                // معالجة TranslationsJson إذا وجد
                var translationsJson = Request.Form["TranslationsJson"].FirstOrDefault();
                if (!hasUpdates && !string.IsNullOrWhiteSpace(translationsJson))
                {
                    try
                    {
                        var jsonTranslations = JsonSerializer.Deserialize<List<ProjectCardTranslationDto>>(translationsJson);
                        if (jsonTranslations != null && jsonTranslations.Any())
                        {
                            foreach (var trDto in jsonTranslations)
                            {
                                var existingTrans = card.Translations
                                    .FirstOrDefault(t => t.LanguageCode == trDto.LanguageCode);

                                if (existingTrans != null)
                                {
                                    existingTrans.Title = trDto.Title;
                                    existingTrans.LocationText = trDto.LocationText ?? "";
                                    _unitOfWork.ProjectCardTranslation.Update(existingTrans);
                                }
                                else
                                {
                                    var newTrans = new ProjectCardTranslation
                                    {
                                        ProjectCardId = card.Id,
                                        LanguageCode = trDto.LanguageCode,
                                        Title = trDto.Title,
                                        LocationText = trDto.LocationText ?? ""
                                    };
                                    _unitOfWork.ProjectCardTranslation.Add(newTrans);
                                }
                            }
                            hasUpdates = true;
                        }
                    }
                    catch { }
                }

                if (!hasUpdates)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No data provided for update"
                    });
                }

                _unitOfWork.ProjectCard.Update(card);
                _unitOfWork.Save();

                // تجهيز الرد
                var updatedTranslations = card.Translations.Select(t => new
                {
                    id = t.Id,
                    languageCode = t.LanguageCode,
                    title = t.Title,
                    locationText = t.LocationText
                });

                return Ok(new
                {
                    success = true,
                    message = "Project card updated successfully",
                    data = new
                    {
                        id = card.Id,
                        imageRelativePath = GetImageUrl(card.ImageRelativePath),
                        translations = updatedTranslations
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ==================== DELETE METHOD ====================

        [HttpDelete("{id}")]
        [Authorize(Roles = "MasterAdmin,CreateDeleteAdmin")]
        public IActionResult Delete(int id)
        {
            try
            {
                var card = _unitOfWork.ProjectCard.Get(c => c.Id == id, includeProperties: "Translations");
                if (card == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Project card not found"
                    });
                }

                // حذف الصورة
                if (!string.IsNullOrEmpty(card.ImageRelativePath))
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, card.ImageRelativePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // حذف الترجمات
                foreach (var trans in card.Translations)
                {
                    _unitOfWork.ProjectCardTranslation.Remove(trans);
                }

                // حذف الكارد
                _unitOfWork.ProjectCard.Remove(card);
                _unitOfWork.Save();

                return Ok(new
                {
                    success = true,
                    message = "Project card deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ==================== HELPER METHODS ====================

        private string HandleImageUpload(IFormFile file)
        {
            try
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
            catch
            {
                return "";
            }
        }

        private string GetImageUrl(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return "";

            // إرجاع المسار النسبي فقط (الـ Frontend هيكمل الـ URL)
            return relativePath;
        }

        private string GetLocalizedValue(ICollection<ProjectCardTranslation> translations, string lang, string field)
        {
            var translation = translations.FirstOrDefault(t => t.LanguageCode == lang);
            if (translation == null)
                translation = translations.FirstOrDefault();

            if (translation == null)
                return "";

            return field == "Title" ? translation.Title : translation.LocationText;
        }
    }
}