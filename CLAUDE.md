# CLAUDE.md

Este arquivo orienta o Claude Code (e outras IAs) ao trabalhar neste repositório. Ele define princípios, padrões e práticas obrigatórias de desenvolvimento. **Estas diretrizes têm precedência sobre soluções "rápidas" ou "convenientes".**

---

## 1. Stack Tecnológica

- **Runtime/Framework:** .NET 10 / ASP.NET Core
- **API:** Controllers (não usar Minimal APIs neste projeto)
- **ORM:** Entity Framework Core
- **Banco de Dados:** PostgreSQL (Neon)
- **Autenticação:** Firebase Authentication (validação de ID Tokens) + OAuth providers
- **Deploy:** Docker (containerizado)
- **Arquitetura:** API REST com Controllers organizados por feature/domínio

---

## 2. Princípios Fundamentais (não-negociáveis)

1. **Nunca confie no frontend.** Todo dado vindo do cliente é hostil até prova em contrário. Toda validação, autorização e regra de negócio acontece no backend.
2. **Falhe de forma segura.** Em caso de dúvida, negue acesso, rejeite a entrada e retorne erro genérico.
3. **Explícito > implícito.** Nada de "mágica" escondida. Convenções devem ser óbvias e documentadas.
4. **YAGNI + KISS.** Não implemente o que não foi pedido. Não adicione abstrações antecipadas.
5. **Código é lido muito mais do que escrito.** Clareza vence concisão.
6. **Se não tem teste, está quebrado.** Lógica de negócio sem teste não existe.

---

## 3. Arquitetura e Organização

### 3.1 Estrutura sugerida (Clean Architecture leve)

```
src/
 ├── Api/                  # Controllers, Middleware, Filters, DI, Program.cs
 ├── Application/          # Use Cases, DTOs, Validators, Interfaces
 ├── Domain/               # Entidades, Value Objects, Regras de negócio puras
 ├── Infrastructure/       # EF DbContext, Repositórios, Firebase, integrações externas
 └── Shared/               # Result<T>, Errors, helpers cross-cutting
tests/
 ├── UnitTests/
 ├── IntegrationTests/
 └── ArchitectureTests/
```

### 3.2 Regras de dependência

- **Domain** não depende de nada.
- **Application** depende apenas de Domain.
- **Infrastructure** e **Api** podem depender de Application/Domain.
- **Nunca** Domain/Application referenciam EF Core, Firebase SDK ou HTTP. Use abstrações (interfaces).

### 3.3 Padrões obrigatórios

- **Controllers finos.** Controller apenas: recebe request, chama use case/serviço, retorna resposta. Sem regra de negócio dentro de controller.
- **Result Pattern** em vez de exceções para fluxo de negócio (`Result<T>` ou `OneOf`). Exceções apenas para casos verdadeiramente excepcionais.
- **CQRS leve** quando fizer sentido (MediatR opcional — só adicione se o volume justificar).
- **DTOs separados** de entidades. Nunca exponha entidades EF diretamente em controllers.
- **FluentValidation** para validação de entrada (registrar com auto-validation no pipeline MVC).

### 3.4 Padrão de Controllers

- Herdar de `ControllerBase` (não de `Controller` — não há views).
- Usar `[ApiController]` para validação automática de ModelState e binding inference.
- Rotas explícitas com `[Route("api/v1/[controller]")]`.
- Um controller por agregado/recurso (ex.: `UsersController`, `OrdersController`).
- Action methods retornam `Task<ActionResult<T>>` ou `Task<IActionResult>`.
- Sempre `async` com `CancellationToken` propagado.
- Use atributos HTTP explícitos: `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpPatch]`, `[HttpDelete]`.

Exemplo de estrutura mínima:

```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService) => _orderService = orderService;

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetById(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId(); // extraído do token, NUNCA do body
        var result = await _orderService.GetByIdAsync(id, userId, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToActionResult();
    }
}
```

