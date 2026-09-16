# ASP.NET Core Research Assignment — Research Answers (Parts 1–5, 8)

هذا الملف يغطي الأجزاء 1 إلى 5 والجزء 8 فقط، حسب "Submission requirements" في الـPDF
("One research document containing answers, examples, and source links for Parts 1-5
and 8"). أسئلة الـComparison في Part 7 موجودة في `docs/comparison-table.md` (Stage 6)
لأنها مرتبطة بالتنفيذ الفعلي للمشروعين. الـPDF هو مصدر الأسئلة فقط — كل إجابة مبنية على
توثيق Microsoft / Serilog الرسمي المذكور تحت كل سؤال.

## ⚠️ TO REVIEW

- الفرق بين environment name و environment variable (Part 2) — صياغتي الخاصة، تأكد إنها
  منطقية بالنسبة لك.
- "What information must never be written to logs" (Part 5) — مبني على مبادئ أمان عامة
  (OWASP-style) وليس على جملة واحدة محددة بالحرف من توثيق Microsoft، راجعها.
- "How does Kestrel hosting differ when IIS/Nginx/LB/ingress terminates TLS" (Part 4) —
  إجابة تركيبية من عدة أجزاء من توثيق Kestrel، تأكد إنها تغطي اللي محتاجه.

---

# Part 1 — User Secrets

### 1. What are User Secrets in ASP.NET Core?
الـUser Secrets (أداة Secret Manager) هي طريقة لتخزين بيانات حساسة زي الـconnection
strings أو الـAPI keys أثناء التطوير المحلي، بعيدًا عن شجرة المشروع نفسها. القيم بتتخزن
كملف JSON في الـuser profile على الجهاز، مش جوه الـrepo، فمينفعش تتسرب مع الكود بالغلط.
الأداة دي مخصصة لبيئة الـDevelopment بس، ومش بديل لأي secret store حقيقي في الإنتاج.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 2. What problem do User Secrets solve?
بتحل مشكلة إن المطور يحط secrets (زي passwords) جوه appsettings.json فيتم commit-هم
بالغلط لسورس كونترول زي git. لأن الملف بيتخزن برا الـproject directory، مفيش احتمال إنه
ينزل مع الكود أو يتشارك مع فريق التطوير كله زي باقي الملفات.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 3. Where are User Secrets stored on Windows, Linux, and macOS?
على Windows بيتخزنوا في
`%APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json`، وعلى Linux وmacOS في
`~/.microsoft/usersecrets/<user_secrets_id>/secrets.json`. الـ`<user_secrets_id>` هو
نفس قيمة الـUserSecretsId المكتوبة في ملف الـ.csproj بتاع المشروع.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 4. Are User Secrets encrypted? Are they considered a trusted production secret store?
لأ، الـSecret Manager مش بيعمل encryption للقيم — بتتخزن كـplain text جوه ملف JSON.
Microsoft نفسها بتحذر إنه "shouldn't be treated as a trusted store" وإنه لأغراض
الـdevelopment بس، فمش صالح أبدًا كمخزن secrets للإنتاج.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 5. How is a project connected to its User Secrets store? Research UserSecretsId.
لما تشغل `dotnet user-secrets init`، الأمر بيضيف عنصر `<UserSecretsId>` جوه
`<PropertyGroup>` في ملف الـ.csproj، وقيمته GUID عشوائي فريد للمشروع ده. الـGUID ده هو
اللي بيربط المشروع بمجلد الـsecrets.json بتاعه على الجهاز، فحتى لو نسخت المشروع لمكان
تاني، لسه بيقدر يوصل لنفس الـsecrets طالما الـUserSecretsId ثابت.

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <UserSecretsId>0000a1a1-b2b2-c3c3-d4d4-eeeeee555555</UserSecretsId>
</PropertyGroup>
```

المصدر: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 6. Research the commands used to initialize, set, list, remove, and clear secrets.
الأوامر الأساسية بتتشغل من مجلد المشروع (اللي فيه ملف الـ.csproj):

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<value>"
dotnet user-secrets list
dotnet user-secrets remove "ConnectionStrings:DefaultConnection"
dotnet user-secrets clear
```

`init` بيولّد الـUserSecretsId، `set` بيضيف أو يعدّل secret واحد (النقطتين `:` بتعبّر عن
تسلسل هرمي)، `list` بيعرض كل الـsecrets الحالية، `remove` بيمسح secret واحد بمفتاحه،
و`clear` بيمسح كل الـsecrets دفعة واحدة.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 7. How does an ASP.NET Core application read a secret through IConfiguration?
لما الـEnvironment يكون Development، الـWebApplicationBuilder بيسجّل الـUser Secrets
configuration source تلقائيًا (عبر AddUserSecrets)، فبيتقرا زي أي مصدر configuration
تاني من خلال الـIConfiguration indexer العادي:

```csharp
var builder = WebApplication.CreateBuilder(args);
var apiKey = builder.Configuration["Movies:ServiceApiKey"];
```

المصدر: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 8. When should User Secrets be used?
لما تحتاج تختبر التطبيق محليًا وهو متصل بموارد حقيقية (زي SQL Server محلي بمصادقة، أو
API key تجريبي) من غير ما تحط القيمة دي في appsettings.json أو تعرّضها لباقي الفريق عبر
git. أنسب استخدام لها هو أثناء الـlocal development على جهاز المطور نفسه بس.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 9. When should User Secrets not be used?
متستخدمهاش في الإنتاج أو في أي بيئة deployment، ومتستخدمهاش لمشاركة secrets بين أعضاء
الفريق (لأنها محلية للجهاز بس ومش بتتزامن). كمان متعتمدش عليها كـbackup أو audit trail
لأنها غير مشفّرة وسهل حد يوصلها لو قدر يوصل لملفات الـuser profile.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 10. Why must User Secrets not contain production credentials?
لأنها غير مشفّرة، متخزنة كـplain text على جهاز المطور، ومش معمولة أصلًا كـsecure store —
فلو أي حد وصل للجهاز أو للملف يقدر يقرا الـcredentials مباشرة. الإنتاج لازم يستخدم مصدر
أقوى زي environment variables أو managed secret store (زي Azure Key Vault) بصلاحيات
وصول محكومة وrotation واضح.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

---

# Part 2 — Environment Variables

### 1. What is an environment variable?
متغيّر بيتخزن في الـprocess environment بتاع نظام التشغيل أو الـshell أو الـcontainer،
وأي عملية (process) شغّالة جواه بتقدر تقراه وقت التشغيل. مش جزء من الكود ولا من ملفات
المشروع، فبيسمح بتغيير سلوك التطبيق من غير إعادة build.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0

### 2. Who can create environment variables: the operating system, terminal, IDE, container, or cloud host?
كل الأربعة فعلاً: نظام التشغيل بيحدد متغيرات عامة (زي PATH)، الـterminal session نفسها
ممكن تحط متغيرات مؤقتة، الـIDE (زي Visual Studio) بيحقنها عبر launchSettings.json،
الـcontainer runtime (Docker) بيحقنها عبر ENV أو docker run -e، والـcloud host (زي Azure
App Service) بيحقنها عبر App Settings في لوحة التحكم.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-10.0

### 3. How does ASP.NET Core load environment variables into IConfiguration?
الـWebApplicationBuilder بيسجّل الـEnvironment Variables Configuration Provider ضمن
مصادر الـconfiguration الافتراضية، وده بيقرا كل الـenvironment variables المتاحة
للـprocess ويحولها لمفاتيح جوه الـIConfiguration، بنفس شكل باقي المصادر (appsettings,
User Secrets, إلخ).

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0

### 4. How are hierarchical configuration keys represented in environment variables? Research the double underscore separator.
الـIConfiguration بيستخدم النقطتين `:` عادةً للتسلسل الهرمي (زي
`ConnectionStrings:DefaultConnection`)، لكن مش كل الأنظمة بتدعم `:` في اسم متغير بيئة —
مثلاً Bash مش بيدعمها. لذلك كل المنصات بتدعم الـdouble underscore `__` كبديل، وبيتحول
تلقائيًا لـ`:` وقت قراءة الـconfiguration:

```bash
export ConnectionStrings__DefaultConnection="Server=...;"
```

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0

### 5. How can environment variables be set temporarily in PowerShell, Command Prompt, Bash, Visual Studio, Docker, and Azure App Service?
في PowerShell: `$Env:ASPNETCORE_ENVIRONMENT = "Staging"` (لنفس الـsession بس). في CMD:
`set ASPNETCORE_ENVIRONMENT=Staging`. في Bash: `export ASPNETCORE_ENVIRONMENT=Staging`.
في Visual Studio: من launch profile في launchSettings.json (قسم environmentVariables).
في Docker: عبر `ENV` جوه الـDockerfile، أو `-e ASPNETCORE_ENVIRONMENT=Staging` مع
`docker run`، أو قسم `environment` في docker-compose.yml. في Azure App Service: من
Settings > Configuration > Application settings، وبيتحول تلقائيًا لـenvironment
variable وقت التشغيل.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-10.0

### 6. What is ASPNETCORE_ENVIRONMENT, and what values are normally used?
متغير بيئة بيحدد الـruntime environment بتاع التطبيق، وبيتقرا بواسطة الـhost وقت الإقلاع.
القيم المعتادة هي Development وStaging وProduction، لكن أي قيمة نصية مقبولة تقنيًا. لو
مفيش قيمة متظبطة، الـdefault هو Production.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-10.0

### 7. What is the difference between an environment name and an environment variable?
الـenvironment variable هو الآلية (متغير بيئة نظام التشغيل زي ASPNETCORE_ENVIRONMENT)
اللي بينقل قيمة للتطبيق. الـenvironment name هو القيمة النصية نفسها (زي "Development")
اللي بتحدد سلوك التطبيق عبر IHostEnvironment.EnvironmentName. يعني الـvariable هي وسيلة
النقل، والـname هي البيانة المنقولة.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-10.0

### 8. Do environment variables automatically encrypt secrets?
لأ. الـenvironment variables بتتخزن عادةً كـplain text غير مشفّر، فلو الجهاز أو
الـprocess اتخترق، أي حد وصل ليهم يقدر يقراهم مباشرة. Microsoft بتحذر صراحة من كده وبتنصح
بإجراءات إضافية لو محتاج حماية أقوى.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 9. When are environment variables more suitable than User Secrets?
في بيئات الـdeployment (staging, production, CI/CD, containers) لأنها متاحة في أي بيئة
تشغيل عادية ومش مربوطة بجهاز مطور معين زي الـUser Secrets. كمان معظم منصات الاستضافة
(Docker, Kubernetes, Azure App Service) مبنية أصلًا على حقن environment variables وقت
الـdeploy.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 10. What risks exist when sensitive values are stored directly in environment variables?
ممكن تظهر في process listings أو crash dumps أو logs غير محكومة، وأي عملية تانية أو
مستخدم عنده صلاحية على نفس الـprocess ممكن يشوفها. كمان لو التطبيق بيسجّل الـenvironment
كله في الـlogs بالغلط (زي وقت الـdebugging) الـsecret بيتسرب، وهي غير مشفّرة أصلًا فمفيش
طبقة حماية إضافية.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

---

# Part 3 — appsettings Files and Configuration Sources

### 1. What is appsettings.json, and when is it loaded?
ملف JSON بيحمل الإعدادات العامة للتطبيق اللي مش سرية (زي log levels أو feature flags).
بيتحمّل مبكرًا جدًا في بداية بناء الـWebApplicationBuilder، كواحد من مصادر الـconfiguration
الافتراضية، وبيتطبّق على كل الـenvironments.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0

### 2. What is appsettings.{Environment}.json? Give examples for Development, Staging, and Production.
نسخة إضافية من الإعدادات مخصصة لـenvironment معين، بتتحمّل بعد appsettings.json مباشرة
فبتقدر تعدّل أو تضيف مفاتيح بتاعتها. أمثلة: `appsettings.Development.json` (لوج مفصّل،
تفاصيل exceptions)، `appsettings.Staging.json` (إعدادات قريبة من الإنتاج لكن بيانات
اختبار)، `appsettings.Production.json` (log level أعلى، بلا تفاصيل أخطاء للمستخدم).

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0

### 3. Research the default configuration precedence in ASP.NET Core.
ترتيب الأولوية الافتراضي من الأعلى للأقل (اللي جاي بعده بيكسب لو فيه تعارض في المفتاح):

1. Command-line arguments (Command-line Configuration Provider)
2. Environment variables غير المبدوءة بـASPNETCORE_ أو DOTNET_
3. User Secrets (في بيئة Development بس)
4. `appsettings.{Environment}.json`
5. `appsettings.json`
6. Fallback host configuration

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0

### 4. Which provider wins when the same key exists in appsettings.json, User Secrets, an environment variable, and the command line?
الـcommand-line argument بيكسب، لأنه أعلى مصدر في ترتيب الأولوية، يليه environment
variable، ثم User Secrets، وأخيرًا appsettings.json. القاعدة العامة: آخر مصدر بيتسجّل
هو اللي بيفوز لو المفتاح واحد.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0

### 5. What kinds of safe settings belong in appsettings.json?
إعدادات مش حساسة زي: minimum log levels، feature flags، أسماء endpoints عامة، timeouts،
pagination defaults، أو أي قيمة لو اتسربت مش هتسبب مشكلة أمنية. باختصار: أي حاجة ممكن
تتعرض في الـrepo من غير خطر.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0

### 6. What values must never be committed to appsettings.json?
Passwords، connection strings فيها credentials، API keys، أي token أو secret، مفاتيح
تشفير، أو certificate passwords. أي قيمة زي دي لازم تروح لـUser Secrets في التطوير
وenvironment variables / managed secret store في الإنتاج.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 7. Should a connection string always be considered a secret? Explain the difference between a credentialed and non-credentialed connection string.
لأ مش دايمًا. الـconnection string غير الـcredentialed (زي LocalDB بـ
`Trusted_Connection=True` اللي بيعتمد على Windows Integrated Security) مفيهوش password
ظاهر فمش سر بنفس درجة الخطورة. لكن الـcredentialed connection string (فيه `User
Id=...;Password=...` صريح) لازم يتعامل معاه كـsecret كامل لأنه بيدي وصول مباشر
للداتابيز.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 8. When should a developer use User Secrets instead of environment variables?
لما يشتغل محليًا على جهازه الشخصي وعايز الإعداد يفضل خاص بيه بس (مش متشارك مع الفريق
كله زي متغير بيئة على السيرفر)، وعايز إدارة سهلة عبر أوامر `dotnet user-secrets` بدل ما
يفتح إعدادات النظام يدويًا.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 9. When should a deployment use environment variables or a managed secret store instead of User Secrets?
في أي بيئة deployment حقيقية (staging/production) لأن الـUser Secrets أصلًا مش متاحة
هناك (مربوطة بجهاز التطوير بس ومش بتتنشر مع الـpublish). الـdeployment لازم يستخدم
environment variables المحقونة من منصة الاستضافة، أو الأفضل managed secret store زي
Azure Key Vault لسريّة وrotation أقوى.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0

### 10. Research IConfiguration and the Options pattern. When is each approach appropriate?
الـIConfiguration مناسب للوصول السريع لقيمة واحدة أو مفتاح بسيط مباشرة (`config["Key"]`).
الـOptions pattern (عبر `IOptions<T>` أو `IOptionsSnapshot<T>` أو `IOptionsMonitor<T>`)
أفضل لما عندك مجموعة إعدادات مرتبطة ببعض وعايز تربطهم بـclass قوي النوع (strongly typed)
مع دعم الـDI. الـIOptions بيتحمّل مرة واحدة (singleton-like)، الـIOptionsSnapshot بيتحدّث
لكل request (scoped)، والـIOptionsMonitor بيدعم التحديث اللحظي حتى في singleton services.

```csharp
builder.Services.Configure<MovieSettings>(
    builder.Configuration.GetSection("Movies"));
```

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-10.0

---

# Part 4 — Kestrel Configuration

### 1. What is Kestrel, and what role does it play in an ASP.NET Core application?
Kestrel هو الـweb server الافتراضي المدمج جوه ASP.NET Core، cross-platform ومبني على
libuv/managed sockets. دوره إنه يستقبل الاتصالات (HTTP/HTTPS) ويحوّلها لـrequest
pipeline بتاع التطبيق. المشاريع الافتراضية بتستخدمه سواء لوحده كـedge server أو خلف
reverse proxy.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-10.0

### 2. What is a Kestrel endpoint?
نقطة استماع (listen point) بيحددها Kestrel — تجميعة من address + port + protocol (وممكن
certificate لو HTTPS) — بحيث السيرفر يقدر يستقبل اتصالات عليها. التطبيق ممكن يكون عنده
أكتر من endpoint في نفس الوقت (مثلاً واحد HTTP وواحد HTTPS).

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-10.0

### 3. Which properties can be configured for an endpoint: address, port, protocol, and certificate?
الأربعة كلهم قابلين للتظبيط: الـaddress (localhost، IP معين، أو 0.0.0.0)، الـport
(رقمي)، الـprotocol (Http1، Http2، أو الاتنين)، والـcertificate لو الـendpoint HTTPS
(المسار والباسورد أو certificate الـDevelopment الافتراضي).

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-10.0

### 4. Research the supported ways to configure Kestrel endpoints: appsettings, environment variables, command line, and C# code.
كل الأربعة مدعومين: appsettings.json عبر قسم `"Kestrel": { "Endpoints": {...} }`،
environment variables بنفس الأسماء بصيغة `Kestrel__Endpoints__Https__Url`، command-line
عبر `--urls` أو مفاتيح Kestrel صريحة، وأخيرًا C# code عبر
`builder.WebHost.ConfigureKestrel(...)` مع `serverOptions.Listen(...)` لأقصى تحكم برمجي.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-10.0

### 5. Why might a team configure Kestrel in appsettings.json?
لأنها طريقة declarative وسهلة القراءة، بتسمح بتغيير الـports أو الـcertificates من غير
إعادة كومبايل، وبتدعم إعدادات مختلفة لكل environment عبر appsettings.{Environment}.json،
وبتتكامل تلقائيًا مع باقي نظام الـconfiguration (بما فيه إعادة التحميل reload on change).

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-10.0

### 6. When is appsettings.json not the best place for final production endpoint configuration?
لما الـorchestrator أو منصة الاستضافة (Kubernetes, Docker, Azure) هي اللي بتحدد الـports
والـbindings ديناميكيًا وقت الـdeploy — هنا الأفضل إن الإعدادات تيجي من environment
variables أو من الـplatform نفسها، مش تتثبّت جوه ملف بيتنشر مع الكود. كمان لو الـTLS
بيتعامل معاه reverse proxy، مفيش داعي أصلًا لتفاصيل certificate جوه appsettings.json بتاع
Kestrel.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-10.0

### 7. What is the difference between localhost, 0.0.0.0, and a specific IP address when binding Kestrel?
`localhost` بيحاول يربط على loopback interfaces IPv4 وIPv6 مع بعض، ومتاح بس من نفس
الجهاز. `0.0.0.0` بيربط على كل عناوين IPv4 المتاحة على الجهاز فبيقبل اتصالات من أي شبكة
(مفيد للـcontainers أو السيرفرات). عنوان IP محدد بيربط بس على الـnetwork interface اللي
بيمثله، وده مفيد لو عايز تقيّد الوصول لواجهة شبكة معينة.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-10.0

### 8. How can separate HTTP and HTTPS endpoints be configured?
عبر تعريف اسمين مختلفين جوه قسم `Kestrel:Endpoints` في appsettings.json، واحد بـURL
`http://...` وتاني بـURL `https://...` (مع Certificate section اختياري للـHTTPS)، أو
برمجيًا عبر استدعاء `serverOptions.Listen(...)` مرتين — مرة عادية ومرة مع
`listenOptions.UseHttps(...)`.

```json
"Kestrel": {
  "Endpoints": {
    "Http": { "Url": "http://localhost:5000" },
    "Https": { "Url": "https://localhost:5001" }
  }
}
```

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-10.0

### 9. What happens when an HTTPS endpoint has no explicit certificate in Development?
Kestrel بيرجع لترتيب fallback: أولاً الـcertificate المحدد في الـendpoint نفسه، بعدين
الـdefault certificate تحت `Certificates:Default`، وأخيرًا الـASP.NET Core development
certificate المولّد تلقائيًا بالـSDK (لو موجود ومتوثّق). لو مفيش أي واحد من التلاتة، السيرفر
بيرمي استثناء ويفشل يبدأ.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-10.0

### 10. Where should a certificate path be stored? Where should its password be stored?
مسار الـcertificate (الـpath) عادةً بيتحط في appsettings.json نفسه لأنه مش سر (مجرد مسار
ملف)، ويفضّل يكون relative لمجلد المحتوى بتاع التطبيق. أما الباسورد فلازم يروح لـUser
Secrets في التطوير (`Kestrel:Endpoints:Https:Certificate:Password`) أو managed secret
store زي Azure Key Vault في الإنتاج — مش جوه appsettings.json أبدًا.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-10.0

### 11. How does Kestrel hosting differ when IIS, Nginx, a load balancer, or an ingress terminates TLS?
لما TLS يتقفّل (terminate) عند reverse proxy زي IIS أو Nginx أو LB أو Kubernetes ingress،
Kestrel نفسه بيستقبل traffic عادي HTTP من الـproxy على الشبكة الداخلية، وبس الـproxy هو
اللي محتاج certificate X.509 حقيقي. لازم هنا تفعّل forwarded headers middleware عشان
Kestrel يعرف الـscheme/IP الأصلي بتاع الطلب. ده بيبسّط إدارة الشهادات وload balancing،
بعكس لما Kestrel نفسه يكون edge server ويتعامل مع TLS termination مباشرة.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-10.0

---

# Part 5 — Logging, ILogger, and Serilog

### 1. What is application logging, and why is it required in production systems?
الـlogging هو تسجيل أحداث حصلت أثناء تشغيل التطبيق (requests, errors, state changes) في
مكان ممكن تراجعه بعدين. في الإنتاج بيبقى ضروري لأنك مش قادر تشغّل debugger على سيرفر
حي، فالـlogs هي الوسيلة الوحيدة لمعرفة إيه اللي حصل لما مشكلة تظهر، وكمان بتستخدم
للـmonitoring والـalerting وتحليل الأداء.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-10.0

### 2 & 3. Every .NET log level (Trace, Debug, Information, Warning, Error, Critical, None) + two realistic situations each

| Level | القيمة | الوصف | موقفان واقعيان |
|---|---|---|---|
| Trace | 0 | أدق تفاصيل ممكنة، ممكن تحتوي بيانات حساسة، متتفعلش في الإنتاج | (1) طباعة القيم الداخلية لخوارزمية حساب سعر منتج خطوة بخطوة. (2) تتبع كل استدعاء لدالة داخلية أثناء تصحيح bug معقّد. |
| Debug | 1 | مفيد للتطوير والتصحيح، حجمه كبير فاستخدمه بحذر في الإنتاج | (1) طباعة query اللي EF Core هيبعته للداتابيز قبل تنفيذه. (2) تسجيل قيمة parameter داخلي وقت تطوير endpoint جديد. |
| Information | 2 | تتبّع للسير العام للتطبيق، له قيمة طويلة المدى | (1) "Product {Id} created successfully". (2) "Application started, listening on port 5001". |
| Warning | 3 | حدث غير متوقع لكن مش مسبب فشل كامل | (1) محاولة إنشاء منتج بسعر سالب اترفضت بالـvalidation. (2) استعلام استغرق وقت أطول من المتوقع لكنه نجح. |
| Error | 4 | فشل في العملية الحالية أو الـrequest، مش فشل للتطبيق كله | (1) فشل الاتصال بقاعدة البيانات أثناء تنفيذ طلب. (2) استثناء غير متوقع أثناء معالجة POST request. |
| Critical | 5 | فشل يحتاج تدخل فوري | (1) نفاد مساحة القرص بشكل كامل. (2) فشل التطبيق في الإقلاع بسبب فقدان اتصال حرج بالكامل بقاعدة البيانات. |
| None | 6 | مفيش أي رسالة بتتسجل خالص | يستخدم لإيقاف provider معين بالكامل، مش موقف تشغيلي بحد ذاته. |

المصدر: https://learn.microsoft.com/en-us/dotnet/core/extensions/logging

### 4. What does a configured minimum log level mean?
هو الحد الأدنى للـseverity اللي التطبيق هيسجّله فعليًا؛ أي رسالة log بمستوى أقل من الحد
ده بيتم تجاهلها تمامًا ومتوصلش حتى للـprovider. مثلاً لو الـminimum level = Warning،
رسائل Trace وDebug وInformation مش هتتسجل خالص، وده بيقلل الضوضاء والتكلفة في الإنتاج.

المصدر: https://learn.microsoft.com/en-us/dotnet/core/extensions/logging

### 5. What is ILogger? What is ILogger<T>?
`ILogger` هو الـinterface الأساسي للـlogging، بياخد category name نصي (زي
"Example.DefaultService") بيتحدد يدويًا عبر `ILoggerFactory.CreateLogger("...")`.
`ILogger<T>` بيرث من `ILogger` وبياخد الـcategory تلقائيًا من الاسم الكامل للـclass
النوع T — يعني `ILogger<ProductService>` بيدي category = الاسم الكامل لـProductService،
من غير ما تكتبه يدويًا، وده الأكثر استخدامًا مع الـDI.

المصدر: https://learn.microsoft.com/en-us/dotnet/core/extensions/logging

### 6. How is ILogger obtained through dependency injection?
بحقن `ILogger<T>` كـparameter في constructor أي class مسجلة في الـDI container، والـDI
بيوفّرها تلقائيًا لأن `Microsoft.Extensions.Logging` بيسجّل `ILoggerFactory` وimplementation
جاهز لـ`ILogger<T>` بشكل افتراضي:

```csharp
public class ProductService(ILogger<ProductService> logger) : IProductService
{
    // logger.LogInformation("Product {ProductId} created", id);
}
```

المصدر: https://learn.microsoft.com/en-us/dotnet/core/extensions/logging

### 7. What is a logging provider? Research the built-in Console and Debug providers.
الـlogging provider هو المكوّن اللي بياخد رسائل الـlog وبيوديها لوجهة مخصصة (destination)
زي شاشة الـconsole، أو ملف، أو خدمة سحابية. الـConsole provider بيطبع الرسائل مباشرة على
شاشة الـterminal، مفيد وقت تشغيل التطبيق محليًا للمراقبة اللحظية. الـDebug provider بيبعت
الرسائل لنافذة الـDebug في أدوات زي Visual Studio عبر `System.Diagnostics.Debug`.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-10.0

### 8. What is structured logging? Compare message templates with string interpolation.
الـstructured logging بيحافظ على قيم الـparameters كـfields منفصلة بدل ما يدمجهم في نص
واحد، عن طريق message templates فيها placeholders بأسماء (زي `{ProductId}`) والقيم
بتتبعت كـarguments منفصلة. ده بيسمح للـprovider يخزن القيم كحقول قابلة للاستعلام
(searchable). في المقابل، الـstring interpolation (زي `$"Product {id} created"`) بيدمج
كل حاجة كنص واحد نهائي قبل ما يوصل للـlogger، فبتفقد إمكانية الفلترة أو الاستعلام على
القيمة، وكمان بتتنفذ حتى لو الـlog level متفعلش (أداء أسوأ).

```csharp
logger.LogInformation("Product {ProductId} created at {CreatedAt}", id, DateTime.UtcNow);
// غلط: logger.LogInformation($"Product {id} created at {DateTime.UtcNow}");
```

المصدر: https://learn.microsoft.com/en-us/dotnet/core/extensions/logging

### 9. What information must never be written to logs?
Passwords، connection strings كاملة، API keys أو tokens، بيانات بطاقات ائتمان، أرقام هوية
أو أي Personally Identifiable Information (PII) حساسة، وأي secret تاني. حتى مستوى Trace
اللي Microsoft بتحذر إنه "might contain sensitive app data" لازم يتقفل في الإنتاج بالضبط
لنفس السبب ده.

المصدر: https://learn.microsoft.com/en-us/dotnet/core/extensions/logging

### 10. What is Serilog, and how does it integrate with Microsoft.Extensions.Logging?
Serilog هي مكتبة structured logging خارجية لـ.NET، بتديك API نظيف ومرونة أكبر في
التحكم بالـsinks والـenrichment مقارنة بالـproviders الافتراضية. بتتكامل مع
Microsoft.Extensions.Logging عبر حزمة Serilog.AspNetCore، اللي بتسجّل Serilog كـ
logging provider بديل عبر `builder.Services.AddSerilog()`، فكل استدعاء لـ`ILogger<T>`
جوه التطبيق (المُحقون بالـDI العادي) بيتوجّه فعليًا لـSerilog pipeline.

المصدر: https://github.com/serilog/serilog-aspnetcore

### 11. What is a Serilog sink?
الـsink هو وجهة الإخراج اللي Serilog بيكتب فيها أحداث الـlog — ممكن يكون console، ملف،
قاعدة بيانات، أو خدمة سحابية زي Seq أو Elasticsearch. التطبيق الواحد ممكن يكتب لأكتر من
sink في نفس الوقت (مثلاً console + file مع بعض).

المصدر: https://github.com/serilog/serilog

### 12. Research how to write Serilog events to the console and a file.
بيتظبط عن طريق `LoggerConfiguration` مع `.WriteTo.Console()` و`.WriteTo.File(...)`
مع بعض في نفس الـpipeline:

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
```

المصدر: https://github.com/serilog/serilog-sinks-file

### 13. Research daily rolling files, file-size limits, and retained-file limits.
`rollingInterval: RollingInterval.Day` بيخلّي Serilog يفتح ملف جديد كل يوم (اسم الملف
بيتذيّل بالتاريخ). `fileSizeLimitBytes` بيحدد أقصى حجم للملف الواحد (الافتراضي 1GB)، ومع
`rollOnFileSizeLimit: true` بيتعمل ملف رقمي جديد لو اتعدّى الحد ده. `retainedFileCountLimit`
بيحدد أقصى عدد ملفات محتفَظ بيها (الافتراضي 31 ملف تقريبًا شهر) وبعدها الأقدم بيتمسح
تلقائيًا لتوفير مساحة القرص.

المصدر: https://github.com/serilog/serilog-sinks-file

### 14. Research request logging in Serilog.AspNetCore.
`UseSerilogRequestLogging()` middleware بيستبدل الـlogging الافتراضي المفصّل لكل request
برسالة واحدة مجمّعة لكل طلب (زي "HTTP GET /products responded 200 in 45ms")، بدل عشرات
الأسطر لكل request. لازم يتحط في الـpipeline قبل أي middleware عايز تقيسه أو تسجله
(زي routing/endpoints)، لأنه مش بيقيس أي حاجة قبله.

المصدر: https://github.com/serilog/serilog-aspnetcore

### 15. When are local log files unsuitable, especially in containers or multiple application instances?
لما التطبيق شغّال في containers أو orchestrator (Kubernetes مثلاً)، الـfile system
بيكون ephemeral غالبًا — أي ملف log بيضيع لما الـcontainer يعاد تشغيله أو يتقتل. كمان لو
عندك أكتر من instance من نفس التطبيق شغّالين مع بعض (scale-out)، كل instance هيكتب
لملفه المحلي بس، فمفيش رؤية موحّدة (centralized view) للـlogs. الحل المعتاد هنا هو
الكتابة لـsink مركزي (console اللي orchestrator بيجمعه، أو Elasticsearch/Seq/Application
Insights) بدل ملفات محلية.

المصدر: https://github.com/serilog/serilog-aspnetcore

---

# Part 8 — HTTPS

### 1. Explain the difference among HTTP, HTTPS, TLS, and an HTTPS certificate.
HTTP هو بروتوكول نقل بيانات غير مشفّر. HTTPS هو نفس بروتوكول HTTP لكن فوق طبقة تشفير
TLS. الـTLS (Transport Layer Security) هو البروتوكول التشفيري نفسه اللي بيؤمّن القناة
(تشفير + تحقق هوية). شهادة HTTPS (X.509 certificate) هي المستند الرقمي اللي بيثبت هوية
السيرفر ويحمل المفتاح العام المستخدم في إنشاء اتصال TLS المشفّر.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-10.0

### 2. Research the dotnet dev-certs commands used to create, clean, check, and trust the development certificate.

```bash
dotnet dev-certs https                # ينشئ الشهادة لو مش موجودة (بدون trust)
dotnet dev-certs https --trust        # يثق بالشهادة على الجهاز المحلي
dotnet dev-certs https --clean        # يشيل كل شهادات dev الموجودة
dotnet dev-certs https --check --trust # يتأكد إن الشهادة موجودة وموثوقة
```

المصدر: https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-dev-certs

### 3. Configure local HTTP and HTTPS endpoints for Kestrel.
ده تنفيذ عملي مش سؤال بحثي — هيتظبط فعليًا في appsettings.json لكل مشروع في Stage 4
(زي مثال الـJSON في Part 4، سؤال 8 فوق)، مع endpoint HTTP للـfallback المحلي وendpoint
HTTPS للاختبار عبر Postman.

### 4. Add HTTPS redirection middleware and explain where it belongs in the middleware pipeline.
`app.UseHttpsRedirection()` لازم يتحط بدري قوي في الـpipeline — قبل أي middleware تاني
بيتعامل مع الطلب (زي static files، routing، authentication) — عشان أي طلب HTTP يتحوّل
لـHTTPS فورًا قبل ما يوصل لأي منطق تاني في التطبيق.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-10.0

### 5. Research the default HTTPS redirection status code.
الافتراضي هو `307 Temporary Redirect` (`Status307TemporaryRedirect`)، مش 301 أو 302،
عشان يتجنب مشاكل الـcaching الدائم للروابط أثناء التطوير. ممكن تغيّره لـ`308 Permanent
Redirect` في الإنتاج عبر `AddHttpsRedirection`.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-10.0

### 6. Research why a production API handling sensitive data should preferably listen only on HTTPS instead of depending on a redirect.
لأن عملاء الـAPI (زي تطبيقات موبايل أو خدمات تانية) مش دايمًا بيتبعوا HTTP redirects
زي المتصفحات، فممكن يبعتوا بيانات حساسة فعليًا عبر HTTP قبل ما الـredirect يحصل أصلًا.
التوصية الرسمية إن الـWeb API إما ميستمعش على HTTP خالص، أو يقفل الاتصال بـ400 Bad
Request بدل ما يعتمد على `RequireHttpsAttribute` المصمم أصلًا للمتصفحات.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-10.0

### 7. What is HSTS? Do non-browser API clients such as Postman enforce it?
HSTS (HTTP Strict Transport Security) هو header بيقول للمتصفح "متتصلش بالدومين ده عبر
HTTP تاني، استخدم HTTPS بس، وارفض شهادات غير موثوقة" لفترة زمنية محددة. عملاء غير
المتصفح زي Postman أو تطبيقات موبايل أو خدمات backend تانية مش بيطبّقوا HSTS أصلًا —
الـheader ده معياره وتطبيقه في المتصفحات بس، فمش وسيلة حماية كافية لـAPI.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-10.0

### 8. Explain the difference between direct TLS termination in Kestrel and TLS termination at a reverse proxy.
في الـdirect termination، Kestrel نفسه بيحمل certificate حقيقي وبيعمل فك تشفير TLS
مباشرة. في الـreverse-proxy termination، الـTLS بيتقفل عند طبقة وسيطة (IIS, Nginx, load
balancer, ingress)، وKestrel بياخد traffic عادي (HTTP) على الشبكة الداخلية. الطريقة
التانية بتبسّط إدارة الشهادات (شهادة واحدة عند الـproxy بدل كل instance) وبتسهّل الـload
balancing، لكنها بتحتاج forwarded headers middleware عشان Kestrel يعرف الـscheme الأصلي.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-10.0

### 9. Explain how production certificate paths and passwords should be supplied without committing secrets.
مسار ملف الـcertificate ممكن يتحط في appsettings.Production.json لأنه مجرد path مش سر،
لكن الباسورد لازم يجي من مصدر خارجي وقت الـruntime — environment variable محقونة من
منصة الاستضافة، أو الأفضل managed secret store زي Azure Key Vault — وبيتقرا عبر
IConfiguration بنفس الطريقة اللي بتتقرا بيها أي secret تاني، من غير ما يتكتب حرفيًا في
أي ملف بيتنشر مع الكود.

المصدر: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-10.0

### 10. Test both applications over HTTPS using Postman.
ده تنفيذ عملي هيحصل في Stage 5 — كل الـrequests في Postman collection هتستهدف
الـHTTPS base URL بتاع كل مشروع (endpoint المُعرّف في Part 8 سؤال 3)، والـenvironment
variable بتاعت الـbaseUrl هتتظبط على `https://localhost:<port>`.

### 11. Investigate and document how to solve an untrusted local development certificate correctly.
الحل الرسمي هو `dotnet dev-certs https --trust`. لو فضلت المشكلة موجودة (خصوصًا بعد
تحديث SDK أو تغيير جهاز)، الأسلوب الصحيح هو التنظيف الكامل بعدين إعادة الإنشاء والثقة:

```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

بعد كده لازم تقفل المتصفحات بالكامل (بتعمل cache لحالة الثقة) وتفتحها تاني. ده أسلم من أي
حل بديل زي تعطيل التحقق من الشهادة (certificate validation) في الكود، لأن ده بيفتح ثغرة
أمنية حقيقية حتى لو مؤقتة.

المصدر: https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-dev-certs
