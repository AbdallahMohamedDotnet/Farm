using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Models;

namespace WebApplication2.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        
        public DbSet<Farm> Farms { get; set; }
        public DbSet<Animal> Animals { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<UserOtp> UserOtps { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<PendingRegistration> PendingRegistrations { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // User configuration
            builder.Entity<User>(entity =>
            {
                entity.Property(u => u.FirstName).HasMaxLength(50);
                entity.Property(u => u.LastName).HasMaxLength(50);
                entity.HasIndex(u => u.Email).IsUnique();
            });
            
            // Farm configuration
            builder.Entity<Farm>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Name).IsRequired().HasMaxLength(100);
                entity.HasOne(f => f.Owner)
                      .WithOne(u => u.Farm)
                      .HasForeignKey<Farm>(f => f.OwnerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Animal configuration
            builder.Entity<Animal>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Name).IsRequired().HasMaxLength(50);
                entity.Property(a => a.Type).IsRequired().HasMaxLength(20);
                entity.Property(a => a.Weight).HasColumnType("decimal(18,2)");
                entity.Property(a => a.BuyingPrice).HasColumnType("decimal(18,2)");
                entity.Property(a => a.SellingPrice).HasColumnType("decimal(18,2)");
                
                entity.HasOne(a => a.Farm)
                      .WithMany(f => f.Animals)
                      .HasForeignKey(a => a.FarmId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasOne(a => a.Category)
                      .WithMany(c => c.Animals)
                      .HasForeignKey(a => a.CategoryId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
            
            // Category configuration
            builder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Description).HasMaxLength(500);
                entity.HasIndex(c => c.Name).IsUnique();
            });
            
            // UserOtp configuration - support both real users and pending registrations
            builder.Entity<UserOtp>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.OtpCode).IsRequired().HasMaxLength(6);
                entity.Property(o => o.Purpose).IsRequired().HasMaxLength(50);
                entity.Property(o => o.UserId).IsRequired().HasMaxLength(450); // Support both real UserIds and temp strings
                
                // Make the relationship optional to support pending registrations
                entity.HasOne(o => o.User)
                      .WithMany(u => u.UserOtps)
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired(false); // This allows UserId to reference non-existent users for pending registrations
                      
                entity.HasIndex(o => new { o.UserId, o.OtpCode, o.Purpose });
            });
            
            // AuditLog configuration
            builder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Action).IsRequired().HasMaxLength(100);
                entity.Property(a => a.EntityName).IsRequired().HasMaxLength(100);
                entity.Property(a => a.Details).HasMaxLength(1000);
                
                entity.HasOne(a => a.User)
                      .WithMany()
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            
            // PendingRegistration configuration
            builder.Entity<PendingRegistration>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Email).IsRequired().HasMaxLength(256);
                entity.Property(p => p.Username).IsRequired().HasMaxLength(50);
                entity.Property(p => p.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(p => p.LastName).IsRequired().HasMaxLength(50);
                entity.Property(p => p.PasswordHash).IsRequired();
                
                entity.HasIndex(p => p.Email).IsUnique();
            });
            
            // Offer configuration
            builder.Entity<Offer>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.Title).IsRequired().HasMaxLength(100);
                entity.Property(o => o.Description).HasMaxLength(500);
                entity.Property(o => o.SellingPrice).HasColumnType("decimal(18,2)");
                entity.Property(o => o.BuyingPrice).HasColumnType("decimal(18,2)");
                
                entity.HasOne(o => o.Animal)
                      .WithMany(a => a.Offers)
                      .HasForeignKey(o => o.AnimalId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasOne(o => o.CreatedBy)
                      .WithMany()
                      .HasForeignKey(o => o.CreatedById)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Stock configuration
            builder.Entity<Stock>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.BuyingPrice).HasColumnType("decimal(18,2)");
                entity.Property(s => s.SellingPrice).HasColumnType("decimal(18,2)");
                entity.Property(s => s.Notes).HasMaxLength(200);
                
                entity.HasOne(s => s.Animal)
                      .WithMany(a => a.Stocks)
                      .HasForeignKey(s => s.AnimalId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasOne(s => s.AddedBy)
                      .WithMany()
                      .HasForeignKey(s => s.AddedById)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Invoice configuration
            builder.Entity<Invoice>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(20);
                entity.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(i => i.BuyingCost).HasColumnType("decimal(18,2)");
                entity.Property(i => i.Notes).HasMaxLength(500);
                
                entity.HasOne(i => i.Customer)
                      .WithMany()
                      .HasForeignKey(i => i.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);
                      
                entity.HasOne(i => i.Offer)
                      .WithMany(o => o.Invoices)
                      .HasForeignKey(i => i.OfferId)
                      .OnDelete(DeleteBehavior.Restrict);
                      
                entity.HasIndex(i => i.InvoiceNumber).IsUnique();
            });
        }
    }
}