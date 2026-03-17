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

## AI Picks — Curadoria Inteligente com Gemini

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

### Trigger manual

```bash
curl -X POST https://seudominio.com/api/curator/run \
  -H "X-Curator-Secret: SEU_SECRET"
```

### Free Tier do Gemini 2.5 Flash

| Limite | Valor |
|--------|-------|
| Requests por minuto (RPM) | 5 |
| Requests por dia (RPD) | 20 |
| **Uso do WesNews** | **12 req/dia** (6 categorias x 2 runs) |

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
| Infra | Oracle Cloud (ARM A1), Docker, Caddy, Cloudflare |
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

### Deploy (Oracle Cloud)

```
Cloudflare DNS → Caddy (:443/:80)
                    ├── /api/* → wes-news-api (:5000)
                    └── /*     → wes-news-web (:80)
                                    └── SQLite (/data/wesnews.db)
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

### 3. Docker (dev local)

```bash
docker compose up -d
```

---

## Deploy em Produção (Oracle Cloud)

### 1. Pré-requisitos

- VM Oracle Cloud **Always Free** ARM A1 (shape `VM.Standard.A1.Flex`)
- Docker e Docker Compose instalados na VM
- Domínio próprio com DNS apontando via Cloudflare para o IP público da VM
- Portas **80, 443, 22** abertas na Security List da OCI

### 2. Setup inicial na VM

```bash
# Clonar os arquivos de deploy
sudo mkdir -p /opt/wes-news
cd /opt/wes-news

# Copiar do repo: docker-compose.prod.yml, Caddyfile, deploy.sh
# Ou usar scp/git clone

# Criar .env com suas variáveis (copie o .env.example como base)
cp .env.example .env
nano .env  # preencher com valores reais

# Dar permissão ao script
chmod +x deploy.sh

# Primeiro deploy
./deploy.sh
```

### 3. Variáveis de ambiente (.env)

| Variável | Descrição | Obrigatória |
|----------|-----------|-------------|
| `DOMAIN` | Domínio (ex: `wesnews.seudominio.com`) | Sim |
| `JWT_SECRET` | Chave secreta JWT (min. 32 chars) | Sim |
| `GEMINI_APIKEY` | API Key do Google Gemini (AI Picks) | Nao |
| `CURATOR_SECRET` | Secret para trigger manual do curator | Nao |
| `RESEND_APITOKEN` | API Key do Resend (digest) | Nao |
| `DIGEST_EMAIL_TO` | Email destinatario do digest | Nao |
| `DIGEST_EMAIL_FROM` | Email remetente | Nao |

### 4. Deploy automatico (CI/CD)

Cada push no `main` dispara o GitHub Actions que:
1. Builda e pusha imagens Docker para GHCR
2. Faz SSH na VM Oracle e executa `deploy.sh`

**GitHub Secrets necessarios:**

| Secret | Valor |
|--------|-------|
| `ORACLE_SSH_HOST` | IP publico da VM |
| `ORACLE_SSH_USER` | Usuario SSH (ex: `ubuntu`) |
| `ORACLE_SSH_KEY` | Chave SSH privada |

### 5. HTTPS

O **Caddy** gera certificados Let's Encrypt automaticamente. Basta que:
- O dominio aponte para o IP da VM (registro A no Cloudflare, modo DNS only/cinza)
- As portas 80 e 443 estejam abertas

---

## CI/CD

Pipeline GitHub Actions:
- **CI** (`ci.yml`): Build + Testes (.NET), Type-check + Build (React)
- **Release** (`release.yml`): Build Docker images → Push GHCR → Deploy SSH na VM Oracle

---

## TODO / Roadmap

- [ ] **Multi-user Feeds**: Permitir que cada usuario customize sua propria lista de fontes RSS
- [ ] **Role-based Authorization**: Granularidade de permissoes Admin/User
- [ ] **Social Sharing**: Opcao para compartilhar artigos interessantes
- [ ] **Push Notifications**: Alertas para noticias criticas de fontes especificas
- [ ] **Multi-idioma**: Suporte a interface em ingles

---

## Licenca

MIT
