using System;
using System.Collections.Generic;
using System.Text;

namespace BookingPlatform.Application.DTOs
{

    public record RegisterRequest(string Email, string Password, string Role);
    public record LoginRequest(string Email, string Password);
    public record AuthResponse(string Token, Guid UserId, string Role);
}
