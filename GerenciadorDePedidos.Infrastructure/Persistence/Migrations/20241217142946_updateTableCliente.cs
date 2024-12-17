﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GerenciadorDePedidos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class updateTableCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Senha",
                table: "Clientes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Senha",
                table: "Clientes");
        }
    }
}
