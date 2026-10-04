# BuliaPortfolio

Сайт-портфолио: ASP.NET Core (Razor Pages) + SQL Server.

Разделы главной страницы: «Обо мне», «Навыки», «Проекты» (только с
`IsPublished = 1`), «Услуги», форма заявки. Заявки падают в таблицу
`ContactRequests`, администратор видит их в `/Admin/Requests`.

## Что нужно для запуска

| Что | Версия |
|---|---|
| .NET SDK | 9.0 |
| SQL Server | 2019+ / LocalDB / SQLEXPRESS |

## Запуск на своей машине

1. Открыть решение `BuliaPortfolio.sln` в Visual Studio 2022.
2. Восстановить пакеты и убедиться, что есть база `BuliaPortfolioDb`.
3. Задать строку подключения и учётные данные администратора.
   Всё это хранится в **user secrets** и в репозиторий не попадает:

   ```powershell
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.\SQLEXPRESS;Database=BuliaPortfolioDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
   dotnet user-secrets set "Admin:Email" "ваш@email.ru"
   dotnet user-secrets set "Admin:Password" "пароль-минимум-10-символов"
   ```

   Если используется SQL-аутентификация вместо Windows, замените
   `Trusted_Connection=True` на `User Id=...;Password=...`.

4. Запустить проект (`F5`) и открыть `https://localhost:7013`.
5. Войти как администратор и проверить `/Admin/Requests`.

## Развёртывание на сервере

Строка подключения и пароль администратора передаются переменными
окружения — не записывайте их в `appsettings.json`:

```
ConnectionStrings__DefaultConnection=Server=...;Database=BuliaPortfolioDb;...
Admin__Email=admin@site.ru
Admin__Password=НадёжныйПароль
ASPNETCORE_ENVIRONMENT=Production
```

При первом запуске роль `Admin` и аккаунт из `Admin__Email` создаются
автоматически. Повторные запуски ничего не дублируют: если аккаунт уже
есть, пароль не меняется.

## Миграции базы

Схема таблиц `Skills`, `Projects`, `ContactRequests` и Identity создаётся
миграциями EF Core:

```powershell
dotnet tool install --global dotnet-ef
dotnet ef database update                  # применить миграции
dotnet ef migrations add ИмяИзменения     # после правки моделей
dotnet ef migrations script                # SQL-скрипт для DBA
```

Длины строк в моделях (`MaxLength`) совпадают с реальными колонками в
SQL Server. После добавления `MaxLength` стоит создать миграцию и
применить её — на существующих данных это безопасные `ALTER`.

## Структура

```
BuliaPortfolio/
├── Data/
│   ├── PortfolioDbContext.cs   контекст EF: Skills, Projects, ContactRequests + Identity
│   └── SeedData.cs             создание роли Admin и аккаунта администратора
├── Models/
│   ├── Skill.cs, Project.cs, ContactRequest.cs        сущности БД
│   └── ContactRequestInput.cs   данные формы + правила проверки
├── Pages/
│   ├── Index.cshtml(.cs)       главная + форма заявки
│   ├── Error.cshtml(.cs)       404 / 403 / 500 по-русски
│   ├── Admin/Requests.cshtml   заявки, доступно только роли Admin
│   └── Shared/_LoginPartial    Вход / Выход
├── Migrations/                 миграции EF Core
└── wwwroot/css/site.css        стили сайта
```

## Безопасность

- Пароль администратора и строка подключения — только в user secrets
  или переменных окружения, в репозитории их нет.
- На странице `/Identity/Account/Register` anyone может зарегистрироваться.
  Если это не нужно, уберите ссылку «Регистрация» или запретите регистрацию
  в `Program.cs`.
- В форме заявки есть honeypot — скрытое поле, которое заполняют боты.
  Для серьёзной защиты добавьте rate limiter или капчу.

## Запуск без Visual Studio

```powershell
dotnet restore
dotnet run --urls http://localhost:5091
```
