using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Models;

namespace WebApplication2.Services
{
    public class DemoDataService : IDemoDataService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<DemoDataService> _logger;

        public DemoDataService(
            ApplicationDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<DemoDataService> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<bool> HasDemoDataAsync()
        {
            return await _context.Categories.AnyAsync() || 
                   await _context.Animals.AnyAsync() || 
                   await _context.Offers.AnyAsync();
        }

        public async Task ClearAllDataAsync()
        {
            _logger.LogInformation("Clearing all demo data...");

            // Clear in correct order due to foreign key constraints
            _context.Invoices.RemoveRange(_context.Invoices);
            _context.Stocks.RemoveRange(_context.Stocks);
            _context.Offers.RemoveRange(_context.Offers);
            _context.Animals.RemoveRange(_context.Animals);
            _context.Farms.RemoveRange(_context.Farms.Where(f => f.Owner.UserName != "SuperAdmin"));
            _context.Categories.RemoveRange(_context.Categories);
            _context.AuditLogs.RemoveRange(_context.AuditLogs);
            _context.UserOtps.RemoveRange(_context.UserOtps);
            _context.PendingRegistrations.RemoveRange(_context.PendingRegistrations);

            // Remove demo users (keep SuperAdmin)
            var demoUsers = await _context.Users.Where(u => u.UserName != "SuperAdmin").ToListAsync();
            foreach (var user in demoUsers)
            {
                await _userManager.DeleteAsync(user);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Demo data cleared successfully");
        }

        public async Task SeedDemoDataAsync()
        {
            _logger.LogInformation("Starting demo data seeding...");

            try
            {
                // 1. Seed Categories
                await SeedCategoriesAsync();

                // 2. Seed Demo Users
                var demoUsers = await SeedDemoUsersAsync();

                // 3. Seed Farms
                await SeedFarmsAsync(demoUsers);

                // 4. Seed Animals
                await SeedAnimalsAsync(demoUsers);

                // 5. Seed Offers
                await SeedOffersAsync(demoUsers);

                // 6. Seed Invoices (some demo purchases)
                await SeedInvoicesAsync(demoUsers);

                await _context.SaveChangesAsync();
                _logger.LogInformation("Demo data seeded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding demo data");
                throw;
            }
        }

        private async Task SeedCategoriesAsync()
        {
            if (await _context.Categories.AnyAsync()) return;

            var categories = new[]
            {
                new Category 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Livestock", 
                    Description = "Farm animals for general agriculture purposes", 
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Category 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Dairy Animals", 
                    Description = "Animals specifically raised for milk production", 
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Category 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Meat Animals", 
                    Description = "Animals raised specifically for meat production", 
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Category 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Breeding Stock", 
                    Description = "Premium animals for breeding purposes", 
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Category 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Sacrifice Animals", 
                    Description = "Animals suitable for Islamic sacrifice (Halal)", 
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Seeded {categories.Length} categories");
        }

        private async Task<List<User>> SeedDemoUsersAsync()
        {
            var demoUsers = new List<User>();

            // Create DataEntry users
            var dataEntryUsers = new[]
            {
                new { Username = "farm_manager1", Email = "manager1@farm.com", FirstName = "Ahmed", LastName = "Hassan", Role = UserRoles.DataEntry },
                new { Username = "farm_manager2", Email = "manager2@farm.com", FirstName = "Mohamed", LastName = "Ali", Role = UserRoles.DataEntry },
                new { Username = "livestock_expert", Email = "expert@farm.com", FirstName = "Fatima", LastName = "Abdullah", Role = UserRoles.DataEntry }
            };

            foreach (var userData in dataEntryUsers)
            {
                if (await _userManager.FindByEmailAsync(userData.Email) == null)
                {
                    var user = new User
                    {
                        UserName = userData.Username,
                        Email = userData.Email,
                        FirstName = userData.FirstName,
                        LastName = userData.LastName,
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await _userManager.CreateAsync(user, "Demo123!@#");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, userData.Role);
                        demoUsers.Add(user);
                        _logger.LogInformation($"Created demo user: {userData.Email}");
                    }
                }
            }

            // Create Customer users
            var customerUsers = new[]
            {
                new { Username = "customer1", Email = "customer1@example.com", FirstName = "Omar", LastName = "Ibrahim" },
                new { Username = "customer2", Email = "customer2@example.com", FirstName = "Aisha", LastName = "Mohamed" },
                new { Username = "customer3", Email = "customer3@example.com", FirstName = "Yusuf", LastName = "Ahmad" },
                new { Username = "customer4", Email = "customer4@example.com", FirstName = "Maryam", LastName = "Hassan" },
                new { Username = "customer5", Email = "customer5@example.com", FirstName = "Khalid", LastName = "Ali" }
            };

            foreach (var userData in customerUsers)
            {
                if (await _userManager.FindByEmailAsync(userData.Email) == null)
                {
                    var user = new User
                    {
                        UserName = userData.Username,
                        Email = userData.Email,
                        FirstName = userData.FirstName,
                        LastName = userData.LastName,
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await _userManager.CreateAsync(user, "Customer123!");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, UserRoles.Customer);
                        demoUsers.Add(user);
                        _logger.LogInformation($"Created demo customer: {userData.Email}");
                    }
                }
            }

            return demoUsers;
        }

        private async Task SeedFarmsAsync(List<User> demoUsers)
        {
            foreach (var user in demoUsers)
            {
                if (!await _context.Farms.AnyAsync(f => f.OwnerId == user.Id))
                {
                    var farm = new Farm
                    {
                        Id = Guid.NewGuid(),
                        Name = $"{user.FirstName} {user.LastName}'s Farm",
                        OwnerId = user.Id
                    };

                    await _context.Farms.AddAsync(farm);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Created farms for demo users");
        }

        private async Task SeedAnimalsAsync(List<User> demoUsers)
        {
            if (await _context.Animals.AnyAsync()) return;

            var categories = await _context.Categories.ToListAsync();
            var farms = await _context.Farms.Include(f => f.Owner).ToListAsync();
            var dataEntryFarms = farms.Where(f => demoUsers.Any(u => u.Id == f.OwnerId)).ToList();

            var animalData = new[]
            {
                // Sheep
                new { Type = "Sheep", Names = new[] { "Fluffy", "Woolly", "Snow", "Cotton", "Cloud", "Pearl", "Silver", "Golden" }, 
                      AgeRange = new[] { 6, 8, 12, 18, 24, 36 }, Weight = new[] { 25.0m, 35.0m, 45.0m }, 
                      BuyingPrice = new[] { 800m, 1200m, 1500m }, Category = "Sacrifice Animals" },
                
                // Goats
                new { Type = "Goat", Names = new[] { "Billy", "Nanny", "Pepper", "Cocoa", "Hazel", "Ginger", "Rusty", "Copper" }, 
                      AgeRange = new[] { 12, 18, 24, 30, 36, 48 }, Weight = new[] { 20.0m, 30.0m, 40.0m }, 
                      BuyingPrice = new[] { 600m, 1000m, 1300m }, Category = "Meat Animals" },
                
                // Cows
                new { Type = "Cow", Names = new[] { "Bella", "Daisy", "Luna", "Star", "Princess", "Queen", "Duchess", "Lady" }, 
                      AgeRange = new[] { 24, 36, 48, 60, 72 }, Weight = new[] { 300.0m, 450.0m, 600.0m }, 
                      BuyingPrice = new[] { 8000m, 12000m, 15000m }, Category = "Dairy Animals" },
                
                // Camels
                new { Type = "Camel", Names = new[] { "Desert", "Sahara", "Oasis", "Mirage", "Dune", "Sultan", "Majesty", "Royal" }, 
                      AgeRange = new[] { 60, 72, 84, 96, 108 }, Weight = new[] { 400.0m, 550.0m, 700.0m }, 
                      BuyingPrice = new[] { 15000m, 25000m, 35000m }, Category = "Breeding Stock" }
            };

            var random = new Random();
            var animalsToAdd = new List<Animal>();

            foreach (var animalType in animalData)
            {
                var category = categories.FirstOrDefault(c => c.Name == animalType.Category);
                
                for (int i = 0; i < animalType.Names.Length; i++)
                {
                    var farm = dataEntryFarms[random.Next(dataEntryFarms.Count)];
                    var age = animalType.AgeRange[random.Next(animalType.AgeRange.Length)];
                    var weight = animalType.Weight[random.Next(animalType.Weight.Length)];
                    var buyingPrice = animalType.BuyingPrice[random.Next(animalType.BuyingPrice.Length)];
                    var sellingPrice = buyingPrice * (1.2m + (decimal)(random.NextDouble() * 0.3)); // 20-50% markup
                    
                    var animal = new Animal
                    {
                        Id = Guid.NewGuid(),
                        Name = animalType.Names[i],
                        Type = animalType.Type,
                        Age = age,
                        Weight = weight,
                        BuyingPrice = buyingPrice,
                        SellingPrice = Math.Round(sellingPrice, 2),
                        FarmId = farm.Id,
                        CategoryId = category?.Id,
                        IsForSale = random.Next(100) < 70, // 70% chance to be for sale
                        StockQuantity = random.Next(1, 5),
                        IsFed = random.Next(100) < 80,
                        IsGroomed = random.Next(100) < 60,
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 90)),
                        Description = $"High-quality {animalType.Type.ToLower()} from {farm.Owner.FirstName}'s farm"
                    };

                    animalsToAdd.Add(animal);
                }
            }

            await _context.Animals.AddRangeAsync(animalsToAdd);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Seeded {animalsToAdd.Count} demo animals");
        }

        private async Task SeedOffersAsync(List<User> demoUsers)
        {
            if (await _context.Offers.AnyAsync()) return;

            var dataEntryUsers = demoUsers.Where(u => _userManager.IsInRoleAsync(u, UserRoles.DataEntry).Result).ToList();
            var animals = await _context.Animals.Where(a => a.IsForSale).ToListAsync();
            var random = new Random();

            var offers = new List<Offer>();

            foreach (var animal in animals.Take(20)) // Create offers for first 20 animals
            {
                var dataEntryUser = dataEntryUsers[random.Next(dataEntryUsers.Count)];
                
                var offer = new Offer
                {
                    Id = Guid.NewGuid(),
                    Title = $"Premium {animal.Type} - {animal.Name}",
                    Description = $"High-quality {animal.Type.ToLower()} perfect for Eid Al-Adha sacrifice. Well-fed and healthy animal from our certified farm.",
                    AnimalId = animal.Id,
                    SellingPrice = animal.SellingPrice,
                    BuyingPrice = animal.BuyingPrice,
                    IsActive = random.Next(100) < 85, // 85% active
                    IsSold = false,
                    CreatedById = dataEntryUser.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                    UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 10))
                };

                offers.Add(offer);
            }

