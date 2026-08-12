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

namespace BookingPlatform.Infrastructure.Bookings
{
    public class BookingService : IBookingService
    {
        private readonly AppDbContext _context;

        public BookingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BookingResponse> CreateBookingAsync(Guid customerId, CreateBookingRequest request)
        {
            var slot = await _context.Slots
                .Include(s => s.Provider)
                .FirstOrDefaultAsync(s => s.Id == request.SlotId);
            if (slot == null)
                throw new NotFoundException("Slot not found.");

            if (slot.Status != SlotStatus.Open)
                throw new ConflictException("This slot is no longer available.");
            if (!slot.Provider.IsActive)
                throw new ConflictException("This provider is not currently accepting bookings.");

            var providerService = await _context.ProviderServices
                .Include(ps => ps.Service)
                .Include(ps => ps.Provider)
                .FirstOrDefaultAsync(ps => ps.Id == request.ProviderServiceId);
            if (providerService == null)
                throw new NotFoundException("Provider service not found.");

            if (providerService.ProviderId != slot.ProviderId)
                throw new ValidationException("This service is not offered by the provider who owns this slot.");

            var slotDurationMinutes = (slot.EndTime.ToTimeSpan() - slot.StartTime.ToTimeSpan()).TotalMinutes;
            if (slotDurationMinutes < providerService.DurationMinutes)
                throw new ValidationException("This slot is too short for the selected service.");

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                SlotId = slot.Id,
                ProviderServiceId = providerService.Id,
                Status = BookingStatus.Pending,
                PriceAtBooking = providerService.Price,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);
            slot.Status = SlotStatus.Booked;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                throw new ConflictException("This slot was just booked by someone else. Please choose another slot.");
            }

            return new BookingResponse(booking.Id, slot.Id, slot.Date, slot.StartTime, providerService.Service.Name, providerService.Provider.BusinessName, booking.PriceAtBooking, booking.Status.ToString());
        }

        public async Task<List<BookingResponse>> GetMyBookingsAsync(Guid customerId)
        {
            return await _context.Bookings
                .Where(b => b.CustomerId == customerId)
                .Include(b => b.Slot)
                .Include(b => b.ProviderService).ThenInclude(ps => ps.Service)
                .Include(b => b.ProviderService).ThenInclude(ps => ps.Provider)
                .Select(b => new BookingResponse(
                    b.Id, b.SlotId, b.Slot.Date, b.Slot.StartTime,
                    b.ProviderService.Service.Name, b.ProviderService.Provider.BusinessName,
                    b.PriceAtBooking, b.Status.ToString()))
                .ToListAsync();
        }

        public async Task CancelBookingAsync(Guid customerId, Guid bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Slot)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                throw new NotFoundException("Booking not found.");
            if (booking.CustomerId != customerId)
                throw new ValidationException("You do not own this booking.");
            if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Confirmed)
                throw new ValidationException("Only Pending or Confirmed bookings can be cancelled.");

            booking.Status = BookingStatus.Cancelled;
            booking.Slot.Status = SlotStatus.Open;

            await _context.SaveChangesAsync();
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            return ex.InnerException?.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true
                || ex.InnerException?.Message.Contains("23505") == true;
        }
        public async Task CompleteBookingAsync(Guid providerId, Guid bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.ProviderService)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                throw new NotFoundException("Booking not found.");

            if (booking.ProviderService.ProviderId != providerId)
                throw new ValidationException("This booking does not belong to your services.");

            if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Confirmed)
                throw new ValidationException("Only Pending or Confirmed bookings can be marked Completed.");

            booking.Status = BookingStatus.Completed;
            await _context.SaveChangesAsync();
        }

    }
}
