using System.Security.Claims;
using BookingPlatform.Application.DTOs;
using BookingPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace BookingPlatform.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ReviewResponse>> Create(CreateReviewRequest request)
        {
            var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _reviewService.CreateReviewAsync(customerId, request);
            return Created($"api/reviews/{result.Id}", result);
        }

        [HttpGet("provider/{providerId}")]
        public async Task<ActionResult<List<ReviewResponse>>> GetForProvider(Guid providerId)
        {
            return Ok(await _reviewService.GetReviewsForProviderAsync(providerId));
        }
    }
}
