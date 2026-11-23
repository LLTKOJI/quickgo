using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace quickgo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class B4B5_EnviosSubastas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "monto",
                table: "cotizaciones",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "moneda_codigo",
                table: "cotizaciones",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_cotizaciones_envio_id",
                table: "cotizaciones",
                column: "envio_id");

            migrationBuilder.CreateIndex(
                name: "IX_cotizaciones_moneda_codigo",
                table: "cotizaciones",
                column: "moneda_codigo");

            migrationBuilder.AddForeignKey(
                name: "FK_cotizaciones_envios_envio_id",
                table: "cotizaciones",
                column: "envio_id",
                principalTable: "envios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_cotizaciones_monedas_moneda_codigo",
                table: "cotizaciones",
                column: "moneda_codigo",
                principalTable: "monedas",
                principalColumn: "codigo",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cotizaciones_envios_envio_id",
                table: "cotizaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_cotizaciones_monedas_moneda_codigo",
                table: "cotizaciones");

            migrationBuilder.DropIndex(
                name: "IX_cotizaciones_envio_id",
                table: "cotizaciones");

            migrationBuilder.DropIndex(
                name: "IX_cotizaciones_moneda_codigo",
                table: "cotizaciones");

            migrationBuilder.AlterColumn<decimal>(
                name: "monto",
                table: "cotizaciones",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(12,2)",
                oldPrecision: 12,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "moneda_codigo",
                table: "cotizaciones",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3);
        }
    }
}
