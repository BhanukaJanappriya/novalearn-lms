# Authentication Slice — Design

The first vertical slice implements account registration, email verification, login with JWT +
rotating refresh tokens, and session refresh. It exercises every architectural layer end-to-end.

## Data model

```mermaid
erDiagram
    Users ||--o{ RefreshTokens : owns
    Users }o--o{ Roles : "via UserRoles"

    Users {
        uuid Id PK
        string Email UK
        string FirstName
        string LastName
        bool   EmailConfirmed
        bool   IsActive
        bool   IsDeleted
        timestamptz CreatedAtUtc
    }
    RefreshTokens {
        uuid Id PK
        uuid UserId FK
        string TokenHash UK
        string JwtId
        timestamptz ExpiresAtUtc
        timestamptz RevokedAtUtc
        string ReplacedByTokenHash
    }
    Roles {
        uuid Id PK
        string Name UK
        string Description
        bool   IsSystemRole
    }
```

Only the **SHA-256 hash** of each refresh token is stored. Tokens rotate on every use; presenting an
already-rotated token is treated as theft and revokes the user's whole active token chain.

## Login + refresh flow

```mermaid
sequenceDiagram
    participant SPA as React SPA
    participant API as AuthController
    participant App as MediatR Handler
    participant Id as IIdentityService
    participant DB as PostgreSQL

    SPA->>API: POST /auth/login (email, password)
    API->>App: LoginCommand
    App->>Id: ValidateCredentials
    Id->>DB: find user, check password + state
    Id-->>App: AuthenticatedUser
    App->>App: issue JWT + refresh token (store hash)
    App-->>API: AuthenticationResponse
    API-->>SPA: 200 { accessToken } + httpOnly refresh cookie

    Note over SPA,API: access token expires
    SPA->>API: POST /auth/refresh (cookie)
    API->>App: RefreshTokenCommand
    App->>DB: lookup by token hash
    App->>App: rotate (revoke old, issue new)
    API-->>SPA: 200 { accessToken } + new refresh cookie
```

## Layer responsibilities

| Concern | Layer | Type |
|---|---|---|
| Password/lockout/roles | Infrastructure | `IdentityService` (behind `IIdentityService`) |
| JWT signing | Infrastructure | `JwtTokenService` |
| Token rotation policy | Application | `RefreshTokenCommandHandler`, `AuthTokenIssuer` |
| Refresh token storage | Persistence | `RefreshTokenRepository`, EF Core |
| HTTP, cookies, rate limiting | API | `AuthController`, middleware |

## Security properties

- Refresh tokens stored hashed; rotated on use; replay revokes the chain.
- Access tokens short-lived (15 min); refresh cookie is `httpOnly`.
- Login is rate-limited (10/min/IP) and increments Identity lockout counters.
- Credential errors are generic to prevent account enumeration.
- Email must be verified before first login.
