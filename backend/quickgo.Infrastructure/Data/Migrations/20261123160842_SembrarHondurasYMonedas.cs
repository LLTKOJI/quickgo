using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace quickgo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SembrarHondurasYMonedas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "monedas",
                columns: new[] { "codigo", "nombre", "simbolo" },
                values: new object[,]
                {
                    { "HNL", "Lempira", "L" },
                    { "USD", "Dólar estadounidense", "$" }
                });

            migrationBuilder.InsertData(
                table: "paises",
                columns: new[] { "id", "codigo_iso", "nombre" },
                values: new object[] { new Guid("11111111-0000-0000-0000-000000000001"), "HND", "Honduras" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "monedas",
                keyColumn: "codigo",
                keyValue: "HNL");

            migrationBuilder.DeleteData(
                table: "monedas",
                keyColumn: "codigo",
                keyValue: "USD");

            migrationBuilder.DeleteData(
                table: "paises",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000001"));
        }
    }
}
