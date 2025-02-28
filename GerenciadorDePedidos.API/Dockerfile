# Imagem base para o runtime da aplicação
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Imagem para o build da aplicação
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiando os arquivos do projeto
COPY GerenciadorDePedidos.API/GerenciadorDePedidos.API.csproj GerenciadorDePedidos.API/
COPY GerenciadorDePedidos.Application/GerenciadorDePedidos.Application.csproj GerenciadorDePedidos.Application/
COPY GerenciadorDePedidos.Core/GerenciadorDePedidos.Core.csproj GerenciadorDePedidos.Core/
COPY GerenciadorDePedidos.Infrastructure/GerenciadorDePedidos.Infrastructure.csproj GerenciadorDePedidos.Infrastructure/

# Restaurando as dependências
RUN dotnet restore "GerenciadorDePedidos.API/GerenciadorDePedidos.API.csproj"

# Copiando o restante dos arquivos e compilando a aplicação
COPY . .
WORKDIR "/src/GerenciadorDePedidos.API"
RUN dotnet build "GerenciadorDePedidos.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publicando a aplicação
FROM build AS publish
RUN dotnet publish "GerenciadorDePedidos.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Configurando o contêiner final para execução da aplicação
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GerenciadorDePedidos.API.dll"]
