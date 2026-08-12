using System.Security.Claims;
using BookingPlatform.Application.DTOs;
using BookingPlatform.Application.Interfaces;
using BookingPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace BookingPlatform.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProviderServicesController : ControllerBase
    {
        private readonly IServiceCatalogService _catalogService;
        private readonly AppDbContext _context;

        public ProviderServicesController(IServiceCatalogService catalogService, AppDbContext context)
        {
            _catalogService = catalogService;
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "Provider")]
        public async Task<ActionResult<ProviderServiceResponse>> Add(AddProviderServiceRequest request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var provider = await _context.Providers.FirstOrDefaultAsync(p => p.UserId == userId);
            if (provider == null)
                return NotFound(new { error = "You don't have a provider profile yet." });

            var result = await _catalogService.AddProviderServiceAsync(provider.Id, request);
            return Created($"api/provider-services/{result.Id}", result);
        }

        [HttpGet("{providerId}")]
        public async Task<ActionResult<List<ProviderServiceResponse>>> GetForProvider(Guid providerId)
        {
            return Ok(await _catalogService.GetProviderServicesAsync(providerId));
        }
        [HttpGet]
        public async Task<ActionResult<List<ProviderServiceResponse>>> GetAll()
        {
            return Ok(await _catalogService.GetAllProviderServicesAsync());
        }
        [HttpPatch("{providerServiceId}")]
        [Authorize(Roles = "Provider")]
        public async Task<ActionResult<ProviderServiceResponse>> Update(Guid providerServiceId, UpdateProviderServiceRequest request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var provider = await _context.Providers.FirstOrDefaultAsync(p => p.UserId == userId);
            if (provider == null) return NotFound(new { error = "You don't have a provider profile yet." });

            var result = await _catalogService.UpdateProviderServiceAsync(provider.Id, providerServiceId, request);
            return Ok(result);
        }

        [HttpDelete("{providerServiceId}")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> Delete(Guid providerServiceId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var provider = await _context.Providers.FirstOrDefaultAsync(p => p.UserId == userId);
            if (provider == null) return NotFound(new { error = "You don't have a provider profile yet." });

            await _catalogService.DeleteProviderServiceAsync(provider.Id, providerServiceId);
            return NoContent();
        }
    }
}
