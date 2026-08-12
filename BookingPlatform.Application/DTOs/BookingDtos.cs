using System;
using System.Collections.Generic;
using System.Text;

namespace BookingPlatform.Application.DTOs
{
    public record CreateBookingRequest(Guid SlotId, Guid ProviderServiceId);
    public record BookingResponse(Guid Id, Guid SlotId, DateOnly Date, TimeOnly StartTime, string ServiceName, string ProviderBusinessName, decimal PriceAtBooking, string Status);
}
