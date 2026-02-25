using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarSystem.DataAccess1.Repository.IRepository;
using SolarSystem.Models1.Models;
using SolarSystem.Models1.Dtos;
using SolarSystem.Models1.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarSystem.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomerController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] string lang = "en")
        {
            var customers = await _unitOfWork.Customer.GetAllAsync(includeProperties: "Translations");
            var customerDtos = customers.Select(c => c.ToDto(lang)).ToList();
            return Ok(customerDtos);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(int id, [FromQuery] string lang = "en")
        {
            var customer = await _unitOfWork.Customer.GetAsync(u => u.Id == id, includeProperties: "Translations");
            if (customer == null)
            {
                return NotFound(new ErrorResponseDto { Message = "Customer not found" });
            }
            return Ok(customer.ToDto(lang));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateCustomerDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customer = createDto.ToModel();
            _unitOfWork.Customer.Add(customer);
            await _unitOfWork.SaveAsync();

            // Add translations
            var enTranslation = new CustomerTranslation
            {
                LanguageCode = "en",
                Name = createDto.NameEn ?? string.Empty,
                Job = createDto.JobEn,
                CustomerId = customer.Id
            };
            _unitOfWork.CustomerTranslation.Add(enTranslation);

            var arTranslation = new CustomerTranslation
            {
                LanguageCode = "ar",
                Name = createDto.NameAr ?? string.Empty,
                Job = createDto.JobAr,
                CustomerId = customer.Id
            };
            _unitOfWork.CustomerTranslation.Add(arTranslation);

            await _unitOfWork.SaveAsync();

            var result = await _unitOfWork.Customer.GetAsync(u => u.Id == customer.Id, includeProperties: "Translations");
            return CreatedAtAction(nameof(Get), new { id = customer.Id }, result.ToDto());
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] UpdateCustomerDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customer = await _unitOfWork.Customer.GetAsync(u => u.Id == updateDto.Id, includeProperties: "Translations");
            if (customer == null)
            {
                return NotFound(new ErrorResponseDto { Message = "Customer not found" });
            }

            // Update translations
            var enTranslation = customer.Translations.FirstOrDefault(t => t.LanguageCode == "en");
            if (enTranslation != null)
            {
                enTranslation.Name = updateDto.NameEn ?? string.Empty;
                enTranslation.Job = updateDto.JobEn;
                _unitOfWork.CustomerTranslation.Update(enTranslation);
            }
            else
            {
                 _unitOfWork.CustomerTranslation.Add(new CustomerTranslation { LanguageCode = "en", Name = updateDto.NameEn ?? "", Job = updateDto.JobEn, CustomerId = customer.Id });
            }

            var arTranslation = customer.Translations.FirstOrDefault(t => t.LanguageCode == "ar");
            if (arTranslation != null)
            {
                arTranslation.Name = updateDto.NameAr ?? string.Empty;
                arTranslation.Job = updateDto.JobAr;
                _unitOfWork.CustomerTranslation.Update(arTranslation);
            }
            else
            {
                 _unitOfWork.CustomerTranslation.Add(new CustomerTranslation { LanguageCode = "ar", Name = updateDto.NameAr ?? "", Job = updateDto.JobAr, CustomerId = customer.Id });
            }

            await _unitOfWork.SaveAsync();

            return Ok(new SuccessResponseDto { Message = "Customer updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _unitOfWork.Customer.GetAsync(u => u.Id == id);
            if (customer == null)
            {
                return NotFound(new ErrorResponseDto { Message = "Customer not found" });
            }

            _unitOfWork.Customer.Remove(customer);
            await _unitOfWork.SaveAsync();

            return Ok(new SuccessResponseDto { Message = "Customer deleted successfully" });
        }
    }
}
