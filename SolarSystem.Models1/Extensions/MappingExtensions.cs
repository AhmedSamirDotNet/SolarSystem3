using SolarSystem.Models1.Models;
using SolarSystem.Models1.Dtos;

namespace SolarSystem.Models1.Extensions
{
    /// <summary>Extension methods for mapping between Domain Models and DTOs</summary>
    public static class MappingExtensions
    {
        // ============ PRODUCT MAPPINGS ============

        /// <summary>Convert Product domain model to ProductDto (with language-aware translation)</summary>
        public static ProductDto ToDto(this Product product, string lang = "en")
        {
            var translation = product.Translations?.FirstOrDefault(t => t.LanguageCode == lang)
                ?? product.Translations?.FirstOrDefault();

            return new ProductDto
            {
                Id = product.Id,
                Price = product.Price,
                SectionId = product.SectionId,
                Name = translation?.Name,
                MainDesc = translation?.MainDesc,
                SubDesc = translation?.SubDesc,
                Images = product.Images?.Select(img => img.ToDto()).ToList() ?? new()
            };
        }

        /// <summary>Convert Product to ProductDetailDto (with full translations - for admin)</summary>
        public static ProductDetailDto ToDetailDto(this Product product)
        {
            return new ProductDetailDto
            {
                Id = product.Id,
                Price = product.Price,
                SectionId = product.SectionId,
                Translations = product.Translations?
                    .Select(t => t.ToDto())
                    .ToList() ?? new(),
                Images = product.Images?
                    .Select(img => img.ToDto())
                    .ToList() ?? new()
            };
        }

        /// <summary>Convert CreateProductDto to Product domain model</summary>
        public static Product ToModel(this CreateProductDto dto)
        {
            return new Product
            {
                Price = dto.Price,
                SectionId = dto.SectionId
            };
        }

        /// <summary>Update Product from UpdateProductDto</summary>
        public static void UpdateFromDto(this Product product, UpdateProductDto dto)
        {
            product.Id = dto.Id;
            product.Price = dto.Price;
            product.SectionId = dto.SectionId;
        }

        /// <summary>Convert list of Products to list of ProductDtos</summary>
        public static List<ProductDto> ToDtoList(this IEnumerable<Product> products, string lang = "en")
        {
            return products.Select(p => p.ToDto(lang)).ToList();
        }

        // ============ PRODUCT TRANSLATION MAPPINGS ============

        /// <summary>Convert ProductTranslation to ProductTranslationDto</summary>
        public static ProductTranslationDto ToDto(this ProductTranslation translation)
        {
            return new ProductTranslationDto
            {
                Id = translation.Id,
                LanguageCode = translation.LanguageCode,
                Name = translation.Name,
                MainDesc = translation.MainDesc,
                SubDesc = translation.SubDesc,
                ProductId = translation.ProductId
            };
        }

        /// <summary>Convert ProductTranslationDto to ProductTranslation domain model</summary>
        public static ProductTranslation ToModel(this ProductTranslationDto dto)
        {
            return new ProductTranslation
            {
                Id = dto.Id,
                LanguageCode = dto.LanguageCode,
                Name = dto.Name,
                MainDesc = dto.MainDesc,
                SubDesc = dto.SubDesc,
                ProductId = dto.ProductId
            };
        }

        // ============ SECTION MAPPINGS ============

        /// <summary>Convert Section to SectionDto (with language-aware translation)</summary>
        public static SectionDto ToDto(this Section section, string lang = "en")
        {
            var translation = section.Translations?.FirstOrDefault(t => t.LanguageCode == lang)
                ?? section.Translations?.FirstOrDefault();

            return new SectionDto
            {
                Id = section.Id,
                Name = translation?.Name,
                Translations = section.Translations?
                    .Select(t => t.ToDto())
                    .ToList() ?? new()
            };
        }

        /// <summary>Convert Section to SectionDetailDto (with all translations - for admin)</summary>
        public static SectionDetailDto ToDetailDto(this Section section)
        {
            return new SectionDetailDto
            {
                Id = section.Id,
                Translations = section.Translations?
                    .Select(t => t.ToDto())
                    .ToList() ?? new()
            };
        }

        /// <summary>Convert list of Sections to list of SectionDtos</summary>
        public static List<SectionDto> ToDtoList(this IEnumerable<Section> sections, string lang = "en")
        {
            return sections.Select(s => s.ToDto(lang)).ToList();
        }

        // ============ SECTION TRANSLATION MAPPINGS ============

        /// <summary>Convert SectionTranslation to SectionTranslationDto</summary>
        public static SectionTranslationDto ToDto(this SectionTranslation translation)
        {
            return new SectionTranslationDto
            {
                Id = translation.Id,
                LanguageCode = translation.LanguageCode,
                Name = translation.Name,
                SectionId = translation.SectionId
            };
        }

        /// <summary>Convert SectionTranslationDto to SectionTranslation domain model</summary>
        public static SectionTranslation ToModel(this SectionTranslationDto dto)
        {
            return new SectionTranslation
            {
                Id = dto.Id,
                LanguageCode = dto.LanguageCode,
                Name = dto.Name,
                SectionId = dto.SectionId
            };
        }

        // ============ IMAGE MAPPINGS ============

        /// <summary>Convert Image to ImageDto</summary>
        public static ImageDto ToDto(this Image image)
        {
            return new ImageDto
            {
                Id = image.Id,
                RelativePath = image.RelativePath,
                ProductId = image.ProductId
            };
        }

        // ============ ADMIN MAPPINGS ============

