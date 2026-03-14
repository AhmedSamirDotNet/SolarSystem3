using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarSystem.DataAccess1;
using SolarSystem.Models1.Models;

namespace SolarSystem.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectCardsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProjectCardsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 1. GET: api/ProjectCards (عرض الكل بالشكل اللي طلبته)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetProjectCards()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

            var cards = await _context.ProjectHomePageCards
                .Include(c => c.Translations)
                .ToListAsync();

            var result = cards.Select(c => new
            {
                id = c.Id,
                imageUrl = $"{baseUrl}{c.ImageRelativePath}",
                translation = c.Translations.Select(t => new {
                    t.Id,
                    t.ProjectCardId,
                    projectCard = new
                    {
                        c.Id,
                        c.ImageRelativePath,
                        translations = c.Translations.Select(innerT => new {
                            innerT.Id,
                            innerT.ProjectCardId,
                            projectCard = (object)null, // عشان ميعملش Loop لا نهائي
                            innerT.LanguageCode,
                            innerT.Title,
                            innerT.LocationText
                        })
                    },
                    t.LanguageCode,
                    t.Title,
                    t.LocationText
                }).FirstOrDefault(t => t.LanguageCode == "en") // بيجيب الترجمة الإنجليزي كـ Default
            });

            return Ok(result);
        }

        // 2. GET: api/ProjectCards/5 (عرض كارد واحد)
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectHomePageCard>> GetProjectCard(int id)
        {
            var card = await _context.ProjectHomePageCards
                .Include(c => c.Translations)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (card == null) return NotFound(new { message = "الكارد ده مش موجود يا هندسة! 🧐" });

            return Ok(card);
        }

        // 3. POST: api/ProjectCards (إضافة كارد جديد مع رفع الصورة)
        // ملاحظة: يفضل تستخدم FormData في الـ Frontend عشان تبعت الصورة والنصوص مع بعض
        [HttpPost]
        public async Task<ActionResult<ProjectHomePageCard>> PostProjectCard([FromForm] ProjectCardUploadDto dto)
        {
            if (dto.ImageFile == null) return BadRequest("لازم ترفع صورة! 🖼️");

            // 1. حفظ الصورة في wwwroot
            string folder = Path.Combine(_env.WebRootPath, "images", "projects");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ImageFile.FileName);
            string filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.ImageFile.CopyToAsync(stream);
            }

            // 2. تجهيز الموديل للحفظ
            var newCard = new ProjectHomePageCard
            {
                ImageRelativePath = $"/images/projects/{fileName}",
                Translations = new List<ProjectCardTranslation>
                {
                    new ProjectCardTranslation {
                        LanguageCode = "en",
                        Title = dto.TitleEn,
                        LocationText = dto.LocationEn
                    },
                    new ProjectCardTranslation {
                        LanguageCode = "ar",
                        Title = dto.TitleAr,
                        LocationText = dto.LocationAr
                    }
                }
            };

            _context.ProjectHomePageCards.Add(newCard);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProjectCard), new { id = newCard.Id }, newCard);
        }

        // 4. DELETE: api/ProjectCards/5 (حذف الكارد وصورته)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProjectCard(int id)
        {
            var card = await _context.ProjectHomePageCards.FindAsync(id);
            if (card == null) return NotFound();

            // مسح ملف الصورة من السيرفر عشان ميملاش مساحة الفاضي
            var fullPath = Path.Combine(_env.WebRootPath, card.ImageRelativePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            _context.ProjectHomePageCards.Remove(card);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم الحذف بنجاح.. السيرفر بيشكرك! 🧹" });
        }
    }

    // DTO بسيط لاستلام البيانات من الـ Form
    public class ProjectCardUploadDto
    {
        public IFormFile ImageFile { get; set; }
        public string TitleEn { get; set; }
        public string LocationEn { get; set; }
        public string TitleAr { get; set; }
        public string LocationAr { get; set; }
    }
}