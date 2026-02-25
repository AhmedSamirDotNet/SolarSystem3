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
    public class SectionController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public SectionController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // عرض جميع الأقسام (متاح للجميع)
        [HttpGet]
        public IActionResult GetAll([FromQuery] string lang = "en")
        {
            var sections = _unitOfWork.Section.GetAll(includeProperties: "Translations");
            var result = sections.ToDtoList(lang);
            return Ok(result);
        }

        // عرض تفاصيل قسم معين مع المنتجات التابعة له
        [HttpGet("{id}")]
        public IActionResult GetById(int id, [FromQuery] string lang = "en")
        {
            var section = _unitOfWork.Section.Get(u => u.Id == id, includeProperties: "Translations");
            if (section == null)
            {
                return NotFound(new ErrorResponseDto { Message = "القسم غير موجود" });
            }
            return Ok(section.ToDto(lang));
        }

        // إنشاء قسم جديد (للأدمن فقط)
        [HttpPost]
        [Authorize(Roles = "MasterAdmin,CreateDeleteAdmin")]
        public IActionResult Create([FromBody] CreateSectionDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponseDto { Message = "بيانات القسم غير صالحة", Errors = ModelState.Values.SelectMany(v => v.Errors).GroupBy(e => "validation").ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()) });
            }

            var section = new Section();
            _unitOfWork.Section.Add(section);
            _unitOfWork.Save();

            // Create English translation
            var enTranslation = new SectionTranslation
            {
                LanguageCode = "en",
                Name = createDto.NameEn!,
                SectionId = section.Id
            };
            _unitOfWork.SectionTranslation.Add(enTranslation);

            // Create Arabic translation
            var arTranslation = new SectionTranslation
            {
                LanguageCode = "ar",
                Name = createDto.NameAr!,
                SectionId = section.Id
            };
            _unitOfWork.SectionTranslation.Add(arTranslation);

            _unitOfWork.Save();

            return Ok(new SuccessResponseDto { Message = "تم إنشاء القسم بنجاح", Data = new { sectionId = section.Id } });
        }

        // تحديث قسم موجود (للأدمن فقط)
        [HttpPut]
        [Authorize(Roles = "MasterAdmin,Editor,3,2")]
        public IActionResult Update([FromBody] UpdateSectionDto updateDto)
        {
            if (!ModelState.IsValid || updateDto.Id <= 0)
            {
                return BadRequest(new ErrorResponseDto { Message = "بيانات القسم غير صالحة" });
            }

            var sectionFromDb = _unitOfWork.Section.Get(u => u.Id == updateDto.Id, includeProperties: "Translations");
            if (sectionFromDb == null)
            {
                return NotFound(new ErrorResponseDto { Message = "القسم غير موجود لتحديثه" });
            }

            // Update English translation
            var enTranslation = sectionFromDb.Translations?.FirstOrDefault(t => t.LanguageCode == "en");
            if (enTranslation != null)
            {
                enTranslation.Name = updateDto.NameEn!;
                _unitOfWork.SectionTranslation.Update(enTranslation);
            }
            else
            {
                var newEnTranslation = new SectionTranslation
                {
                    LanguageCode = "en",
                    Name = updateDto.NameEn!,
                    SectionId = sectionFromDb.Id
                };
                _unitOfWork.SectionTranslation.Add(newEnTranslation);
            }

            // Update Arabic translation
            var arTranslation = sectionFromDb.Translations?.FirstOrDefault(t => t.LanguageCode == "ar");
            if (arTranslation != null)
            {
                arTranslation.Name = updateDto.NameAr!;
                _unitOfWork.SectionTranslation.Update(arTranslation);
            }
            else
            {
                var newArTranslation = new SectionTranslation
                {
                    LanguageCode = "ar",
                    Name = updateDto.NameAr!,
                    SectionId = sectionFromDb.Id
                };
                _unitOfWork.SectionTranslation.Add(newArTranslation);
            }

            _unitOfWork.Save();

            return Ok(new SuccessResponseDto { Message = "تم تحديث القسم بنجاح" });
        }

        // حذف قسم (للماستر أدمن فقط)
        [HttpDelete("{id}")]
        [Authorize(Roles = "MasterAdmin,CreateDeleteAdmin")]
        public IActionResult Delete(int id)
        {
            var section = _unitOfWork.Section.Get(u => u.Id == id, includeProperties: "Products");

            if (section == null)
            {
                return NotFound(new ErrorResponseDto { Message = "القسم غير موجود" });
            }

            if (section.Products != null && section.Products.Any())
            {
                return BadRequest(new ErrorResponseDto { Message = "لا يمكن حذف قسم يحتوي على منتجات مرتبطة." });
            }

            _unitOfWork.Section.Remove(section);
            _unitOfWork.Save();

            return Ok(new SuccessResponseDto { Message = "تم حذف القسم بنجاح" });
        }
    }
}