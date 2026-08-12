using BookingPlatform.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookingPlatform.Application.Interfaces
{

    public interface IProviderService
    {
        Task<ProviderResponse> CreateProfileAsync(Guid userId, CreateProviderProfileRequest request);
        Task DeactivateAsync(Guid userId);
        Task ReactivateAsync(Guid userId);
    }
}
