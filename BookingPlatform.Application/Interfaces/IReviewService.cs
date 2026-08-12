using BookingPlatform.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookingPlatform.Application.Interfaces
{
    public interface IReviewService
    {
        Task<ReviewResponse> CreateReviewAsync(Guid customerId, CreateReviewRequest request);
        Task<List<ReviewResponse>> GetReviewsForProviderAsync(Guid providerId);
    }
}
