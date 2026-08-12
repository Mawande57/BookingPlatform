using System.Security.Claims;
using BookingPlatform.Application.DTOs;
using BookingPlatform.Application.Interfaces;
using BookingPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookingPlatform.Application.Exceptions;

namespace BookingPlatform.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SlotsController : ControllerBase
    {
        private readonly ISlotService _slotService;
        private readonly AppDbContext _context;

        public SlotsController(ISlotService slotService, AppDbContext context)
        {
            _slotService = slotService;
            _context = context;
        }

        private async Task<Guid> GetOwnedProviderIdAsync()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var provider = await _context.Providers.FirstOrDefaultAsync(p => p.UserId == userId);
            if (provider == null)
                throw new NotFoundException("You don't have a provider profile yet.");
            return provider.Id;
        }

        [HttpPost("templates")]
        [Authorize(Roles = "Provider")]
        public async Task<ActionResult<TemplateResponse>> CreateTemplate(CreateTemplateRequest request)
        {
            var providerId = await GetOwnedProviderIdAsync();
            var result = await _slotService.CreateTemplateAsync(providerId, request);
            return Created($"api/slots/templates/{result.Id}", result);
        }

        [HttpPost("generate")]
        [Authorize(Roles = "Provider")]
        public async Task<ActionResult<List<SlotResponse>>> Generate(GenerateSlotsRequest request)
        {
            var providerId = await GetOwnedProviderIdAsync();
            var result = await _slotService.GenerateSlotsAsync(providerId, request);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Provider")]
        public async Task<ActionResult<SlotResponse>> CreateManual(CreateManualSlotRequest request)
        {
            var providerId = await GetOwnedProviderIdAsync();
            var result = await _slotService.CreateManualSlotAsync(providerId, request);
            return Created($"api/slots/{result.Id}", result);
        }

        [HttpGet("provider/{providerId}")]
        public async Task<ActionResult<List<SlotResponse>>> GetOpenSlots(Guid providerId, [FromQuery] int? minDuration)
        {
            return Ok(await _slotService.GetOpenSlotsForProviderAsync(providerId, minDuration));
        }

        [HttpPatch("{slotId}/lock")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> Lock(Guid slotId)
        {
            var providerId = await GetOwnedProviderIdAsync();
            await _slotService.LockSlotAsync(providerId, slotId);
            return NoContent();
        }

        [HttpPatch("{slotId}/unlock")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> Unlock(Guid slotId)
        {
            var providerId = await GetOwnedProviderIdAsync();
            await _slotService.UnlockSlotAsync(providerId, slotId);
            return NoContent();
        }

        [HttpDelete("{slotId}")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> Delete(Guid slotId)
        {
            var providerId = await GetOwnedProviderIdAsync();
            await _slotService.DeleteSlotAsync(providerId, slotId);
            return NoContent();
        }
    }
}
