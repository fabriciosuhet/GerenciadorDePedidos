using System;
using GerenciadorDePedidos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GerenciadorDePedidos.Infrastructure.Migrations
{
    [DbContext(typeof(GerenciadorDePedidosDbContext))]
    [Migration("20261127160000_AddAuditoriaSoftDeleteESnapshotMovimentacao")]
    public partial class AddAuditoriaSoftDeleteESnapshotMovimentacao : Migration
    {
        private static readonly string[] TabelasAuditaveis =
        [
            "Clientes",
            "ItensPedidos",
            "Login",
            "MovimentacaoEstoques",
            "Pedidos",
            "Produtos",
            "RefreshTokens"
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var tabela in TabelasAuditaveis)
            {
                migrationBuilder.AddColumn<bool>(
                    name: "Ativo",
                    table: tabela,
                    type: "tinyint(1)",
                    nullable: false,
                    defaultValue: true);

                migrationBuilder.AddColumn<DateTime>(
                    name: "AtualizadoEm",
                    table: tabela,
                    type: "datetime(6)",
                    nullable: true);

                migrationBuilder.AddColumn<DateTime>(
                    name: "CriadoEm",
                    table: tabela,
                    type: "datetime(6)",
                    nullable: false,
                    defaultValue: new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

                migrationBuilder.AddColumn<DateTime>(
                    name: "DesativadoEm",
                    table: tabela,
                    type: "datetime(6)",
                    nullable: true);
            }

            migrationBuilder.AddColumn<int>(
                name: "Motivo",
                table: "MovimentacaoEstoques",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PedidoId",
                table: "MovimentacaoEstoques",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecoUnitario",
                table: "MovimentacaoEstoques",
                type: "decimal(15,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacaoEstoques_PedidoId",
                table: "MovimentacaoEstoques",
                column: "PedidoId");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimentacaoEstoques_Produtos_ProdutoId",
                table: "MovimentacaoEstoques");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimentacaoEstoques_Produtos_ProdutoId",
                table: "MovimentacaoEstoques",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimentacaoEstoques_Pedidos_PedidoId",
                table: "MovimentacaoEstoques",
                column: "PedidoId",
                principalTable: "Pedidos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimentacaoEstoques_Pedidos_PedidoId",
                table: "MovimentacaoEstoques");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimentacaoEstoques_Produtos_ProdutoId",
                table: "MovimentacaoEstoques");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimentacaoEstoques_Produtos_ProdutoId",
                table: "MovimentacaoEstoques",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.DropIndex(
                name: "IX_MovimentacaoEstoques_PedidoId",
                table: "MovimentacaoEstoques");

            migrationBuilder.DropColumn(
                name: "Motivo",
                table: "MovimentacaoEstoques");

            migrationBuilder.DropColumn(
                name: "PedidoId",
                table: "MovimentacaoEstoques");

            migrationBuilder.DropColumn(
                name: "PrecoUnitario",
                table: "MovimentacaoEstoques");

            foreach (var tabela in TabelasAuditaveis)
            {
                migrationBuilder.DropColumn(name: "Ativo", table: tabela);
                migrationBuilder.DropColumn(name: "AtualizadoEm", table: tabela);
                migrationBuilder.DropColumn(name: "CriadoEm", table: tabela);
                migrationBuilder.DropColumn(name: "DesativadoEm", table: tabela);
            }
        }
    }
}

