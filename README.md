# BuliaPortfolio

Сайт-портфолио и демонстрационные страницы: ASP.NET Core Razor Pages,
C#, SQL Server. Без Bootstrap и тяжёлых библиотек — свой CSS и немного JS.

## Что на сайте

| Адрес | Что это |
|---|---|
| `/` | Главная: обо мне, навыки, проекты, услуги, цены, демо, форма заявки |
| `/Case` | Разбор сайта автосервиса по шагам: структура, сроки, цена |
| `/Landing` | Светлый пример лендинга: блоки, цены, калькулятор, форма |
| `/SaitDlyaAvtoservisa` | Отраслевая страница: что важно на сайте автосервиса |
| `/SaitDlyaHimii` | Отраслевая страница: сайт поставщика химии |
| `/SaitDlyaSelhoztehniki` | Отраслевая страница: магазин сельхозтехники и запчастей |
| `/SaitDlyaKrasoty` | Отраслевая страница: салон красоты, услуги и запись |
| `/Demo/Autoservice` | Демо-сайт автосервиса |
| `/Demo/Him` | Демо-сайт поставщика химии |
| `/Demo/Agro` | Демо-сайт магазина сельхозтехники |
| `/Demo/Beauty` | Демо-сайт студии красоты |
| `/Admin/Requests` | Заявки с сайта, доступно только роли Admin |
| `/sitemap.xml`, `/robots.txt` | Карта сайта и правила обхода, отдаются из `Program.cs` |

Демо-сайты и проекты в портфолио — технические примеры. Они не являются
заказами клиентов, и на каждой демо-странице стоит плашка об этом.
Данные в демо (адреса, телефоны, отзывы) вымышленные.

## Стек

| Что | Версия |
|---|---|
| .NET SDK | 9.0 |
| ASP.NET Core Razor Pages | 9.0 |
| EF Core + SQL Server | 9.0.10 |
| Identity | встроенные страницы входа |
| Яндекс.Метрика | счётчик `114025560`, цели `lead` и `messenger` |

## Запуск на своей машине

1. Открыть решение `BuliaPortfolio.sln` в Visual Studio 2022 или VS Code.
2. Задать строку подключения и учётные данные администратора в user secrets
   (в репозиторий они не попадают):

   ```powershell
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=<экземпляр>;Database=<имя базы>;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
   dotnet user-secrets set "Admin:Email" "<ваш email>"
   dotnet user-secrets set "Admin:Password" "<пароль минимум 10 символов>"
   ```

   Для SQL-аутентификации замените `Trusted_Connection=True` на
   `User Id=<логин>;Password=<пароль>`.

3. Применить миграции (см. ниже).
4. Запустить проект: `dotnet run` или `F5`.
5. Открыть сайт и проверить `/Admin/Requests` после тестовой заявки.

## Миграции базы

Схема таблиц `Skills`, `Projects`, `ContactRequests` и Identity создаётся
миграциями EF Core:

```powershell
dotnet tool install --global dotnet-ef
dotnet ef database update                  # применить миграции
dotnet ef migrations add ИмяИзменения     # после правки моделей
dotnet ef migrations script                # SQL-скрипт для DBA
```

После изменения `MaxLength` в моделях стоит создать миграцию и применить её:
на существующих данных это безопасные `ALTER`.

## Публикация

Строка подключения и пароль администратора передаются переменными окружения:

```
ConnectionStrings__DefaultConnection=Server=<экземпляр>;Database=<имя базы>;...
Admin__Email=<email администратора>
Admin__Password=<пароль>
ASPNETCORE_ENVIRONMENT=Production
```

Сборка и архив для хостинга:

```powershell
dotnet publish BuliaPortfolio/BuliaPortfolio.csproj -c Release -o publish
```

Содержимое `publish` загружается на хостинг вручную (ZIP + перезапуск
приложения). `appsettings.Development.json` в публикацию не попадает.

При первом запуске роль `Admin` и аккаунт из `Admin__Email` создаются
автоматически. Повторные запуски ничего не дублируют: если аккаунт уже
есть, пароль не меняется.

## Уведомления о заявках

Заявки сохраняются в `ContactRequests` и отправляются в мессенджер MAX
через бота (`Services/MaxBot.cs`). Нужны переменные окружения с токеном
бота и ID чата. Если ID чата неизвестен, бот находит его по первому
входящему сообщению. Telegram для уведомлений не используется.

## Структура

```
BuliaPortfolio/
├── Data/
│   ├── PortfolioDbContext.cs   контекст EF: Skills, Projects, ContactRequests + Identity
│   └── SeedData.cs             роль Admin и аккаунт администратора
├── Models/                     сущности БД и модель формы заявки
├── Services/
│   └── MaxBot.cs               отправка заявок в MAX
├── Pages/
│   ├── Index.cshtml            главная + форма заявки
│   ├── Case.cshtml             разбор кейса
│   ├── Landing.cshtml          пример лендинга
│   ├── SaitDlya*.cshtml        отраслевые страницы
│   ├── Demo/                   четыре демо-сайта
│   ├── Admin/Requests.cshtml   заявки, только для роли Admin
│   └── Shared/                 layout, метрика, вход
├── Migrations/                 миграции EF Core
└── wwwroot/
    ├── css/site.css            основной тёмный стиль портфолио
    ├── css/offer.css           кейс, цены, отраслевые блоки
    ├── css/landing.css         светлая тема /Landing
    ├── css/demo.css            стили демо-сайтов
    ├── js/                     меню, калькулятор, счётчики, анимации
    └── images/                 фото проектов и обложки
```

## Безопасность

- Пароль администратора и строка подключения — только в user secrets
  или переменных окружения, в репозитории их нет.
- Регистрация новых аккаунтов закрыта в `Program.cs` (ответ 404).
- В форме заявки есть honeypot — скрытое поле для ботов.
  Для серьёзной защиты можно добавить rate limiter или капчу.
- Админка и Identity закрыты от индексации в `robots.txt`.

## Запуск без Visual Studio

```powershell
dotnet restore
dotnet run --urls http://localhost:5091
```
