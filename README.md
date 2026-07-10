# 🏢 CondoSync API v2.0

**Sistema SaaS multi-tenant para gestão completa de condomínios**

API RESTful em .NET 10 + C# 13 com arquitetura DDD, CQRS e Event Sourcing.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-13.0-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql)](https://www.postgresql.org/)
[![Redis](https://img.shields.io/badge/Redis-7-DC382D?logo=redis)](https://redis.io/)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3.13-FF6600?logo=rabbitmq)](https://www.rabbitmq.com/)
[![Docker](https://img.shields.io/badge/Docker-2496ED?logo=docker)](https://www.docker.com/)

---

## 📋 Índice

- [Visão Geral](#-visão-geral)
- [Stack Tecnológica](#-stack-tecnológica)
- [Arquitetura](#-arquitetura)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [Pré-requisitos](#-pré-requisitos)
- [Instalação e Execução](#-instalação-e-execução)
- [Endpoints da API](#-endpoints-da-api)
- [Autenticação](#-autenticação)
- [Multi-tenancy](#-multi-tenancy)
- [Banco de Dados](#-banco-de-dados)
- [Testes](#-testes)
- [Deploy (Render)](#-deploy-render)
- [Documentação](#-documentação)
- [Licença](#-licença)

---

## 🔍 Visão Geral

O **CondoSync** é uma plataforma SaaS completa para administração de condomínios. Permite que cada condomínio se cadastre, configure seus serviços, gerencie unidades, moradores, finanças, reservas, chamados e muito mais — tudo em uma plataforma unificada com isolamento completo de dados entre tenants.

### ✨ Funcionalidades Principais

| Módulo | Descrição |
|--------|-----------|
| 🏘️ **Multi-tenancy** | Isolamento completo por condomínio via slug/domínio |
| 👥 **Gestão de Moradores** | Proprietários, locatários, dependentes, veículos e pets |
| 🏠 **Unidades** | Apartamentos, casas, salas comerciais com cadastro em lote |
| 📅 **Reservas** | Salão de festas, churrasqueira, academia com calendário |
| 📢 **Mural de Avisos** | Comunicados com comentários, reações e fixação |
| 🎫 **Tickets/Chamados** | Manutenção com SLA, prioridades e avaliação |
| 💰 **Financeiro** | Taxas condominiais, multas, juros, boletos e PIX |
| 🚗 **Visitantes** | Autorização com QR Code e registro de entrada/saída |
| 📊 **Enquetes** | Votações anônimas ou nominais por unidade |
| 📁 **Documentos** | Atas, regulamentos com versionamento |
| 🔔 **Notificações** | In-app, email e push notification |
| 👑 **Painel Admin** | SuperAdmin global gerencia todos os condomínios |

### 👤 Perfis de Usuário

**Domínio Global (SuperAdmin):**
- **SuperAdmin** — Dono da plataforma SaaS
- **Support** — Suporte técnico com acesso limitado
- **Analyst** — Visualização de métricas

**Domínio Multi-tenant (Condomínio):**
- **Síndico/Admin** — Gestor máximo do condomínio
- **Subsíndico** — Apoio com permissões delegadas
- **Funcionário/Portaria** — Acesso operacional
- **Proprietário** — Dono da unidade
- **Locatário** — Inquilino
- **Morador** — Familiar/dependente

---

## 🛠 Stack Tecnológica

| Tecnologia | Versão | Finalidade |
|------------|--------|------------|
| .NET | 10.0 | Runtime principal |
| C# | 13.0 | Linguagem de programação |
| ASP.NET Core | 10.0 | Framework Web API |
| Entity Framework Core | 10.0 | ORM (escrita) |
| Dapper | 2.x | Micro-ORM (leitura otimizada) |
| PostgreSQL | 16 | Banco de dados relacional |
| Redis | 7.x | Cache distribuído |
| RabbitMQ | 3.13.x | Mensageria assíncrona |
| MinIO | latest | Armazenamento de objetos (S3) |
| DbUp | latest | Migrations versionadas |
| MediatR | 12.x | CQRS / Mediator pattern |
| FluentValidation | 11.x | Validação de entrada |
| AutoMapper | 14.x | Mapeamento objeto-objeto |
| Serilog | 5.x | Logging estruturado |
| Quartz.NET | 3.x | Jobs agendados |
| Scalar | latest | Documentação interativa da API |
| Swashbuckle | 6.6.x | Geração OpenAPI/Swagger |
| xUnit | 2.x | Testes unitários e integração |
| BCrypt.Net | latest | Hash de senhas |

---

## 🏗 Arquitetura

O sistema segue **Clean Architecture** com **Domain-Driven Design (DDD)** e **CQRS**:

```
┌─────────────────────────────────────────────────────────────┐
│                     CondoSync.Api                            │
│           Controllers | Middlewares | Filters | Program.cs    │
├─────────────────────────────────────────────────────────────┤
│                   CondoSync.Application                       │
│    CQRS Commands/Queries | Handlers | DTOs | Services        │
│             MediatR | FluentValidation | AutoMapper           │
├─────────────────────────────────────────────────────────────┤
│                     CondoSync.Core                            │
│     Entities | Value Objects | Enums | Domain Events         │
│             Interfaces | Exceptions | Specifications          │
├─────────────────────────────────────────────────────────────┤
│                  CondoSync.Infrastructure                     │
│     DbContexts | Configurations | Repositories               │
│         PostgreSQL | Redis | RabbitMQ | MinIO | DbUp         │
├─────────────────────────────────────────────────────────────┤
│                   CondoSync.Scheduler                         │
│           Quartz.NET Jobs: cobranças, multas, notificações    │
└─────────────────────────────────────────────────────────────┘
```

### Padrões Implementados

- ✅ **DDD** — Agregados, Entidades, Value Objects, Domain Events
- ✅ **CQRS** — Separação de comandos (escrita) e consultas (leitura)
- ✅ **Repository Pattern** — Abstração de acesso a dados
- ✅ **Unit of Work** — Controle transacional
- ✅ **Outbox Pattern** — Consistência em mensageria
- ✅ **Event Sourcing** — Rastreabilidade financeira (preparado)
- ✅ **Saga Pattern** — Fluxos distribuídos (preparado)
- ✅ **Multi-tenancy** — Shared Database + Row-Level Security
- ✅ **Soft Delete** — Remoção lógica de registros

---

## 📁 Estrutura do Projeto

```
condosync-api/
├── CondoSync.sln
├── docker-compose.yml
├── README.md
├── .gitignore
│
├── src/
│   ├── CondoSync.Api/               # ASP.NET Core Web API
│   │   ├── Controllers/
│   │   │   ├── Admin/                # SuperAdmin (global)
│   │   │   │   ├── AdminAuthController.cs
│   │   │   │   ├── AdminCondominiumsController.cs
│   │   │   │   └── AdminDashboardController.cs
│   │   │   ├── AuthController.cs
│   │   │   ├── UnitsController.cs
│   │   │   ├── ResidentsController.cs
│   │   │   ├── ServicesController.cs
│   │   │   ├── BookingsController.cs
│   │   │   ├── NoticesController.cs
│   │   │   ├── TicketsController.cs
│   │   │   ├── BillsController.cs
│   │   │   ├── VisitorsController.cs
│   │   │   ├── PollsController.cs
│   │   │   └── DashboardController.cs
│   │   ├── Middlewares/
│   │   │   ├── TenantMiddleware.cs
│   │   │   ├── GlobalExceptionMiddleware.cs
│   │   │   └── RequestLoggingMiddleware.cs
│   │   ├── Extensions/
│   │   │   └── ServiceCollectionExtensions.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── CondoSync.Core/               # Domínio
│   │   ├── Entities/                  # 23 entidades
│   │   ├── ValueObjects/              # Email, Money, Address...
│   │   ├── Enums/                     # 20 enums
│   │   ├── Events/                    # Domain Events
│   │   ├── Interfaces/                # IRepository, IUnitOfWork...
│   │   └── Exceptions/                # Domain exceptions
│   │
│   ├── CondoSync.Application/         # Aplicação
│   │   ├── Common/
│   │   │   ├── Interfaces/            # IPasswordHasher, ITokenService
│   │   │   ├── Behaviors/             # MediatR pipeline
│   │   │   ├── Mappings/              # AutoMapper profiles
│   │   │   └── DTOs/                  # PaginatedResult, ErrorResponse
│   │   ├── Features/                  # DTOs por feature
│   │   ├── Services/                  # Auth, Unit, Booking, Bill...
│   │   └── DependencyInjection.cs
│   │
│   ├── CondoSync.Infrastructure/      # Infraestrutura
│   │   ├── Data/
│   │   │   ├── CondoSyncDbContext.cs
│   │   │   ├── AdminDbContext.cs
│   │   │   ├── Configurations/        # EF Core entity configs
│   │   │   └── Migrations/
│   │   │       ├── Scripts/           # SQL versionado (DbUp)
│   │   │       └── Runner.cs
│   │   ├── Repositories/
│   │   ├── Tenant/
│   │   └── DependencyInjection.cs
│   │
│   └── CondoSync.Scheduler/           # Jobs agendados
│       └── Jobs/
│
└── tests/
    ├── CondoSync.Tests.Unit/
    └── CondoSync.Tests.Integration/
```

---

## 📋 Pré-requisitos

### Opção 1: Com Docker (Recomendado)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) 4.x+
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Opção 2: Instalação Local
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL 16](https://www.postgresql.org/download/)
- [Redis 7](https://redis.io/download/)
- [RabbitMQ 3.13](https://www.rabbitmq.com/download.html)

---

## 🚀 Instalação e Execução

### 1. Clonar o Repositório

```bash
git clone https://github.com/seu-usuario/condosync-api.git
cd condosync-api
```

### 2. Configurar Variáveis de Ambiente

```bash
# Copiar arquivo de exemplo
cp .env.example .env

# Editar .env com suas configurações
```

Variáveis obrigatórias no `appsettings.json`:

```json
{
  "Database": {
    "ConnectionString": "Host=localhost;Port=5432;Database=condosync;Username=postgres;Password=sua-senha"
  },
  "Jwt": {
    "SecretKey": "<gerar com script PowerShell>"
  }
}
```

### 3. Gerar Chave JWT

```powershell
# PowerShell
$bytes = New-Object byte[] 64
[Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
$secretKey = [Convert]::ToBase64String($bytes)
Write-Host "Chave JWT: $secretKey"
```

Copie a chave gerada para `Jwt:SecretKey` no `appsettings.json`.

### 4. Iniciar Serviços

**Com Docker:**

```bash
# Iniciar PostgreSQL, Redis, RabbitMQ, MinIO
docker-compose up -d

# Verificar status
docker-compose ps
```

**Sem Docker (instalação local):**

```bash
# Certifique-se de que PostgreSQL, Redis e RabbitMQ estão rodando
# Crie o banco de dados:
psql -U postgres -c "CREATE DATABASE condosync;"
```

### 5. Executar a API

```bash
# Restaurar pacotes
dotnet restore

# Compilar
dotnet build

# Executar (as migrations rodam automaticamente)
cd src/CondoSync.Api
dotnet run
```

A API estará disponível em:

- 🌐 **API:** http://localhost:5000
- 📖 **Scalar (Docs):** http://localhost:5000/scalar/v1
- 📚 **Swagger:** http://localhost:5000/swagger
- ❤️ **Health Check:** http://localhost:5000/health

### 6. Credenciais Iniciais

**SuperAdmin (painel global):**

| Campo | Valor |
|-------|-------|
| Email | `admin@condosync.com.br` |
| Senha | `Admin@123!ChangeMe` |

> ⚠️ Altere a senha em produção!

---

## 📡 Endpoints da API

### Resumo de Endpoints (84 implementados)

| Módulo | Base URL | Endpoints |
|--------|----------|-----------|
| Auth Tenant | `/api/v1/auth` | register, login, refresh, me |
| Auth Admin | `/api/v1/admin/auth` | login, me |
| Condomínios (Admin) | `/api/v1/admin/condominiums` | CRUD + suspend/activate/change-plan |
| Dashboard Admin | `/api/v1/admin/dashboard` | summary, subscriptions, growth, churn |
| Unidades | `/api/v1/{slug}/units` | CRUD + batch + blocks |
| Moradores | `/api/v1/{slug}/residents` | CRUD + by-unit + toggle-access |
| Serviços | `/api/v1/{slug}/services` | CRUD + toggle |
| Reservas | `/api/v1/{slug}/bookings` | CRUD + calendar + approve/reject + checkin/out |
| Avisos | `/api/v1/{slug}/notices` | CRUD + pin + comments + reactions |
| Tickets | `/api/v1/{slug}/tickets` | CRUD + status + messages |
| Financeiro | `/api/v1/{slug}/bills` | CRUD + batch + pay + cancel + reports |
| Visitantes | `/api/v1/{slug}/visitors` | authorize + arrive/depart + cancel |
| Enquetes | `/api/v1/{slug}/polls` | CRUD + open/close + vote + results |
| Dashboard | `/api/v1/{slug}/dashboard` | summary + activity |

### Exemplo de Requisição

```bash
# Registrar novo condomínio
curl -X POST http://localhost:5000/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "condominiumName": "Condomínio Exemplo",
    "condominiumSlug": "exemplo",
    "adminName": "Síndico Silva",
    "adminEmail": "sindico@exemplo.com",
    "password": "Senha@123"
  }'

# Login
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "sindico@exemplo.com",
    "password": "Senha@123"
  }'

# Listar unidades (com token JWT)
curl http://localhost:5000/api/v1/exemplo/units \
  -H "Authorization: Bearer {seu-token}"
```

---

## 🔐 Autenticação

O sistema possui duas estratégias JWT independentes:

### JWT Multi-tenant (Condomínio)

| Propriedade | Valor |
|-------------|-------|
| Issuer | `CondoSync` |
| Claims | `userId`, `tenantId`, `role`, `name`, `email` |
| Expiração | 15 minutos (access token) / 7 dias (refresh token) |

### JWT Admin (SuperAdmin)

| Propriedade | Valor |
|-------------|-------|
| Issuer | `CondoSync.Admin` |
| Claims | `adminId`, `role`, `name`, `email` |
| Expiração | 2 horas |
| Rotas | `/api/v1/admin/*` |

### Rate Limiting

| Limite | Configuração |
|--------|-------------|
| Global | 100 requisições/minuto por IP |
| Login | 5 tentativas/minuto por IP |
| Registro | 3 por hora por IP |

---

## 🏢 Multi-tenancy

### Estratégia

**Shared Database, Shared Schema** — PostgreSQL com isolamento por `condominium_id`.

Row-Level Security (RLS) preparado para camada extra de segurança.

### Resolução de Tenant

O middleware identifica o tenant pela ordem:

1. **Slug na URL:** `/api/v1/{slug}/...`
2. **Header:** `X-Tenant-Slug`
3. **Subdomínio:** `{slug}.condosync.app` (futuro)

### Rotas Isentas de Tenant

| Rota | Motivo |
|------|--------|
| `/api/v1/auth/*` | Autenticação pública |
| `/api/v1/admin/*` | Domínio global SuperAdmin |
| `/health`, `/swagger`, `/scalar` | Saúde e documentação |

---

## 🗄 Banco de Dados

### Migrations (DbUp)

As migrations são scripts SQL versionados executados automaticamente na inicialização:

```
src/CondoSync.Infrastructure/Data/Migrations/Scripts/
├── V001_InitialSchema.sql    # Schema completo (23 tabelas)
├── V002_*.sql                # Futuras alterações
└── ...
```

Para criar nova migration:

```bash
# Criar arquivo VXXX_Descricao.sql na pasta Scripts
# A migration será executada automaticamente no próximo startup
```

### Entidades (23 tabelas)

| Schema | Tabela | Descrição |
|--------|--------|-----------|
| admin | super_admins | SuperAdmin (global, sem tenant) |
| public | condominiums | Condomínios |
| public | users | Usuários do condomínio |
| public | units | Unidades |
| public | residents | Moradores |
| public | services | Serviços configuráveis |
| public | bookings | Reservas |
| public | notices | Avisos |
| public | notice_comments | Comentários |
| public | tickets | Chamados |
| public | ticket_messages | Mensagens dos chamados |
| public | bills | Faturas |
| public | visitors | Visitantes |
| public | common_areas | Áreas comuns |
| public | polls | Enquetes |
| public | poll_votes | Votos |
| public | documents | Documentos |
| public | notifications | Notificações |
| public | activity_logs | Logs de atividade |
| public | unit_invitations | Convites |
| public | condominium_settings | Configurações |
| public | outbox_messages | Outbox pattern |
| public | event_store | Event sourcing |

---

## 🧪 Testes

```bash
# Testes unitários
dotnet test tests/CondoSync.Tests.Unit

# Testes de integração
dotnet test tests/CondoSync.Tests.Integration

# Todos os testes
dotnet test
```

---

## 🚢 Deploy (Render)

### Pré-requisitos

- Conta no [Render](https://render.com)
- Repositório GitHub conectado

### Passos para Deploy

1. **Criar PostgreSQL no Render:**
   - Dashboard → New → PostgreSQL
   - Database: `condosync`
   - User: `condosync_user`

2. **Criar Redis no Render:**
   - Dashboard → New → Redis

3. **Criar Web Service:**
   - Dashboard → New → Web Service
   - Conectar repositório GitHub
   - Build Command: `dotnet publish src/CondoSync.Api -c Release -o out`
   - Start Command: `dotnet out/CondoSync.Api.dll`
   - Environment Variables:
     - `Database__ConnectionString` — String do PostgreSQL Render
     - `Redis__ConnectionString` — String do Redis Render
     - `Jwt__SecretKey` — Chave JWT gerada
     - `SuperAdmin__Password` — Senha forte
   - Health Check: `/health`
   - Porta: `5000`

---

## 📖 Documentação

| Ferramenta | URL |
|------------|-----|
| Scalar (Recomendado) | `http://localhost:5000/scalar/v1` |
| Swagger UI | `http://localhost:5000/swagger` |
| Health Check | `http://localhost:5000/health` |
| Liveness | `http://localhost:5000/healthz` |

---

## 🔧 Comandos Úteis

```bash
# Restaurar pacotes
dotnet restore

# Compilar
dotnet build

# Executar API
cd src/CondoSync.Api && dotnet run

# Executar Scheduler (separado)
cd src/CondoSync.Scheduler && dotnet run

# Subir dependências Docker
docker-compose up -d

# Parar dependências Docker
docker-compose down

# Ver logs Docker
docker-compose logs -f

# Criar banco de dados
psql -U postgres -c "CREATE DATABASE condosync;"

# Dropar banco (reiniciar)
psql -U postgres -c "DROP DATABASE condosync; CREATE DATABASE condosync;"

# Testar endpoint health
curl http://localhost:5000/health | jq
```

---

## 📊 Progresso do Projeto

### ✅ Implementado (84 endpoints)

| Status | Módulo |
|--------|--------|
| ✅ | Autenticação Tenant + Admin |
| ✅ | Multi-tenancy (middleware, interceptor, provider) |
| ✅ | CRUD Condomínios (Admin) |
| ✅ | Dashboard Global Admin |
| ✅ | CRUD Unidades + Batch |
| ✅ | CRUD Moradores + Veículos + Pets |
| ✅ | CRUD Serviços Configuráveis |
| ✅ | Reservas + Calendário + Check-in/out |
| ✅ | Mural de Avisos + Comentários + Reações |
| ✅ | Tickets/Chamados + SLA + Mensagens |
| ✅ | Financeiro + Multas + Juros + Relatórios |
| ✅ | Visitantes + QR Code + Entrada/Saída |
| ✅ | Enquetes + Votação + Resultados |
| ✅ | Dashboard do Condomínio |
| ✅ | DbUp Migrations + Seed |
| ✅ | Logging estruturado (Serilog) |
| ✅ | Documentação Scalar + Swagger |
| ✅ | Health Checks |

### 🚧 Em Desenvolvimento

| Status | Funcionalidade |
|--------|---------------|
| 🔄 | Documentos do condomínio |
| 🔄 | Notificações (email/push/in-app) |
| 🔄 | Schedulers (Quartz.NET) |
| 🔄 | Testes automatizados |
| 🔄 | Deploy Render |

### 📅 Roadmap Futuro

- Notificações push (Firebase/OneSignal)
- Integração com gateways de pagamento (Mercado Pago, Stripe)
- Aplicativo mobile (React Native)
- Frontend web (React + Next.js)
- White-label (domínio customizado por condomínio)
- Relatórios avançados (PDF/Excel)
- Integração com portaria eletrônica
- API Pública para integrações
- Kubernetes (Helm Charts)

---

## 🤝 Contribuindo

1. Fork o repositório
2. Crie uma branch: `git checkout -b feature/nova-funcionalidade`
3. Commit suas mudanças: `git commit -m 'feat: adiciona nova funcionalidade'`
4. Push para a branch: `git push origin feature/nova-funcionalidade`
5. Abra um Pull Request

### Padrão de Commits (Conventional Commits)

```
feat: nova funcionalidade
fix: correção de bug
docs: documentação
refactor: refatoração de código
test: adiciona/ajusta testes
chore: tarefas de build/config
```

---

## 📄 Licença

Este projeto está licenciado sob a licença MIT — veja o arquivo `LICENSE` para detalhes.

---

## 👨‍💻 Autor

**CondoSync API v2.0**

Desenvolvido com ❤️ usando .NET 10 + C# 13 + PostgreSQL

⭐ Se este projeto te ajudou, considere dar uma estrela no GitHub!

---

*Última atualização: Julho 2026*
