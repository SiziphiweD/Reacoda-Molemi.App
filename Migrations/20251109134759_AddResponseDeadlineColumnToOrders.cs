using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReacodeApp.Migrations
{
    /// <inheritdoc />
    public partial class AddResponseDeadlineColumnToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ResponseDeadline",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7336));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7343));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7346));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7348));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7350));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7751), new DateTime(2025, 11, 14, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7749), new DateTime(2025, 11, 7, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7743), new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7752) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7760), new DateTime(2025, 11, 16, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7758), new DateTime(2025, 11, 8, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7757), new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7761) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7768), new DateTime(2025, 11, 12, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7766), new DateTime(2025, 11, 8, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7765), new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7769) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7778), new DateTime(2025, 11, 23, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7776), new DateTime(2025, 11, 6, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7774), new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7779) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7787), new DateTime(2025, 11, 14, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7785), new DateTime(2025, 11, 8, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7783), new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7787) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7795), new DateTime(2025, 11, 16, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7794), new DateTime(2025, 11, 7, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7792), new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7796) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7802), new DateTime(2026, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7801), new DateTime(2025, 11, 4, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7800), new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7803) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7809), new DateTime(2025, 11, 19, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7808), new DateTime(2025, 11, 7, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7807), new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7810) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7960), new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7960) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7966), new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7967) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7670), new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7670) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7680), new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7680) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7686), new DateTime(2025, 11, 9, 13, 47, 58, 719, DateTimeKind.Utc).AddTicks(7686) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResponseDeadline",
                table: "Orders");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5345));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5350));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5351));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5353));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5354));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5670), new DateTime(2025, 11, 14, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5668), new DateTime(2025, 11, 7, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5662), new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5670) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5676), new DateTime(2025, 11, 16, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5675), new DateTime(2025, 11, 8, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5674), new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5677) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5681), new DateTime(2025, 11, 12, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5680), new DateTime(2025, 11, 8, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5680), new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5682) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5686), new DateTime(2025, 11, 23, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5685), new DateTime(2025, 11, 6, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5685), new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5687) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5691), new DateTime(2025, 11, 14, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5690), new DateTime(2025, 11, 8, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5689), new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5692) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5696), new DateTime(2025, 11, 16, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5695), new DateTime(2025, 11, 7, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5695), new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5697) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5701), new DateTime(2026, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5700), new DateTime(2025, 11, 4, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5700), new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5702) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5706), new DateTime(2025, 11, 19, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5705), new DateTime(2025, 11, 7, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5704), new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5707) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5737), new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5738) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5740), new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5741) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5614), new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5614) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5621), new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5621) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5625), new DateTime(2025, 11, 9, 13, 44, 24, 509, DateTimeKind.Utc).AddTicks(5625) });
        }
    }
}
