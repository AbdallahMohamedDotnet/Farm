using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Models;
using WebApplication2.Models.DTOs;

namespace WebApplication2.Services
{
    public class AnimalService : IAnimalService
    {
        private readonly ApplicationDbContext _context;
        
        public AnimalService(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<AnimalResponseDto> CreateAnimalAsync(CreateAnimalDto createAnimalDto, string userId)
        {
            var farm = await _context.Farms.FirstOrDefaultAsync(f => f.OwnerId == userId);
            if (farm == null)
            {
                throw new InvalidOperationException("User does not have a farm");
            }
            
            var animal = new Animal
            {
                Id = Guid.NewGuid(),
                Name = createAnimalDto.Name,
                Type = createAnimalDto.Type,
                Age = createAnimalDto.Age,
                FarmId = farm.Id,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Animals.Add(animal);
            await _context.SaveChangesAsync();
            
            return MapToResponseDto(animal);
        }
        
        public async Task<IEnumerable<AnimalResponseDto>> GetUserAnimalsAsync(string userId)
        {
            var animals = await _context.Animals
                .Include(a => a.Farm)
                .Where(a => a.Farm.OwnerId == userId)
                .ToListAsync();
            
            return animals.Select(MapToResponseDto);
        }
        
        public async Task<ApiResponseDto> FeedAnimalAsync(Guid animalId, string userId)
        {
            var animal = await GetUserAnimalAsync(animalId, userId);
            if (animal == null)
            {
                return new ApiResponseDto { Message = "Animal not found", Success = false };
            }
            
            if (animal.IsSacrificed)
            {
                return new ApiResponseDto { Message = "Cannot feed a sacrificed animal", Success = false };
            }
            
            animal.IsFed = true;
            await _context.SaveChangesAsync();
            
            return new ApiResponseDto { Message = "Animal fed successfully" };
        }
        
        public async Task<ApiResponseDto> GroomAnimalAsync(Guid animalId, string userId)
        {
            var animal = await GetUserAnimalAsync(animalId, userId);
            if (animal == null)
            {
                return new ApiResponseDto { Message = "Animal not found", Success = false };
            }
            
            if (animal.IsSacrificed)
            {
                return new ApiResponseDto { Message = "Cannot groom a sacrificed animal", Success = false };
            }
            
            animal.IsGroomed = true;
            await _context.SaveChangesAsync();
            
            return new ApiResponseDto { Message = "Animal groomed successfully" };
        }
        
        public async Task<ApiResponseDto> SacrificeAnimalAsync(Guid animalId, string userId)
        {
            var animal = await GetUserAnimalAsync(animalId, userId);
            if (animal == null)
            {
                return new ApiResponseDto { Message = "Animal not found", Success = false };
            }
            
            if (animal.IsSacrificed)
            {
                return new ApiResponseDto { Message = "Animal is already sacrificed", Success = false };
            }
            
            if (!animal.IsEligibleForSacrifice())
            {
                return new ApiResponseDto 
                { 
                    Message = $"Animal is not eligible for sacrifice. Minimum age requirements: Camel (60 months), Cow (24 months), Goat (12 months), Sheep (6 months)",
                    Success = false 
                };
            }
            
            animal.IsSacrificed = true;
            await _context.SaveChangesAsync();
            
            return new ApiResponseDto { Message = "Animal sacrificed successfully" };
        }
        
        public async Task<ApiResponseDto> DeleteAnimalAsync(Guid animalId, string userId)
        {
            var animal = await GetUserAnimalAsync(animalId, userId);
            if (animal == null)
            {
                return new ApiResponseDto { Message = "Animal not found", Success = false };
            }
            
            _context.Animals.Remove(animal);
            await _context.SaveChangesAsync();
            
            return new ApiResponseDto { Message = "Animal deleted" };
        }
        
        private async Task<Animal?> GetUserAnimalAsync(Guid animalId, string userId)
        {
            return await _context.Animals
                .Include(a => a.Farm)
                .FirstOrDefaultAsync(a => a.Id == animalId && a.Farm.OwnerId == userId);
        }
        
        private static AnimalResponseDto MapToResponseDto(Animal animal)
        {
            return new AnimalResponseDto
            {
                Id = animal.Id,
                Name = animal.Name,
                Type = animal.Type,
                Age = animal.Age,
                IsFed = animal.IsFed,
                IsGroomed = animal.IsGroomed,
                IsSacrificed = animal.IsSacrificed,
                IsEligibleForSacrifice = animal.IsEligibleForSacrifice(),
                CreatedAt = animal.CreatedAt
            };
        }
    }
}