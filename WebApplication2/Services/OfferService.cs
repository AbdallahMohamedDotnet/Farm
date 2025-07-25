using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Models;
using WebApplication2.Models.DTOs;

namespace WebApplication2.Services
{
    public class OfferService : IOfferService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        
        public OfferService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }
        
        public async Task<OfferResponseDto> CreateOfferAsync(CreateOfferDto createOfferDto, string userId)
        {
            var animal = await _context.Animals
                .Include(a => a.Farm)
                .FirstOrDefaultAsync(a => a.Id == createOfferDto.AnimalId);
            
            if (animal == null)
            {
                throw new InvalidOperationException("Animal not found");
            }
            
            var offer = new Offer
            {
                Id = Guid.NewGuid(),
                Title = createOfferDto.Title,
                Description = createOfferDto.Description,
                AnimalId = createOfferDto.AnimalId,
                SellingPrice = createOfferDto.SellingPrice,
                BuyingPrice = createOfferDto.BuyingPrice,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Offers.Add(offer);
            
            // Update animal pricing
            animal.BuyingPrice = createOfferDto.BuyingPrice;
            animal.SellingPrice = createOfferDto.SellingPrice;
            animal.IsForSale = true;
            animal.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            await _auditService.LogActionAsync(userId, "CreateOffer", "Offer", offer.Id.ToString(), 
                $"Created offer '{offer.Title}' for animal '{animal.Name}'");
            
            return await MapToResponseDto(offer);
        }
        
        public async Task<IEnumerable<OfferResponseDto>> GetAllOffersAsync()
        {
            var offers = await _context.Offers
                .Include(o => o.Animal)
                .Include(o => o.CreatedBy)
                .Where(o => o.IsActive)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            
            var responseDtos = new List<OfferResponseDto>();
            foreach (var offer in offers)
            {
                responseDtos.Add(await MapToResponseDto(offer));
            }
            
            return responseDtos;
        }
        
        public async Task<IEnumerable<OfferResponseDto>> GetUserOffersAsync(string userId)
        {
            var offers = await _context.Offers
                .Include(o => o.Animal)
                .Include(o => o.CreatedBy)
                .Where(o => o.CreatedById == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            
            var responseDtos = new List<OfferResponseDto>();
            foreach (var offer in offers)
            {
                responseDtos.Add(await MapToResponseDto(offer));
            }
            
            return responseDtos;
        }
        
        public async Task<OfferResponseDto?> GetOfferByIdAsync(Guid offerId)
        {
            var offer = await _context.Offers
                .Include(o => o.Animal)
                .Include(o => o.CreatedBy)
                .FirstOrDefaultAsync(o => o.Id == offerId);
            
            return offer != null ? await MapToResponseDto(offer) : null;
        }
        
        public async Task<ApiResponseDto> UpdateOfferAsync(Guid offerId, UpdateOfferDto updateOfferDto, string userId)
        {
            var offer = await _context.Offers
                .Include(o => o.Animal)
                .FirstOrDefaultAsync(o => o.Id == offerId && o.CreatedById == userId);
            
            if (offer == null)
            {
                return new ApiResponseDto { Message = "Offer not found or access denied", Success = false };
            }
            
            if (offer.IsSold)
            {
                return new ApiResponseDto { Message = "Cannot update a sold offer", Success = false };
            }
            
            // Update offer properties
            if (!string.IsNullOrEmpty(updateOfferDto.Title))
                offer.Title = updateOfferDto.Title;
                
            if (!string.IsNullOrEmpty(updateOfferDto.Description))
                offer.Description = updateOfferDto.Description;
                
            if (updateOfferDto.SellingPrice.HasValue)
            {
                offer.SellingPrice = updateOfferDto.SellingPrice.Value;
                offer.Animal.SellingPrice = updateOfferDto.SellingPrice.Value;
            }
                
            if (updateOfferDto.BuyingPrice.HasValue)
            {
                offer.BuyingPrice = updateOfferDto.BuyingPrice.Value;
                offer.Animal.BuyingPrice = updateOfferDto.BuyingPrice.Value;
            }
                
            if (updateOfferDto.IsActive.HasValue)
                offer.IsActive = updateOfferDto.IsActive.Value;
            
            offer.UpdatedAt = DateTime.UtcNow;
            offer.Animal.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            await _auditService.LogActionAsync(userId, "UpdateOffer", "Offer", offer.Id.ToString(), 
                $"Updated offer '{offer.Title}'");
            
            return new ApiResponseDto { Message = "Offer updated successfully" };
        }
        
        public async Task<ApiResponseDto> DeleteOfferAsync(Guid offerId, string userId)
        {
            var offer = await _context.Offers
                .Include(o => o.Animal)
                .FirstOrDefaultAsync(o => o.Id == offerId && o.CreatedById == userId);
            
            if (offer == null)
            {
                return new ApiResponseDto { Message = "Offer not found or access denied", Success = false };
            }
            
            if (offer.IsSold)
            {
                return new ApiResponseDto { Message = "Cannot delete a sold offer", Success = false };
            }
            
            // Update animal status
            offer.Animal.IsForSale = false;
            offer.Animal.UpdatedAt = DateTime.UtcNow;
            
            _context.Offers.Remove(offer);
            await _context.SaveChangesAsync();
            
            await _auditService.LogActionAsync(userId, "DeleteOffer", "Offer", offer.Id.ToString(), 
                $"Deleted offer '{offer.Title}'");
            
            return new ApiResponseDto { Message = "Offer deleted successfully" };
        }
        
        public async Task<ApiResponseDto> ActivateOfferAsync(Guid offerId, string userId)
        {
            var offer = await _context.Offers
                .FirstOrDefaultAsync(o => o.Id == offerId && o.CreatedById == userId);
            
            if (offer == null)
            {
                return new ApiResponseDto { Message = "Offer not found or access denied", Success = false };
            }
            
            offer.IsActive = true;
            offer.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            return new ApiResponseDto { Message = "Offer activated successfully" };
        }
        
        public async Task<ApiResponseDto> DeactivateOfferAsync(Guid offerId, string userId)
        {
            var offer = await _context.Offers
                .FirstOrDefaultAsync(o => o.Id == offerId && o.CreatedById == userId);
            
            if (offer == null)
            {
                return new ApiResponseDto { Message = "Offer not found or access denied", Success = false };
            }
            
            offer.IsActive = false;
            offer.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            return new ApiResponseDto { Message = "Offer deactivated successfully" };
        }
        
        private async Task<OfferResponseDto> MapToResponseDto(Offer offer)
        {
            return new OfferResponseDto
            {
                Id = offer.Id,
                Title = offer.Title,
                Description = offer.Description,
                AnimalId = offer.AnimalId,
                AnimalName = offer.Animal.Name,
                AnimalType = offer.Animal.Type,
                SellingPrice = offer.SellingPrice,
                BuyingPrice = offer.BuyingPrice,
                Profit = offer.Profit,
                IsActive = offer.IsActive,
                IsSold = offer.IsSold,
                CreatedByName = $"{offer.CreatedBy.FirstName} {offer.CreatedBy.LastName}",
                CreatedAt = offer.CreatedAt
            };
        }
    }
}