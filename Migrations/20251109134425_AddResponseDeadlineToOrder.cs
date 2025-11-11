using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReacodeApp.Migrations
{
    /// <inheritdoc />
    public partial class AddResponseDeadlineToOrder : Migration
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
    }
}
