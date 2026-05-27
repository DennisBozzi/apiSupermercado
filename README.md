# apiSupermercado

## Fluxo de push (mandar alterações pro repositório)

```bash
git add .
git commit -m "mensagem do commit"
git push
```

## Fluxo de pull (atualizar o projeto)

```bash
git pull
```

## Rodando o projeto

### Opção A — com .NET 10 instalado

Primeira vez (restaura dependências e ferramentas locais):

```bash
dotnet restore
dotnet tool restore
```

Rodar a API:

```bash
dotnet run --project src/Api
```

### Opção B — com Docker

```bash
docker compose up --build
```

API disponível em `http://localhost:8080`.

## Fluxo de criar migration e atualizar o db

### Opção A — com .NET 10 instalado

```bash
dotnet ef migrations add <NomeDaMigration> -p src/Infrastructure -s src/Api
dotnet ef database update -p src/Infrastructure -s src/Api
```

### Opção B — com Docker (não precisa ter o .NET instalado)

```bash
docker compose run --rm migrator migrations add <NomeDaMigration>
docker compose run --rm migrator database update
```
