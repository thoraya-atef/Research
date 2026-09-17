# ASP.NET Core Research Assignment — Research Answers (Parts 1–5, 8)


# Part 1 — User Secrets

### 1. What are User Secrets in ASP.NET Core?
User Secrets (the Secret Manager tool) is a way to store sensitive development-time data
— like connection strings or API keys — outside the project folder itself. Values are
stored as a JSON file in the user profile on the local machine, not inside the repo, so
they can't accidentally be committed. It's meant for local development only, not as any
kind of production secret store.

Source: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 2. What problem do User Secrets solve?
They solve the problem of secrets ending up inside appsettings.json and getting
committed to source control by accident. Because the file lives outside the project
directory, there's no risk of it being pushed to git or shared with the whole team the
way tracked files are.

Source: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 3. Where are User Secrets stored on Windows, Linux, and macOS?
On Windows: `%APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json`. On Linux
and macOS: `~/.microsoft/usersecrets/<user_secrets_id>/secrets.json`. The
`<user_secrets_id>` portion is the same value as the `UserSecretsId` element written into
the project's .csproj file.

Source: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 4. Are User Secrets encrypted? Are they considered a trusted production secret store?
No. Secret Manager does not encrypt the values — they're stored as plain text inside a
JSON file. Microsoft explicitly warns it "shouldn't be treated as a trusted store" and
that it's for development purposes only, so it is never appropriate as a production
secret store.

Source: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 5. How is a project connected to its User Secrets store? Research UserSecretsId.
Running `dotnet user-secrets init` adds a `<UserSecretsId>` element inside a
`<PropertyGroup>` in the .csproj file, with a randomly generated GUID unique to that
project. This GUID links the project to its secrets.json folder on disk, so even if the
project is copied elsewhere, it still resolves to the same secrets as long as the
UserSecretsId stays the same.

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <UserSecretsId>0000a1a1-b2b2-c3c3-d4d4-eeeeee555555</UserSecretsId>
</PropertyGroup>
```

Source: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 6. Research the commands used to initialize, set, list, remove, and clear secrets.
Commands run from the project directory (where the .csproj lives):

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<value>"
dotnet user-secrets list
dotnet user-secrets remove "ConnectionStrings:DefaultConnection"
dotnet user-secrets clear
```

`init` generates the UserSecretsId, `set` adds or updates one secret (the colon `:`
represents a hierarchical key), `list` shows every current secret, `remove` deletes one
secret by its key, and `clear` deletes all secrets at once.

Source: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 7. How does an ASP.NET Core application read a secret through IConfiguration?
When the environment is Development, `WebApplicationBuilder` registers the User Secrets
configuration source automatically (via `AddUserSecrets`), so it's read like any other
configuration source, through the normal `IConfiguration` indexer:

```csharp
var builder = WebApplication.CreateBuilder(args);
var apiKey = builder.Configuration["Movies:ServiceApiKey"];
```

Source: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 8. When should User Secrets be used?
When you need to run the app locally against real resources (a local SQL Server with
credentials, a test API key) without putting that value in appsettings.json or exposing
it to the rest of the team through git. It's best suited for local development on the
developer's own machine only.

Source: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 9. When should User Secrets not be used?
Never in production or in any deployed environment, and never as a way to share secrets
between team members — they're local to the machine and don't sync. Don't treat them as
a backup or audit trail either, since they're unencrypted and easy to read for anyone
with access to the user profile files.

Source: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 10. Why must User Secrets not contain production credentials?
Because they're unencrypted plain text on the developer's machine and were never
designed as a secure store — anyone with access to that machine or file can read the
credentials directly. Production needs a stronger source, such as environment variables
or a managed secret store (e.g. Azure Key Vault), with controlled access and a clear
rotation policy.

Source: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

---

# Part 2 — Environment Variables

### 1. What is an environment variable?
A variable stored in the process environment of the operating system, shell, or
container, readable at runtime by any process running inside it. It's not part of the
code or the project files, so it lets you change app behavior without rebuilding.

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0

