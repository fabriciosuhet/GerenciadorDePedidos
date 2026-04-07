# Fase base para execução
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

# Fase de compilação
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia os arquivos .csproj preservando a estrutura de pastas
COPY ["GerenciadorDePedidos.API/GerenciadorDePedidos.API.csproj", "GerenciadorDePedidos.API/"]
COPY ["GerenciadorDePedidos.Application/GerenciadorDePedidos.Application.csproj", "GerenciadorDePedidos.Application/"]
COPY ["GerenciadorDePedidos.Core/GerenciadorDePedidos.Core.csproj", "GerenciadorDePedidos.Core/"]
COPY ["GerenciadorDePedidos.Infrastructure/GerenciadorDePedidos.Infrastructure.csproj", "GerenciadorDePedidos.Infrastructure/"]

# Restaura as dependências (via API, que já referencia os outros)
RUN dotnet restore "./GerenciadorDePedidos.API/GerenciadorDePedidos.API.csproj"

# Copia o restante do código da solution
COPY . .

# Compila o projeto
WORKDIR "/src/GerenciadorDePedidos.API"
RUN dotnet build "./GerenciadorDePedidos.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publica o projeto
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GerenciadorDePedidos.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Fase final (produção/execução)
FROM base AS final
ENV ASPNETCORE_URLS="http://+:8080"
ENV ASPNETCORE_ENVIRONMENT="Development"

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GerenciadorDePedidos.API.dll"]