---

## 4. Segurança (LEIA COM ATENÇÃO)

### 4.1 Princípio Zero: Nunca confie no cliente

- **Toda** validação acontece no servidor, mesmo que o frontend já valide.
- **Toda** autorização é verificada no backend, em cada endpoint, sem exceção.
- IDs vindos do cliente (`userId`, `tenantId`, etc.) **nunca** são fonte de verdade — extraia do token JWT autenticado.
- Nunca permita que o cliente diga "qual é o seu papel" ou "qual usuário você está representando".

### 4.2 Autenticação com Firebase

- Valide o **ID Token** do Firebase no backend usando `FirebaseAdmin.Auth` com verificação de assinatura, issuer, audience e expiração.
- **Nunca** aceite o `uid` enviado no body — extraia sempre das claims do token validado.
- Tokens expirados ou revogados devem retornar `401 Unauthorized` sem detalhes adicionais.
- Implemente verificação de e-mail (`email_verified`) antes de liberar funcionalidades sensíveis.
- Para OAuth, valide o provider e mapeie claims com cuidado — não confie em `email` sem `email_verified`.
- Configure o middleware de autenticação JWT para validar tokens do Firebase contra o JWKS do Google.

### 4.3 Autorização

- Use `[Authorize]` **no controller inteiro por padrão** e `[AllowAnonymous]` por exceção e somente quando intencional (ex.: health check, signup público).
- Implemente autorização baseada em policies/claims, não em strings mágicas espalhadas no código.
- Para multi-tenant: **sempre** filtre queries por `TenantId` extraído do token. Considere `Global Query Filters` no EF Core para garantir isso por padrão.
- Verifique **ownership** de recursos antes de qualquer operação (ex.: `Pedido` pertence ao `UserId` do token?).

### 4.4 Proteção contra ataques comuns

- **SQL Injection:** use sempre LINQ/parâmetros do EF. Nunca interpole strings em SQL bruto. Se precisar de `FromSqlRaw`, use `FromSqlInterpolated`.
- **Mass Assignment:** nunca faça bind direto de request → entidade. Use DTOs específicos por endpoint.
- **IDOR (Insecure Direct Object Reference):** valide ownership em todo recurso acessado por ID.
- **XSS:** use `System.Text.Json` (que escapa por padrão). Nunca retorne HTML montado com input do usuário.
- **CSRF:** APIs stateless com JWT no header `Authorization` estão razoavelmente protegidas, mas evite cookies de autenticação cross-site sem `SameSite=Strict`.
- **Rate limiting:** configure `AddRateLimiter` em endpoints sensíveis (login, signup, reset, endpoints custosos).
- **CORS:** lista branca explícita de origens. Nunca use `AllowAnyOrigin()` em produção.
- **HTTPS:** obrigatório. `UseHsts()` em produção. Redirecionamento forçado.

### 4.5 Segredos e configuração

- **Nenhum** segredo no código ou no Git. Use User Secrets em dev, variáveis de ambiente ou Azure Key Vault / AWS Secrets Manager / Doppler em produção.
- Connection string do Neon, chaves do Firebase, client secrets do OAuth: **sempre** fora do repositório.
- `.env`, `appsettings.Development.json` com segredos, `firebase-adminsdk-*.json`: no `.gitignore`.
- Rode `git secrets` ou similar antes de commits sensíveis.
- Em Docker: segredos via **environment variables** ou **Docker secrets**, nunca via `COPY` para dentro da imagem.

### 4.6 Headers e respostas

