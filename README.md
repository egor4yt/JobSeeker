# About project

JobSeeker is an open-source app that takes the hassle out of finding your next role. 
It scans thousands of listings from leading job boards worldwide and delivers a 
personalized feed of opportunities tailored to your skills, interests, and location — so 
you can focus on applying, not searching.


# Structure of the repository

- src - source code
- docs - project documentation

# Stack

JobSeeker is a microservice-oriented application that collects job listings from multiple 
sources and delivers a personalized feed to end-users. The entire system is built on .NET 8 
and is containerized with Docker Compose for easy local development and deployment

## Backend
- .NET 8.0
- Entity Framework (ORM)
- Serilog (logging)
- MassTransit (pub/sub)
- Hangfire (delayed background tasks)
- Amazon.S3 (S3 object storage manager)
- HtmlAgilityPack (HTML parsing)
- Playwright (Chromium emulation)

## Data
- PostgreSQL (OLTP database)
- Redis (cache)
- Kafka (message broker)
- Minio (S3 object storage)

# Infrastructure of the JobSeeker

<img alt="Infrastructure" src="docs%2Fimages%2FInfrastructure.svg" title="Infrastructure"/>

## How it works

Before the user saw the vacancies, JobSeeker did a lot of work.

### WebScraper

Every night, **WebScraper** creates scrap tasks based on the configuration of scrap groups and downloads 
HTML pages from various job boards using a Chromium-based browser and proxies for better efficiency. 
When one of the scrap tasks is completed, **WebScraper** publishes an event in the message queue.

### PagesAnalyzer
Consumes "Scarp Task Completed" events and transforms raw HTML files into JSON vacancy details. 
When all vacancies from one scrap task have been transformed, "PagesAnalyzer" publishes an event in the message queue.

### Deduplication
Consumes "Scrap Task Analyzed" events and removes duplicate vacancies (including comparison 
with previous vacancies) using a light matching algorithm to remove most duplicates and
heavy matching algorithm to remove remaining duplicates. When one of the vacancy type deduplication
is completed, **Deduplication** publishes an event in the message queue

### Web Api
Consumes "Raw Vacancy Deduplicated" events and creates companies, locations, vacancies according to results of deduplication. 

# Contacts
You can contact me at:
- Telegram: [@egor4yt](https://t.me/egor4yt)
- Gmail: [egor4yt@gmail.com](mailto:egor4yt@gmail.com)

# Contributing

Pull requests are welcome! Please open an issue first to discuss significant changes
