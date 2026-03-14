using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarSystem.DataAccess1.Repository.IRepository;
using SolarSystem.Models1.Helpers;
using SolarSystem.Models1.Models;
using SolarSystem.Models1.Dtos;
using SolarSystem.Models1.Extensions;
using System.Text.Json;

namespace SolarSystem.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("full/{id}")]
        [Authorize(Roles = "MasterAdmin,CreateDeleteAdmin")]
        public IActionResult GetFull(int id)
        {
            var product = _unitOfWork.Product.Get(u => u.Id == id, includeProperties: "Images,Section,Translations");
            if (product == null) return NotFound(new ErrorResponseDto { Message = "Product not found" });
            return Ok(NormalizeProductDetailImageUrls(product.ToDetailDto()));
        }

        [HttpGet]
        public IActionResult GetAll(int? sectionId, int pageNumber = 1, int pageSize = 10, [FromQuery] string lang = "en")
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            IEnumerable<Product> productList;

            if (sectionId != null && sectionId > 0)
            {
                productList = _unitOfWork.Product.GetAll(
                    u => u.SectionId == sectionId,
                    includeProperties: "Images,Translations");
            }
            else
            {
                productList = _unitOfWork.Product.GetAll(includeProperties: "Images,Translations");
            }

            int totalCount = productList.Count();

            var pagedData = productList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Map to language-aware DTO using extensions
            var items = pagedData
                .Select(p => NormalizeProductDtoImageUrls(p.ToDto(lang)))
                .ToList();

            var response = new ProductPagedResponseDto
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        public IActionResult Details(int id, [FromQuery] string lang = "en")
        {
            var product = _unitOfWork.Product.Get(u => u.Id == id, includeProperties: "Images,Section,Translations");
            if (product == null) return NotFound(new ErrorResponseDto { Message = "المنتج غير موجود." });

            return Ok(NormalizeProductDtoImageUrls(product.ToDto(lang)));
        }

        // Translation endpoints
        [HttpPost("{productId}/translation")]
        [Authorize(Roles = "MasterAdmin,CreateDeleteAdmin")]
        public IActionResult AddTranslation(int productId, [FromBody] ProductTranslationDto translationDto)
        {
            if (translationDto == null || translationDto.ProductId != productId) return BadRequest(new ErrorResponseDto { Message = "Invalid translation data" });
            var translation = translationDto.ToModel();
            _unitOfWork.ProductTranslation.Add(translation);
            _unitOfWork.Save();
            return Ok(translation.ToDto());
        }

        [HttpPut("translation")]
        [Authorize(Roles = "MasterAdmin,CreateDeleteAdmin")]
        public IActionResult UpdateTranslation([FromBody] ProductTranslationDto translationDto)
        {
            if (translationDto == null || translationDto.Id <= 0) return BadRequest(new ErrorResponseDto { Message = "Invalid translation data" });
            var translation = translationDto.ToModel();
            _unitOfWork.ProductTranslation.Update(translation);
            _unitOfWork.Save();
            return Ok(translation.ToDto());
        }

        // إضافة منتج جديد مع رفع الصور
        [HttpPost]
        [Authorize(Roles = "MasterAdmin,CreateDeleteAdmin")]
        public IActionResult Create([FromForm] CreateProductDto createDto, [FromForm] string? TranslationsJson, List<IFormFile> files)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            files = ResolveFiles(files);

            var product = createDto.ToModel();
            _unitOfWork.Product.Add(product);
            _unitOfWork.Save(); 

            if (files != null && files.Count > 0)
            {
                HandleImageUpload(product.Id, files);
                _unitOfWork.Save();
            }

            // Parse translations JSON if provided (multipart/form-data sends it as string)
            bool translationsCreated = false;
            if (!string.IsNullOrWhiteSpace(TranslationsJson))
            {
                try
                {
                    var translations = JsonSerializer.Deserialize<List<ProductTranslationDto>>(TranslationsJson);
                    if (translations != null && translations.Count > 0)
                    {
                        foreach (var tr in translations)
                        {
                            var translation = tr.ToModel();
                            translation.ProductId = product.Id;
                            _unitOfWork.ProductTranslation.Add(translation);
                        }
                        _unitOfWork.Save();
                        translationsCreated = true;
                    }
                }
                catch (JsonException)
                {
                    // Log error but don't fail - create default translations instead
                }
            }

            // If no translations provided or parsing failed, create default EN/AR translations
            if (!translationsCreated)
            {
                var enTranslation = new ProductTranslation
                {
                    LanguageCode = "en",
                    Name = "Product",
                    MainDesc = string.Empty,
                    SubDesc = string.Empty,
                    ProductId = product.Id
                };
                _unitOfWork.ProductTranslation.Add(enTranslation);

                var arTranslation = new ProductTranslation
                {
                    LanguageCode = "ar",
                    Name = "منتج",
                    MainDesc = string.Empty,
                    SubDesc = string.Empty,
                    ProductId = product.Id
                };
                _unitOfWork.ProductTranslation.Add(arTranslation);
                _unitOfWork.Save();
            }

            return Ok(new SuccessResponseDto { Message = "تمت إضافة المنتج بنجاح", Data = new { productId = product.Id } });
        }

        // تحديث المنتج
        [HttpPut]
        [AllowAnonymous]
        public IActionResult Update([FromForm] UpdateProductDto updateDto, [FromForm] string? TranslationsJson, List<IFormFile> files)
        {
            if (!ModelState.IsValid || updateDto.Id <= 0) return BadRequest(new ErrorResponseDto { Message = "بيانات غير صالحة." });

            files = ResolveFiles(files);

            var objFromDb = _unitOfWork.Product.Get(u => u.Id == updateDto.Id, includeProperties: "Translations");
            if (objFromDb == null) return NotFound(new ErrorResponseDto { Message = "المنتج غير موجود." });

            objFromDb.UpdateFromDto(updateDto);
            _unitOfWork.Product.Update(objFromDb);

            if (!string.IsNullOrWhiteSpace(TranslationsJson))
            {
                try
                {
                    var translations = JsonSerializer.Deserialize<List<ProductTranslationDto>>(TranslationsJson);
                    if (translations != null)
                    {
                        foreach (var tr in translations)
                        {
                            var translation = tr.ToModel();
                            translation.ProductId = updateDto.Id;
                            if (tr.Id == 0)
                                _unitOfWork.ProductTranslation.Add(translation);
                            else
                                _unitOfWork.ProductTranslation.Update(translation);
                        }
                    }
                }
                catch (JsonException)
                {
                    // ignore invalid translations JSON
                }
            }

            if (files != null && files.Count > 0)
            {
                HandleImageUpload(updateDto.Id, files);
            }

            _unitOfWork.Save();
            return Ok(new SuccessResponseDto { Message = "تم تحديث بيانات المنتج بنجاح" });
        }

        // حذف صورة واحدة محددة (مهمة للداشبورد)
        [HttpDelete("DeleteImage/{imageId}")]
        [Authorize(Roles = "MasterAdmin,CreateDeleteAdmin")]
        public IActionResult DeleteImage(int imageId)
        {
            var image = _unitOfWork.Image.Get(u => u.Id == imageId);
            if (image == null) return NotFound(new ErrorResponseDto { Message = "الصورة غير موجودة." });

            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, image.RelativePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

            _unitOfWork.Image.Remove(image);
            _unitOfWork.Save();

            return Ok(new SuccessResponseDto { Message = "تم حذف الصورة بنجاح" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "MasterAdmin,CreateDeleteAdmin")]
        public IActionResult Delete(int id)
        {
            var product = _unitOfWork.Product.Get(u => u.Id == id, includeProperties: "Images");
            if (product == null) return NotFound(new ErrorResponseDto { Message = "Product not found" });
            foreach (var img in product.Images)
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, img.RelativePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            }

            _unitOfWork.Product.Remove(product);
            _unitOfWork.Save();

            return Ok(new SuccessResponseDto { Message = "تم حذف المنتج وكافة الملفات المرتبطة به" });
        }

        private void HandleImageUpload(int productId, List<IFormFile> files)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string productPath = Path.Combine(wwwRootPath, "images");

            if (!Directory.Exists(productPath)) Directory.CreateDirectory(productPath);

            foreach (var file in files)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                _unitOfWork.Image.Add(new Image
                {
                    RelativePath = "/images/" + fileName,
                    ProductId = productId
                });
            }
        }

        private List<IFormFile> ResolveFiles(List<IFormFile>? files)
        {
            if (files != null && files.Count > 0)
            {
                return files;
            }

            var requestFiles = Request?.Form?.Files;
            if (requestFiles == null || requestFiles.Count == 0)
            {
                return new List<IFormFile>();
            }

            var mapped = requestFiles
                .Where(f => f.Name == "files" || f.Name == "files[]")
                .ToList();

            return mapped;
        }

        private ProductDto NormalizeProductDtoImageUrls(ProductDto dto)
        {
            if (dto.Images == null || dto.Images.Count == 0)
            {
                return dto;
            }

            foreach (var image in dto.Images)
            {
                image.RelativePath = ToAbsoluteImageUrl(image.RelativePath);
            }

            return dto;
        }

        private ProductDetailDto NormalizeProductDetailImageUrls(ProductDetailDto dto)
        {
            if (dto.Images == null || dto.Images.Count == 0)
            {
                return dto;
            }

            foreach (var image in dto.Images)
            {
                image.RelativePath = ToAbsoluteImageUrl(image.RelativePath);
            }

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