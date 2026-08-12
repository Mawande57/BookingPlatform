using BookingPlatform.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookingPlatform.Application.Interfaces
{
    public interface ISlotService
    {
        Task<TemplateResponse> CreateTemplateAsync(Guid providerId, CreateTemplateRequest request);
        Task<List<SlotResponse>> GenerateSlotsAsync(Guid providerId, GenerateSlotsRequest request);
        Task<SlotResponse> CreateManualSlotAsync(Guid providerId, CreateManualSlotRequest request);
        Task<List<SlotResponse>> GetOpenSlotsForProviderAsync(Guid providerId, int? minDurationMinutes);
        Task LockSlotAsync(Guid providerId, Guid slotId);
        Task UnlockSlotAsync(Guid providerId, Guid slotId);
        Task DeleteSlotAsync(Guid providerId, Guid slotId);
    }
}