- Headers de segurança: `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `Content-Security-Policy` (quando servir conteúdo), `Strict-Transport-Security`.
- **Mensagens de erro genéricas** para o cliente. Detalhes vão para logs, não para a resposta.
- Nunca retorne stack traces em produção.
- Use `ProblemDetails` (RFC 7807) para erros padronizados. Configure via `AddProblemDetails()` + middleware global de exception handling.

### 4.7 Logging e dados sensíveis

- **Nunca** logue: senhas, tokens, ID tokens, refresh tokens, dados de cartão, CPF/CNPJ completos, dados de saúde.
- Logue: `UserId`, `TraceId`, ação, recurso afetado, resultado.
- Use `ILogger<T>` com logging estruturado (Serilog recomendado, com sink para stdout em containers).

---

## 5. Entity Framework Core

### 5.1 Boas práticas

- Use **migrations** versionadas. Toda mudança de schema = migration revisada antes do merge.
- **Nunca** rode `EnsureCreated()` em produção. Use `dbContext.Database.MigrateAsync()` no startup ou via job dedicado.
- DbContext registrado como **Scoped**. Não compartilhe entre threads.
- Use `AsNoTracking()` para queries de leitura pura.
- Cuidado com **N+1**: use `Include`/`ThenInclude` ou projeções com `Select` para DTOs.
- Defina **índices** explicitamente para colunas de busca/filtro frequentes.
- Configure entidades via `IEntityTypeConfiguration<T>`, não com Data Annotations excessivas.

### 5.2 Performance

- Paginação obrigatória em endpoints de listagem (`Skip`/`Take` com limite máximo enforced no servidor — ex.: `pageSize <= 100`).
- Use `Select` para projetar apenas colunas necessárias.
- Para queries complexas/relatórios, considere SQL bruto parametrizado ou views.

### 5.3 Neon (PostgreSQL)

- Use `Npgsql.EntityFrameworkCore.PostgreSQL`.
- Configure **connection pooling** apropriado. Neon tem modo serverless — atenção a cold starts.
- Use a connection string do **Pooler endpoint** do Neon, não a connection direta, para melhor performance em ambientes com muitas conexões curtas.
- Timezone: armazene em **UTC**. Converta no boundary (API).
- Considere `EnableRetryOnFailure()` para lidar com desconexões transientes do Neon.

---

## 6. Padrões de API

- Endpoints versionados via rota (`/api/v1/...`). Considere `Asp.Versioning.Mvc` se evoluir para múltiplas versões.
- Verbos HTTP corretos (GET idempotente, POST cria, PUT substitui, PATCH atualiza parcial, DELETE remove).
- Status codes corretos: `200`, `201` (com `Location` via `CreatedAtAction`), `204`, `400`, `401`, `403`, `404`, `409`, `422`, `429`, `500`.
- Não confunda `401` (não autenticado) com `403` (autenticado mas sem permissão).
- Respostas consistentes. Erros sempre em `ProblemDetails`.
- Idempotência em operações sensíveis (use `Idempotency-Key` header quando aplicável).
- Documente todos os endpoints com OpenAPI/Swagger (`Swashbuckle.AspNetCore`). Use `[ProducesResponseType]` para tipar respostas.

---

## 7. Testes

- **Unit tests** para regras de domínio e use cases — sem I/O, sem banco.
- **Integration tests** para controllers + EF + banco real (use `WebApplicationFactory` + Testcontainers com PostgreSQL).
- **Architecture tests** (NetArchTest) para validar regras de dependência entre camadas.
- Mock apenas o que está na borda (Firebase, e-mail, HTTP externo). Não mock o EF — use Testcontainers.
- Cobertura é métrica fraca; **comportamento testado** é o que importa.
- Para testar autenticação: crie um `TestAuthHandler` que injeta claims controladas em testes de integração.

---

## 8. Qualidade de Código

- **Nullable reference types** habilitado (`<Nullable>enable</Nullable>`).
- **Warnings as errors** em CI (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`).
- Analyzers: `Microsoft.CodeAnalysis.NetAnalyzers`, `SonarAnalyzer.CSharp`, `Roslynator`.
- Formatação: `.editorconfig` + `dotnet format` no pre-commit.
- Async **end-to-end**. Nunca `.Result` ou `.Wait()`. Sempre `CancellationToken` propagado.
- Nomes em **inglês** no código. Comentários onde necessário, em inglês também.
- Métodos curtos (regra de bolso: cabe na tela). Classes com responsabilidade única.

