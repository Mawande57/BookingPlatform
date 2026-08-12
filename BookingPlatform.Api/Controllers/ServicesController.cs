using BookingPlatform.Application.DTOs;
using BookingPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace BookingPlatform.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceCatalogService _catalogService;

        public ServicesController(IServiceCatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        [HttpPost]
        [Authorize(Roles = "Provider")]
        public async Task<ActionResult<ServiceResponse>> Create(CreateServiceRequest request)
        {
            var result = await _catalogService.CreateServiceAsync(request);
            return Created($"api/services/{result.Id}", result);
        }

        [HttpGet]
        public async Task<ActionResult<List<ServiceResponse>>> GetAll()
        {
            return Ok(await _catalogService.GetAllServicesAsync());
        }
    }
}
