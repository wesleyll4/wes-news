# WesNews

Agregador pessoal de notícias tech — busca automaticamente artigos de RSS feeds curados, exibe tudo em uma web app moderna e envia um digest diário por email.

> Feito para desenvolvedores que querem se manter atualizados no mundo .NET, IA, arquitetura e DevOps sem depender de algoritmos de redes sociais.

---

## TODO's
- **Sistema de login
- **Melhorar visual dos cards

---

---

## Funcionalidades

- **Aggregator automático** — busca feeds RSS em background a cada 30 minutos
- **Web app** — leitura paginada, filtro por categoria, busca por texto, marcar como lido
- **Digest diário por email** — envia um resumo com os top artigos não lidos de cada categoria (padrão: 7h da manhã)
- **Gerenciamento de feeds** — adicione, ative/desative ou remova fontes RSS pela interface
- **22 feeds pré-configurados** — .NET, IA, Arquitetura, DevOps e General
- **Docker-ready** — sobe tudo com um único `docker compose up`

---

## Stack

| Camada | Tecnologia |
|---|---|
| Backend | .NET 9, ASP.NET Core, EF Core, SQLite |
| Scheduler | Quartz.NET |
| Email | Resend API |
| Frontend | React 18, Vite, TypeScript, Tailwind CSS |
| State / Cache | TanStack Query, Zustand |
| Testes | xUnit, NSubstitute, FluentAssertions |
| Container | Docker, nginx |
| CI/CD | GitHub Actions |

---

## Arquitetura

O backend segue **Clean Architecture** com quatro camadas isoladas:

```
src/
├── WesNews.Domain/          # Entidades, enums — zero dependências externas
├── WesNews.Application/     # Interfaces, DTOs, serviços de aplicação
├── WesNews.Infrastructure/  # EF Core, repositórios, Quartz, Resend, RSS parser
└── WesNews.Api/             # Controllers REST, DI root, Swagger
```

### Fluxo de dados

```
[Quartz Job: 7h]
      │
      ▼
DigestSchedulerJob → DigestService → DigestEmailService (Resend)

[Background Service: 30min]
      │
      ▼
BackgroundFetchService → FeedAggregatorService → RSS Parser → NewsArticleRepository

[HTTP Request]
      │
      ▼
Controller → Application Service → Repository → SQLite (EF Core)
```

---

## Feeds pré-configurados

| Categoria | Feeds |
|---|---|
| .NET | .NET Blog, Scott Hanselman, Andrew Lock, Khalid Abuhakmeh, Steven Giesel |
| AI | OpenAI Blog, Google AI Blog, HuggingFace, Microsoft AI, DeepLearning.AI |
| Architecture | Martin Fowler, InfoQ Architecture, Pragmatic Engineer |
| DevOps | GitHub Blog, Azure Blog, Kubernetes Blog |
| General | Hacker News, The New Stack, Stack Overflow Blog, InfoQ |

---

## Rodando localmente

### Pré-requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- Conta gratuita no [Resend](https://resend.com) (100 emails/dia no free tier)

### 1. Clone o repositório

```bash
git clone https://github.com/wesleyll4/wes-news.git
cd wes-news
```

### 2. Configure as variáveis de ambiente

```bash
cp .env.example .env
```

Edite o `.env`:

```env
RESEND_APITOKEN=re_sua_chave_aqui
DIGEST_EMAIL_TO=seu@email.com
DIGEST_EMAIL_FROM=digest@wesnews.app
DIGEST_CRON=0 0 7 * * ?
```

> O `DIGEST_CRON` usa o formato Quartz: `segundo minuto hora dia mês dia-da-semana`.
> Exemplo: `0 30 8 * * ?` = todo dia às 8h30.

### 3. Suba os containers

```bash
docker compose up -d
```

| Serviço | URL |
|---|---|
| Web App | http://localhost:3000 |
| API | http://localhost:5000 |
| Swagger | http://localhost:5000/swagger |

### 4. Pare os containers

```bash
docker compose down
```

Os dados do SQLite ficam no volume `wesnews_data` e são preservados entre reinicializações.

---

## Rodando em modo desenvolvimento

### Backend (.NET)

