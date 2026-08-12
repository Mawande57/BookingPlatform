using BookingPlatform.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookingPlatform.Application.Interfaces
{
    public interface IBookingService
    {
        Task<BookingResponse> CreateBookingAsync(Guid customerId, CreateBookingRequest request);
        Task<List<BookingResponse>> GetMyBookingsAsync(Guid customerId);
        Task CancelBookingAsync(Guid customerId, Guid bookingId);
        Task CompleteBookingAsync(Guid providerId, Guid bookingId);
    }
}
