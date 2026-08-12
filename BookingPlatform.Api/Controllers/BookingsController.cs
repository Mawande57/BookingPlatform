using BookingPlatform.Application.DTOs;
using BookingPlatform.Application.Exceptions;
using BookingPlatform.Application.Interfaces;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BookingPlatform.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly AppDbContext _context;
        

        public BookingsController(IBookingService bookingService , AppDbContext context)
        {
            _bookingService = bookingService;
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<BookingResponse>> Create(CreateBookingRequest request)
        {
            var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _bookingService.CreateBookingAsync(customerId, request);
            return Created($"api/bookings/{result.Id}", result);
        }

        [HttpGet("my")]
        public async Task<ActionResult<List<BookingResponse>>> GetMine()
        {
            var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await _bookingService.GetMyBookingsAsync(customerId));
        }

        [HttpPatch("{bookingId}/cancel")]
        public async Task<IActionResult> Cancel(Guid bookingId)
        {
            var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _bookingService.CancelBookingAsync(customerId, bookingId);
            return NoContent();
        }
        [HttpPatch("{bookingId}/complete")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> Complete(Guid bookingId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            // reuse the same owned-provider lookup pattern from SlotsController
            var providerId = await GetOwnedProviderIdAsync(); // you'll need this helper here too — third repeat, worth extracting now as we flagged earlier
            await _bookingService.CompleteBookingAsync(providerId, bookingId);
            return NoContent();
        }
        private async Task<Guid> GetOwnedProviderIdAsync()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var provider = await _context.Providers.FirstOrDefaultAsync(p => p.UserId == userId);
            if (provider == null)
                throw new NotFoundException("You don't have a provider profile yet.");
            return provider.Id;
        }
    }
}
