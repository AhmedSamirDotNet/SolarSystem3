using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarSystem.DataAccess1;
using SolarSystem.Models1.Models;
using System.ComponentModel.DataAnnotations;

namespace SolarSystem.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectCardsController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // استبدل بـ DbContext الخاص بك

        public ProjectCardsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ProjectCards
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectHomePageCard>>> GetAll()
        {
            return await _context.ProjectHomePageCards
                .Include(c => c.Translations)
                .ToListAsync();
        }

        // GET: api/ProjectCards/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectHomePageCard>> Get(int id)
        {
            var card = await _context.ProjectHomePageCards
                .Include(c => c.Translations)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (card == null)
            {
                return NotFound($"Project card with ID {id} not found.");
            }

            return card;
        }

        // POST: api/ProjectCards
        [HttpPost]
        public async Task<ActionResult<ProjectHomePageCard>> Create([FromBody] CreateProjectCardDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1. إنشاء الكارد
            var card = new ProjectHomePageCard
            {
                ImageRelativePath = createDto.ImageRelativePath ?? ""
            };

            _context.ProjectHomePageCards.Add(card);
            await _context.SaveChangesAsync(); // حفظ مؤقت للحصول على ID

            // 2. إضافة الترجمات
            if (createDto.Translations != null && createDto.Translations.Any())
            {
                foreach (var transDto in createDto.Translations)
                {
                    var translation = new ProjectCardTranslation
                    {
                        ProjectCardId = card.Id,
                        LanguageCode = transDto.LanguageCode,
                        Title = transDto.Title,
                        LocationText = transDto.LocationText ?? ""
                    };
                    _context.ProjectCardTranslations.Add(translation);
                }
                await _context.SaveChangesAsync();
            }

            // إعادة الكارد بالترجمات
            return CreatedAtAction(nameof(Get), new { id = card.Id }, card);
        }

        // PUT: api/ProjectCards/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectCardDto updateDto)
        {
            if (id != updateDto.Id)
                return BadRequest("ID mismatch");

            var card = await _context.ProjectHomePageCards
                .Include(c => c.Translations)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (card == null)
                return NotFound($"Project card with ID {id} not found.");

            // تحديث الصورة
            if (updateDto.ImageRelativePath != null)
                card.ImageRelativePath = updateDto.ImageRelativePath;

            // تحديث الترجمات
            if (updateDto.Translations != null && updateDto.Translations.Any())
            {
                foreach (var transDto in updateDto.Translations)
                {
                    var existingTrans = card.Translations
                        .FirstOrDefault(t => t.LanguageCode == transDto.LanguageCode);

                    if (existingTrans != null)
                    {
                        // تحديث موجود
                        existingTrans.Title = transDto.Title;
                        existingTrans.LocationText = transDto.LocationText ?? "";
                    }
                    else
                    {
                        // إضافة جديد
                        var newTrans = new ProjectCardTranslation
                        {
                            ProjectCardId = card.Id,
                            LanguageCode = transDto.LanguageCode,
                            Title = transDto.Title,
                            LocationText = transDto.LocationText ?? ""
                        };
                        _context.ProjectCardTranslations.Add(newTrans);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Ok(card);
        }

        // DELETE: api/ProjectCards/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var card = await _context.ProjectHomePageCards
                .Include(c => c.Translations)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (card == null)
                return NotFound($"Project card with ID {id} not found.");

            // حذف الترجمات أولاً (EF Core ممكن تعملها automatically لو في Cascade Delete)
            _context.ProjectCardTranslations.RemoveRange(card.Translations);

            // حذف الكارد
            _context.ProjectHomePageCards.Remove(card);

            await _context.SaveChangesAsync();
            return Ok($"Project card with ID {id} deleted successfully.");
        }
    }

    // ============ DTOs ============

    public class CreateProjectCardDto
    {
        public string? ImageRelativePath { get; set; }

        [Required]
        public List<ProjectCardTranslationDto> Translations { get; set; } = new();
    }

    public class UpdateProjectCardDto
    {
        [Required]
        public int Id { get; set; }

        public string? ImageRelativePath { get; set; }

        public List<ProjectCardTranslationDto>? Translations { get; set; }
    }

    public class ProjectCardTranslationDto
    {
        [Required]
        [StringLength(10)]
        public string LanguageCode { get; set; } = "en";

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? LocationText { get; set; }
    }
}