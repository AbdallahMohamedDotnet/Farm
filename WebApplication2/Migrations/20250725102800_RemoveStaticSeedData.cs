using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebApplication2.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStaticSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("366f02b7-bf70-4707-8307-9ebf2aac47ca"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("3c4432ad-058b-4bad-83dc-4a9fa7142686"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("a1a3324d-3aac-47ae-963a-e95d577d6bf6"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("c158d4dd-d53a-48aa-a91f-1e1ed9de4de6"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { new Guid("366f02b7-bf70-4707-8307-9ebf2aac47ca"), new DateTime(2025, 7, 25, 9, 42, 46, 274, DateTimeKind.Utc).AddTicks(2033), "Animals for milk production", true, "Dairy" },
                    { new Guid("3c4432ad-058b-4bad-83dc-4a9fa7142686"), new DateTime(2025, 7, 25, 9, 42, 46, 274, DateTimeKind.Utc).AddTicks(737), "Farm animals for agriculture", true, "Livestock" },
                    { new Guid("a1a3324d-3aac-47ae-963a-e95d577d6bf6"), new DateTime(2025, 7, 25, 9, 42, 46, 274, DateTimeKind.Utc).AddTicks(2044), "Animals for breeding purposes", true, "Breeding" },
                    { new Guid("c158d4dd-d53a-48aa-a91f-1e1ed9de4de6"), new DateTime(2025, 7, 25, 9, 42, 46, 274, DateTimeKind.Utc).AddTicks(2042), "Animals for meat production", true, "Meat" }
                });
        }
    }
}