### 2. Who can create environment variables: the operating system, terminal, IDE, container, or cloud host?
All four, actually: the operating system sets system-wide variables (like PATH), a
terminal session can set temporary ones, an IDE (such as Visual Studio) injects them via
launchSettings.json, a container runtime (Docker) injects them via `ENV` or `docker run
-e`, and a cloud host (such as Azure App Service) injects them through its application
settings panel.

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-10.0

### 3. How does ASP.NET Core load environment variables into IConfiguration?
`WebApplicationBuilder` registers the Environment Variables Configuration Provider as one
of the default configuration sources. It reads every environment variable available to
the process and maps it into `IConfiguration` keys, the same way any other source does.

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0

### 4. How are hierarchical configuration keys represented in environment variables? Research the double underscore separator.
`IConfiguration` normally uses a colon `:` for hierarchy (e.g.
`ConnectionStrings:DefaultConnection`), but not every platform supports `:` in an
environment variable name — Bash doesn't. So every platform supports the double
underscore `__` instead, which is automatically converted to `:` when configuration is
read:

```bash
export ConnectionStrings__DefaultConnection="Server=...;"
```

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0

### 5. How can environment variables be set temporarily in PowerShell, Command Prompt, Bash, Visual Studio, Docker, and Azure App Service?
PowerShell: `$Env:ASPNETCORE_ENVIRONMENT = "Staging"` (current session only). Command
Prompt: `set ASPNETCORE_ENVIRONMENT=Staging`. Bash: `export
ASPNETCORE_ENVIRONMENT=Staging`. Visual Studio: through a launch profile in
launchSettings.json (`environmentVariables` section). Docker: via `ENV` in the
Dockerfile, or `-e ASPNETCORE_ENVIRONMENT=Staging` with `docker run`, or the
`environment` block in docker-compose.yml. Azure App Service: Settings > Configuration >
Application settings, automatically exposed as an environment variable at runtime.

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-10.0

### 6. What is ASPNETCORE_ENVIRONMENT, and what values are normally used?
An environment variable that sets the app's runtime environment, read by the host at
startup. The usual values are Development, Staging, and Production, though technically
any string value is accepted. If it isn't set, the app defaults to Production.

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-10.0

### 7. What is the difference between an environment name and an environment variable?
The environment variable is the mechanism — an OS-level variable like
`ASPNETCORE_ENVIRONMENT` — that carries a value into the app. The environment name is the
value itself (such as "Development") that drives app behavior through
`IHostEnvironment.EnvironmentName`. In short: the variable is the transport, the name is
the payload it carries.

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-10.0

### 8. Do environment variables automatically encrypt secrets?
No. Environment variables are usually stored as plain, unencrypted text, so anyone with
access to the machine or process can read them directly. Microsoft explicitly warns
about this and recommends extra measures if stronger protection is needed.

Source: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 9. When are environment variables more suitable than User Secrets?
In deployment environments (staging, production, CI/CD, containers), because they're
available in any normal runtime environment and aren't tied to one developer's machine
the way User Secrets are. Most hosting platforms (Docker, Kubernetes, Azure App Service)
are already built around injecting environment variables at deploy time.

Source: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 10. What risks exist when sensitive values are stored directly in environment variables?
They can show up in process listings, crash dumps, or uncontrolled logs, and any other
process or user with access to the same machine may be able to see them. If the app
accidentally logs the whole environment (for example while debugging), the secret leaks
— and since it's unencrypted to begin with, there's no extra layer of protection.

Source: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

---

# Part 3 — appsettings Files and Configuration Sources

### 1. What is appsettings.json, and when is it loaded?
A JSON file holding an app's general, non-secret settings (such as log levels or feature
flags). It's loaded very early, during `WebApplicationBuilder` construction, as one of
the default configuration sources, and it applies to every environment.

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0

### 2. What is appsettings.{Environment}.json? Give examples for Development, Staging, and Production.
An additional settings file scoped to one specific environment. It's loaded right after
appsettings.json, so it can override or add keys on top of it. Examples:
`appsettings.Development.json` (verbose logging, detailed exception pages),
`appsettings.Staging.json` (production-like settings but test data),
`appsettings.Production.json` (higher log level, no exception details shown to users).

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0

