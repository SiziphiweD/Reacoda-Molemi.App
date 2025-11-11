using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReacodeApp.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectionReasonAndEstimatedDeliveryToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedDeliveryDate",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1240));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1245));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1247));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1249));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1250));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1636), new DateTime(2025, 11, 14, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1634), new DateTime(2025, 11, 7, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1627), new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1636) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1717), new DateTime(2025, 11, 16, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1715), new DateTime(2025, 11, 8, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1714), new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1717) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1723), new DateTime(2025, 11, 12, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1722), new DateTime(2025, 11, 8, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1721), new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1723) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1729), new DateTime(2025, 11, 23, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1727), new DateTime(2025, 11, 6, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1727), new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1729) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1734), new DateTime(2025, 11, 14, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1733), new DateTime(2025, 11, 8, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1732), new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1735) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1740), new DateTime(2025, 11, 16, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1739), new DateTime(2025, 11, 7, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1738), new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1740) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1745), new DateTime(2026, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1744), new DateTime(2025, 11, 4, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1743), new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1746) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "ExpiryDate", "HarvestDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1751), new DateTime(2025, 11, 19, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1750), new DateTime(2025, 11, 7, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1749), new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1751) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1795), new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1796) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1800), new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1800) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1563), new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1564) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1572), new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1572) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1576), new DateTime(2025, 11, 9, 14, 38, 24, 9, DateTimeKind.Utc).AddTicks(1577) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedDeliveryDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Orders");

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
    }
}
