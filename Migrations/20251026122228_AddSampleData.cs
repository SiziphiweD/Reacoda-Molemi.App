using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ReacodeApp.Migrations
{
    /// <inheritdoc />
    public partial class AddSampleData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8383));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8389));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8391));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8393));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8394));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8817), new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8818) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8822), new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8822) });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Address", "BankAccountNumber", "BankName", "City", "CreatedAt", "Email", "FirstName", "IsActive", "IsVerified", "LastName", "PasswordHash", "PhoneNumber", "PostalCode", "ProfileImage", "Province", "Role", "UpdatedAt" },
                values: new object[,]
                {
                    { 10, "123 Farm Road", null, null, "Cape Town", new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8638), "farmer1@example.com", "John", true, true, "Smith", "JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=", "+27 82 123 4567", "8000", null, "Western Cape", 1, new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8638) },
                    { 11, "456 Harvest Lane", null, null, "Johannesburg", new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8646), "farmer2@example.com", "Sarah", true, true, "Johnson", "JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=", "+27 83 987 6543", "2000", null, "Gauteng", 1, new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8647) },
                    { 12, "789 Green Valley", null, null, "Durban", new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8652), "farmer3@example.com", "Mike", true, true, "Brown", "JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=", "+27 84 555 1234", "4000", null, "KwaZulu-Natal", 1, new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8652) }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "AvailableQuantity", "CategoryId", "CreatedAt", "Description", "ExpiryDate", "FarmerId", "HarvestDate", "ImageUrl", "IsActive", "IsAvailable", "Location", "Name", "PricePerKg", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 50, 1, new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8715), "Premium organic tomatoes grown in our greenhouse. Perfect for salads, cooking, and fresh eating.", new DateTime(2025, 10, 31, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8713), 10, new DateTime(2025, 10, 24, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8707), "https://images.unsplash.com/photo-1546470427-5bb7c3a1b7b8?w=400&h=300&fit=crop", true, true, "Cape Town, Western Cape", "Fresh Organic Tomatoes", 45.00m, 1, new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8716) },
                    { 2, 30, 1, new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8723), "Fresh sweet corn harvested daily. Great for grilling, boiling, or adding to salads.", new DateTime(2025, 11, 2, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8722), 10, new DateTime(2025, 10, 25, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8721), "https://images.unsplash.com/photo-1551754655-cd27e38d2076?w=400&h=300&fit=crop", true, true, "Cape Town, Western Cape", "Sweet Corn", 35.00m, 1, new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8724) },
                    { 3, 25, 2, new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8730), "Sweet, juicy strawberries perfect for desserts, smoothies, or eating fresh.", new DateTime(2025, 10, 29, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8729), 11, new DateTime(2025, 10, 25, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8728), "https://images.unsplash.com/photo-1464965911861-746a04b4bca6?w=400&h=300&fit=crop", true, true, "Johannesburg, Gauteng", "Fresh Strawberries", 80.00m, 1, new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8731) },
                    { 4, 40, 2, new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8738), "Crisp, sweet organic apples from our orchard. Perfect for snacking or baking.", new DateTime(2025, 11, 9, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8736), 11, new DateTime(2025, 10, 23, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8736), "https://images.unsplash.com/photo-1560806887-1e4cd0b6cbd6?w=400&h=300&fit=crop", true, true, "Johannesburg, Gauteng", "Organic Apples", 55.00m, 1, new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8738) },
                    { 5, 15, 4, new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8744), "Aromatic fresh basil perfect for Italian dishes, pesto, and garnishing.", new DateTime(2025, 10, 31, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8743), 12, new DateTime(2025, 10, 25, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8742), "https://images.unsplash.com/photo-1615485925442-7b4b8b5b5b5b?w=400&h=300&fit=crop", true, true, "Durban, KwaZulu-Natal", "Fresh Basil", 120.00m, 1, new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8744) },
                    { 6, 20, 4, new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8750), "Fragrant rosemary perfect for roasting, grilling, and Mediterranean dishes.", new DateTime(2025, 11, 2, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8749), 12, new DateTime(2025, 10, 24, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8748), "https://images.unsplash.com/photo-1615485925442-7b4b8b5b5b5b?w=400&h=300&fit=crop", true, true, "Durban, KwaZulu-Natal", "Fresh Rosemary", 100.00m, 1, new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8751) },
                    { 7, 35, 3, new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8757), "Premium organic quinoa, high in protein and perfect for healthy meals.", new DateTime(2026, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8756), 10, new DateTime(2025, 10, 21, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8755), "https://images.unsplash.com/photo-1586201375761-83865001e31c?w=400&h=300&fit=crop", true, true, "Cape Town, Western Cape", "Organic Quinoa", 95.00m, 1, new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8758) },
                    { 8, 60, 1, new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8764), "Sweet, crunchy carrots perfect for snacking, cooking, or juicing.", new DateTime(2025, 11, 5, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8762), 11, new DateTime(2025, 10, 24, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8761), "https://images.unsplash.com/photo-1598170845058-32b9d6a5da35?w=400&h=300&fit=crop", true, true, "Johannesburg, Gauteng", "Fresh Carrots", 25.00m, 1, new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8764) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 26, 8, 21, 18, 257, DateTimeKind.Utc).AddTicks(3897));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 26, 8, 21, 18, 257, DateTimeKind.Utc).AddTicks(3903));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 26, 8, 21, 18, 257, DateTimeKind.Utc).AddTicks(3905));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 26, 8, 21, 18, 257, DateTimeKind.Utc).AddTicks(3907));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 26, 8, 21, 18, 257, DateTimeKind.Utc).AddTicks(3908));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 8, 21, 18, 257, DateTimeKind.Utc).AddTicks(4197), new DateTime(2025, 10, 26, 8, 21, 18, 257, DateTimeKind.Utc).AddTicks(4199) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 8, 21, 18, 257, DateTimeKind.Utc).AddTicks(4205), new DateTime(2025, 10, 26, 8, 21, 18, 257, DateTimeKind.Utc).AddTicks(4206) });
        }
    }
}
