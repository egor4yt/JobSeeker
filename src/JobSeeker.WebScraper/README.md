# About project

The project is designed for automated collection (scraping) 
of information about vacancies from various job boards.

# Key features
- Uses the Playwright library to automate work with the browser
- Supports work through proxy servers to circumvent restrictions
- Provides multithreading with protection from competitive access
- Can work with different job boards

# Stack

## Backend
- .NET 8.0
- Entity Framework (ORM)
- Serilog (logging)
- MassTransit (pub/sub)
- Hangfire (delayed background tasks)
- Amazon.S3 (S3 object storage manager)
- Playwright (Chromium emulation)

## Data
- PostgreSQL (OLTP database)
- Kafka (message broker)
- Minio (S3 object storage)