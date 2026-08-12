using BookingPlatform.Application.DTOs;
using BookingPlatform.Application.Exceptions;
using BookingPlatform.Application.Interfaces;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.enums;
using BookingPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookingPlatform.Infrastructure.Services
{
    public class ProviderService : IProviderService
    {
        private readonly AppDbContext _context;

        public ProviderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProviderResponse> CreateProfileAsync(Guid userId, CreateProviderProfileRequest request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new NotFoundException("User not found.");

            if (user.Role != UserRole.Provider)
                throw new ValidationException("Only users registered as Provider can create a provider profile.");

            var existing = await _context.Providers.FirstOrDefaultAsync(p => p.UserId == userId);
            if (existing != null)
                throw new ValidationException("Provider profile already exists for this user.");

            var provider = new Provider
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                BusinessName = request.BusinessName,
                Bio = request.Bio,
                IsActive = true
            };

            _context.Providers.Add(provider);
            await _context.SaveChangesAsync();

            return new ProviderResponse(provider.Id, provider.BusinessName, provider.Bio, provider.IsActive);
        }
        public async Task DeactivateAsync(Guid userId)
        {
            var provider = await _context.Providers.FirstOrDefaultAsync(p => p.UserId == userId);
            if (provider == null)
                throw new NotFoundException("Provider profile not found.");

            provider.IsActive = false;

            var templates = await _context.AvailabilityTemplates.Where(t => t.ProviderId == provider.Id).ToListAsync();
            _context.AvailabilityTemplates.RemoveRange(templates);

            var openSlots = await _context.Slots
                .Where(s => s.ProviderId == provider.Id && s.Status == SlotStatus.Open)
                .ToListAsync();
            _context.Slots.RemoveRange(openSlots);

            await _context.SaveChangesAsync();
        }

        public async Task ReactivateAsync(Guid userId)
        {
            var provider = await _context.Providers.FirstOrDefaultAsync(p => p.UserId == userId);
            if (provider == null)
                throw new NotFoundException("Provider profile not found.");

            provider.IsActive = true;
            await _context.SaveChangesAsync();
        }
    }
}
