using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Models;
using WebApplication2.Models.DTOs;
using WebApplication2.Services;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = UserRoles.SuperAdmin)]
    public class DemoDataController : ControllerBase
    {
        private readonly IDemoDataService _demoDataService;
        private readonly ILogger<DemoDataController> _logger;

        public DemoDataController(IDemoDataService demoDataService, ILogger<DemoDataController> logger)
        {
            _demoDataService = demoDataService;
            _logger = logger;
        }

        /// <summary>
        /// Check if demo data exists in the database
        /// </summary>
        [HttpGet("status")]
        public async Task<ActionResult<object>> GetDemoDataStatus()
        {
            try
            {
                var hasData = await _demoDataService.HasDemoDataAsync();
                return Ok(new
                {
                    HasDemoData = hasData,
                    Message = hasData ? "Demo data exists in the database" : "No demo data found"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking demo data status");
                return StatusCode(500, new ApiResponseDto 
                { 
                    Message = "An error occurred while checking demo data status", 
                    Success = false 
                });
            }
        }

        /// <summary>
        /// Seed demo data into the database
        /// </summary>
        [HttpPost("seed")]
        public async Task<ActionResult<ApiResponseDto>> SeedDemoData()
        {
            try
            {
                if (await _demoDataService.HasDemoDataAsync())
                {
                    return BadRequest(new ApiResponseDto 
                    { 
                        Message = "Demo data already exists. Clear existing data first or use force parameter.", 
                        Success = false 
                    });
                }

                await _demoDataService.SeedDemoDataAsync();
                
                return Ok(new ApiResponseDto 
                { 
                    Message = "Demo data seeded successfully. The database now contains sample users, animals, offers, and invoices.",
                    Success = true 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding demo data");
                return StatusCode(500, new ApiResponseDto 
                { 
                    Message = "An error occurred while seeding demo data", 
                    Success = false 
                });
            }
        }

        /// <summary>
        /// Force seed demo data (clears existing data first)
        /// </summary>
        [HttpPost("seed/force")]
        public async Task<ActionResult<ApiResponseDto>> ForceSeedDemoData()
        {
            try
            {
                if (await _demoDataService.HasDemoDataAsync())
                {
                    await _demoDataService.ClearAllDataAsync();
                }

                await _demoDataService.SeedDemoDataAsync();
                
                return Ok(new ApiResponseDto 
                { 
                    Message = "Demo data force seeded successfully. All existing data was cleared and new demo data was created.",
                    Success = true 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error force seeding demo data");
                return StatusCode(500, new ApiResponseDto 
                { 
                    Message = "An error occurred while force seeding demo data", 
                    Success = false 
                });
            }
        }

        /// <summary>
        /// Clear all demo data from the database
        /// </summary>
        [HttpDelete("clear")]
        public async Task<ActionResult<ApiResponseDto>> ClearDemoData()
        {
            try
            {
                if (!await _demoDataService.HasDemoDataAsync())
                {
                    return BadRequest(new ApiResponseDto 
                    { 
                        Message = "No demo data found to clear", 
                        Success = false 
                    });
                }

                await _demoDataService.ClearAllDataAsync();
                
                return Ok(new ApiResponseDto 
                { 
                    Message = "Demo data cleared successfully. All demo users, animals, offers, and invoices have been removed.",
                    Success = true 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing demo data");
                return StatusCode(500, new ApiResponseDto 
                { 
                    Message = "An error occurred while clearing demo data", 
                    Success = false 
                });
            }
        }

        /// <summary>
        /// Get information about the demo data
        /// </summary>
        [HttpGet("info")]
        public ActionResult<object> GetDemoDataInfo()
        {
            return Ok(new
            {
                Description = "Demo Data Management for Eid Al-Adha Farm API",
                DemoDataIncludes = new
                {
                    Categories = new[] { "Livestock", "Dairy Animals", "Meat Animals", "Breeding Stock", "Sacrifice Animals" },
                    Users = new
                    {
                        DataEntryUsers = 3,
                        CustomerUsers = 5,
                        DefaultPassword = "Demo123!@# (DataEntry) / Customer123! (Customers)"
                    },
                    Animals = new
                    {
                        Types = new[] { "Sheep", "Goat", "Cow", "Camel" },
                        Count = "~32 animals with realistic pricing"
                    },
                    Offers = "~20 offers created by DataEntry users",
                    Invoices = "~8 sample purchase invoices with various statuses"
                },
                Endpoints = new
                {
                    CheckStatus = "GET /api/demodata/status",
                    SeedData = "POST /api/demodata/seed",
                    ForceSeed = "POST /api/demodata/seed/force",
                    ClearData = "DELETE /api/demodata/clear"
                },
                Note = "All demo data operations require SuperAdmin role authentication"
            });
        }
    }
}