using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace GerenciadorDePedidos.Infrastructure.Persistence
{
    public class GerenciadorDePedidosDbContextFactory : IDesignTimeDbContextFactory<GerenciadorDePedidosDbContext>
    {
        // Construtor público sem parâmetros
        public GerenciadorDePedidosDbContextFactory() { }

        public GerenciadorDePedidosDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GerenciadorDePedidosDbContext>();

            // Connection string apontando para seu MySQL Docker
            optionsBuilder.UseMySql(
                "Server=localhost;Port=3306;Database=GerenciadorDePedidos;User=root;Password=1234;AllowPublicKeyRetrieval=True;SslMode=None;",
                new MySqlServerVersion(new Version(8, 0, 36))
            );

            return new GerenciadorDePedidosDbContext(optionsBuilder.Options);
        }
    }
}