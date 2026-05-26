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

## Fluxo de criar migration e atualizar o db

Via Docker (não precisa ter o .NET instalado):

```bash
docker compose run --rm migrator migrations add <NomeDaMigration>
docker compose run --rm migrator database update
```
