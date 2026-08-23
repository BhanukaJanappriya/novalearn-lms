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
| **Courses** (CRUD, role-secured ownership) | ✅ | ✅ | ✅ | Done |
| **Enrollment** (catalog, enrol/unenrol, progress, roster) | ✅ | ✅ | ✅ | Done |
| **Content** (modules, lessons, course builder) | ✅ | ✅ | ✅ | Done |
| **Student dashboard** (progress, subjects, activity, suggestions) | ✅ | ✅ | ✅ | Done |
| **User management** (directory, roles, activation, verification) | ✅ | ✅ | ✅ | Done |
| **Assignments & Gradebook** (author, submit, mark, grade grid) | ✅ | ✅ | ✅ | Done |
| **Quizzes** (5 question types, timed attempts, auto-marking, essays marked by hand) | ✅ | ✅ | ✅ | Done |
| **Real-time** (SignalR notifications, live badge and toast) | ✅ | ✅ | ✅ | Done |
| **Departments** (9 science departments, heads, course assignment) | ✅ | ✅ | ✅ | Done |
| **Profiles & people directory** (own picture only, read-only student and lecturer views) | ✅ | ✅ | ✅ | Done |
| **Assessment hub** (cross course marking queue, deadlines, drafts, scoped per lecturer) | ✅ | ✅ | ✅ | Done |
| **Content wall** (PDF/video/image uploads, YouTube and Drive links, thumbnails) | ✅ | ✅ | ✅ | Done |
| **Analytics** (period trends, course and department performance, mark distribution) | ✅ | ✅ | ✅ | Done |
| **Finance** (Stripe Checkout, webhook-confirmed enrolment, refunds, revenue ledger) | ✅ | ✅ | ✅ | Done |
| **Settings** (branding, registration, maintenance mode, checkout currency, upload limit) | ✅ | ✅ | ✅ | Done |
| **Support portal** (tickets, threaded replies, internal notes, triage queue) | ✅ | ✅ | ✅ | Done |
| **Reports** (exportable enrolment, revenue, course, user and ticket reports, run audit trail) | ✅ | ✅ | ✅ | Done |

---

## Architecture

Clean / Onion Architecture - dependencies point inward.

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
npm run dev                    
```

### 4. Run tests

```bash
cd backend && dotnet test
cd frontend && npm test
```

### 5. Payments (Stripe) — optional, only needed to exercise checkout

Course purchase runs on Stripe Checkout in test mode. The app runs fine without this section —
free courses, and everything else, work regardless — but a paid course's "Pay & enroll" button
will fail at the gateway until it's set up.

1. Create a free Stripe account and switch to **test mode**: <https://dashboard.stripe.com/register>
2. Grab the test secret key from <https://dashboard.stripe.com/test/apikeys> and store it with
   `dotnet user-secrets`, **never** in a committed `appsettings*.json`:
   ```bash
   cd backend
   dotnet user-secrets set "Stripe:SecretKey" "sk_test_..." --project src/NovaLearn.API
   ```
3. Install the [Stripe CLI](https://docs.stripe.com/stripe-cli) and forward webhooks to the local
   API — the route is versioned, so the path has to be exact:
   ```bash
   stripe login
   stripe listen --forward-to https://localhost:7001/api/v1/payments/webhook
   ```
   The CLI prints a webhook signing secret (`whsec_...`) when it starts. Store that too:
   ```bash
   dotnet user-secrets set "Stripe:WebhookSecret" "whsec_..." --project src/NovaLearn.API
   ```
4. Restart the backend so it picks up the secrets, then buy a priced course using
   [any Stripe test card](https://docs.stripe.com/testing#cards) (`4242 4242 4242 4242`, any
   future expiry, any CVC). Keep `stripe listen` running throughout — without it, Stripe has
   nowhere to deliver the webhook that actually confirms the payment and creates the enrolment.

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

Proprietary - © NovaLearn. All rights reserved.