### 3. Research the default configuration precedence in ASP.NET Core.
ASP.NET Core loads configuration sources in a fixed order, and the underlying rule is
simple: **the source added last wins** if the same key exists in more than one place.
In registration order — earliest added first, last added at the bottom:

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. User Secrets (Development only)
4. Environment variables (not prefixed with `ASPNETCORE_` or `DOTNET_`)
5. Command-line arguments

Command-line arguments are registered last — the Command-line Configuration Provider is
actually added twice, once early and again right at the end specifically so it can
override everything else — which is why a command-line value always wins on a
conflicting key.

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0

### 4. Which provider wins when the same key exists in appsettings.json, User Secrets, an environment variable, and the command line?
The command-line argument wins, because it's the last source added. Environment
variables come next, then User Secrets, then appsettings.json last. Same rule as above:
whichever source was added most recently to the configuration builder takes priority.

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0

### 5. What kinds of safe settings belong in appsettings.json?
Non-sensitive settings such as: minimum log levels, feature flags, public endpoint
names, timeouts, pagination defaults — basically anything that would cause no security
problem if it leaked into the repo.

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0

### 6. What values must never be committed to appsettings.json?
Passwords, connection strings that contain credentials, API keys, tokens, encryption
keys, or certificate passwords. Any value like this belongs in User Secrets during
development and in environment variables / a managed secret store in production.

Source: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 7. Should a connection string always be considered a secret? Explain the difference between a credentialed and non-credentialed connection string.
Not always. A non-credentialed connection string (like LocalDB with
`Trusted_Connection=True`, relying on Windows Integrated Security) has no visible
password, so it isn't as sensitive. A credentialed connection string — one containing
`User Id=...;Password=...` explicitly — must be treated as a full secret, because it
grants direct access to the database.

Source: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 8. When should a developer use User Secrets instead of environment variables?
When working locally on a personal machine and wanting the setting to stay private to
that machine — not shared with the whole team the way a server-level environment
variable might be — with easy management through `dotnet user-secrets` commands instead
of editing system settings by hand.

Source: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 9. When should a deployment use environment variables or a managed secret store instead of User Secrets?
In any real deployment (staging/production), because User Secrets aren't even available
there — they're tied to the developer's machine and aren't published with the app.
Deployments should use environment variables injected by the hosting platform, or better,
a managed secret store such as Azure Key Vault for stronger secrecy and rotation.

Source: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 10. Research IConfiguration and the Options pattern. When is each approach appropriate?
`IConfiguration` is good for quick, direct access to a single value or a simple key
(`config["Key"]`). The Options pattern (`IOptions<T>`, `IOptionsSnapshot<T>`,
`IOptionsMonitor<T>`) is better when you have a related group of settings and want them
bound to a strongly typed class with DI support. `IOptions<T>` loads once
(singleton-like), `IOptionsSnapshot<T>` refreshes per request (scoped), and
`IOptionsMonitor<T>` supports live updates even inside singleton services.

```csharp
builder.Services.Configure<MovieSettings>(
    builder.Configuration.GetSection("Movies"));
```

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-10.0

---

# Part 4 — Kestrel Configuration

### 1. What is Kestrel, and what role does it play in an ASP.NET Core application?
Kestrel is the default, cross-platform web server built into ASP.NET Core, built on
managed sockets. Its role is to accept connections (HTTP/HTTPS) and hand them off to the
app's request pipeline. Default project templates use it either standalone as an edge
server or behind a reverse proxy.

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-10.0

