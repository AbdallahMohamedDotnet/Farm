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
    [Authorize(Roles = UserRoles.Customer)]
    public class CustomerController : ControllerBase
    {
        private readonly IOfferService _offerService;
        private readonly IInvoiceService _invoiceService;
        
        public CustomerController(IOfferService offerService, IInvoiceService invoiceService)
        {
            _offerService = offerService;
            _invoiceService = invoiceService;
        }
        
        /// <summary>
        /// Browse all available offers
        /// </summary>
        [HttpGet("offers")]
        public async Task<ActionResult<IEnumerable<OfferResponseDto>>> BrowseOffers()
        {
            var offers = await _offerService.GetAllOffersAsync();
            return Ok(offers);
        }
        
        /// <summary>
        /// Get a specific offer details
        /// </summary>
        [HttpGet("offers/{id}")]
        public async Task<ActionResult<OfferResponseDto>> GetOffer(Guid id)
        {
            var offer = await _offerService.GetOfferByIdAsync(id);
            if (offer == null || !offer.IsActive)
            {
                return NotFound();
            }
            
            return Ok(offer);
        }
        
        /// <summary>
        /// Purchase an offer (create invoice)
        /// </summary>
        [HttpPost("purchase")]
        public async Task<ActionResult<InvoiceResponseDto>> PurchaseOffer([FromBody] CreateInvoiceDto createInvoiceDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(customerId))
            {
                return Unauthorized();
            }
            
            try
            {
                var result = await _invoiceService.CreateInvoiceAsync(createInvoiceDto, customerId);
                return CreatedAtAction(nameof(GetInvoice), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        /// <summary>
        /// Get all customer invoices
        /// </summary>
        [HttpGet("invoices")]
        public async Task<ActionResult<IEnumerable<InvoiceResponseDto>>> GetMyInvoices()
        {
            var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(customerId))
            {
                return Unauthorized();
            }
            
            var invoices = await _invoiceService.GetCustomerInvoicesAsync(customerId);
            return Ok(invoices);
        }
        
        /// <summary>
        /// Get a specific invoice
        /// </summary>
        [HttpGet("invoices/{id}")]
        public async Task<ActionResult<InvoiceResponseDto>> GetInvoice(Guid id)
        {
            var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(customerId))
            {
                return Unauthorized();
            }
            
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id, customerId);
            if (invoice == null)
            {
                return NotFound();
            }
            
            return Ok(invoice);
        }
        
        /// <summary>
        /// Pay an invoice
        /// </summary>
        [HttpPut("invoices/{id}/pay")]
        public async Task<ActionResult<ApiResponseDto>> PayInvoice(Guid id)
        {
            var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(customerId))
            {
                return Unauthorized();
            }
            
            var result = await _invoiceService.PayInvoiceAsync(id, customerId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }
        
        /// <summary>
        /// Cancel an invoice
        /// </summary>
        [HttpPut("invoices/{id}/cancel")]
        public async Task<ActionResult<ApiResponseDto>> CancelInvoice(Guid id)
        {
            var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(customerId))
            {
                return Unauthorized();
            }
            
            var result = await _invoiceService.CancelInvoiceAsync(id, customerId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }
    }
}