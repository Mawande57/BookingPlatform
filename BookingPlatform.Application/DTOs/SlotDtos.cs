using System;
using System.Collections.Generic;
using System.Text;

namespace BookingPlatform.Application.DTOs
{

    public record CreateTemplateRequest(DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime, int SlotDurationMinutes);
    public record TemplateResponse(Guid Id, DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime, int SlotDurationMinutes, bool IsActive);

    public record GenerateSlotsRequest(DateOnly FromDate, DateOnly ToDate);

    public record CreateManualSlotRequest(DateOnly Date, TimeOnly StartTime, TimeOnly EndTime);
    public record SlotResponse(Guid Id, DateOnly Date, TimeOnly StartTime, TimeOnly EndTime, string Status, Guid ProviderId, string ProviderBusinessName);
}
