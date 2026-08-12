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

namespace BookingPlatform.Infrastructure.Reviews
{
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _context;

        public ReviewService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ReviewResponse> CreateReviewAsync(Guid customerId, CreateReviewRequest request)
        {
            if (request.Rating < 1 || request.Rating > 5)
                throw new ValidationException("Rating must be between 1 and 5.");

            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == request.BookingId);
            if (booking == null)
                throw new NotFoundException("Booking not found.");

            if (booking.CustomerId != customerId)
                throw new ValidationException("You can only review your own bookings.");

            if (booking.Status != BookingStatus.Completed)
                throw new ValidationException("You can only review completed bookings.");

            var existing = await _context.Reviews.FirstOrDefaultAsync(r => r.BookingId == request.BookingId);
            if (existing != null)
                throw new ConflictException("This booking has already been reviewed.");

            var review = new Review
            {
                Id = Guid.NewGuid(),
                BookingId = request.BookingId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return new ReviewResponse(review.Id, review.BookingId, review.Rating, review.Comment, review.CreatedAt);
        }

        public async Task<List<ReviewResponse>> GetReviewsForProviderAsync(Guid providerId)
        {
            var provider = await _context.Providers.FindAsync(providerId);
            if (provider == null || !provider.IsActive)
                throw new NotFoundException("Provider not found.");
            return await _context.Reviews
                .Include(r => r.Booking).ThenInclude(b => b.ProviderService)
                .Where(r => r.Booking.ProviderService.ProviderId == providerId)
                .Select(r => new ReviewResponse(r.Id, r.BookingId, r.Rating, r.Comment, r.CreatedAt))
                .ToListAsync();
        }
    }
}
