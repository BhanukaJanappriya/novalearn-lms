# NovaLearn LMS

> _Enterprise learning, engineered like enterprise software: a Clean Architecture .NET core and a premium, data driven React admin control center._

**Backend**
&nbsp;
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-9.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-12-239120?style=flat-square&logo=csharp&logoColor=white)
![EF Core](https://img.shields.io/badge/EF_Core-9.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?style=flat-square&logo=postgresql&logoColor=white)
![Redis](https://img.shields.io/badge/Redis-7-DC382D?style=flat-square&logo=redis&logoColor=white)
![JWT](https://img.shields.io/badge/JWT-Auth-000000?style=flat-square&logo=jsonwebtokens&logoColor=white)
![MediatR](https://img.shields.io/badge/MediatR-CQRS-8B5CF6?style=flat-square)

**Frontend**
&nbsp;
![React](https://img.shields.io/badge/React-19-61DAFB?style=flat-square&logo=react&logoColor=black)
![TypeScript](https://img.shields.io/badge/TypeScript-5-3178C6?style=flat-square&logo=typescript&logoColor=white)
![Vite](https://img.shields.io/badge/Vite-6-646CFF?style=flat-square&logo=vite&logoColor=white)
![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-3-06B6D4?style=flat-square&logo=tailwindcss&logoColor=white)
![TanStack Query](https://img.shields.io/badge/TanStack_Query-5-FF4154?style=flat-square&logo=reactquery&logoColor=white)
![Recharts](https://img.shields.io/badge/Recharts-3-22B5BF?style=flat-square)

**Project**
&nbsp;
![Architecture](https://img.shields.io/badge/Architecture-Clean_%7C_DDD_%7C_CQRS-8B5CF6?style=flat-square)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=flat-square&logo=docker&logoColor=white)
![Status](https://img.shields.io/badge/status-active_development-8B5CF6?style=flat-square)
![License](https://img.shields.io/badge/license-Proprietary-lightgrey?style=flat-square)

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
