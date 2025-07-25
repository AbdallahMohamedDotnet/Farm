using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WebApplication2.Models;
using WebApplication2.Models.DTOs;
using WebApplication2.Services;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = UserRoles.SuperAdmin)]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuditService _auditService;
        private readonly IAnalyticsService _analyticsService;

        public AdminController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IAuditService auditService,
            IAnalyticsService analyticsService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _auditService = auditService;
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Get sales analytics and profit reports
        /// </summary>
        [HttpGet("analytics")]
        public async Task<ActionResult<SalesAnalyticsDto>> GetSalesAnalytics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var analytics = await _analyticsService.GetSalesAnalyticsAsync(startDate, endDate);
            return Ok(analytics);
        }

        /// <summary>
        /// Get all invoices with profit information
        /// </summary>
        [HttpGet("invoices")]
        public async Task<ActionResult<IEnumerable<InvoiceResponseDto>>> GetAllInvoices()
        {
            var invoices = await _analyticsService.GetAllInvoicesAsync();
            return Ok(invoices);
        }

        /// <summary>
        /// Get all offers with sales data
        /// </summary>
        [HttpGet("offers")]
        public async Task<ActionResult<IEnumerable<OfferResponseDto>>> GetAllOffersWithSales()
        {
            var offers = await _analyticsService.GetAllOffersWithSalesAsync();
            return Ok(offers);
        }

        /// <summary>
        /// Assign DataEntry role to a registered user by email
        /// </summary>
        [HttpPost("assign-dataentry")]
        public async Task<ActionResult<ApiResponseDto>> AssignDataEntryRole([FromBody] AssignRoleDto assignRoleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var superAdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(superAdminId))
            {
                return Unauthorized();
            }

            var result = await _analyticsService.AssignDataEntryRoleAsync(assignRoleDto.Email, superAdminId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get all users with their roles
        /// </summary>
        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserInfoDto>>> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<UserInfoDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserInfoDto
                {
                    Id = user.Id,
                    Username = user.UserName!,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.ToList(),
                    EmailConfirmed = user.EmailConfirmed,
                    IsActive = user.IsActive
                });
            }

            return Ok(userDtos);
        }
    }

    public class AssignRoleDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}