            await _context.Offers.AddRangeAsync(offers);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Seeded {offers.Count} demo offers");
        }

        private async Task SeedInvoicesAsync(List<User> demoUsers)
        {
            if (await _context.Invoices.AnyAsync()) return;

            var customerUsers = demoUsers.Where(u => _userManager.IsInRoleAsync(u, UserRoles.Customer).Result).ToList();
            var offers = await _context.Offers.Where(o => o.IsActive).Include(o => o.Animal).ToListAsync();
            var random = new Random();

            var invoices = new List<Invoice>();
            var invoiceNumber = 1;

            // Create some completed purchases
            for (int i = 0; i < Math.Min(8, offers.Count); i++)
            {
                var offer = offers[i];
                var customer = customerUsers[random.Next(customerUsers.Count)];
                var quantity = random.Next(1, Math.Min(3, offer.Animal.StockQuantity + 1));
                var isPaid = random.Next(100) < 70; // 70% chance to be paid
                
                var invoice = new Invoice
                {
                    Id = Guid.NewGuid(),
                    InvoiceNumber = $"INV{invoiceNumber:D6}",
                    CustomerId = customer.Id,
                    OfferId = offer.Id,
                    Quantity = quantity,
                    UnitPrice = offer.SellingPrice,
                    TotalAmount = offer.SellingPrice * quantity,
                    BuyingCost = offer.BuyingPrice * quantity,
                    Status = isPaid ? InvoiceStatus.Paid : (random.Next(100) < 20 ? InvoiceStatus.Cancelled : InvoiceStatus.Pending),
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 60)),
                    PaidAt = isPaid ? DateTime.UtcNow.AddDays(-random.Next(0, 30)) : null,
                    Notes = $"Purchase of {quantity} {offer.Animal.Type}(s) for Eid Al-Adha"
                };

                invoices.Add(invoice);
                invoiceNumber++;

                // Update offer status if sold
                if (invoice.Status == InvoiceStatus.Paid && quantity >= offer.Animal.StockQuantity)
                {
                    offer.IsSold = true;
                    offer.Animal.IsForSale = false;
                    offer.Animal.StockQuantity = 0;
                }
                else if (invoice.Status == InvoiceStatus.Paid)
                {
                    offer.Animal.StockQuantity -= quantity;
                }
            }

            await _context.Invoices.AddRangeAsync(invoices);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Seeded {invoices.Count} demo invoices");
        }
    }
}