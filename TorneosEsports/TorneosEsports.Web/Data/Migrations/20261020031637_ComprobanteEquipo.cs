using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TorneosEsports.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class ComprobanteEquipo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoComprobante",
                table: "Equipos",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaValidacion",
                table: "Equipos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Validado",
                table: "Equipos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Equipos_CodigoComprobante",
                table: "Equipos",
                column: "CodigoComprobante",
                unique: true,
                filter: "[CodigoComprobante] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Equipos_CodigoComprobante",
                table: "Equipos");

            migrationBuilder.DropColumn(
                name: "CodigoComprobante",
                table: "Equipos");

            migrationBuilder.DropColumn(
                name: "FechaValidacion",
                table: "Equipos");

            migrationBuilder.DropColumn(
                name: "Validado",
                table: "Equipos");
        }
    }
}
