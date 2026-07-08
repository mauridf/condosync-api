# CondoSync API v2.0

Sistema SaaS multi-tenant para gestão completa de condomínios.
API em .NET 10 + C# 13 + PostgreSQL + CQRS + DDD.

## Stack
- **Runtime**: .NET 10 / C# 13
- **API**: ASP.NET Core 10
- **Database**: PostgreSQL 16
- **Cache**: Redis 7
- **Messaging**: RabbitMQ 3.13
- **Storage**: MinIO (S3-compatible)

## Status
🚧 Em desenvolvimento - Fase 0: Setup inicial

## Como rodar
```bash
docker-compose up -d
cd src/CondoSync.Api
dotnet run
```