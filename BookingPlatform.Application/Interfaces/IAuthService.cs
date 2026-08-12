using BookingPlatform.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookingPlatform.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
    }
}
