using System;
using System.Collections.Generic;
using System.Text;

namespace BookingPlatform.Application.DTOs
{

    public record CreateReviewRequest(Guid BookingId, int Rating, string? Comment);
    public record ReviewResponse(Guid Id, Guid BookingId, int Rating, string? Comment, DateTime CreatedAt);
}
