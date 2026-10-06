# Team Availability Scheduler

Backend-сервис для управления доступностью команд: отпуска, on-call смены, интеграции с Jira и Telegram.

Pet-project на **.NET 9** с Clean Architecture, CQRS (MediatR), PostgreSQL, Outbox, фоновыми сервисами и integration-тестами.

## Возможности

- Команды, участники, роли (Admin / TeamLead / Member)
- Календарь доступности: отпуск, больничный, часовые пояса
- On-call смены с напоминаниями в Telegram
- Автосоздание задачи в Jira при назначении on-call
- Telegram-бот: быстрый статус (`/status`, `/working`, `/away`)
- Web UI + REST API (Swagger)
- JWT + API Key, rate limiting, OpenTelemetry, Prometheus `/metrics`

## Стек

.NET 9 · ASP.NET Core Minimal API · MediatR · FluentValidation · EF Core · PostgreSQL · Redis · RabbitMQ (MassTransit) · Serilog · Docker · Testcontainers

## Быстрый старт

**Требования:** .NET 9 SDK, PostgreSQL

```bash
copy appsettings.example.json appsettings.json
# Укажите пароль Postgres и JWT Secret в appsettings.json

dotnet run --launch-profile TeamAvailability
```

| URL | Описание |
|-----|----------|
| http://localhost:5180 | Web UI |
| http://localhost:5180/swagger | API |

**Демо-логин:** `lead@teamavailability.local` / `Lead123!`

Для локальной разработки Redis и RabbitMQ не обязательны — в `appsettings.Development.json` включён in-memory режим.

## Тесты

```bash
dotnet test TeamAvailability.sln
```

Integration-тесты через WebApplicationFactory и Testcontainers (PostgreSQL).

## Docker

```bash
docker compose up --build
```

API: http://localhost:8080  
Grafana: http://localhost:3000 · Jaeger: http://localhost:16686 · Prometheus: http://localhost:9090

Переменные окружения для интеграций — см. `.env.example`.

## Архитектура

```
Domain → Application (CQRS) → Infrastructure → API
```

- **Domain** — сущности и бизнес-правила
- **Application** — команды/запросы, валидаторы, pipeline behaviors (logging, validation, transactions)
- **Infrastructure** — EF Core, кэш, message bus, Jira/Telegram, background services
- **Outbox** — надёжная доставка integration events

## CI

GitHub Actions: restore → build → test на push/PR в `main` / `master`.
