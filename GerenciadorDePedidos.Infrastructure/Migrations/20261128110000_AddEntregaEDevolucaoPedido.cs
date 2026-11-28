using System;
using GerenciadorDePedidos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GerenciadorDePedidos.Infrastructure.Migrations
{
    [DbContext(typeof(GerenciadorDePedidosDbContext))]
    [Migration("20261128110000_AddEntregaEDevolucaoPedido")]
    public partial class AddEntregaEDevolucaoPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConcluidoEm",
                table: "Pedidos",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DevolvidoEm",
                table: "Pedidos",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoDevolucao",
                table: "Pedidos",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConcluidoEm",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "DevolvidoEm",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "MotivoDevolucao",
                table: "Pedidos");
        }
    }
}

