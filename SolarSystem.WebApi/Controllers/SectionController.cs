using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarSystem.DataAccess1.Repository.IRepository;
using SolarSystem.Models1.Models;

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

        // 1. عرض جميع الأقسام (متاح للجميع)
        [HttpGet]
        public IActionResult GetAll([FromQuery] string lang = "en")
        {
            var sections = _unitOfWork.Section.GetAll(includeProperties: "Translations");

            // Map to language-specific DTOs
            var result = sections.Select(s => new
            {
                s.Id,
                Name = s.Translations.FirstOrDefault(t => t.LanguageCode == lang)?.Name
                       ?? s.Translations.FirstOrDefault()?.Name
            });

            return Ok(result);
        }

        // 2. عرض تفاصيل قسم معين مع المنتجات التابعة له
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var section = _unitOfWork.Section.Get(u => u.Id == id, includeProperties: "Products");
            if (section == null)
            {
                return NotFound(new { message = "القسم غير موجود" });
            }
            return Ok(section);
        }

        // 3. إنشاء قسم جديد (للأدمن فقط)
        [HttpPost]
        [Authorize(Roles = "MasterAdmin,Editor,3,2")]
        public IActionResult Create([FromBody] Section section)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // If translations were provided, ensure at least a default translation exists
            if (section.Translations == null || !section.Translations.Any())
            {
                section.Translations = new List<SectionTranslation>
                {
                    new SectionTranslation { LanguageCode = "en", Name = "New Section" }
                };
            }

            _unitOfWork.Section.Add(section);
            _unitOfWork.Save();
            return Ok(new { message = "تم إنشاء القسم بنجاح", sectionId = section.Id });
        }

        // 4. تحديث قسم موجود (للأدمن فقط)
        [HttpPut]
        [Authorize(Roles = "MasterAdmin,Editor,3,2")]
        public IActionResult Update([FromBody] Section section)
        {
            if (!ModelState.IsValid || section.Id <= 0)
            {
                return BadRequest(new { message = "بيانات القسم غير صالحة" });
            }

            var sectionFromDb = _unitOfWork.Section.Get(u => u.Id == section.Id);
            if (sectionFromDb == null)
            {
                return NotFound(new { message = "القسم غير موجود لتحديثه" });
            }

            // Update translations if provided
            if (section.Translations != null && section.Translations.Any())
            {
                foreach (var tr in section.Translations)
                {
                    if (tr.Id == 0)
                    {
                        _unitOfWork.SectionTranslation.Add(tr);
                    }
                    else
                    {
                        _unitOfWork.SectionTranslation.Update(tr);
                    }
                }
            }

            _unitOfWork.Section.Update(section);
            _unitOfWork.Save();

            return Ok(new { message = "تم تحديث القسم بنجاح" });
        }

        // 5. حذف قسم (للماستر أدمن فقط)
        [HttpDelete("{id}")]
        [Authorize(Roles = "MasterAdmin,3")]
        public IActionResult Delete(int id)
        {
            // نتحقق أولا إذا كان القسم يحتوي على منتجات مرتبطة به
            var section = _unitOfWork.Section.Get(u => u.Id == id, includeProperties: "Products");

            if (section == null)
            {
                return NotFound(new { message = "القسم غير موجود" });
            }

            // منطق أمان: منع حذف القسم إذا كان يحتوي على منتجات لتجنب مشاكل الربط
            if (section.Products != null && section.Products.Any())
            {
                return BadRequest(new { message = "لا يمكن حذف قسم يحتوي على منتجات مرتبطة. قم بنقل أو حذف المنتجات أولاً." });
            }

            _unitOfWork.Section.Remove(section);
            _unitOfWork.Save();

            return Ok(new { message = "تم حذف القسم بنجاح" });
        }
    }
}