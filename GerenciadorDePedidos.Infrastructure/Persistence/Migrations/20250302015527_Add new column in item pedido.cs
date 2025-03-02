using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GerenciadorDePedidos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Addnewcolumninitempedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "ItensPedidos",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Total",
                table: "ItensPedidos");
        }
    }
}