### 2. What is a Kestrel endpoint?
A listening point that Kestrel defines — a combination of address, port, and protocol
(plus a certificate if it's HTTPS) — so the server can accept connections there. An app
can have more than one endpoint active at the same time, e.g. one HTTP and one HTTPS.

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-10.0

### 3. Which properties can be configured for an endpoint: address, port, protocol, and certificate?
All four are configurable: the address (localhost, a specific IP, or 0.0.0.0), the port
(numeric), the protocol (Http1, Http2, or both), and the certificate for HTTPS endpoints
(its path and password, or falling back to the Development certificate).

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-10.0

### 4. Research the supported ways to configure Kestrel endpoints: appsettings, environment variables, command line, and C# code.
All four are supported: appsettings.json via a `"Kestrel": { "Endpoints": {...} }`
section, environment variables with the same names in `Kestrel__Endpoints__Https__Url`
form, the command line via `--urls` or explicit Kestrel keys, and C# code via
`builder.WebHost.ConfigureKestrel(...)` with `serverOptions.Listen(...)` for full
programmatic control.

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-10.0

### 5. Why might a team configure Kestrel in appsettings.json?
It's declarative and easy to read, lets you change ports or certificates without
recompiling, supports different settings per environment via
appsettings.{Environment}.json, and integrates automatically with the rest of the
configuration system, including reload-on-change.

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-10.0

### 6. When is appsettings.json not the best place for final production endpoint configuration?
When the orchestrator or hosting platform (Kubernetes, Docker, Azure) determines ports
and bindings dynamically at deploy time — here it's better for those values to come from
environment variables or the platform itself, not be baked into a file that ships with
the code. Also, if TLS is handled by a reverse proxy, there's no need for certificate
details in Kestrel's appsettings.json section at all.

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-10.0

### 7. What is the difference between localhost, 0.0.0.0, and a specific IP address when binding Kestrel?
`localhost` tries to bind both the IPv4 and IPv6 loopback interfaces, reachable only from
the same machine. `0.0.0.0` binds to every available IPv4 address, so it accepts
connections from any network — useful for containers or servers. A specific IP address
binds only to the network interface it represents, useful for restricting access to one
interface.

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-10.0

### 8. How can separate HTTP and HTTPS endpoints be configured?
By defining two named entries under the `Kestrel:Endpoints` section in appsettings.json
— one with an `http://...` URL and another with an `https://...` URL (with an optional
Certificate section for HTTPS) — or programmatically, by calling
`serverOptions.Listen(...)` twice: once plain, once with `listenOptions.UseHttps(...)`.

```json
"Kestrel": {
  "Endpoints": {
    "Http": { "Url": "http://localhost:5000" },
    "Https": { "Url": "https://localhost:5001" }
  }
}
```

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-10.0

### 9. What happens when an HTTPS endpoint has no explicit certificate in Development?
Kestrel falls back in this order: first, the certificate defined on the endpoint itself;
then the default certificate under `Certificates:Default`; then the ASP.NET Core
Development certificate auto-generated by the SDK, if it exists and is trusted. If none
of the three exist, the server throws an exception and fails to start.

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-10.0

### 10. Where should a certificate path be stored? Where should its password be stored?
The certificate path can go in appsettings.json since it's not a secret, just a file
path, and it should be relative to the app's content root. The password must come from
User Secrets in development (`Kestrel:Endpoints:Https:Certificate:Password`) or a
managed secret store such as Azure Key Vault in production — never appsettings.json.

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-10.0

### 11. How does Kestrel hosting differ when IIS, Nginx, a load balancer, or an ingress terminates TLS?
When TLS is terminated at a reverse proxy (IIS, Nginx, a load balancer, or a Kubernetes
ingress), Kestrel itself receives plain HTTP traffic from the proxy over the internal
network, and only the proxy needs a real X.509 certificate. Forwarded headers middleware
must be enabled so Kestrel knows the original request scheme and IP. This simplifies
certificate management and load balancing compared to Kestrel acting as the edge server
and terminating TLS itself.

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-10.0

---

# Part 5 — Logging, ILogger, and Serilog

### 1. What is application logging, and why is it required in production systems?
Logging records events that happen while an app runs — requests, errors, state changes —
somewhere they can be reviewed later. In production it's essential because you can't
attach a debugger to a live server; logs are the main way to find out what happened when
something goes wrong, and they're also used for monitoring, alerting, and performance
analysis.

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-10.0

### 2 & 3. Every .NET log level (Trace, Debug, Information, Warning, Error, Critical, None) + two realistic situations each

| Level | Value | Description | Two realistic situations |
|---|---|---|---|
| Trace | 0 | Most detailed messages possible, may contain sensitive data, disabled by default, must not be enabled in production | (1) Printing the internal step-by-step values of a price calculation algorithm. (2) Tracing every call to an internal method while debugging a complex bug. |
| Debug | 1 | Useful for development and debugging, high volume — use with caution in production | (1) Logging the exact SQL query EF Core is about to send, before it executes. (2) Logging an internal parameter value while building a new endpoint. |
| Information | 2 | Tracks the general flow of the app, has long-term value | (1) "Product {Id} created successfully." (2) "Application started, listening on port 5001." |
| Warning | 3 | An unexpected event, but not a full failure | (1) A request to create a product with a negative price was rejected by validation. (2) A query took longer than expected but still succeeded. |
| Error | 4 | Failure of the current operation or request, not the whole app | (1) A database connection failed while handling a request. (2) An unhandled exception while processing a POST request. |
| Critical | 5 | Failure that needs immediate attention | (1) Disk space completely exhausted. (2) The app fails to start because a critical database dependency is completely unreachable. |
| None | 6 | No messages are written at all | Used to turn a specific provider off entirely, not a runtime situation itself. |

Source: https://learn.microsoft.com/en-us/dotnet/core/extensions/logging

### 4. What does a configured minimum log level mean?
It's the lowest severity the app will actually record; anything below it is dropped
completely and never even reaches the provider. For example, if the minimum level is
Warning, Trace, Debug, and Information messages aren't logged at all, which reduces
noise and cost in production.

Source: https://learn.microsoft.com/en-us/dotnet/core/extensions/logging

### 5. What is ILogger? What is ILogger<T>?
`ILogger` is the base logging interface, taking a string category name set manually via
`ILoggerFactory.CreateLogger("...")`. `ILogger<T>` inherits from `ILogger` and derives
its category automatically from the full type name of `T` — so `ILogger<ProductService>`
gets the category "the full name of ProductService" without it being typed by hand,
which is why it's the one commonly used with DI.

Source: https://learn.microsoft.com/en-us/dotnet/core/extensions/logging

### 6. How is ILogger obtained through dependency injection?
By injecting `ILogger<T>` as a constructor parameter on any class registered in the DI
container; the DI container supplies it automatically because
`Microsoft.Extensions.Logging` registers `ILoggerFactory` and a ready-made
implementation for `ILogger<T>` by default.

```csharp
public class ProductService(ILogger<ProductService> logger) : IProductService
{
    // logger.LogInformation("Product {ProductId} created", id);
}
```

Source: https://learn.microsoft.com/en-us/dotnet/core/extensions/logging

### 7. What is a logging provider? Research the built-in Console and Debug providers.
A logging provider is the component that takes log messages and sends them to a
specific destination — a console screen, a file, a cloud service. The Console provider
prints messages directly to the terminal, useful when running the app locally for
real-time monitoring. The Debug provider sends messages to the Debug output window in
tools such as Visual Studio, via `System.Diagnostics.Debug`.

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-10.0

### 8. What is structured logging? Compare message templates with string interpolation.
Structured logging keeps parameter values as separate fields instead of merging them
into one string, using message templates with named placeholders (like `{ProductId}`)
whose values are passed as separate arguments. This lets a provider store the values as
searchable, queryable fields. String interpolation (like `$"Product {id} created"`)
merges everything into one final string before it reaches the logger, so the ability to
filter or query on the value is lost, and the string is always built even if the log
level is disabled — worse performance.

```csharp
logger.LogInformation("Product {ProductId} created at {CreatedAt}", id, DateTime.UtcNow);
// wrong: logger.LogInformation($"Product {id} created at {DateTime.UtcNow}");
```

Source: https://learn.microsoft.com/en-us/dotnet/core/extensions/logging

### 9. What information must never be written to logs?
Passwords, full connection strings, API keys or tokens, credit card numbers, national
IDs or other sensitive PII, and any other secret. Even Trace level, which Microsoft
warns "might contain sensitive app data," must be disabled in production for exactly
this reason.

Source: https://learn.microsoft.com/en-us/dotnet/core/extensions/logging

### 10. What is Serilog, and how does it integrate with Microsoft.Extensions.Logging?
Serilog is a third-party structured logging library for .NET, offering a clean API and
more flexibility over sinks and enrichment than the built-in providers. It integrates
with Microsoft.Extensions.Logging through the Serilog.AspNetCore package, which
registers Serilog as a replacement logging provider via `builder.Services.AddSerilog()`,
so every `ILogger<T>` call in the app — injected the normal way — is routed to the
Serilog pipeline.

Source: https://github.com/serilog/serilog-aspnetcore

### 11. What is a Serilog sink?
A sink is the output destination Serilog writes log events to — console, file, a
database, or a cloud service such as Seq or Elasticsearch. One app can write to more
than one sink at the same time, for example console and file together.

Source: https://github.com/serilog/serilog

### 12. Research how to write Serilog events to the console and a file.
Configured through `LoggerConfiguration` with `.WriteTo.Console()` and
`.WriteTo.File(...)` together in the same pipeline:

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
```

Source: https://github.com/serilog/serilog-sinks-file

### 13. Research daily rolling files, file-size limits, and retained-file limits.
`rollingInterval: RollingInterval.Day` makes Serilog open a new file every day (the
filename gets a date suffix). `fileSizeLimitBytes` caps the size of a single file (1GB
by default), and with `rollOnFileSizeLimit: true` a new numbered file is created once
that limit is hit. `retainedFileCountLimit` caps how many files are kept (about 31 by
default, roughly one month), automatically deleting the oldest ones to save disk space.

Source: https://github.com/serilog/serilog-sinks-file

### 14. Research request logging in Serilog.AspNetCore.
`UseSerilogRequestLogging()` middleware replaces ASP.NET Core's default verbose
per-request logging with a single consolidated event per request (like "HTTP GET
/products responded 200 in 45ms") instead of dozens of lines per request. It must be
placed in the pipeline before anything you want it to measure or log (such as
routing/endpoints), since it doesn't time anything that ran before it.

Source: https://github.com/serilog/serilog-aspnetcore

### 15. When are local log files unsuitable, especially in containers or multiple application instances?
When the app runs in containers or an orchestrator (like Kubernetes), the filesystem is
usually ephemeral — any log file is lost when the container restarts. And if there's
more than one instance of the app running (scale-out), each instance writes to its own
local file, so there's no unified view of the logs. The usual fix is writing to a
centralized sink instead — console output collected by the orchestrator, or a service
like Elasticsearch/Seq/Application Insights — instead of local files.

Source: https://github.com/serilog/serilog-aspnetcore

---

# Part 8 — HTTPS

### 1. Explain the difference among HTTP, HTTPS, TLS, and an HTTPS certificate.
HTTP is an unencrypted data transfer protocol. HTTPS is the same HTTP protocol running
over a TLS encryption layer. TLS (Transport Layer Security) is the cryptographic
protocol that secures the channel — encryption plus identity verification. An HTTPS
certificate (an X.509 certificate) is the digital document that proves the server's
identity and carries the public key used to establish the encrypted TLS connection.

Source: https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-10.0

### 2. Research the dotnet dev-certs commands used to create, clean, check, and trust the development certificate.

```bash
dotnet dev-certs https                 # creates the certificate if it doesn't exist (not trusted yet)
dotnet dev-certs https --trust         # trusts the certificate on the local machine
dotnet dev-certs https --clean         # removes all existing dev certificates
dotnet dev-certs https --check --trust # verifies the certificate exists and is trusted
```

Source: https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-dev-certs

### 3. Configure local HTTP and HTTPS endpoints for Kestrel.
The approach is to define two named endpoints in the `Kestrel:Endpoints` section of
appsettings.json (or appsettings.Development.json) for each project — one with an
`http://` URL and one with an `https://` URL — so Kestrel listens on both protocols at
once without touching Program.cs. The HTTPS endpoint relies on the trusted ASP.NET Core
Development certificate rather than an explicit certificate file, since this is local
development, not production. The actual endpoint configuration and evidence that both
projects listen correctly is in each project's appsettings files — see Stage 4.

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-10.0

### 4. Add HTTPS redirection middleware and explain where it belongs in the middleware pipeline.
`app.UseHttpsRedirection()` must go very early in the pipeline — before any other
middleware that handles the request (static files, routing, authentication) — so any
HTTP request is redirected to HTTPS immediately, before it reaches any other app logic.

Source: https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-10.0

### 5. Research the default HTTPS redirection status code.
The default is `307 Temporary Redirect` (`Status307TemporaryRedirect`), not 301 or 302,
to avoid permanent-caching issues with links during development. It can be changed to
`308 Permanent Redirect` in production via `AddHttpsRedirection`.

Source: https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-10.0

### 6. Research why a production API handling sensitive data should preferably listen only on HTTPS instead of depending on a redirect.
API clients — mobile apps, other services — don't always follow HTTP-to-HTTPS redirects
the way browsers do, so sensitive data could actually be sent over HTTP before any
redirect happens. The official recommendation is that a Web API either doesn't listen on
HTTP at all, or closes the connection with 400 Bad Request instead of relying on
`RequireHttpsAttribute`, which is designed for browsers, not API clients.

Source: https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-10.0

### 7. What is HSTS? Do non-browser API clients such as Postman enforce it?
HSTS (HTTP Strict Transport Security) is a response header that tells a browser "never
talk to this domain over HTTP again, use HTTPS only, and reject untrusted certificates"
for a set period of time. Non-browser clients — Postman, mobile apps, other backend
services — don't enforce HSTS at all; it's a browser-only mechanism, so it isn't
sufficient protection for an API on its own.

Source: https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-10.0

### 8. Explain the difference between direct TLS termination in Kestrel and TLS termination at a reverse proxy.
With direct termination, Kestrel itself holds a real certificate and decrypts TLS
directly. With reverse-proxy termination, TLS is terminated at an intermediate layer
(IIS, Nginx, a load balancer, an ingress), and Kestrel receives plain HTTP traffic over
the internal network. The second approach simplifies certificate management — one
certificate at the proxy instead of one per instance — and load balancing, but requires
forwarded headers middleware so Kestrel knows the original scheme.

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-10.0

### 9. Explain how production certificate paths and passwords should be supplied without committing secrets.
The certificate file path can go in appsettings.Production.json since it's just a path,
not a secret. The password must come from an external source at runtime — an
environment variable injected by the hosting platform, or better, a managed secret store
such as Azure Key Vault — read through `IConfiguration` the same way any other secret
is, never typed literally into a file that ships with the code.

Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-10.0

### 10. Test both applications over HTTPS using Postman.
The approach is a Postman environment holding an HTTPS `baseUrl` variable that points at
each project's Kestrel HTTPS endpoint (e.g. `https://localhost:7000`), with every
request in the collection using `{{baseUrl}}` so switching environments doesn't require
editing each request individually. Because the local ASP.NET Core development
certificate is self-signed, Postman needs its SSL certificate verification handled
correctly — trusting the dev cert, or turning verification off for local-only testing —
for the HTTPS calls to succeed. The actual requests, responses, and status codes
captured this way are in the Postman collection and environment — see Stage 5.

Source: https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-10.0

### 11. Investigate and document how to solve an untrusted local development certificate correctly.
The official fix is `dotnet dev-certs https --trust`. If the problem persists — often
after an SDK update or a machine change — the correct approach is a full clean, then
recreate and trust:

```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

Afterward, browsers must be fully closed (they cache certificate trust state) and
reopened. This is safer than any workaround like disabling certificate validation in
code, which opens a real security hole even if only meant to be temporary.

Source: https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-dev-certs
