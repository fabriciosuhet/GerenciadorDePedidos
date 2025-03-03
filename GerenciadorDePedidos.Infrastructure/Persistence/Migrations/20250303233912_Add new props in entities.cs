using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GerenciadorDePedidos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Addnewpropsinentities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClienteId",
                table: "MovimentacaoEstoques",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacaoEstoques_ClienteId",
                table: "MovimentacaoEstoques",
                column: "ClienteId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimentacaoEstoques_Clientes_ClienteId",
                table: "MovimentacaoEstoques",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimentacaoEstoques_Clientes_ClienteId",
                table: "MovimentacaoEstoques");

            migrationBuilder.DropIndex(
                name: "IX_MovimentacaoEstoques_ClienteId",
                table: "MovimentacaoEstoques");

            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "MovimentacaoEstoques");
        }
    }
}
