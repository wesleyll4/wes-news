# WesNews: Core Feed

Agregador pessoal de notícias tech — busca automaticamente artigos de RSS feeds curados, exibe tudo em uma interface premium **Aero** com glassmorphism e envia um digest diário por email.

> Feito para desenvolvedores que querem se manter atualizados no mundo .NET, IA, arquitetura e DevOps com uma experiência de leitura fluida e moderna.

---

## Funcionalidades

- **Aggregator automático** — busca feeds RSS em background a cada 30 minutos com resiliência (Polly)
- **Autenticação JWT** — Login seguro e persistente com tokens JWT
- **Registro de Usuários** — Cadastro com confirmação de senha, validação em tempo real e toggle de visibilidade
- **Web app** — leitura paginada, filtro por categoria, busca por texto, marcar como lido
- **Digest diário por email** — envia um resumo com os top artigos não lidos de cada categoria (padrão: 7h da manhã)
- **Admin CRUD** — Interface administrativa para gerenciamento completo da tabela de usuários (acesso restrito)
- **Gerenciamento de feeds** — adicione, ative/desative ou remova fontes RSS pela interface
- **GitHub Trends** — mostra os repositórios em alta no GitHub (geral, .NET e segurança)
- **API Documentation** — Swagger habilitado em todos os ambientes com suporte a autorização JWT
- **Docker-ready** — sobe tudo com um único `docker compose up`

---

## ✨ AI Picks — Curadoria Inteligente com Gemini

Um agente de IA powered by **Google Gemini 2.5 Flash** roda automaticamente 2x ao dia (6h e 14h) e seleciona as **3 notícias mais relevantes de cada categoria** com base em:

- **Impacto técnico** — mudanças de paradigma, lançamentos importantes, breaking changes
- **Viralidade** — o que a comunidade dev está discutindo agora

### Como funciona

```
Quartz Job (6h / 14h)
    └── CuratorSchedulerJob
            └── IAiCuratorService.CurateAsync()
                    ├── Busca até 30 artigos recentes por categoria (últimas 48h)
                    ├── Envia para Gemini com prompt especializado
                    ├── Recebe JSON com os 3 GUIDs mais relevantes
                    └── Marca artigos como IsFeatured = true no banco
```

Os artigos escolhidos aparecem no topo de cada categoria com o badge **"AI Pick"** e destaque visual em gradiente indigo/violet.

### Configuração da API Key (Gemini)

1. Acesse [Google AI Studio](https://aistudio.google.com/apikey) e gere uma API Key gratuita
2. Configure a variável de ambiente:

```bash
# .env local ou variável no Render/Railway
Gemini__ApiKey=SUA_API_KEY_AQUI
```

Ou via `appsettings.json` (apenas em desenvolvimento local, **nunca commitar**):

```json
"Gemini": {
  "ApiKey": "SUA_API_KEY_AQUI"
}
```

### Free Tier do Gemini 2.5 Flash

| Limite | Valor |
|--------|-------|
| Requests por minuto (RPM) | 5 |
| Tokens por minuto (TPM) | 250.000 |
| Requests por dia (RPD) | 20 |
| **Uso do WesNews** | **12 req/dia** (6 categorias × 2 runs) |

O sistema respeita o limite de 5 RPM com um delay de **15 segundos entre categorias**.

### Trigger manual

Para forçar uma curação imediata sem esperar o scheduler:

```bash
curl -X POST https://sua-api/api/curator/run \
  -H "Authorization: Bearer SEU_JWT_TOKEN"
```

---

## Stack

| Camada | Tecnologia |
|---|---|
| Backend | .NET 9, ASP.NET Core, EF Core, SQLite |
| Segurança | JWT Bearer, BCrypt.Net-Next, FluentValidation |
| Resiliência | Microsoft.Extensions.Http.Resilience (Polly v8) |
| Scheduler | Quartz.NET |
| Email | Resend API |
| AI Curator | Google Gemini 2.5 Flash (free tier) |
| Frontend | React 18, Vite, TypeScript, Tailwind CSS, Framer Motion, Lucide Icons |
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
- API Key do [Google AI Studio](https://aistudio.google.com/apikey) (opcional para AI Picks)

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

### 3. Docker (recomendado)

```bash
docker compose up -d
```

---

## Variáveis de Ambiente

| Variável | Descrição | Obrigatória |
|----------|-----------|-------------|
| `Jwt__Secret` | Chave secreta JWT (mín. 32 chars) | Sim |
| `Gemini__ApiKey` | API Key do Google Gemini (AI Picks) | Não |
| `Resend__ApiKey` | API Key do Resend (digest por email) | Não |
| `Resend__FromEmail` | Email remetente | Não |

---

## CI/CD

Pipeline GitHub Actions configurado para:
- Build e Testes (.NET)
- Type-checking e Build (React)
- Validação de Integração (32+ testes passando)

---

## TODO / Roadmap

- [ ] **Multi-user Feeds**: Permitir que cada usuário customize sua própria lista de fontes RSS
- [ ] **Role-based Authorization**: Granularidade de permissões Admin/User
- [ ] **Social Sharing**: Opção para compartilhar artigos interessantes
- [ ] **Push Notifications**: Alertas para notícias críticas de fontes específicas
- [ ] **Multi-idioma**: Suporte a interface em inglês

---

## Licença

MIT