        /// <summary>Convert Admin to AdminDto (excludes password hash)</summary>
        public static AdminDto ToDto(this Admin admin)
        {
            return new AdminDto
            {
                Id = admin.Id,
                Username = admin.Username,
                Role = Enum.GetName(typeof(AdminRole), admin.Role) ?? admin.Role.ToString()
            };
        }

        // ============ PROJECT CARD MAPPINGS ============

        public static ProjectHomePageCardDto ToDto(this ProjectHomePageCard card, string lang = "en")
        {
            var translation = card.Translations?.FirstOrDefault(t => t.LanguageCode == lang)
                ?? card.Translations?.FirstOrDefault(t => t.LanguageCode == "en")
                ?? card.Translations?.FirstOrDefault();

            return new ProjectHomePageCardDto
            {
                Id = card.Id,
                ImageRelativePath = card.ImageRelativePath,
                Title = translation?.Title,
                LocationText = translation?.LocationText
            };
        }

        public static ProjectCardDetailDto ToDetailDto(this ProjectHomePageCard card)
        {
            return new ProjectCardDetailDto
            {
                Id = card.Id,
                ImageRelativePath = card.ImageRelativePath,
                Translations = card.Translations?.Select(t => t.ToDto()).ToList() ?? new()
            };
        }

        public static ProjectHomePageCard ToModel(this CreateProjectHomePageCardDto dto)
        {
            return new ProjectHomePageCard
            {
                ImageRelativePath = dto.ImageRelativePath ?? string.Empty
            };
        }

        public static void UpdateFromDto(this ProjectHomePageCard card, UpdateProjectHomePageCardDto dto)
        {
            if (!string.IsNullOrEmpty(dto.ImageRelativePath))
            {
                card.ImageRelativePath = dto.ImageRelativePath;
            }
        }

        public static ProjectCardTranslationDto ToDto(this ProjectCardTranslation translation)
        {
            return new ProjectCardTranslationDto
            {
                Id = translation.Id,
                LanguageCode = translation.LanguageCode,
                Title = translation.Title,
                LocationText = translation.LocationText,
                ProjectCardId = translation.ProjectCardId
            };
        }

        public static ProjectCardTranslation ToModel(this ProjectCardTranslationDto dto)
        {
            return new ProjectCardTranslation
            {
                Id = dto.Id,
                LanguageCode = dto.LanguageCode,
                Title = dto.Title,
                LocationText = dto.LocationText ?? string.Empty,
                ProjectCardId = dto.ProjectCardId
            };
        }

        // ============ CUSTOMER MAPPINGS ============

        public static CustomerDto ToDto(this Customer customer, string lang = "en")
        {
            var translation = customer.Translations?.FirstOrDefault(t => t.LanguageCode == lang)
                ?? customer.Translations?.FirstOrDefault();

            return new CustomerDto
            {
                Id = customer.Id,
                Name = translation?.Name,
                Job = translation?.Job,
                Translations = customer.Translations?.Select(t => t.ToDto()).ToList() ?? new()
            };
        }

        public static Customer ToModel(this CreateCustomerDto dto)
        {
            // The translations will be added separately or handled in controller
            return new Customer();
        }

        public static CustomerTranslationDto ToDto(this CustomerTranslation translation)
        {
            return new CustomerTranslationDto
            {
                Id = translation.Id,
                LanguageCode = translation.LanguageCode,
                Name = translation.Name,
                Job = translation.Job,
                CustomerId = translation.CustomerId
            };
        }

        public static CustomerTranslation ToModel(this CustomerTranslationDto dto)
        {
            return new CustomerTranslation
            {
                Id = dto.Id,
                LanguageCode = dto.LanguageCode,
                Name = dto.Name,
                Job = dto.Job,
                CustomerId = dto.CustomerId
            };
        }

        // ============ CUSTOMER FEEDBACK MAPPINGS ============

        public static CustomerFeedbackDto ToDto(this CustomerFeedBack feedback, string lang = "en")
        {
            var translation = feedback.Translations?.FirstOrDefault(t => t.LanguageCode == lang)
                ?? feedback.Translations?.FirstOrDefault();
            
            var customerTranslation = feedback.Customer?.Translations?.FirstOrDefault(t => t.LanguageCode == lang)
                ?? feedback.Customer?.Translations?.FirstOrDefault();

            return new CustomerFeedbackDto
            {
                Id = feedback.Id,
                Feedback = translation?.FeedbackText,
                CustomerId = feedback.CustomerId,
                CustomerName = customerTranslation?.Name,
                Translations = feedback.Translations?.Select(t => t.ToDto()).ToList() ?? new()
            };
        }

        public static CustomerFeedBack ToModel(this CreateCustomerFeedbackDto dto)
        {
            return new CustomerFeedBack
            {
                CustomerId = dto.CustomerId
            };
        }

        public static CustomerFeedbackTranslationDto ToDto(this CustomerFeedbackTranslation translation)
        {
            return new CustomerFeedbackTranslationDto
            {
                Id = translation.Id,
                LanguageCode = translation.LanguageCode,
                FeedbackText = translation.FeedbackText,
                CustomerFeedbackId = translation.CustomerFeedbackId
            };
        }

        public static CustomerFeedbackTranslation ToModel(this CustomerFeedbackTranslationDto dto)
        {
            return new CustomerFeedbackTranslation
            {
                Id = dto.Id,
                LanguageCode = dto.LanguageCode,
                FeedbackText = dto.FeedbackText,
                CustomerFeedbackId = dto.CustomerFeedbackId
            };
        }
    }
}
