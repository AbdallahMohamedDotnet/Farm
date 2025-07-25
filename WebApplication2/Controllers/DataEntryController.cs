using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication2.Models;
using WebApplication2.Models.DTOs;
using WebApplication2.Services;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = UserRoles.DataEntry)]
    public class DataEntryController : ControllerBase
    {
        private readonly IOfferService _offerService;
        private readonly IAnimalService _animalService;
        
        public DataEntryController(IOfferService offerService, IAnimalService animalService)
        {
            _offerService = offerService;
            _animalService = animalService;
        }
        
        /// <summary>
        /// Create a new offer for an animal
        /// </summary>
        [HttpPost("offers")]
        public async Task<ActionResult<OfferResponseDto>> CreateOffer([FromBody] CreateOfferDto createOfferDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            try
            {
                var result = await _offerService.CreateOfferAsync(createOfferDto, userId);
                return CreatedAtAction(nameof(GetOffer), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        /// <summary>
        /// Get all offers created by this data entry user
        /// </summary>
        [HttpGet("offers")]
        public async Task<ActionResult<IEnumerable<OfferResponseDto>>> GetMyOffers()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            var offers = await _offerService.GetUserOffersAsync(userId);
            return Ok(offers);
        }
        
        /// <summary>
        /// Get a specific offer by ID
        /// </summary>
        [HttpGet("offers/{id}")]
        public async Task<ActionResult<OfferResponseDto>> GetOffer(Guid id)
        {
            var offer = await _offerService.GetOfferByIdAsync(id);
            if (offer == null)
            {
                return NotFound();
            }
            
            return Ok(offer);
        }
        
        /// <summary>
        /// Update an existing offer
        /// </summary>
        [HttpPut("offers/{id}")]
        public async Task<ActionResult<ApiResponseDto>> UpdateOffer(Guid id, [FromBody] UpdateOfferDto updateOfferDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            var result = await _offerService.UpdateOfferAsync(id, updateOfferDto, userId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }
        
        /// <summary>
        /// Delete an offer
        /// </summary>
        [HttpDelete("offers/{id}")]
        public async Task<ActionResult<ApiResponseDto>> DeleteOffer(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            var result = await _offerService.DeleteOfferAsync(id, userId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }
        
        /// <summary>
        /// Activate an offer
        /// </summary>
        [HttpPut("offers/{id}/activate")]
        public async Task<ActionResult<ApiResponseDto>> ActivateOffer(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            var result = await _offerService.ActivateOfferAsync(id, userId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }
        
        /// <summary>
        /// Deactivate an offer
        /// </summary>
        [HttpPut("offers/{id}/deactivate")]
        public async Task<ActionResult<ApiResponseDto>> DeactivateOffer(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            var result = await _offerService.DeactivateOfferAsync(id, userId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }
        
        /// <summary>
        /// Add a new animal to the system (DataEntry specific functionality)
        /// </summary>
        [HttpPost("animals")]
        public async Task<ActionResult<AnimalResponseDto>> AddAnimal([FromBody] CreateAnimalDto createAnimalDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            try
            {
                var result = await _animalService.CreateAnimalAsync(createAnimalDto, userId);
                return CreatedAtAction(nameof(GetAnimal), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        /// <summary>
        /// Get animal details
        /// </summary>
        [HttpGet("animals/{id}")]
        public async Task<ActionResult<AnimalResponseDto>> GetAnimal(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            var animals = await _animalService.GetUserAnimalsAsync(userId);
            var animal = animals.FirstOrDefault(a => a.Id == id);
            
            if (animal == null)
            {
                return NotFound();
            }
            
            return Ok(animal);
        }
    }
}