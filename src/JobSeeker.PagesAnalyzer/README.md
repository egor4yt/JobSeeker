# About project

A system for analyzing raw HTML files and splitting them into simple JSON 
with information about vacancies

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
- HtmlAgilityPack (HTML parsing)

## Data
- PostgreSQL (OLTP database)
- Kafka (message broker)
- Minio (S3 object storage)