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

namespace BookingPlatform.Infrastructure.Scheduling
{
    public class SlotService : ISlotService
    {
        private readonly AppDbContext _context;

        public SlotService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TemplateResponse> CreateTemplateAsync(Guid providerId, CreateTemplateRequest request)
        {
            var provider = await _context.Providers.FindAsync(providerId);
            if (provider == null) throw new NotFoundException("Provider not found.");
            if (!provider.IsActive) throw new ValidationException("Reactivate your profile before making changes.");

            if (request.EndTime <= request.StartTime)
                throw new ValidationException("EndTime must be after StartTime.");

            var template = new AvailabilityTemplate
            {
                Id = Guid.NewGuid(),
                ProviderId = providerId,
                DayOfWeek = request.DayOfWeek,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                SlotDurationMinutes = request.SlotDurationMinutes,
                IsActive = true
            };

            _context.AvailabilityTemplates.Add(template);
            await _context.SaveChangesAsync();

            return new TemplateResponse(template.Id, template.DayOfWeek, template.StartTime, template.EndTime, template.SlotDurationMinutes, template.IsActive);
        }

        public async Task<List<SlotResponse>> GenerateSlotsAsync(Guid providerId, GenerateSlotsRequest request)
        {
            if (request.ToDate < request.FromDate)
                throw new ValidationException("ToDate must be on or after FromDate.");

            var provider = await _context.Providers.FindAsync(providerId);
            if (provider == null) throw new NotFoundException("Provider not found.");
            if (!provider.IsActive) throw new ValidationException("Reactivate your profile before making changes.");

            var templates = await _context.AvailabilityTemplates
                .Where(t => t.ProviderId == providerId && t.IsActive)
                .ToListAsync();

            var newSlots = new List<Slot>();

            for (var date = request.FromDate; date <= request.ToDate; date = date.AddDays(1))
            {
                var matchingTemplates = templates.Where(t => t.DayOfWeek == date.DayOfWeek);

                foreach (var template in matchingTemplates)
                {
                    var slotStart = template.StartTime;
                    while (slotStart.AddMinutes(template.SlotDurationMinutes) <= template.EndTime)
                    {
                        var slotEnd = slotStart.AddMinutes(template.SlotDurationMinutes);

                        bool alreadyExists = await _context.Slots.AnyAsync(s =>
                            s.ProviderId == providerId && s.Date == date && s.StartTime == slotStart);

                        if (!alreadyExists)
                        {
                            newSlots.Add(new Slot
                            {
                                Id = Guid.NewGuid(),
                                ProviderId = providerId,
                                Date = date,
                                StartTime = slotStart,
                                EndTime = slotEnd,
                                Status = SlotStatus.Open,
                                TemplateId = template.Id
                            });
                        }

                        slotStart = slotEnd;
                    }
                }
            }

            _context.Slots.AddRange(newSlots);
            await _context.SaveChangesAsync();

            return newSlots.Select(s => new SlotResponse(s.Id, s.Date, s.StartTime, s.EndTime, s.Status.ToString(), providerId, provider.BusinessName)).ToList();
        }

        public async Task<SlotResponse> CreateManualSlotAsync(Guid providerId, CreateManualSlotRequest request)
        {
            if (request.EndTime <= request.StartTime)
                throw new ValidationException("EndTime must be after StartTime.");

            var provider = await _context.Providers.FindAsync(providerId);
            if (provider == null) throw new NotFoundException("Provider not found.");
            if (!provider.IsActive) throw new ValidationException("Reactivate your profile before making changes.");

            var slot = new Slot
            {
                Id = Guid.NewGuid(),
                ProviderId = providerId,
                Date = request.Date,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Status = SlotStatus.Open,
                TemplateId = null
            };

            _context.Slots.Add(slot);
            await _context.SaveChangesAsync();

            return new SlotResponse(slot.Id, slot.Date, slot.StartTime, slot.EndTime, slot.Status.ToString(), providerId, provider.BusinessName);
        }

        public async Task<List<SlotResponse>> GetOpenSlotsForProviderAsync(Guid providerId, int? minDurationMinutes)
        {
            var query = _context.Slots
                .Include(s => s.Provider)
                .AsNoTracking()
                .Where(s => s.ProviderId == providerId && s.Status == SlotStatus.Open && s.Provider.IsActive);

            var slots = await query.ToListAsync();

            if (minDurationMinutes.HasValue)
            {
                slots = slots
                    .Where(s => (s.EndTime.ToTimeSpan() - s.StartTime.ToTimeSpan()).TotalMinutes >= minDurationMinutes.Value)
                    .ToList();
            }

            return slots.Select(s => new SlotResponse(s.Id, s.Date, s.StartTime, s.EndTime, s.Status.ToString(), s.ProviderId, s.Provider.BusinessName)).ToList();
        }

        public async Task LockSlotAsync(Guid providerId, Guid slotId)
        {
            var slot = await GetOwnedSlotAsync(providerId, slotId);
            if (slot.Status != SlotStatus.Open)
                throw new ValidationException("Only Open slots can be locked.");

            slot.Status = SlotStatus.Locked;
            await _context.SaveChangesAsync();
        }

        public async Task UnlockSlotAsync(Guid providerId, Guid slotId)
        {
            var slot = await GetOwnedSlotAsync(providerId, slotId);
            if (slot.Status != SlotStatus.Locked)
                throw new ValidationException("Only Locked slots can be unlocked.");

            slot.Status = SlotStatus.Open;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSlotAsync(Guid providerId, Guid slotId)
        {
            var slot = await GetOwnedSlotAsync(providerId, slotId);
            if (slot.Status != SlotStatus.Open)
                throw new ValidationException("Only Open slots can be deleted. Booked slots are kept for history.");

            _context.Slots.Remove(slot);
            await _context.SaveChangesAsync();
        }

        private async Task<Slot> GetOwnedSlotAsync(Guid providerId, Guid slotId)
        {
            var slot = await _context.Slots.AsNoTracking().FirstOrDefaultAsync(s => s.Id == slotId);
            if (slot == null)
                throw new NotFoundException("Slot not found.");
            if (slot.ProviderId != providerId)
                throw new ValidationException("You do not own this slot.");

            return slot;
        }
    }
}
