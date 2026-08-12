using System;
using System.Collections.Generic;
using System.Text;
using BookingPlatform.Application.DTOs;

namespace BookingPlatform.Application.Interfaces
{
    public interface IServiceCatalogService
    {
        Task<ServiceResponse> CreateServiceAsync(CreateServiceRequest request);
        Task<List<ServiceResponse>> GetAllServicesAsync();
        Task<ProviderServiceResponse> AddProviderServiceAsync(Guid providerId, AddProviderServiceRequest request);
        Task<List<ProviderServiceResponse>> GetProviderServicesAsync(Guid providerId);
        Task<List<ProviderServiceResponse>> GetAllProviderServicesAsync();
        Task<ProviderServiceResponse> UpdateProviderServiceAsync(Guid providerId, Guid providerServiceId, UpdateProviderServiceRequest request);
        Task DeleteProviderServiceAsync(Guid providerId, Guid providerServiceId);
    }
}
