using SolarSystem.Models1.Models;
using System.ComponentModel.DataAnnotations;

namespace SolarSystem.Models1.Dtos
{
    // ============ PRODUCT DTOs ============
    
    /// <summary>Product Response DTO - for displaying products</summary>
    public class ProductDto
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public int SectionId { get; set; }
        public string? Name { get; set; }
        public string? MainDesc { get; set; }
        public string? SubDesc { get; set; }
        public List<ImageDto> Images { get; set; } = new();
    }

    /// <summary>Create Product DTO - for POST requests</summary>
    public class CreateProductDto
    {
        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "SectionId is required")]
        public int SectionId { get; set; }
    }

    /// <summary>Update Product DTO - for PUT requests</summary>
    public class UpdateProductDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public int SectionId { get; set; }
    }

    /// <summary>Product Translation DTO</summary>
    public class ProductTranslationDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(2, MinimumLength = 2)]
        public string LanguageCode { get; set; } = "en";

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? MainDesc { get; set; }

        [StringLength(500)]
        public string? SubDesc { get; set; }

        public int ProductId { get; set; }
    }

    /// <summary>Product Detail DTO - for GET full details with translations (Admin)</summary>
    public class ProductDetailDto
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public int SectionId { get; set; }
        public List<ProductTranslationDto> Translations { get; set; } = new();
        public List<ImageDto> Images { get; set; } = new();
    }

    /// <summary>Paged Product Response DTO</summary>
    public class ProductPagedResponseDto
    {
        public List<ProductDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    // ============ SECTION DTOs ============

    /// <summary>Section Response DTO</summary>
    public class SectionDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<SectionTranslationDto> Translations { get; set; } = new();
    }

    /// <summary>Create Section DTO - for POST requests</summary>
    public class CreateSectionDto
    {
        [Required(ErrorMessage = "English name is required")]
        [StringLength(200, MinimumLength = 1)]
        public string? NameEn { get; set; }

        [Required(ErrorMessage = "Arabic name is required")]
        [StringLength(200, MinimumLength = 1)]
        public string? NameAr { get; set; }
    }

    /// <summary>Update Section DTO - for PUT requests</summary>
    public class UpdateSectionDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "English name is required")]
        [StringLength(200, MinimumLength = 1)]
        public string? NameEn { get; set; }

        [Required(ErrorMessage = "Arabic name is required")]
        [StringLength(200, MinimumLength = 1)]
        public string? NameAr { get; set; }
    }

    /// <summary>Section Translation DTO</summary>
    public class SectionTranslationDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(2, MinimumLength = 2)]
        public string LanguageCode { get; set; } = "en";

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        public int SectionId { get; set; }
    }

    /// <summary>Section Detail DTO - with all translations</summary>
    public class SectionDetailDto
    {
        public int Id { get; set; }
        public List<SectionTranslationDto> Translations { get; set; } = new();
    }

    // ============ COMMON DTOs ============

    /// <summary>Image Response DTO</summary>
    public class ImageDto
    {
        public int Id { get; set; }
        public string RelativePath { get; set; } = string.Empty;
        public int ProductId { get; set; }
    }

    /// <summary>Admin Login Request DTO</summary>
    public class LoginRequestDto
    {
        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>Admin Login Response DTO</summary>
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    /// <summary>Admin User DTO</summary>
    public class AdminDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    /// <summary>Create Admin DTO - for registration</summary>
    public class CreateAdminDto
    {
        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
        public AdminRole? Role { get; set; }

    }

    /// <summary>Update Admin Role DTO</summary>
    public class UpdateAdminRoleDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = string.Empty;
    }

    /// <summary>Generic Error Response DTO</summary>
    public class ErrorResponseDto
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, string[]>? Errors { get; set; }
    }

    // ============ PROJECT CARD DTOs ============

    public class ProjectHomePageCardDto
    {
        public int Id { get; set; }
        public string ImageRelativePath { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? LocationText { get; set; }
    }

    public class ProjectCardTranslationDto
    {
        public int Id { get; set; }
        [Required]
        public string LanguageCode { get; set; } = "en";
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? LocationText { get; set; }
        public int ProjectCardId { get; set; }
    }

    public class ProjectCardDetailDto
    {
        public int Id { get; set; }
        public string ImageRelativePath { get; set; } = string.Empty;
        public List<ProjectCardTranslationDto> Translations { get; set; } = new();
    }

    public class CreateProjectHomePageCardDto
    {
        public string? ImageRelativePath { get; set; }

        [Required]
        public string TitleEn { get; set; } = string.Empty;
        public string? LocationEn { get; set; }

        [Required]
        public string TitleAr { get; set; } = string.Empty;
        public string? LocationAr { get; set; }
    }

    public class UpdateProjectHomePageCardDto
    {
        public int Id { get; set; }
        public string? ImageRelativePath { get; set; }
    }

    /// <summary>Generic Success Response DTO</summary>
    public class SuccessResponseDto
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
    // ============ CUSTOMER & FEEDBACK DTOs ============

    public class CustomerTranslationDto
    {
        public int Id { get; set; }
        [Required]
        public string LanguageCode { get; set; } = "en";
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Job { get; set; }
        public int CustomerId { get; set; }
    }

    public class CustomerDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Job { get; set; }
        public List<CustomerTranslationDto> Translations { get; set; } = new();
    }

    public class CreateCustomerDto
    {
        // No direct fields, usually we create the entity then add translations
        // or we can allow passing NameEn/NameAr like Section pattern
        [Required(ErrorMessage = "English name is required")]
        public string? NameEn { get; set; }
        public string? JobEn { get; set; }

        [Required(ErrorMessage = "Arabic name is required")]
        public string? NameAr { get; set; }
        public string? JobAr { get; set; }
    }

    public class UpdateCustomerDto
    {
        [Required]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "English name is required")]
        public string? NameEn { get; set; }
        public string? JobEn { get; set; }

        [Required(ErrorMessage = "Arabic name is required")]
        public string? NameAr { get; set; }
        public string? JobAr { get; set; }
    }

    public class CustomerFeedbackTranslationDto
    {
        public int Id { get; set; }
        [Required]
        public string LanguageCode { get; set; } = "en";
        [Required]
        public string FeedbackText { get; set; } = string.Empty;
        public int CustomerFeedbackId { get; set; }
    }

    public class CustomerFeedbackDto
    {
        public int Id { get; set; }
        public string? Feedback { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public List<CustomerFeedbackTranslationDto> Translations { get; set; } = new();
    }

    public class CreateCustomerFeedbackDto
    {
        [Required(ErrorMessage = "CustomerId is required")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "English feedback text is required")]
        public string? FeedbackEn { get; set; }

        [Required(ErrorMessage = "Arabic feedback text is required")]
        public string? FeedbackAr { get; set; }
    }

    public class UpdateCustomerFeedbackDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "CustomerId is required")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "English feedback text is required")]
        public string? FeedbackEn { get; set; }

        [Required(ErrorMessage = "Arabic feedback text is required")]
        public string? FeedbackAr { get; set; }
    }
}
