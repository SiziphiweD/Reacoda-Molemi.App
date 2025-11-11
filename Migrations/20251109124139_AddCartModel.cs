using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ReacodeApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCartModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Carts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Carts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Carts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Carts_ProductId",
                table: "Carts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId_ProductId",
                table: "Carts",
                columns: new[] { "UserId", "ProductId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Carts");

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
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8715), new DateTime(2025, 10, 31, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8713), new DateTime(2025, 10, 24, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8707), new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8716) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8723), new DateTime(2025, 11, 2, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8722), new DateTime(2025, 10, 25, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8721), new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8724) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8730), new DateTime(2025, 10, 29, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8729), new DateTime(2025, 10, 25, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8728), new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8731) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8738), new DateTime(2025, 11, 9, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8736), new DateTime(2025, 10, 23, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8736), new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8738) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8744), new DateTime(2025, 10, 31, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8743), new DateTime(2025, 10, 25, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8742), new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8744) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8750), new DateTime(2025, 11, 2, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8749), new DateTime(2025, 10, 24, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8748), new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8751) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8757), new DateTime(2026, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8756), new DateTime(2025, 10, 21, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8755), new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8758) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8764), new DateTime(2025, 11, 5, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8762), new DateTime(2025, 10, 24, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8761), new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8764) });

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

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8638), new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8638) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8646), new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8647) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8652), new DateTime(2025, 10, 26, 12, 22, 26, 709, DateTimeKind.Utc).AddTicks(8652) });
        }
    }
}
