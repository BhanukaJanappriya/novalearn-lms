# ADR-0002: Placement of the ASP.NET Identity user type

- **Status:** Accepted
- **Date:** 2026-07-08
- **Context:** Authentication vertical slice

## Context

`ApplicationUser` is both a domain aggregate (it owns profile data, refresh tokens, auditing
and soft-delete state) and the ASP.NET Identity principal (`IdentityUser<Guid>`). Clean
Architecture says the Domain layer should not depend on frameworks. Strictly following that would
push `ApplicationUser` into Infrastructure and force a parallel pure-domain `User` entity kept in
sync via mapping.

## Decision

Keep `ApplicationUser : IdentityUser<Guid>` in the **Domain** layer. Domain references only
`Microsoft.Extensions.Identity.Stores` (the abstractions package that defines `IdentityUser`),
not EF Core or ASP.NET Core.

## Consequences

- **Positive:** One source of truth for the user aggregate; no dual-entity mapping; refresh tokens
  and invariants live with the entity. This is the widely-used pattern in production .NET Clean
  Architecture solutions.
- **Negative:** Domain takes a lightweight dependency on the Identity abstractions. We accept this
  as pragmatic — the dependency is on stable, framework-neutral base types, and the Application
  layer still talks to Identity only through the `IIdentityService` port.
- The Application layer remains free of `UserManager`/`SignInManager`; those stay behind
  `IIdentityService` in Infrastructure.
