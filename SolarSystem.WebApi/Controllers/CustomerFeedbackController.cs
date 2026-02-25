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
    public class CustomerFeedbackController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomerFeedbackController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] string lang = "en")
        {
            var feedbacks = await _unitOfWork.CustomerFeedback.GetAllAsync(includeProperties: "Customer,Translations,Customer.Translations");
            var feedbackDtos = feedbacks.Select(f => f.ToDto(lang)).ToList();
            return Ok(feedbackDtos);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(int id, [FromQuery] string lang = "en")
        {
            var feedback = await _unitOfWork.CustomerFeedback.GetAsync(u => u.Id == id, includeProperties: "Customer,Translations,Customer.Translations");
            if (feedback == null)
            {
                return NotFound(new ErrorResponseDto { Message = "Feedback not found" });
            }
            return Ok(feedback.ToDto(lang));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateCustomerFeedbackDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate that Customer exists
            var customer = await _unitOfWork.Customer.GetAsync(u => u.Id == createDto.CustomerId);
            if (customer == null)
            {
                return BadRequest(new ErrorResponseDto { Message = "Cannot create feedback for a non-existing customer" });
            }

            var feedback = createDto.ToModel();
            _unitOfWork.CustomerFeedback.Add(feedback);
            await _unitOfWork.SaveAsync();

            // Add translations
            var enTranslation = new CustomerFeedbackTranslation
            {
                LanguageCode = "en",
                FeedbackText = createDto.FeedbackEn ?? string.Empty,
                CustomerFeedbackId = feedback.Id
            };
            _unitOfWork.CustomerFeedbackTranslation.Add(enTranslation);

            var arTranslation = new CustomerFeedbackTranslation
            {
                LanguageCode = "ar",
                FeedbackText = createDto.FeedbackAr ?? string.Empty,
                CustomerFeedbackId = feedback.Id
            };
            _unitOfWork.CustomerFeedbackTranslation.Add(arTranslation);

            await _unitOfWork.SaveAsync();

            var createdFeedback = await _unitOfWork.CustomerFeedback.GetAsync(u => u.Id == feedback.Id, includeProperties: "Customer,Translations,Customer.Translations");
            return CreatedAtAction(nameof(Get), new { id = feedback.Id }, createdFeedback.ToDto());
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] UpdateCustomerFeedbackDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var feedback = await _unitOfWork.CustomerFeedback.GetAsync(u => u.Id == updateDto.Id, includeProperties: "Translations");
            if (feedback == null)
            {
                return NotFound(new ErrorResponseDto { Message = "Feedback not found" });
            }

            // Validate that Customer exists
            var customer = await _unitOfWork.Customer.GetAsync(u => u.Id == updateDto.CustomerId);
            if (customer == null)
            {
                return BadRequest(new ErrorResponseDto { Message = "Customer not found" });
            }

            feedback.CustomerId = updateDto.CustomerId;

            // Update translations
            var enTranslation = feedback.Translations.FirstOrDefault(t => t.LanguageCode == "en");
            if (enTranslation != null)
            {
                enTranslation.FeedbackText = updateDto.FeedbackEn ?? string.Empty;
                _unitOfWork.CustomerFeedbackTranslation.Update(enTranslation);
            }
            else
            {
                _unitOfWork.CustomerFeedbackTranslation.Add(new CustomerFeedbackTranslation { LanguageCode = "en", FeedbackText = updateDto.FeedbackEn ?? "", CustomerFeedbackId = feedback.Id });
            }

            var arTranslation = feedback.Translations.FirstOrDefault(t => t.LanguageCode == "ar");
            if (arTranslation != null)
            {
                arTranslation.FeedbackText = updateDto.FeedbackAr ?? string.Empty;
                _unitOfWork.CustomerFeedbackTranslation.Update(arTranslation);
            }
            else
            {
                _unitOfWork.CustomerFeedbackTranslation.Add(new CustomerFeedbackTranslation { LanguageCode = "ar", FeedbackText = updateDto.FeedbackAr ?? "", CustomerFeedbackId = feedback.Id });
            }

            await _unitOfWork.SaveAsync();

            return Ok(new SuccessResponseDto { Message = "Feedback updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var feedback = await _unitOfWork.CustomerFeedback.GetAsync(u => u.Id == id);
            if (feedback == null)
            {
                return NotFound(new ErrorResponseDto { Message = "Feedback not found" });
            }

            _unitOfWork.CustomerFeedback.Remove(feedback);
            await _unitOfWork.SaveAsync();

            return Ok(new SuccessResponseDto { Message = "Feedback deleted successfully" });
        }
    }
}
