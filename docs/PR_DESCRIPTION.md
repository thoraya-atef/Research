# ASP.NET Core Research Assignment — Configuration, Secrets, Kestrel, Logging, API Styles, EF Core, DI, HTTPS

Paste this as the PR description once you create the GitHub repo, push
`feature/aspnet-core-research-assignment`, and open the PR against `master`
(`git push -u origin feature/aspnet-core-research-assignment`, then
`gh pr create --base master --head feature/aspnet-core-research-assignment`).

## Summary

Two ASP.NET Core 10 Web APIs implementing identical Product CRUD behavior — one with
Minimal APIs, one with FastEndpoints — backed by SQL Server/EF Core, User Secrets,
Serilog, and explicit HTTPS Kestrel endpoints, plus the research write-up, comparison,
and Postman evidence the assignment requires. No Swagger/OpenAPI anywhere; see
Compliance Evidence in the README.

## Hard constraints checklist

- [x] .NET 10 — confirmed via `dotnet --version` (10.0.302) before Stage 0.
- [x] SQL Server (not SQLite/InMemory) — `Microsoft.EntityFrameworkCore.SqlServer` in
      both `.csproj` files.
- [x] Zero Swagger/OpenAPI — the .NET 10 template's default `Microsoft.AspNetCore.OpenApi`
      package and `AddOpenApi()`/`MapOpenApi()` calls were removed from MinimalApi
      immediately in Stage 2, with the diff shown before continuing. FastEndpointsApi was
      scaffolded from the `web` (empty) template, which never had OpenApi to begin with.
      Verified with `git grep -i "swagger"` — no matches (README, Compliance Evidence).
- [x] `.gitignore` created before the first commit (Stage 0, commit `c9cd42d`).
- [x] SQL Server connection string in User Secrets only — never written to any tracked
      file; verified with `git grep -iE "password=|pwd="` (README, Compliance Evidence).
- [x] Postman only for testing — no Swagger UI, collection + two environments in
      `postman/`.

## Requirement → evidence mapping

| Assignment requirement | Where it's satisfied |
|---|---|
| Part 1 — User Secrets research | [`docs/research-answers.md`](../docs/research-answers.md) § Part 1 |
| Part 2 — Environment Variables research | [`docs/research-answers.md`](../docs/research-answers.md) § Part 2 |
| Part 3 — appsettings / configuration sources research | [`docs/research-answers.md`](../docs/research-answers.md) § Part 3 |
| Part 4 — Kestrel configuration research | [`docs/research-answers.md`](../docs/research-answers.md) § Part 4 |
| Part 5 — Logging, ILogger, Serilog research | [`docs/research-answers.md`](../docs/research-answers.md) § Part 5 |
| Part 6 — Minimal API Product CRUD | [`src/MinimalApi`](../src/MinimalApi) — entity/DbContext/DTOs/service/endpoints; migration in `src/MinimalApi/Migrations` |
| Part 7 — FastEndpoints project | [`src/FastEndpointsApi`](../src/FastEndpointsApi) — one endpoint class per CRUD operation, `Validator<T>` validators, equivalent routes/status codes to MinimalApi |
| Part 7 — Comparison questions, table, recommendation | [`docs/comparison-table.md`](../docs/comparison-table.md) |
| Part 7 — Verify equivalent routes/behavior between implementations | Same Postman collection run unmodified against both projects, 14/14 passing on each — see README § Postman Evidence and `docs/postman-evidence/` screenshots |
| Part 8 — HTTPS research | [`docs/research-answers.md`](../docs/research-answers.md) § Part 8 |
| Part 8 — HTTPS implementation (Kestrel HTTP+HTTPS endpoints, redirection middleware, dev cert) | `Kestrel:Endpoints` in both projects' `appsettings.Development.json`; `UseHttpsRedirection()` placement in both `Program.cs` files; `dotnet dev-certs https --trust` run by the developer (not by the assistant, per constraint) |
| Part 9 — Postman only, no Swagger | [`postman/Product.postman_collection.json`](../postman/Product.postman_collection.json) + two environment files; README § Compliance Evidence |
| DI lifetime choice + justification | README § DI lifetime justification |
| `AsNoTracking` justification | README § AsNoTracking justification |
| EF Core migration files | `src/MinimalApi/Migrations`, `src/FastEndpointsApi/Migrations` |
| Comparison table (7 rows × 3 columns) | [`docs/comparison-table.md`](../docs/comparison-table.md) § Comparison table |
| Final recommendation | [`docs/comparison-table.md`](../docs/comparison-table.md) § Final recommendation |
| Evidence: no secret committed, no Swagger | README § Compliance Evidence (real `git grep`/`git log` output, scoped to exclude the README's own self-referential text) |

## Notable issues found and fixed along the way

- FastEndpoints 8.3.0 renamed its response API mid-project
  (`SendOkAsync(...)` → `Send.OkAsync(...)`); fixed by checking the package's own XML docs.
- FastEndpoints' default endpoint auto-discovery excludes assemblies whose name starts
  with `"FastEndpoints"` — which matched this project's own assembly name
  (`FastEndpointsApi`) and silently found zero endpoints. Fixed by explicitly passing
  the entry assembly to `AddFastEndpoints`.
- FastEndpoints attaches authorization metadata to every endpoint by default; without an
  auth scheme configured, every request 500'd until `AllowAnonymous()` was added to all
  five endpoints.
- A Postman collection variable/type mismatch (`productId` stored as a JS number,
  compared against a stringified response id) caused one flaky assertion; fixed by
  storing it as a string at the point of creation.
- The FastEndpointsApi design-time migration factory's placeholder database name
  (`FastEndpointsApiDb`) didn't match the name the developer actually chose in User
  Secrets (`FastEndpointsDb`), so the migration had been applied to the wrong database.
  Applied to the correct one via `dotnet ef database update --connection "..."`, and
  documented the gotcha in the README so it doesn't happen to anyone else.

🤖 Generated with [Claude Code](https://claude.com/claude-code)
