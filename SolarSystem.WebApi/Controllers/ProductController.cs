using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarSystem.DataAccess1.Repository.IRepository;
using SolarSystem.Models1.Helpers;
using SolarSystem.Models1.Models;

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

        [HttpGet]
        public IActionResult GetAll(int? sectionId, int pageNumber = 1, int pageSize = 10)
        {
            // التحقق من صحة أرقام الصفحات
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            IEnumerable<Product> productList;

            // الفلترة بناءً على وجود القسم
            if (sectionId != null && sectionId > 0)
            {
                productList = _unitOfWork.Product.GetAll(
                    u => u.SectionId == sectionId,
                    includeProperties: "Images");
            }
            else
            {
                productList = _unitOfWork.Product.GetAll(includeProperties: "Images");
            }

            int totalCount = productList.Count();

            var pagedData = productList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new PagedResult<Product>
            {
                Items = pagedData,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        public IActionResult Details(int id)
        {
            var product = _unitOfWork.Product.Get(u => u.Id == id, includeProperties: "Images,Section");
            if (product == null) return NotFound("المنتج غير موجود.");
            return Ok(product);
        }

        // 3. إضافة منتج جديد مع رفع الصور
        [HttpPost]
        [Authorize(Roles = "MasterAdmin,Editor,3,2")]
        public IActionResult Create([FromForm] Product product, List<IFormFile> files)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _unitOfWork.Product.Add(product);
            _unitOfWork.Save(); 

            if (files != null && files.Count > 0)
            {
                HandleImageUpload(product.Id, files);
                _unitOfWork.Save();
            }

            return Ok(new { message = "تمت إضافة المنتج بنجاح", productId = product.Id });
        }

        // 4. تحديث المنتج
        [HttpPut]
        [Authorize(Roles = "MasterAdmin,Editor,3,2")]
        public IActionResult Update([FromForm] Product product, List<IFormFile> files)
        {
            if (!ModelState.IsValid || product.Id <= 0) return BadRequest("بيانات غير صالحة.");

            var objFromDb = _unitOfWork.Product.Get(u => u.Id == product.Id);
            if (objFromDb == null) return NotFound("المنتج غير موجود.");

            _unitOfWork.Product.Update(product);

            if (files != null && files.Count > 0)
            {
                HandleImageUpload(product.Id, files);
            }

            _unitOfWork.Save();
            return Ok(new { message = "تم تحديث بيانات المنتج بنجاح" });
        }

        // 5. حذف صورة واحدة محددة (مهمة للداشبورد)
        [HttpDelete("DeleteImage/{imageId}")]
        [Authorize(Roles = "MasterAdmin,Editor,3,2")]
        public IActionResult DeleteImage(int imageId)
        {
            var image = _unitOfWork.Image.Get(u => u.Id == imageId);
            if (image == null) return NotFound("الصورة غير موجودة.");

            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, image.RelativePath.TrimStart('\\'));
            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

            _unitOfWork.Image.Remove(image);
            _unitOfWork.Save();

            return Ok(new { message = "تم حذف الصورة بنجاح" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "MasterAdmin,3")]
        public IActionResult Delete(int id)
        {
            var product = _unitOfWork.Product.Get(u => u.Id == id, includeProperties: "Images");
            if (product == null) return NotFound();

            foreach (var img in product.Images)
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, img.RelativePath.TrimStart('\\'));
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            }

            _unitOfWork.Product.Remove(product);
            _unitOfWork.Save();

            return Ok(new { message = "تم حذف المنتج وكافة الملفات المرتبطة به" });
        }

        private void HandleImageUpload(int productId, List<IFormFile> files)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string productPath = Path.Combine(wwwRootPath, @"images");

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
                    RelativePath = @"\images" + fileName,
                    ProductId = productId
                });
            }
        }
    }
}