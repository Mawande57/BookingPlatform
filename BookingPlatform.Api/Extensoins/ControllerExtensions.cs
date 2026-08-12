using BookingPlatform.Application.Exceptions;
using BookingPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace BookingPlatform.Api.Extensoins
{
    public static class ControllerExtensions
    {
        public static async Task<Guid> GetOwnedProviderIdAsync(this ControllerBase controller, AppDbContext context)
        {
            var userId = Guid.Parse(controller.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var provider = await context.Providers.FirstOrDefaultAsync(p => p.UserId == userId);
            if (provider == null)
                throw new NotFoundException("You don't have a provider profile yet.");
            return provider.Id;
        }
    }
}
