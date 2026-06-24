# SeatReservation

Сервис бронирования мест на мероприятиях (концерты, конференции, онлайн-события).
Построен на .NET 10 с применением чистой архитектуры (Clean Architecture), CQRS и
паттернов Result/Domain-Driven Design.

## Возможности

- **Площадки (Venues)** — создание площадок, управление местами и лимитом мест,
  переименование (в т.ч. массовое по префиксу).
- **Мероприятия (Events)** — создание мероприятий трёх типов: `CONCERT`,
  `CONFERENCE`, `ONLINE`. Каждый тип хранит специфичную информацию (исполнитель,
  спикер/тема, URL). Получение мероприятия по id и постраничный список с поиском.
- **Бронирование (Reservations)** — бронирование конкретных мест, а также
  автоматический подбор соседних свободных мест (до 10 за раз) с учётом
  предпочтительного ряда.
- Поиск мероприятий по названию через GIN-индекс на триграммах (`pg_trgm`).
- Конкурентное бронирование через транзакции с блокировкой мероприятия
  (`GetByIdWithLock`).
- Два варианта чтения данных: через EF Core и через Dapper (для сравнения
  производительности — эндпоинты `/dapper`).

## Технологический стек

- **.NET 10** / ASP.NET Core (Web API)
- **PostgreSQL** + **Entity Framework Core 10** (запись) и **Dapper** (чтение)
- **FluentValidation** — валидация запросов
- **CSharpFunctionalExtensions** — Result-паттерн
- **OpenAPI / Swagger UI** — документация API (в Development)
- **StyleCop.Analyzers** + .NET Analyzers — статический анализ кода
- **Docker Compose** — локальный PostgreSQL

## Архитектура

Решение разбито на слои:

| Проект | Назначение |
| --- | --- |
| `SeatReservation.Domain` | Доменные сущности и бизнес-правила (Venue, Event, Reservation, Seat) |
| `SeatReservation.Application` | Сценарии (CQRS-команды и запросы), интерфейсы репозиториев |
| `SeatReservation.Contracts` | DTO и контракты запросов/ответов |
| `SeatReservation.Infrastructure.Postgres` | EF Core, репозитории, миграции, сидинг |
| `SeatReservationService.Web` | Контроллеры, точка входа, конфигурация DI |
| `Core` | Абстракции CQRS, HTTP-коммуникация, валидация |
| `Framework` | Сквозная инфраструктура: middleware, эндпоинты, CORS, логирование, Swagger |
| `SharedKernel` | Общие типы: `Error`, `Envelope`, исключения, константы |

## Требования

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (или локальный PostgreSQL)
- `dotnet-ef` версии **10.x** (для работы с миграциями)

## Запуск

### 1. Поднять базу данных

```bash
cd SeatReservationService
docker compose up -d
```

PostgreSQL поднимется на порту `5434` (БД `reservation_service_db`,
пользователь/пароль `postgres`/`postgres`). Строка подключения настроена в
`SeatReservationService.Web/appsettings.json`.

### 2. Применить миграции

```bash
dotnet ef database update \
  --project SeatReservation.Infrastructure.Postgres \
  --startup-project SeatReservationService.Web
```

### 3. Запустить сервис

```bash
dotnet run --project SeatReservationService.Web
```

Swagger UI будет доступен в Development-режиме по адресу `/swagger`
(спецификация — `/openapi/v1.json`).

### Заполнение тестовыми данными (опционально)

```bash
dotnet run --project SeatReservationService.Web -- --seeding
```

## API

Базовый префикс — `/api`.

### Venues — `/api/venues`

| Метод | Путь | Описание |
| --- | --- | --- |
| `POST` | `/api/venues` | Создать площадку |
| `PATCH` | `/name` | Переименовать площадку |
| `PATCH` | `/name/by-prefix` | Массовое переименование по префиксу |
| `PATCH` | `/seats` | Обновить места площадки |

### Events — `/api/events`

| Метод | Путь | Описание |
| --- | --- | --- |
| `POST` | `/api/events` | Создать мероприятие |
| `GET` | `/api/events/{eventId}` | Получить мероприятие (EF Core) |
| `GET` | `/api/events/{eventId}/dapper` | Получить мероприятие (Dapper) |
| `GET` | `/api/events` | Список мероприятий с пагинацией (EF Core) |
| `GET` | `/api/events/dapper` | Список мероприятий с пагинацией (Dapper) |

### Reservations — `/api/reservations`

| Метод | Путь | Описание |
| --- | --- | --- |
| `POST` | `/api/reservations` | Забронировать конкретные места |
| `POST` | `/api/reservations/adjacent` | Подобрать и забронировать соседние места |

#### Пример: бронирование соседних мест

```http
POST /api/reservations/adjacent
Content-Type: application/json

{
  "eventId": "...",
  "userId": "...",
  "venueId": "...",
  "requiredSeatsCount": 3,
  "preferredRowNumber": 5
}
```

## Разработка

Создание новой миграции:

```bash
dotnet ef migrations add <Name> \
  --project SeatReservation.Infrastructure.Postgres \
  --startup-project SeatReservationService.Web
```

Сборка решения:

```bash
dotnet build SeatReservationService/SeatReservationService.slnx
```