**Pré-requisitos:** [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

```bash
# Restaurar dependências
dotnet restore

# Rodar a API (porta 5000)
dotnet run --project src/WesNews.Api
```

A API sobe com Swagger em `http://localhost:5000/swagger`.

Configurações de desenvolvimento ficam em `src/WesNews.Api/appsettings.Development.json`.
Para configurar email em dev, edite `appsettings.json`:

```json
{
  "RESEND_APITOKEN": "re_sua_chave",
  "DigestEmail": {
    "ToEmail": "seu@email.com"
  }
}
```

### Frontend (React + Vite)

**Pré-requisitos:** [Node.js 22+](https://nodejs.org/)

```bash
cd src/wes-news-web

npm install
npm run dev
```

O Vite sobe em `http://localhost:5173` com proxy `/api` apontando para `http://localhost:5000`.

---

## API Reference

### Notícias — `GET /api/news`

Retorna artigos paginados com filtros opcionais.

| Query Param | Tipo | Descrição |
|---|---|---|
| `page` | int | Página (padrão: 1) |
| `pageSize` | int | Itens por página (padrão: 20) |
| `category` | string | Filtro: `DotNet`, `AI`, `Architecture`, `DevOps`, `General` |
| `search` | string | Busca no título e resumo |
| `unreadOnly` | bool | Apenas não lidos |

**Response:**
```json
{
  "items": [
    {
      "id": "uuid",
      "title": "string",
      "summary": "string",
      "url": "string",
      "publishedAt": "2025-01-01T07:00:00Z",
      "category": "DotNet",
      "feedName": "string",
      "isRead": false
    }
  ],
  "totalCount": 100,
  "page": 1,
  "pageSize": 20,
  "totalPages": 5
}
```

---

### `PATCH /api/news/{id}/read`

Marca um artigo como lido.

| Resposta | Significado |
|---|---|
| `204 No Content` | Sucesso |
| `404 Not Found` | Artigo não encontrado |

---

### `DELETE /api/news/{id}`

Remove um artigo.

| Resposta | Significado |
|---|---|
| `204 No Content` | Sucesso |

---

### Feeds — `GET /api/feeds`

Lista todas as fontes RSS cadastradas.

```json
[
  {
    "id": "uuid",
    "name": "string",
    "url": "string",
    "category": "DotNet",
    "isActive": true
  }
]
```

---

### `POST /api/feeds`

Adiciona uma nova fonte RSS.

```json
{
  "name": "Meu Blog",
  "url": "https://meublog.com/feed.xml",
  "category": "DotNet"
}
```

| Resposta | Significado |
|---|---|
| `201 Created` | Feed criado |
| `400 Bad Request` | Dados inválidos |
| `409 Conflict` | URL já cadastrada |

---

### `PUT /api/feeds/{id}`

Atualiza nome, URL, categoria ou estado ativo de um feed.

---

### `DELETE /api/feeds/{id}`

Remove um feed e todos os artigos associados.

---

### Digest — `GET /api/digest/preview`

Retorna o HTML do email de digest com os artigos não lidos mais recentes.

---

### `POST /api/digest/send`

Dispara o envio do digest imediatamente (sem aguardar o agendamento).

| Resposta | Significado |
|---|---|
| `202 Accepted` | Email sendo enviado |

---

## Testes

```bash
# Todos os testes
dotnet test

# Apenas unitários
dotnet test tests/WesNews.UnitTests

# Apenas integração
dotnet test tests/WesNews.IntegrationTests
```

**Cobertura atual:** 30 testes — 16 unitários + 14 de integração — 100% passando.

| Projeto | Framework | Mocks |
|---|---|---|
| `WesNews.UnitTests` | xUnit | NSubstitute |
| `WesNews.IntegrationTests` | xUnit + WebApplicationFactory | SQLite in-memory |

Os testes de integração sobem a aplicação completa com banco isolado por teste — sem efeitos colaterais entre execuções.

---

## CI/CD

O pipeline GitHub Actions executa em todo push:

```
push → backend job          frontend job
         │                      │
         ├─ dotnet restore       ├─ npm ci
         ├─ dotnet build         ├─ tsc --noEmit
         ├─ unit tests           └─ vite build
         └─ integration tests
```

O workflow de release (`release.yml`) é acionado ao criar uma tag `v*` e pode ser estendido para build e push de imagens Docker.

---

## Variáveis de ambiente

| Variável | Obrigatória | Descrição |
|---|---|---|
| `RESEND_APITOKEN` | Sim | Token da API do Resend |
| `DIGEST_EMAIL_TO` | Sim | Email de destino do digest |
| `DIGEST_EMAIL_FROM` | Não | Remetente (padrão: `digest@wesnews.app`) |
| `DIGEST_CRON` | Não | Cron Quartz do envio (padrão: `0 0 7 * * ?`) |
| `ConnectionStrings__DefaultConnection` | Não | Path do SQLite (padrão: `wesnews.db`) |

---

## Estrutura do projeto

```
wes-news/
├── .github/
│   └── workflows/
│       ├── ci.yml
│       └── release.yml
├── src/
│   ├── WesNews.Domain/
│   │   ├── Entities/
│   │   │   ├── NewsArticle.cs
│   │   │   └── FeedSource.cs
│   │   └── Enums/
│   │       └── Category.cs
│   ├── WesNews.Application/
│   │   ├── DTOs/
│   │   ├── Interfaces/
│   │   │   ├── Repositories/
│   │   │   └── Services/
│   │   └── Services/
│   │       ├── NewsService.cs
│   │       ├── FeedService.cs
│   │       └── DigestService.cs
│   ├── WesNews.Infrastructure/
│   │   ├── BackgroundServices/
│   │   ├── Data/
│   │   ├── Repositories/
│   │   ├── Seed/
│   │   └── Services/
│   ├── WesNews.Api/
│   │   ├── Controllers/
│   │   ├── Program.cs
│   │   └── Dockerfile
│   └── wes-news-web/
│       ├── src/
│       │   ├── components/
│       │   ├── pages/
│       │   ├── api/
│       │   └── store/
│       ├── Dockerfile
│       └── nginx.conf
├── tests/
│   ├── WesNews.UnitTests/
│   │   ├── Services/
│   │   └── Helpers/
│   │       └── FakeData.cs
│   └── WesNews.IntegrationTests/
│       ├── Controllers/
│       ├── CustomWebAppFactory.cs
│       └── Helpers/
│           └── TestDbSeeder.cs
├── docker-compose.yml
├── .env.example
└── WesNews.slnx
```

---

## Licença

MIT
