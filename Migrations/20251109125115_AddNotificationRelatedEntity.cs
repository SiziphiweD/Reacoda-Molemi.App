using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReacodeApp.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationRelatedEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RelatedEntityId",
                table: "Notifications",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelatedEntityType",
                table: "Notifications",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8315));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8321));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8324));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8326));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8328));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8770), new DateTime(2025, 11, 14, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8768), new DateTime(2025, 11, 7, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8761), new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8770) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8778), new DateTime(2025, 11, 16, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8777), new DateTime(2025, 11, 8, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8775), new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8779) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8786), new DateTime(2025, 11, 12, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8784), new DateTime(2025, 11, 8, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8783), new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8787) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8793), new DateTime(2025, 11, 23, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8791), new DateTime(2025, 11, 6, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8790), new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8793) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8800), new DateTime(2025, 11, 14, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8799), new DateTime(2025, 11, 8, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8798), new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8801) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8882), new DateTime(2025, 11, 16, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8881), new DateTime(2025, 11, 7, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8880), new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8883) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8889), new DateTime(2026, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8888), new DateTime(2025, 11, 4, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8887), new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8890) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8896), new DateTime(2025, 11, 19, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8894), new DateTime(2025, 11, 7, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8894), new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8896) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8963), new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8964) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8967), new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8968) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8684), new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8684) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8693), new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8694) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8699), new DateTime(2025, 11, 9, 12, 51, 14, 417, DateTimeKind.Utc).AddTicks(8699) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelatedEntityId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RelatedEntityType",
                table: "Notifications");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9256));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9260));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9262));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9263));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9265));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9603), new DateTime(2025, 11, 14, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9602), new DateTime(2025, 11, 7, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9597), new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9604) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9610), new DateTime(2025, 11, 16, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9608), new DateTime(2025, 11, 8, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9608), new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9610) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9614), new DateTime(2025, 11, 12, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9614), new DateTime(2025, 11, 8, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9613), new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9615) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9619), new DateTime(2025, 11, 23, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9618), new DateTime(2025, 11, 6, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9618), new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9620) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9624), new DateTime(2025, 11, 14, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9623), new DateTime(2025, 11, 8, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9622), new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9624) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9629), new DateTime(2025, 11, 16, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9628), new DateTime(2025, 11, 7, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9627), new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9629) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9634), new DateTime(2026, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9633), new DateTime(2025, 11, 4, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9632), new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9634) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9638), new DateTime(2025, 11, 19, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9637), new DateTime(2025, 11, 7, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9637), new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9639) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9670), new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9670) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9673), new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9673) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9543), new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9544) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9551), new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9551) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9555), new DateTime(2025, 11, 9, 12, 41, 38, 504, DateTimeKind.Utc).AddTicks(9556) });
        }
    }
}