---

## 9. Docker e Deploy

### 9.1 Princípios

- Imagem **pequena, segura, reproduzível**.
- **Multi-stage build** sempre (build stage + runtime stage).
- Use imagens oficiais da Microsoft: `mcr.microsoft.com/dotnet/sdk` para build e `mcr.microsoft.com/dotnet/aspnet` para runtime.
- Prefira variantes **`-alpine`** ou **`-chiseled`** (menores, menor superfície de ataque) quando compatível.
- **Nunca rode como root.** Use o usuário não-root (`USER $APP_UID` nas imagens oficiais já configura isso).

### 9.2 Dockerfile (template de referência)

```dockerfile
# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar arquivos de projeto primeiro para aproveitar cache de layers
COPY ["src/Api/Api.csproj", "src/Api/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]
RUN dotnet restore "src/Api/Api.csproj"

# Copiar o resto e publicar
COPY . .
WORKDIR /src/src/Api
RUN dotnet publish "Api.csproj" -c Release -o /app/publish \
    /p:UseAppHost=false \
    /p:PublishTrimmed=false

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-chiseled AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Usuário não-root (já configurado nas imagens chiseled)
USER $APP_UID

# Porta padrão (ASP.NET Core em containers)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["dotnet", "Api.dll"]
```

### 9.3 Regras obrigatórias

- **.dockerignore** sempre presente. Mínimo: `bin/`, `obj/`, `.git/`, `.vs/`, `**/*.user`, `**/appsettings.Development.json`, `tests/`, `*.md`, `.env*`.
- **Não copie segredos** para a imagem. `firebase-adminsdk-*.json`, `.env`, `appsettings.Production.json` com segredos: nunca.
- Segredos chegam ao container via **variáveis de ambiente** em runtime (Docker Compose `environment:`/`env_file:`, secrets do orquestrador, etc.).
- Configure **healthcheck** no Dockerfile ou Compose: endpoint `/health` (com `AddHealthChecks()`).
- **Logs no stdout/stderr** (não em arquivo). O orquestrador coleta. Serilog → `WriteTo.Console()`.
- **Tag imagens** com versão semântica + hash do commit (`myapp:1.2.3` e `myapp:sha-abc123`). Evite `latest` em produção.
- **Scan da imagem** com Trivy, Grype ou Docker Scout antes de subir para produção.

### 9.4 docker-compose para desenvolvimento

Compose local pode subir a API + um Postgres local para testes (sem precisar do Neon em dev). Exemplo:

```yaml
services:
  api:
    build: .
    ports:
      - "8080:8080"
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ConnectionStrings__Default: ${DB_CONNECTION_STRING}
      Firebase__ProjectId: ${FIREBASE_PROJECT_ID}
    env_file:
      - .env.local
    depends_on:
      db:
        condition: service_healthy

  db:
    image: postgres:16-alpine
    environment:
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
      POSTGRES_DB: appdb
    volumes:
      - pgdata:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 5s
      retries: 5

volumes:
  pgdata:
```

`.env.local` **nunca** comitado.

### 9.5 Considerações de runtime em container

- Configure `ForwardedHeaders` middleware se houver proxy reverso na frente (Nginx, Traefik, Cloudflare, etc.).
- `AddHealthChecks()` com endpoints `/health/live` (liveness) e `/health/ready` (readiness, validando conexão com banco).
- Graceful shutdown: respeite `IHostApplicationLifetime` e o `CancellationToken` do host para drenar requests em SIGTERM.
- Resource limits: defina `cpus` e `memory` no orquestrador. .NET respeita cgroups automaticamente, mas vale tunar `DOTNET_GCHeapHardLimit` em containers pequenos.
- **Migrations em deploy:** não rode automaticamente no startup da API em produção. Use um job/init container separado, ou um comando manual via CI/CD. Isso evita race conditions com múltiplas réplicas.

