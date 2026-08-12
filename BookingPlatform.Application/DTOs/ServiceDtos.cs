using System;
using System.Collections.Generic;
using System.Text;

namespace BookingPlatform.Application.DTOs
{
    public record CreateServiceRequest(string Name, int DefaultDurationMinutes);
    public record ServiceResponse(Guid Id, string Name, int DefaultDurationMinutes);

    public record AddProviderServiceRequest(Guid ServiceId, decimal Price, int DurationMinutes);
    public record ProviderServiceResponse(Guid Id, Guid ServiceId, string ServiceName, Guid ProviderId, string ProviderBusinessName, decimal Price, int DurationMinutes, byte[] RowVersion);
    public record UpdateProviderServiceRequest(decimal Price, int DurationMinutes, byte[] RowVersion);
}
