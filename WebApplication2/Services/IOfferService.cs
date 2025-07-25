using WebApplication2.Models.DTOs;

namespace WebApplication2.Services
{
    public interface IOfferService
    {
        Task<OfferResponseDto> CreateOfferAsync(CreateOfferDto createOfferDto, string userId);
        Task<IEnumerable<OfferResponseDto>> GetAllOffersAsync();
        Task<IEnumerable<OfferResponseDto>> GetUserOffersAsync(string userId);
        Task<OfferResponseDto?> GetOfferByIdAsync(Guid offerId);
        Task<ApiResponseDto> UpdateOfferAsync(Guid offerId, UpdateOfferDto updateOfferDto, string userId);
        Task<ApiResponseDto> DeleteOfferAsync(Guid offerId, string userId);
        Task<ApiResponseDto> ActivateOfferAsync(Guid offerId, string userId);
        Task<ApiResponseDto> DeactivateOfferAsync(Guid offerId, string userId);
    }
}