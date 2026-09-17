# Minimal APIs vs FastEndpoints vs Controllers — Comparison (Part 7)

This is based on what was actually built in this repo: [src/MinimalApi](../src/MinimalApi)
(Minimal APIs) and [src/FastEndpointsApi](../src/FastEndpointsApi) (FastEndpoints), both
implementing the same Product CRUD behavior. Controllers weren't built for this
assignment, so that column is based on how ASP.NET Core MVC controllers work in general.

## Comparison questions

**How does endpoint registration differ among Minimal APIs, FastEndpoints, and Controllers?**
Minimal APIs register each route with a lambda call directly in `Program.cs`
(`app.MapPost("/products", ...)`), no separate class needed — that's what
[Program.cs](../src/MinimalApi/Program.cs) does. FastEndpoints registers routes implicitly:
each endpoint is its own class that declares its route and verb inside `Configure()`
(see [CreateProductEndpoint.cs](../src/FastEndpointsApi/Endpoints/Products/CreateProductEndpoint.cs)),
and `app.UseFastEndpoints()` discovers every endpoint class in the assembly automatically
— nothing is registered by hand in `Program.cs`. Controllers register routes through
attributes on controller action methods (`[HttpPost("products")]`) or conventional
routing set up once in `Program.cs`, with one controller class typically grouping every
action for a resource.

**How is dependency injection used in each style?**
All three use the same built-in ASP.NET Core DI container underneath. Minimal APIs
inject dependencies as parameters directly on the route delegate (`(IProductService
service, ...) => ...`). FastEndpoints and Controllers both use constructor injection on
a class — one endpoint class per use case in FastEndpoints (see
`ProductService productService` in the endpoint constructors), one controller class
serving several actions in Controllers.

**How are routes, request binding, validation, filters or processors, and responses handled?**
Minimal APIs: routes are strings passed to `Map*` calls; binding comes from method
parameters (route values, query, body inferred by type); there's no built-in validation
pipeline, so this project does it by hand (`ValidateProduct` in
[Program.cs](../src/MinimalApi/Program.cs)) returning `Results.ValidationProblem`; there's
no first-class "filter" concept beyond ASP.NET Core middleware and route filters.
FastEndpoints: routes and verbs are declared per endpoint class in `Configure()`;
binding is automatic from route + JSON body into the request DTO's properties by name;
validation is first-class via `Validator<TRequest>` (FluentValidation-based, see
[CreateProductEndpoint.cs](../src/FastEndpointsApi/Endpoints/Products/CreateProductEndpoint.cs)),
auto-returning 400 on failure; FastEndpoints also has "pre/post processors" as a
dedicated cross-cutting-concern mechanism. Controllers: routes via attributes; binding
via `[FromRoute]`/`[FromBody]`/`[FromQuery]` or convention; validation typically via Data
Annotations plus `ModelState.IsValid` (auto-400 with `[ApiController]`); filters
(`IActionFilter`, `IExceptionFilter`, etc.) are a mature, well-documented pipeline.

**Which style has the fewest dependencies?**
Minimal APIs — it's built into `Microsoft.AspNetCore.App`, no extra NuGet package
needed at all (confirmed by [MinimalApi.csproj](../src/MinimalApi/MinimalApi.csproj),
which only adds EF Core and Serilog, nothing routing-related). FastEndpoints needs the
third-party `FastEndpoints` package. Controllers are also built-in, but pull in the
larger MVC framework machinery even for simple JSON APIs.

**Which style provides the strongest built-in project convention?**
FastEndpoints, deliberately — one class per endpoint, structured request/response/
validator per use case, and an opinionated file/naming convention the framework nudges
you toward (visible in this project's [Endpoints/Products/](../src/FastEndpointsApi/Endpoints/Products/)
folder). Controllers have a strong convention too (one controller class per resource,
actions inside it) but it's coarser-grained than FastEndpoints' one-class-per-use-case
model. Minimal APIs impose no structure at all — [Program.cs](../src/MinimalApi/Program.cs)
would grow unboundedly without deliberate discipline (this project used `MapGroup` and a
separate service layer to keep it manageable).

