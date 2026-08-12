using System;
using System.Collections.Generic;
using System.Text;

namespace BookingPlatform.Application.DTOs
{

    public record CreateProviderProfileRequest(string BusinessName, string? Bio);
    public record ProviderResponse(Guid Id, string BusinessName, string? Bio, bool IsActive);
}
