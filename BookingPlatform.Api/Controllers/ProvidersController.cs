using BookingPlatform.Application.DTOs;
using BookingPlatform.Application.Interfaces;
using BookingPlatform.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingPlatform.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProvidersController : ControllerBase
    {
        private readonly IProviderService _providerService;

        public ProvidersController(IProviderService providerService)
        {
            _providerService = providerService;
        }

        [HttpPost("profile")]
        [Authorize(Roles = "Provider")]
        public async Task<ActionResult<ProviderResponse>> CreateProfile(CreateProviderProfileRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var userId = Guid.Parse(userIdClaim);

            var result = await _providerService.CreateProfileAsync(userId, request);
            return Created($"/api/providers/{result.Id}", result);
        }
        [HttpPatch("deactivate")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> Deactivate()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _providerService.DeactivateAsync(userId);
            return NoContent();
        }

        [HttpPatch("reactivate")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> Reactivate()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _providerService.ReactivateAsync(userId);
            return NoContent();
        }
    }
}
