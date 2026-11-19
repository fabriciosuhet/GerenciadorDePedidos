# Contexto e Regras do Projeto: Gerenciador de Pedidos

## Idioma e Comunicação
- Responda **sempre em Português do Brasil (pt-BR)**, incluindo explicações, planos de execução e mensagens de commit.
- Mantenha nomes de classes, métodos, propriedades e variáveis no código em **Inglês** (ou ajuste para Português se o seu domínio atual já estiver em pt-BR).

## Stack Tecnológica e Infraestrutura
- **Framework:** .NET 8 (C# 12)
- **Arquitetura:** DDD (Domain-Driven Design) + CQRS com **MediatR**
- **Persistência:** Entity Framework Core (EF Core)
- **Containerização:** Docker, `Dockerfile` e `docker-compose.yml` para o banco de dados e serviços.
- **Nota sobre Docker:** A imagem/container do banco de dados ainda não foi criada ou subida recentemente. Antes de rodar migrations ou testes de integração que dependam do banco, verifique se o container está ativo ou oriente/execute o `docker-compose up -d`.

## Diretrizes de Arquitetura (DDD + CQRS)
- **Domain:** Entidades ricas, Value Objects, Aggregates, Domain Events e Interfaces de Repositório. Nunca referencie pacotes de infraestrutura ou EF Core nesta camada.
- **Application:** Commands, Queries, Handlers (via `IRequest` / `IRequestHandler` do MediatR), Validators (FluentValidation, se houver) e DTOs.
- **Infrastructure:** Implementação do `DbContext`, Mappings (Fluent API), Repositórios concretos e Migrations do EF Core.
- **API / Presentation:** Controllers ou Minimal APIs enxutos, apenas despachando Commands/Queries via `IMediator` e retornando respostas HTTP padronizadas.

## Padrões de Código C#
- Utilize **Primary Constructors** (construtores primários do C# 12) para injeção de dependência sempre que possível.
- Prefira `records` para Commands, Queries e DTOs imutáveis.
- Use programação assíncrona (`async`/`await`) propagando o `CancellationToken` em todas as chamadas de banco e handlers.
- Nunca exponha entidades de domínio diretamente nos endpoints da API; mapeie sempre para DTOs de resposta.

## Modo de Execução e Autonomia
- Aplique edições de arquivos, criações de classes e refatorações diretamente sem pedir confirmação passo a passo, mantendo o código compilável.
- Sempre que criar ou alterar código estrutural, valide se a solução compila usando `dotnet build`.
- Antes de iniciar novas alterações, vá sempre para a branch `main`, execute `git pull` e crie uma nova branch baseada na `main`.
- Após concluir a implementação e validar que todos os testes passam, faça o commit detalhado (sem coautor), execute o `git push` da branch e abra o PR para a `main` automaticamente (ou forneça o link direto de criação do PR caso o `gh` CLI não esteja disponível), sem perguntar se deve fazê-lo.
