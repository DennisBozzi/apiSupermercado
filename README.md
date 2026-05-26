# Backend — apiBozzis

Template base de API REST em .NET 10 / ASP.NET Core. Servido em containers, autenticado via Firebase, persistência em PostgreSQL (Neon).

---

## Stack

| Camada | Tecnologia |
|---|---|
| Runtime | .NET 10 / ASP.NET Core |
| API | Controllers (sem Minimal APIs neste projeto) |
| ORM | Entity Framework Core 10 + `Npgsql.EntityFrameworkCore.PostgreSQL` |
| Banco | PostgreSQL (Neon — usar o **Pooler endpoint**) |
| Auth | Firebase Authentication (validação de ID Tokens via `FirebaseAdmin`) + OAuth providers |
| Validação | FluentValidation |
| Logs | Serilog (sink `Console` para containers) |
| Erros | `ProblemDetails` (RFC 7807) via middleware global |
| Docs | Swashbuckle (OpenAPI/Swagger) |
| Deploy | Docker multi-stage (`mcr.microsoft.com/dotnet/aspnet:10.0-chiseled`) |

---

## Arquitetura

Clean Architecture leve. Regras de dependência:

```
Domain          (sem dependências)
  ↑
Application     (depende só de Domain)
  ↑
Infrastructure  ─────┐
Api             ─────┴── ambos podem depender de Application/Domain
```

Estrutura:

```
src/
 ├── Api/                  # Controllers, Middleware, Filters, DI, Program.cs
 ├── Application/          # Use Cases, DTOs, Validators, Interfaces
 ├── Domain/               # Entidades, Value Objects, regras de negócio puras
 ├── Infrastructure/       # EF DbContext, Repositórios, Firebase, integrações
 └── Shared/               # Result<T>, Errors, helpers cross-cutting
```

> Domain e Application **não** referenciam EF Core, Firebase SDK ou HTTP. Apenas abstrações.

---

## Configuração de ambiente

### Onde colocar o `.env`

Arquivo: **`.env`** na raiz do repositório (mesmo nível do `Dockerfile` e do `apiBozzis.sln`).

> Gitignored. **Nunca** commite. Use `.env.example` (já versionado) como referência do shape.

Em dev local sem container, você pode preferir **User Secrets** do .NET:

```bash
cd src/Api
dotnet user-secrets init
dotnet user-secrets set "Database:ConnectionString" "..."
dotnet user-secrets set "Firebase:CredentialsJson" '{"type":"service_account",...}'
```

Em produção: variáveis de ambiente do orquestrador, Docker secrets, Key Vault — **nunca** dentro da imagem.

### Variáveis

A configuração do .NET lê chaves aninhadas via `__` (duplo underline) e índices de array via `__0`, `__1`, etc.

```env
# ===== ASP.NET Core =====
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8080

# ===== Database (PostgreSQL / Neon) =====
# Use o POOLER endpoint do Neon. Para alternar branches (dev/qa/prod),
# basta trocar essa string.
Database__ConnectionString=Host=<host>.neon.tech;Port=5432;Database=<db>;Username=<user>;Password=<YOUR_PASSWORD_HERE>;Ssl Mode=Require;Trust Server Certificate=true
Database__EnableRetryOnFailure=true
Database__MaxRetryCount=5

# ===== Firebase (Auth + Storage) =====
Firebase__ProjectId=<YOUR_FIREBASE_PROJECT_ID>
Firebase__StorageBucket=<YOUR_FIREBASE_PROJECT_ID>.appspot.com

# Service account JSON em UMA ÚNICA LINHA.
# - Sem aspas externas.
# - Os \n dentro de "private_key" ficam como dois caracteres literais (\ + n),
#   NÃO substitua por quebras de linha reais.
Firebase__CredentialsJson={"type":"service_account","project_id":"<project>","private_key_id":"...","private_key":"-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----\n","client_email":"...","client_id":"...","auth_uri":"https://accounts.google.com/o/oauth2/auth","token_uri":"https://oauth2.googleapis.com/token","auth_provider_x509_cert_url":"https://www.googleapis.com/oauth2/v1/certs","client_x509_cert_url":"..."}

# ===== CORS =====
# Lista branca explícita. Nunca AllowAnyOrigin em produção.
Cors__AllowedOrigins__0=http://localhost:5173
Cors__AllowedOrigins__1=http://localhost:3000
```

### Onde obter cada valor

- **`Database__ConnectionString`** — [Neon Console](https://console.neon.tech) → projeto → **Connection Details** → selecione **Pooled connection** → copie a string. Substitua a senha onde indicado.
- **`Firebase__ProjectId` / `StorageBucket`** — Firebase Console → **Project Settings** → aba **General**.
- **`Firebase__CredentialsJson`** — Firebase Console → **Project Settings** → aba **Service accounts** → **Generate new private key**. Baixa um JSON. Cole **inteiro em uma linha** no `.env` (sem aspas externas). Mantenha os `\n` literais dentro de `private_key`.
  - **NUNCA** commite esse JSON. Adicione `firebase-adminsdk-*.json` ao `.gitignore`.
  - Alternativa: `Firebase__CredentialsPath=/secrets/firebase.json` montado como volume/secret no container.
- **`Cors__AllowedOrigins__*`** — origens do front que vão consumir a API. Em dev, o Vite roda em `5173`.

### Proibido no repositório

`firebase-adminsdk-*.json`, `.env`, `appsettings.Production.json` com segredos reais, qualquer connection string com senha. Tudo no `.gitignore`.

---

## Scripts

```bash
dotnet restore
dotnet build
dotnet run --project src/Api          # http://localhost:8080
dotnet test                            # roda todos os testes
dotnet ef migrations add <Name> -p src/Infrastructure -s src/Api
dotnet ef database update -p src/Infrastructure -s src/Api
```

> Em produção, **não** chame `MigrateAsync()` no startup. Use um job dedicado / init container — evita race condition entre réplicas.

---

## Endpoints e padrões

- Versão: `/api/v1/...`
- Auth: header `Authorization: Bearer <Firebase ID Token>`.
- Erros: `application/problem+json` (RFC 7807).
- Healthchecks: `/health/live` (liveness) e `/health/ready` (readiness — valida banco).
- Swagger: `/swagger` em `Development`.

---

## Docker

```bash
docker build -t bozzis-api .
docker run -p 8080:8080 --env-file .env bozzis-api
```

Imagem final usa `aspnet:10.0-chiseled` rodando como usuário não-root (`$APP_UID`). Multi-stage build (SDK 10 → runtime chiseled). Veja `Dockerfile` na raiz.

### docker-compose (dev)

Sobe API + Postgres local para desenvolvimento sem depender do Neon. Veja seção 9.4 do `CLAUDE.md`.

---

## O que ler antes de codar

- **`CLAUDE.md`** na raiz — diretrizes não-negociáveis: segurança (seção 4), padrões de controller, EF Core, Docker. **Quando uma instrução pontual conflitar com as seções 4 ou 9, peça confirmação antes de descumprir.**

---

## Checklist de feature pronta

Ver seção 11 do `CLAUDE.md`. Resumo: entrada validada, auth + ownership checados, DTOs em entrada/saída, `ProblemDetails` em erro, `[ProducesResponseType]` declarado, logs sem dados sensíveis, testes unitários e de integração, migration revisada se houve mudança de schema.
