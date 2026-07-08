# NovaLearn LMS

A modern, minimal, enterprise-grade Learning Management System.

Built with **ASP.NET Core 9** (Clean Architecture + DDD + CQRS) and **React 19** (TypeScript, Vite, Tailwind, shadcn/ui).

---

## Status

> 🚧 **Under active development.** Building in production-quality vertical slices.

| Slice | Backend | Frontend | Tests | Status |
|-------|:-------:|:--------:|:-----:|--------|
| **Authentication** (register, login, JWT + refresh, email verification, roles) | ✅ | ✅ | ✅ | In progress |
| Courses & Enrollment | — | — | — | Planned |
| Content (video, PDF, CMS) | — | — | — | Planned |
| Assignments & Quizzes | — | — | — | Planned |
| Real-time (SignalR) | — | — | — | Planned |

---

## Architecture

Clean / Onion Architecture — dependencies point inward.

```
API ─▶ Application ─▶ Domain ◀─ Shared
 │           ▲   ▲
 ├─▶ Infrastructure ┘
 └─▶ Persistence ────┘
```

| Layer | Project | Responsibility |
|-------|---------|----------------|
| **Domain** | `NovaLearn.Domain` | Entities, value objects, domain events, enums. No external deps. |
| **Application** | `NovaLearn.Application` | Use cases (CQRS via MediatR), DTOs, validation, interfaces (ports). |
| **Persistence** | `NovaLearn.Persistence` | EF Core `DbContext`, configurations, migrations, seeding. |
| **Infrastructure** | `NovaLearn.Infrastructure` | Adapters: JWT, ASP.NET Identity, email, caching, storage. |
| **API** | `NovaLearn.API` | HTTP endpoints, middleware, DI composition root. |
| **Shared** | `NovaLearn.Shared` | Cross-cutting primitives (`Result`, errors, security helpers). |

See [`docs/architecture/`](docs/architecture) for ADRs and diagrams.

---

## Tech Stack

**Backend:** ASP.NET Core 9 · EF Core 9 · PostgreSQL · Redis · ASP.NET Identity · JWT + refresh
tokens · MediatR (CQRS) · FluentValidation · AutoMapper · Serilog · Asp.Versioning · Swagger

**Frontend:** React 19 · TypeScript · Vite · Tailwind CSS · shadcn/Radix UI · TanStack Query ·
React Hook Form · Zod · Axios · Framer Motion

**Infra:** Docker Compose · PostgreSQL · Redis · MinIO (local S3)

---

## Getting Started

### Prerequisites

| Tool | Version | Notes |
|------|---------|-------|
| .NET SDK | **9.0+** | `winget install Microsoft.DotNet.SDK.9` |
| Node.js | 22+ | Installed ✅ |
| Docker | 24+ | For Postgres / Redis / MinIO |

### 1. Start infrastructure

```bash
cd infra
cp .env.example .env
docker compose up -d           # Postgres :5432 · Redis :6379 · MinIO :9000/:9001
```

### 2. Run the backend

```bash
cd backend
dotnet restore
dotnet ef database update -p src/NovaLearn.Persistence -s src/NovaLearn.API
dotnet run --project src/NovaLearn.API
# Swagger → https://localhost:7001/swagger
```

### 3. Run the frontend

```bash
cd frontend
npm install
npm run dev                    # http://localhost:5173
```

### 4. Run tests

```bash
cd backend && dotnet test
cd frontend && npm test
```

---

## Design System

| Token | Value |
|-------|-------|
| Primary | `#8B5CF6` |
| Accent | `#A78BFA` |
| Success | `#22C55E` · Danger `#EF4444` · Warning `#F59E0B` |
| Surface | `#FFFFFF` / `#F8FAFC` · Text `#1F2937` |

---

## License

Proprietary — © NovaLearn. All rights reserved.
