# About project

A system for analyzing raw JSON files with vacancies 
and combining duplicates into a single extended job description

# Key features
- Uses the unique vacancies fingerprints to quickly find duplicates
- Uses the LSH + Murmur + Jaccard for detailed duplicates search 

# Stack

## Backend
- .NET 8.0
- Entity Framework (ORM)
- Serilog (logging)
- MassTransit (pub/sub)
- Hangfire (delayed background tasks)
- Amazon.S3 (S3 object storage manager)
- MurmurHash.Net (H
- TML parsing)

## Data
- PostgreSQL (OLTP database)
- Kafka (message broker)
- Redis (fast repository of hashing results)
- Minio (S3 object storage)