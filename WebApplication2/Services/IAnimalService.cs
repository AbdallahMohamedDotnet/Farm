using WebApplication2.Models;
using WebApplication2.Models.DTOs;

namespace WebApplication2.Services
{
    public interface IAnimalService
    {
        Task<AnimalResponseDto> CreateAnimalAsync(CreateAnimalDto createAnimalDto, string userId);
        Task<IEnumerable<AnimalResponseDto>> GetUserAnimalsAsync(string userId);
        Task<ApiResponseDto> FeedAnimalAsync(Guid animalId, string userId);
        Task<ApiResponseDto> GroomAnimalAsync(Guid animalId, string userId);
        Task<ApiResponseDto> SacrificeAnimalAsync(Guid animalId, string userId);
        Task<ApiResponseDto> DeleteAnimalAsync(Guid animalId, string userId);
    }
}