### 9.6 CI/CD (diretrizes)

- Build da imagem em pipeline (GitHub Actions, GitLab CI, etc.).
- Rode testes **dentro** do pipeline antes de buildar a imagem final.
- Scan de vulnerabilidades obrigatório antes de push.
- Push para registry privado (GHCR, ECR, ACR, Docker Hub privado).
- Deploy só promove imagem já testada — **nunca** builda em produção.

---

## 10. Diretrizes para a IA (Claude / Copilot / etc.)

Quando trabalhar neste projeto, você **deve**:

1. **Perguntar antes de assumir.** Se a especificação for ambígua, pergunte em vez de adivinhar.
2. **Não inventar pacotes, APIs ou métodos.** Verifique se existem antes de usar.
3. **Aplicar todos os princípios de segurança da seção 4** sem exceção, mesmo que o usuário não peça explicitamente.
4. **Nunca comitar segredos**, nem mesmo em exemplos. Use placeholders (`<YOUR_KEY_HERE>`).
5. **Sugerir testes** ao implementar lógica de negócio nova.
6. **Justificar trade-offs** em decisões arquiteturais relevantes.
7. **Recusar atalhos perigosos**, mesmo a pedido. Ex.: "desabilita validação só pra testar" → não. Crie um teste apropriado.
8. **Preferir o padrão do projeto** ao seu próprio gosto. Consistência > preferência pessoal.
9. **Quando em dúvida sobre versão de API ou pacote** (.NET 10, EF 10, FirebaseAdmin, Npgsql), sinalize que precisa verificar a documentação atual em vez de inventar.
10. **Não gere código "exemplo didático"** quando o usuário pediu código de produção. Produção = robusto, seguro, testável.
11. **Use Controllers**, nunca Minimal APIs neste projeto (decisão arquitetural fixa).

### O que NÃO fazer

- ❌ Aceitar `userId` do body sem checar o token.
- ❌ Concatenar strings em SQL.
- ❌ Retornar entidades EF cruas de controllers.
- ❌ Capturar `Exception` genérico e engolir.
- ❌ Usar `async void` (exceto event handlers).
- ❌ Marcar tudo como `public` por padrão.
- ❌ Adicionar dependência nova sem justificar.
- ❌ Misturar lógica de negócio em controllers.
- ❌ Commitar `appsettings.json` com segredos reais.
- ❌ "Resolver" warnings desabilitando-os.
- ❌ Copiar segredos para dentro da imagem Docker.
- ❌ Rodar container como root.
- ❌ Usar `latest` como tag em produção.

---

## 11. Checklist antes de considerar uma feature "pronta"

- [ ] Entrada validada (FluentValidation).
- [ ] Autenticação e autorização verificadas.
- [ ] Ownership do recurso validado.
- [ ] DTOs usados na entrada e saída do controller.
- [ ] Erros tratados com `ProblemDetails`.
- [ ] `[ProducesResponseType]` declarado para cada status possível.
- [ ] Logs estruturados sem dados sensíveis.
- [ ] Testes unitários da regra de negócio.
- [ ] Teste de integração do endpoint (pelo menos caminho feliz + 1 erro).
- [ ] Migration criada e revisada (se houve mudança de schema).
- [ ] Sem warnings, sem TODO órfão, sem código morto.
- [ ] Documentação/OpenAPI atualizada.
- [ ] Imagem Docker continua buildando sem novas vulnerabilidades.

---

**Quando este documento conflitar com uma instrução pontual do usuário, peça confirmação explícita antes de descumprir qualquer item da seção 4 (Segurança) ou da seção 9 (Docker).**
