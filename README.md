# WesNews

Agregador pessoal de notícias tech — busca automaticamente artigos de RSS feeds curados, exibe tudo em uma web app moderna e envia um digest diário por email.

> Feito para desenvolvedores que querem se manter atualizados no mundo .NET, IA, arquitetura e DevOps sem depender de algoritmos de redes sociais.

---

## Todo

- **Role-based Authorization** (Admin/User)
- **Melhorar visual dos cards**
- **Multi linguagem**

---

## Funcionalidades

- **Aggregator automático** — busca feeds RSS em background a cada 30 minutos com resiliência (Polly)
- **Autenticação JWT** — Login seguro e persistente com tokens JWT
- **Registro de Usuários** — Cadastro com identificação única por **Username** e suporte a Roles (Admin/User)
- **Web app** — leitura paginada, filtro por categoria, busca por texto, marcar como lido
- **Digest diário por email** — envia um resumo com os top artigos não lidos de cada categoria (padrão: 7h da manhã)
- **Admin CRUD** — Interface administrativa para gerenciamento completo da tabela de usuários (acesso restrito)
- **Gerenciamento de feeds** — adicione, ative/desative ou remova fontes RSS pela interface
- **API Documentation** — Swagger habilitado em todos os ambientes com suporte a autorização JWT
- **Docker-ready** — sobe tudo com um único `docker compose up`

---

## Stack

| Camada | Tecnologia |
|---|---|
| Backend | .NET 9, ASP.NET Core, EF Core, SQLite |
| Segurança | JWT Bearer, BCrypt.Net-Next, FluentValidation |
| Resiliência | Microsoft.Extensions.Http.Resilience (Polly v8) |
| Scheduler | Quartz.NET |
| Email | Resend API |
| Frontend | React 18, Vite, TypeScript, Tailwind CSS, Lucide Icons |
| State / Cache | TanStack Query, Zustand (Persist) |
| Testes | xUnit, NSubstitute, FluentAssertions |

---

## Arquitetura

O backend segue **Clean Architecture** com quatro camadas isoladas:

```
src/
├── WesNews.Domain/          # Entidades (User, NewsArticle), enums — zero dependências externas
├── WesNews.Application/     # Interfaces, DTOs, serviços, validators (FluentValidation)
├── WesNews.Infrastructure/  # EF Core, repositórios, persistência, Quartz, RSS parser
└── WesNews.Api/             # Controllers REST, Exception Handling, DI root, Swagger
```

---

## Rodando localmente

### Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 22+](https://nodejs.org/)
- Conta no [Resend](https://resend.com) (opcional para digest)

### 1. Backend

```bash
cd src/WesNews.Api
dotnet run
```
A API rodará em `http://localhost:5031`. O Swagger está disponível em `/swagger`.

### 2. Frontend

```bash
cd src/wes-news-web
npm install
npm run dev
```

---

## TODO / Roadmap 🚀

- [ ] **Multi-user Feeds**: Permitir que cada usuário customize sua própria lista de fontes RSS partindo de um conjunto padrão.
- [ ] **UI Refinement**: Melhorar o visual da lista de notícias para uma experiência de leitura mais premium e fluida.
- [ ] **Social Sharing**: Opção para compartilhar artigos interessantes diretamente.
- [ ] **Push Notifications**: Alertas para notícias críticas de fontes específicas.

---

## CI/CD

Pipeline GitHub Actions configurado para:
- Build e Testes (.NET)
- Type-checking e Build (React)
- Validação de Integração (32+ testes passando)

---

## Licença

MIT
