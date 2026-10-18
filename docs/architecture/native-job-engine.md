# موتور وظایف بومی پیشنهادی

BackgroundService فقط میزبان اجراست. ذخیرهٔ برنامه، lease، history و recovery مستقل در SQL Server طراحی می‌شوند؛ هیچ engine جدیدی در این PR پیاده نشده است.

## موجودی رفتار فعلی

| job | زمان و trigger | ترتیب و قاعده | شکست، هم‌زمانی و وابستگی |
|---|---|---|---|
| logDeleterJob | startup هر ۱۰ دقیقه؛ constructor کنترلر عمومی هم همین ID را ثبت می‌کند | DeleteServerLogs همهٔ logها را بدون استفاده از count حذف می‌کند | SQL؛ scope ریشه در CronJobService؛ retention و lock ندارد |
| CheckZoneId | startup با `*/45` در دقیقهٔ cron؛ دقیقه‌های ۰ و ۴۵ هر ساعت، نه فاصلهٔ همواره ۴۵ دقیقه | CronJobService.CheckZoneId -> DomainService.InitZoneId؛ دامنهٔ active بدون zone را از Cloudflare می‌گیرد و update می‌کند | خطای یک domain ادامهٔ بقیه را متوقف می‌کند؛ SQL+Cloudflare |
| DomainJob | CreateDomainJob -> InsertJob -> cron `*/JobPeriodTime` | برای هر سرور active/available، آمار non-null، سن دامنه بزرگ‌تر از expiry و درصد و تعداد شکست حداقل آستانه؛ ChangeDomain | خطا حلقه را متوقف می‌کند؛ `.Result` و queryهای متعدد؛ آمار client قابل‌دستکاری |
| SubDomainJob | CreateSubDomainJob و cron بالا | job موجود -> ChangeSubDomain همهٔ سرورهای active/available | scope record محدود نشده؛ شکست میانی و رزرو دامنه محافظت ندارد |
| DeleteDnsJob | CreateDeleteDnsJobJob و cron بالا | انتخاب config، DeleteDnsAsync و GenerateDns برای سرورها | قبل از null check، JobConfig خوانده می‌شود؛ content رکورد DNS به تاریخ parse می‌شود، نه comment |
| IpConfigJob | CreateIpConfigJob و cron بالا | فقط سرور با آمار non-null و سن بیشتر از expiry؛ GenerateUpdateAsync -> تغییر address از اولین IP | email/key پارامتر مصرف نمی‌شوند؛ انتخاب IP بدون active/order؛ transaction مشترک ندارد |
| ResetServerDomain/SubDomain/Dns | registrationهای startup کامنت‌اند | wrapper همان سرویس دامنه | runtime registration از مسیر دیگری اثبات نشده |
| DeleteTcpJob / RefreshTcpJob و کلاس‌های Jobs | فایل خالی | اجرای واقعی در این کلاس‌ها نیست | جایگزینی این پوسته‌ها migration job محسوب نمی‌شود |

شاهد اصلی `Areas/Admin/Controllers/JobController.cs`، `CronJob/CronJobService.cs` و Program.cs است. JobService.Detail جدیدترین ردیف را می‌گیرد، ولی callbackها `OrderByDescending(...).LastOrDefault` می‌گیرند؛ قدیمی‌ترین حتی soft-deleted می‌تواند انتخاب شود. Disable در InsertJob پیش از persist برمی‌گردد؛ وضعیت DB الزاماً با scheduler برابر نمی‌شود. MemoryStorage پس از restart ثبت‌های پویا را از دست می‌دهد؛ بازسازی از Jobs در startup مشاهده نشد.

timeout کاربردی، CancellationToken سرتاسری، lease پایدار و منع overlap وجود ندارد؛ Cloudflare اغلب CancellationToken.None می‌گیرد. retry سفارشی تعریف نشده؛ رفتار پیش‌فرض Hangfire به‌معنی ایمنی retry DNS نیست. وضعیت اجرای فعلی در حافظه است و engine جدید نباید سابقهٔ ساختگی تولید کند.

## اجزای هدف

