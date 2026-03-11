# PriceLoaderApp

Консольное приложение для автоматической загрузки прайс-листов автозапчастей
из почтового ящика (IMAP), обработки CSV-файлов от разных поставщиков
и сохранения данных в базу PostgreSQL.

Проект реализован в рамках тестового задания и демонстрирует
расширяемую архитектуру, конфигурируемую обработку данных
и автоматизированный запуск окружения.

---

## Используемый технологический стек

- Язык: C# (.NET 10.0 )
- Тип приложения: Console Application
- База данных: PostgreSQL 16 (Docker)
- Администрирование БД: pgAdmin 9.11
- Работа с почтой: IMAP (MailKit, MimeKit)
- Обработка CSV: CsvHelper
- Доступ к БД: Npgsql
- Контейнеризация: Docker, docker-compose

---

## Функциональные возможности

- Подключение к почтовому ящику по протоколу IMAP
- Поиск писем от заданных отправителей
- Автоматическая загрузка CSV-вложений
- Поддержка нескольких поставщиков
- Конфигурируемое сопоставление колонок CSV
- Автоматическое определение кодировки (UTF-8 / Windows-1251)
- Обработка нестандартных форматов количества:
  - >10 -> 10
  - <13 -> 13
  - 10-50 -> 50
- Очистка поисковых полей (удаление небуквенно-цифровых символов, upper-case)
- Обрезка описания до 512 символов
- Сохранение данных в PostgreSQL
- Логирование обработки
- Автоматическое создание таблиц БД при старте Docker-контейнера

---

## Структура проекта

E:\DevTestChern\
|
|-- docker
|   |-- postgres
|       |-- docker-compose.yml
|       |-- init
|           |-- 001_init_tables.sql
|
|-- PriceLoaderApp
|   |-- Models
|   |-- Services
|   |-- Utils
|   |-- Configuration
|   |-- appsettings.json
|   |-- Program.cs
|   |-- PriceLoaderApp.csproj
|
|-- scripts
|   |-- run-windows.ps1
|   |-- run-linux.sh
|   |-- run-macos.sh
|
|-- README.md

---

## Быстрый запуск

Для удобства и воспроизводимости в проекте предусмотрены
скрипты автозапуска для Windows, Linux и macOS.

Для работы приложения требуются установленные:

.NET 10.0 и выше;
Docker Desktop.

Скрипты выполняют следующие шаги:

1. Проверка доступности Docker
2. Запуск PostgreSQL и pgAdmin в Docker
3. Ожидание готовности базы данных
4. Запуск консольного приложения PriceLoaderApp

Особенности скриптов:

- выводят пошаговый лог выполнения
- останавливаются при возникновении ошибки
- не закрывают окно автоматически после завершения
- ожидают нажатия ENTER перед выходом

---

## Запуск (Windows)

cd E:\DevTestChern\scripts
.\run-windows.ps1

После завершения выполнения скрипт ожидает нажатия клавиши ENTER
и не закрывает окно автоматически.

---

## Запуск (Linux / macOS)

chmod +x scripts/run-linux.sh
./scripts/run-linux.sh

Скрипт выводит статус выполнения каждого этапа
и ожидает нажатия ENTER перед завершением.

---

## Конфигурация

Основные настройки приложения находятся в файле:

PriceLoaderApp/appsettings.json

### Настройки почты (IMAP)

"Mail": {
  "ImapServer": "imap.example.com",
  "Port": 993,
  "UseSsl": true,
  "Username": "user@example.com",
  "Password": "password"
}

---

### Конфигурация поставщиков

Поддерживается несколько поставщиков.
Для каждого поставщика настраиваются:

- имя поставщика
- email отправителя
- разделитель CSV
- сопоставление колонок

Пример конфигурации:

"Suppliers": [
  {
    "Name": "ООО Доставим в срок",
    "SenderEmail": "supplier1@example.com",
    "Delimiter": ";",
    "Columns": {
      "Vendor": "Бренд",
      "Number": "Каталожный номер",
      "Description": "Описание",
      "Price": "Цена",
      "Count": "Наличие"
    }
  },
  {
    "Name": "ООО Быстрая поставка",
    "SenderEmail": "supplier2@example.com",
    "Delimiter": ",",
    "Columns": {
      "Vendor": "Brand",
      "Number": "PartNumber",
      "Description": "Description",
      "Price": "Price",
      "Count": "Stock"
    }
  }
]

---

## Схема базы данных

### Таблица PriceItems

- Vendor varchar(64)
- Number varchar(64)
- SearchVendor varchar(64)
- SearchNumber varchar(64)
- Description varchar(512)
- Price decimal(18,2)
- Count int
- SupplierName varchar(128)
- FileName varchar(256)
- ProcessedAt timestamp

### Таблица ProcessingLogs

- CreatedAt timestamp
- SupplierName varchar(128)
- FileName varchar(256)
- Message text
- IsError boolean

Таблицы создаются автоматически при первом запуске PostgreSQL контейнера.

---

## Принятые архитектурные решения

1. Конфигурация через appsettings.json  
Все внешние зависимости вынесены в конфигурацию.
Это позволяет добавлять новых поставщиков без изменения кода.

2. Разделение ответственности  
Каждый сервис выполняет одну задачу:
MailService, CsvProcessor, DatabaseRepository, PriceLoaderService.

3. Отказ от ORM  
Используется Npgsql напрямую для прозрачности и предсказуемости SQL.

4. Docker-first подход  
PostgreSQL и pgAdmin запускаются в Docker, обеспечивая единое окружение.

5. Расширяемость  
Архитектура рассчитана на добавление поставщиков, форматов и логики.

---

## Проверка результата

### Доступ к базе данных PostgreSQL

База данных запускается в Docker-контейнере со следующими параметрами
по умолчанию:

Host: localhost / postgres
Port: 5432  
Database: priceloader  
Username: priceloader  
Password: priceloader  

Эти параметры задаются в файле:

docker/postgres/docker-compose.yml

При необходимости их можно изменить в секции environment
сервиса postgres.

---

### Доступ к pgAdmin

pgAdmin доступен по адресу:

http://localhost:5050

Данные для входа по умолчанию:

Email: admin@example.com  
Password: admin  

Эти параметры также настраиваются в файле:

docker/postgres/docker-compose.yml

в секции environment сервиса pgadmin.

---

### Проверка загруженных данных

После запуска приложения данные доступны в таблице PriceItems.
Пример SQL-запроса:

SELECT * FROM PriceItems;

---

## Примечания

Если в почтовом ящике отсутствуют новые письма с CSV-вложениями,
приложение корректно завершит работу без ошибок.
Это считается нормальным поведением.

---

## Возможные дальнейшие улучшения

- Пометка обработанных писем
- Обработка дубликатов файлов
- Retry-механизмы
- Healthcheck PostgreSQL
- Web API версия
- Юнит-тесты

---

## Статус проекта

Проект корректно собирается, запускается
и соответствует требованиям тестового задания.
