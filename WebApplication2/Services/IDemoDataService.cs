using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Models;

namespace WebApplication2.Services
{
    public interface IDemoDataService
    {
        Task SeedDemoDataAsync();
        Task ClearAllDataAsync();
        Task<bool> HasDemoDataAsync();
    }
}