- `JobDefinition`: handler شناخته‌شده و نسخهٔ config، بدون credential در payload.
- `JobConfigurationService`: enable/disable، interval یا cron با TimeZoneId صریح و optimistic concurrency.
- `JobScheduleCalculator`: تابع خالص با TimeProvider؛ deadline بعدی از slot برنامه محاسبه شود، نه زمان پایان job.
- `JobDispatcher : BackgroundService`: PeriodicTimer صرفاً بیدارکننده؛ query SQL برای due work authoritative است.
- `JobExecutionCoordinator`: claim اتمیک، scope جدید برای هر اجرا، timeout و cancellation، lease منابع.
- `JobExecutionStore`: تاریخچهٔ ماندگار و transitionهای مجاز با compare-and-set.
- `JobAdministrationController`: فقط command و query؛ اجرای business/DNS داخل controller ممنوع.

رابط handler پیشنهادی `Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken)`؛ context شامل ExecutionId، OperationId، config نسخه‌دار، actor و fencing token است. scheduler فرمان `RequestRunAsync(jobId, idempotencyKey, expectedVersion, ct)` می‌پذیرد و شناسهٔ اجرای پایدار برمی‌گرداند؛ اجرای immediate داخل درخواست HTTP انجام نمی‌شود.

## claim، recovery و چند instance

۱. SQL transaction کوتاه با row lock/UPDATE OUTPUT و unique slot، due job را claim کند؛ الگوی UPDLOCK/READPAST باید با isolation و RCSI محیط آزمایش شود. query انتخاب و update جدا بدون lock کافی نیست.
۲. resource key مشترک مثل zone/server پیش از اثر بیرونی گرفته شود؛ heartbeat و مدت lease بر زمان SQL UTC متکی باشد. lease منقضی به‌تنهایی اجازهٔ replay اثر بیرونی نیست.
۳. heartbeat با ownership+fencing token تمدید شود. گم‌شدن lease cancellation را فعال و writer داخلی قدیمی را رد کند. Cloudflare fencing token را enforce نمی‌کند؛ برای call در حال پرواز state machine، شناسهٔ عملیات، read-before-retry و reconciliation لازم است؛ exactly-once وعده داده نمی‌شود.
۴. نتیجه SQL با شناسهٔ عملیات ثبت شود. timeout یا crash پس از call بیرونی وضعیت نامعلوم می‌سازد؛ recovery ابتدا observe/reconcile و سپس تصمیم می‌گیرد. حذف DNS کور retry نشود.
۵. shutdown: claim جدید قطع، cancellation منتقل، تا deadline منتظر بمانید؛ lease کار نامعلوم بی‌دلیل آزاد نشود. sweeper اجراهای abandoned را شناسایی و با state مجاز بازیابی کند.
۶. missed run policy هر job مشخص: zone-check می‌تواند coalesce شود؛ rotation backlog بدون محدودیت نباید همه را یک‌جا عوض کند؛ log retention باید بازهٔ پردازش و watermark داشته باشد. DST، timezone و بازه‌های نامعتبر تست شوند.

وضعیت‌ها: Pending -> Running -> Completed/Failed/Cancelled؛ Disabled وضعیت configuration است و می‌تواند Pendingهای بدون شروع را لغو کند. fail نامعلوم با ErrorCode و مرحلهٔ reconciliation از fail قابل retry متمایز شود. retry فقط خطای گذرا و عملیات idempotent: exponential backoff محدود با jitter، سقف attempt و هشدار نهایی.

## داشبورد مدیریتی پیشنهادی

نمای RTL با فهرست job، Enabled، زمان قبلی/بعدی، duration، history و error code پاک‌سازی‌شده؛ enable/disable، interval، run now و retry امن با policy JobAdmin، POST، antiforgery، expectedVersion و confirmation. زمان خام UTC و نمایش Asia/Tehran با label؛ credential هرگز بازنمایش نشود. تغییر config actor و before/after version داشته باشد.

پذیرش الزامی: دو worker هم‌زمان، job دستی و scheduled مشترک، crash قبل/بعد از call، lease expiry، network partition، restart، shutdown deadline، DST و cancellation آزمایش شوند. حفظ domain/config دریافتی Android در تمام سناریوها جدا بررسی شود.
