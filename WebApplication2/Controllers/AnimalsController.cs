using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication2.Models.DTOs;
using WebApplication2.Services;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnimalsController : ControllerBase
    {
        private readonly IAnimalService _animalService;
        
        public AnimalsController(IAnimalService animalService)
        {
            _animalService = animalService;
        }
        
        [HttpPost]
        public async Task<ActionResult<AnimalResponseDto>> CreateAnimal([FromBody] CreateAnimalDto createAnimalDto)
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
                return CreatedAtAction(nameof(GetAnimals), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnimalResponseDto>>> GetAnimals()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            var animals = await _animalService.GetUserAnimalsAsync(userId);
            return Ok(animals);
        }
        
        [HttpPut("{id}/feed")]
        public async Task<ActionResult<ApiResponseDto>> FeedAnimal(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            var result = await _animalService.FeedAnimalAsync(id, userId);
            
            if (!result.Success)
            {
                return result.Message == "Animal not found" ? NotFound(result) : BadRequest(result);
            }
            
            return Ok(result);
        }
        
        [HttpPut("{id}/groom")]
        public async Task<ActionResult<ApiResponseDto>> GroomAnimal(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            var result = await _animalService.GroomAnimalAsync(id, userId);
            
            if (!result.Success)
            {
                return result.Message == "Animal not found" ? NotFound(result) : BadRequest(result);
            }
            
            return Ok(result);
        }
        
        [HttpPut("{id}/sacrifice")]
        public async Task<ActionResult<ApiResponseDto>> SacrificeAnimal(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            var result = await _animalService.SacrificeAnimalAsync(id, userId);
            
            if (!result.Success)
            {
                return result.Message == "Animal not found" ? NotFound(result) : BadRequest(result);
            }
            
            return Ok(result);
        }
        
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponseDto>> DeleteAnimal(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            var result = await _animalService.DeleteAnimalAsync(id, userId);
            
            if (!result.Success)
            {
                return NotFound(result);
            }
            
            return Ok(result);
        }
    }
}