**What third-party dependency risk is introduced by FastEndpoints?**
It's a community-maintained open-source package, not a Microsoft-owned or -supported
part of ASP.NET Core. That means its release cadence, breaking-change policy, and
long-term support are independent of .NET's own lifecycle — confirmed directly in this
project: FastEndpoints 8.3.0 renamed its core response API (`SendOkAsync(...)` →
`Send.OkAsync(...)`) between versions, which broke code written against older
documentation and had to be fixed during Stage 3. A team adopting it takes on that
external release risk in exchange for the convention and productivity it provides.

**Which style would you choose for a small service, a vertical-slice application, and an existing controller-based system?**
Small service: **Minimal APIs** — no extra dependency, the whole thing readably fits in
a small `Program.cs` plus a couple of files, matching this project's MinimalApi. Vertical-slice
application: **FastEndpoints** — it's built around exactly that idea, one folder/class per
feature/use case (request, validator, handler, response together), which is what
[Endpoints/Products/](../src/FastEndpointsApi/Endpoints/Products/) demonstrates. Existing
controller-based system: **Controllers** — introducing a second routing style into a
mature MVC codebase adds cognitive overhead and inconsistency for the team; extending
what's already there is usually the pragmatic choice unless there's a strong, specific
reason to migrate.

Performance alone isn't the basis for any of these picks — see the maintainability, team
familiarity, testing, and ecosystem points in the table and recommendation below.

## Comparison table

| Criterion | Minimal APIs | FastEndpoints | Controllers |
|---|---|---|---|
| **Routing** | Routes declared inline as `Map*` calls in `Program.cs`; simple, but can get crowded as the app grows (this project used `MapGroup("/products")` to keep it tidy). | Route + verb declared per endpoint class in `Configure()`; one route per class, auto-discovered — nothing centralized to maintain. | Routes via attributes on controller actions, or centralized conventional routing; well understood by any ASP.NET Core developer. |
| **Dependency injection** | Injected as parameters directly on the route delegate. | Constructor injection on each endpoint class (one class per use case). | Constructor injection on the controller class (shared across all its actions). |
| **Validation** | No built-in pipeline — this project wrote a manual `ValidateProduct` helper returning `Results.ValidationProblem`. | First-class `Validator<TRequest>` (FluentValidation) per request type, wired in automatically, auto-400 on failure. | Typically Data Annotations + `ModelState.IsValid`, auto-400 via `[ApiController]`; FluentValidation also usable but not built-in. |
| **Organization** | Everything can live in `Program.cs` and a thin service layer; no imposed structure. | One class per CRUD operation (request/validator/handler together) — strong, explicit convention (vertical-slice friendly). | One controller class per resource, holding every action for it — resource-centric, not use-case-centric. |
| **Dependencies** | None beyond ASP.NET Core itself. | Adds the third-party `FastEndpoints` NuGet package. | None beyond ASP.NET Core itself (full MVC framework). |
| **Best use cases** | Small APIs, prototypes, microservices with few endpoints. | Larger APIs organized around many discrete use cases; teams that want enforced per-endpoint structure and built-in validation. | Larger, more traditional APIs; teams already invested in MVC conventions, filters, and tooling. |
| **Trade-offs** | Fast to start, but no built-in validation or per-endpoint structure — discipline is manual. | Strong structure and validation out of the box, at the cost of a third-party dependency and its own release/versioning risk. | Mature, huge ecosystem and community knowledge, but more ceremony (attributes, base class, filter pipeline) for simple endpoints. |

## Final recommendation

For this assignment's scope — a small, single-resource CRUD API — **Minimal APIs** was
the more pleasant style to write and read: no extra dependency, and the whole request
flow (route → service → EF Core) stays visible in a handful of files. That said,
**FastEndpoints** earned its keep too: the built-in validator pipeline removed the
hand-rolled `ValidateProduct` helper, and the one-class-per-use-case convention would
scale better than Minimal APIs once the number of endpoints grows past a handful and
multiple people are touching the same resource's operations. If this were a bigger,
longer-lived API with a growing team, FastEndpoints' enforced structure would likely pay
for itself faster than the cost of the extra dependency — but for a small service like
this one, Minimal APIs' zero-dependency simplicity wins on maintainability and team
familiarity (every ASP.NET Core developer already knows it, no framework-specific
conventions to learn). Testing both was comparable in practice, since business logic
lived in the shared, framework-independent `ProductService` in both projects either way.
