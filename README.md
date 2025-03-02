# Gerenciador de Pedidos

![.NET](https://img.shields.io/badge/.NET-8.0-blueviolet) ![MySQL](https://img.shields.io/badge/MySQL-docker:latest-blue) ![Docker](https://img.shields.io/badge/Docker-Supported-brightgreen)

Bem-vindo ao **Gerenciador de Pedidos**, uma aplicação backend robusta desenvolvida em **.NET 8** para cadastro, consulta e gerenciamento de pedidos. Este projeto utiliza uma arquitetura moderna baseada em **DDD (Domain-Driven Design)** e **CQRS (Command Query Responsibility Segregation)**, seguindo os princípios **SOLID** para garantir um código limpo, escalável e manutenível.

## Funcionalidades
- Cadastro e gerenciamento de pedidos.
- Autenticação segura com **JWT (JSON Web Token)**.
- API RESTful com endpoints para operações CRUD.
- Banco de dados **MySQL** configurado via **Docker**.

## Tecnologias Utilizadas
- **.NET 8**: Framework principal para construção da aplicação.
- **DDD**: Abordagem orientada a domínio para modelagem do negócio.
- **CQRS**: Separação de responsabilidades entre comandos e consultas.
- **Entity Framework Core**: ORM para interação com o banco de dados.
- **MySQL**: Banco de dados relacional rodando em container Docker.
- **Injeção de Dependência (DI)**: Gerenciamento de dependências nativo do .NET.
- **Princípios SOLID**: Aplicados para garantir boas práticas de design.
- **JWT**: Autenticação baseada em tokens.

## Pré-requisitos
Para rodar o projeto localmente, você precisará de:
- [Docker](https://www.docker.com/get-started/)
- [Git](https://git-scm.com/downloads)

## Como Rodar o Projeto
Siga os passos abaixo para configurar e executar a aplicação:

1. **Clone o repositório:**
   ```bash
   git clone https://github.com/fabriciosuhet/GerenciadorDePedidos.git
   cd GerenciadorDePedidos

### A FAZER AINDA ⚠️

2. ⚠️**Construa e inicie os containers com Docker Compose O docker-compose irá criar os containers para o MySQL e a aplicação.**
   ```bash
   docker-compose up --build

  **Esse comando irá:**
  - Construir a aplicação e o container do MySQL.
  - Iniciar os containers e configurar a aplicação para rodar.

3. ⚠️**Acesse a aplicação** Após o Docker terminar de subir os containers, você poderá acessar a API na seguinte URL:
   ```bash
   http://localhost:8080
   
- A API estará disponível e pronta para ser consumida.

4. ⚠️**Realize as migrações (Se aplicável)** Caso o seu banco de dados precise de migrações, você pode executar o seguinte comando para aplicar as migrações:
   ```bash
   docker exec -it gerenciadordepedidos_api dotnet ef database update
   

### ⚠️Como Parar os Containers
  ```bash
  docker-compose-down
```

### Diagrama Banco de Dados
![Diagrama banco de dados](https://i.imgur.com/6TiOzMV.png)

### Documentação API Swagger
![Imagem da documentação no Swagger](https://i.imgur.com/3eE2ubm.png)
