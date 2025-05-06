# About project

JobSeeker is an open-source app that takes the hassle out of finding your next role. 
It scans thousands of listings from leading job boards worldwide and delivers a 
personalized feed of opportunities tailored to your skills, interests, and location — so 
you can focus on applying, not searching.


# Structure of the repository

- src - source code of the backend of this application
- docs - project documentation

# Stack

JobSeeker is a microservice-oriented application that collects job listings from multiple 
sources and delivers a personalized feed to end-users. The entire system is built on .NET 8 
and is containerized with Docker Compose for easy local development and deployment

## Backend
- .NET 8.0
- Entity Framework (ORM)
- MediatR (helps to organize CQRS pattern)
- Serilog (logging)
- Saga (StateMachine)
- MassTransit (pub/sub)
- Confluent.Kafka (message broker)
- Telegram.Bot (telegram integration)

## Data
- PostgreSQL (OLTP database)
- Redis (cache)
- Kafka (message broker)
- Zookeeper (distributed applications configs)

## Third-party integration
- Telegram bot (messenger)

# Infrastructure of the JobSeeker

<img alt="Infrastructure" src="docs%2Fimages%2FInfrastructure.svg" title="Infrastructure"/>

# Contacts
You can contact me at:
- Telegram: [@egor4yt](https://t.me/egor4yt)
- Gmail: [egor4yt@gmail.com](mailto:egor4yt@gmail.com)

# Contributing

Pull requests are welcome! Please open an issue first to discuss significant changes

# License

JobSeeker is released under the MIT License.
