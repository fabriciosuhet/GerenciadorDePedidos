using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GerenciadorDePedidos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMovimentacaoEstoqueinDbSetGerenciadorDePEdidosDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimentacaoEstoque_Produtos_ProdutoId",
                table: "MovimentacaoEstoque");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MovimentacaoEstoque",
                table: "MovimentacaoEstoque");

            migrationBuilder.RenameTable(
                name: "MovimentacaoEstoque",
                newName: "MovimentacaoEstoques");

            migrationBuilder.RenameIndex(
                name: "IX_MovimentacaoEstoque_ProdutoId",
                table: "MovimentacaoEstoques",
                newName: "IX_MovimentacaoEstoques_ProdutoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MovimentacaoEstoques",
                table: "MovimentacaoEstoques",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimentacaoEstoques_Produtos_ProdutoId",
                table: "MovimentacaoEstoques",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimentacaoEstoques_Produtos_ProdutoId",
                table: "MovimentacaoEstoques");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MovimentacaoEstoques",
                table: "MovimentacaoEstoques");

            migrationBuilder.RenameTable(
                name: "MovimentacaoEstoques",
                newName: "MovimentacaoEstoque");

            migrationBuilder.RenameIndex(
                name: "IX_MovimentacaoEstoques_ProdutoId",
                table: "MovimentacaoEstoque",
                newName: "IX_MovimentacaoEstoque_ProdutoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MovimentacaoEstoque",
                table: "MovimentacaoEstoque",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimentacaoEstoque_Produtos_ProdutoId",
                table: "MovimentacaoEstoque",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
