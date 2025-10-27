using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReacodeApp.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 26, 7, 42, 55, 538, DateTimeKind.Utc).AddTicks(4969));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 26, 7, 42, 55, 538, DateTimeKind.Utc).AddTicks(4974));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 26, 7, 42, 55, 538, DateTimeKind.Utc).AddTicks(4976));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 26, 7, 42, 55, 538, DateTimeKind.Utc).AddTicks(4978));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 26, 7, 42, 55, 538, DateTimeKind.Utc).AddTicks(4979));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "FirstName", "PasswordHash", "Role", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 7, 42, 55, 538, DateTimeKind.Utc).AddTicks(5282), "System", "JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=", 2, new DateTime(2025, 10, 26, 7, 42, 55, 538, DateTimeKind.Utc).AddTicks(5283) });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Address", "BankAccountNumber", "BankName", "City", "CreatedAt", "Email", "FirstName", "IsActive", "IsVerified", "LastName", "PasswordHash", "PhoneNumber", "PostalCode", "ProfileImage", "Province", "Role", "UpdatedAt" },
                values: new object[] { 2, null, null, null, null, new DateTime(2025, 10, 26, 7, 42, 55, 538, DateTimeKind.Utc).AddTicks(5289), "superadmin@reacoda.com", "Super", true, true, "Admin", "40+SogUyqHPLMYQ5gHC0uCqPopz0hXLCA9xfD6YVgjE=", null, null, null, null, 3, new DateTime(2025, 10, 26, 7, 42, 55, 538, DateTimeKind.Utc).AddTicks(5290) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 26, 6, 12, 11, 570, DateTimeKind.Utc).AddTicks(5987));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 26, 6, 12, 11, 570, DateTimeKind.Utc).AddTicks(5994));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 26, 6, 12, 11, 570, DateTimeKind.Utc).AddTicks(5996));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 26, 6, 12, 11, 570, DateTimeKind.Utc).AddTicks(5998));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 26, 6, 12, 11, 570, DateTimeKind.Utc).AddTicks(6000));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "FirstName", "PasswordHash", "Role", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 6, 12, 11, 570, DateTimeKind.Utc).AddTicks(6321), "Super", "AQAAAAEAACcQAAAAEHashExample", 3, null });
        }
    }
}
