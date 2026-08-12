using BookingPlatform.Application.DTOs;
using BookingPlatform.Application.Exceptions;
using BookingPlatform.Application.Interfaces;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace BookingPlatform.Infrastructure.Catalog
{
    public class ServiceCatalogService : IServiceCatalogService
    {
        private readonly AppDbContext _context;

        public ServiceCatalogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse> CreateServiceAsync(CreateServiceRequest request)
        {
            var existing = await _context.Services
                .FirstOrDefaultAsync(s => s.Name.ToLower() == request.Name.ToLower());
            if (existing != null)
                throw new ConflictException("A service with this name already exists.");

            var service = new Service
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                DefaultDurationMinutes = request.DefaultDurationMinutes
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            return new ServiceResponse(service.Id, service.Name, service.DefaultDurationMinutes);
        }

        public async Task<List<ServiceResponse>> GetAllServicesAsync()
        {
            return await _context.Services
                .Select(s => new ServiceResponse(s.Id, s.Name, s.DefaultDurationMinutes))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ProviderServiceResponse> AddProviderServiceAsync(Guid providerId, AddProviderServiceRequest request)
        {
            var provider = await _context.Providers.FindAsync(providerId);
            if (provider == null) throw new NotFoundException("Provider not found.");
            if (!provider.IsActive) throw new ValidationException("Reactivate your profile before making changes.");

            var service = await _context.Services.FindAsync(request.ServiceId);
            if (service == null)
                throw new NotFoundException("Service not found.");

            var existing = await _context.ProviderServices
                .FirstOrDefaultAsync(ps => ps.ProviderId == providerId && ps.ServiceId == request.ServiceId);
            if (existing != null)
                throw new ConflictException("This provider already offers this service.");

            var providerService = new ProviderService
            {
                Id = Guid.NewGuid(),
                ProviderId = providerId,
                ServiceId = request.ServiceId,
                Price = request.Price,
                DurationMinutes = request.DurationMinutes
            };

            _context.ProviderServices.Add(providerService);
            await _context.SaveChangesAsync();

            return new ProviderServiceResponse(providerService.Id, service.Id, service.Name, provider.Id, provider.BusinessName, providerService.Price, providerService.DurationMinutes , providerService.RowVersion);
        }

        public async Task<List<ProviderServiceResponse>> GetProviderServicesAsync(Guid providerId)
        {
            var provider = await _context.Providers.FindAsync(providerId);
            if (provider == null || !provider.IsActive)
                throw new NotFoundException("Provider not found."); 

            return await _context.ProviderServices
                .Where(ps => ps.ProviderId == providerId)
                .Include(ps => ps.Service)
                .Include(ps => ps.Provider)
                .Select(ps => new ProviderServiceResponse(ps.Id, ps.ServiceId, ps.Service.Name, ps.ProviderId, ps.Provider.BusinessName, ps.Price, ps.DurationMinutes , ps.RowVersion))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<ProviderServiceResponse>> GetAllProviderServicesAsync()
        {
            return await _context.ProviderServices
                .Include(ps => ps.Service)
                .Include(ps => ps.Provider)
                .Where(ps => ps.Provider.IsActive)
                .Select(ps => new ProviderServiceResponse(ps.Id, ps.ServiceId, ps.Service.Name, ps.ProviderId, ps.Provider.BusinessName, ps.Price, ps.DurationMinutes, ps.RowVersion))
                .ToListAsync();
        }
        public async Task<ProviderServiceResponse> UpdateProviderServiceAsync(Guid providerId, Guid providerServiceId, UpdateProviderServiceRequest request)
        {
            var ps = await _context.ProviderServices
                .Include(p => p.Service)
                .Include(p => p.Provider)
                .FirstOrDefaultAsync(p => p.Id == providerServiceId);

            if (ps == null)
                throw new NotFoundException("Provider service not found.");
            if (ps.ProviderId != providerId)
                throw new ValidationException("You do not own this service listing.");

            ps.Price = request.Price;
            ps.DurationMinutes = request.DurationMinutes;
            _context.Entry(ps).Property(x => x.RowVersion).OriginalValue = request.RowVersion;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException("This listing was modified by someone else since you loaded it. Refresh and try again.");
            }

            return new ProviderServiceResponse(ps.Id, ps.ServiceId, ps.Service.Name, ps.ProviderId, ps.Provider.BusinessName, ps.Price, ps.DurationMinutes , ps.RowVersion);
        }

        public async Task DeleteProviderServiceAsync(Guid providerId, Guid providerServiceId)
        {
            var ps = await _context.ProviderServices.FirstOrDefaultAsync(p => p.Id == providerServiceId);
            if (ps == null)
                throw new NotFoundException("Provider service not found.");
            if (ps.ProviderId != providerId)
                throw new ValidationException("You do not own this service listing.");

            var hasBookings = await _context.Bookings.AnyAsync(b => b.ProviderServiceId == providerServiceId);
            if (hasBookings)
                throw new ConflictException("Cannot delete a service listing that has bookings. Consider updating the price instead.");

            _context.ProviderServices.Remove(ps);
            await _context.SaveChangesAsync();
        }


    }
}
