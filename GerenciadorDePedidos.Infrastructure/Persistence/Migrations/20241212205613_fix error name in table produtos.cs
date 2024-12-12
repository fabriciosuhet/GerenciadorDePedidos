using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GerenciadorDePedidos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class fixerrornameintableprodutos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItensPedidos_Prdutos_ProdutoId",
                table: "ItensPedidos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Prdutos",
                table: "Prdutos");

            migrationBuilder.RenameTable(
                name: "Prdutos",
                newName: "Produtos");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Produtos",
                table: "Produtos",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ItensPedidos_Produtos_ProdutoId",
                table: "ItensPedidos",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItensPedidos_Produtos_ProdutoId",
                table: "ItensPedidos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Produtos",
                table: "Produtos");

            migrationBuilder.RenameTable(
                name: "Produtos",
                newName: "Prdutos");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Prdutos",
                table: "Prdutos",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ItensPedidos_Prdutos_ProdutoId",
                table: "ItensPedidos",
                column: "ProdutoId",
                principalTable: "Prdutos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
