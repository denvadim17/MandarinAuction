# Mandarin Auction — Онлайн-аукцион мандаринок

## Стек технологий
- **Backend**: ASP.NET Core 8 Web API
- **Database**: SQLite (файл `mandarin.db` в папке проекта; без отдельного сервера)
- **Auth**: ASP.NET Core Identity + JWT (`AddJwtBearer`, как в типовых туториалах)
- **Real-time**: SignalR
- **Email**: MailKit
- **Frontend**: Vanilla JS SPA (wwwroot/index.html)

## Запуск

База — SQLite, отдельно ничего ставить не нужно. Файл `mandarin.db` создаётся при первом запуске в папке `MandarinAuction.Api`.

```bash
cd src/MandarinAuction.Api
dotnet restore
dotnet run
```

Приложение запустится на `http://localhost:5000` (или порт из launchSettings).

- **Swagger UI**: http://localhost:5000/swagger
- **Фронтенд**: http://localhost:5000

## Аккаунт администратора (создаётся автоматически)
- Email: `admin@mandarin.com`
- Пароль: `Admin123`

## Реализованные уровни

### Level 1
- Генерация мандаринок через заданный интервал (BackgroundService)
- Просмотр мандаринок без авторизации
- Регистрация/авторизация (Identity + JWT)
- Email-уведомления при перебитии ставки
- Чек на почту при покупке/победе
- Очистка испорченных мандаринок (BackgroundService, раз в час)

### Level 2
- Админ-панель для настройки: кэшбек, время жизни, частота генерации
- Кошелёк с пополнением (QR/ссылка — эндпоинт payment-link)
- Формула кэшбека: `min(base + purchased * bonus, max)`
- Real-time обновления через SignalR

## Архитектура

```
MandarinAuction.Api/
├── Domain/           — Сущности и перечисления
│   ├── Entities/     — AppUser, Mandarin, Bid, Transaction, AuctionSettings
│   └── Enums/        — MandarinStatus, TransactionType
├── Data/             — EF Core DbContext
├── DTOs/             — Data Transfer Objects
├── Services/         — Бизнес-логика
├── Controllers/      — API-контроллеры
├── Hubs/             — SignalR хаб
├── BackgroundServices/ — Фоновые задачи
└── wwwroot/          — SPA-фронтенд
```

## API Endpoints

| Метод  | Путь                       | Auth   | Описание                         |
|--------|---------------------------|--------|----------------------------------|
| POST   | /api/auth/register        | -      | Регистрация                      |
| POST   | /api/auth/login           | -      | Авторизация                      |
| GET    | /api/mandarin             | -      | Все активные мандаринки          |
| GET    | /api/mandarin/{id}        | -      | Одна мандаринка                  |
| POST   | /api/bid                  | JWT    | Сделать ставку                   |
| POST   | /api/bid/buy-now/{id}     | JWT    | Мгновенная покупка               |
| GET    | /api/bid/{mandarinId}     | -      | Ставки на мандаринку             |
| GET    | /api/wallet               | JWT    | Баланс кошелька                  |
| POST   | /api/wallet/deposit       | JWT    | Пополнить кошелёк                |
| POST   | /api/wallet/payment-link  | JWT    | Получить ссылку для оплаты       |
| GET    | /api/admin/settings       | Admin  | Настройки аукциона               |
| PUT    | /api/admin/settings       | Admin  | Обновить настройки               |
| POST   | /api/admin/generate-mandarin | Admin | Создать мандаринку вручную    |
| POST   | /api/admin/cleanup        | Admin  | Принудительная очистка           |

## SignalR Hub

Подключение: `/hubs/auction?access_token=<jwt>`

События:
- `NewMandarin` — новая мандаринка
- `BidPlaced` — новая ставка
- `MandarinSold` — мандаринка продана
- `MandarinExpired` — мандаринки истекли
- `AuctionWon` — аукцион завершён, есть победитель
