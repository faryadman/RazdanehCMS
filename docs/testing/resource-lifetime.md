# بررسی Dispose و عمر منابع

تاریخ: ۲۰۲۶-۱۰-۱۹ UTC. مبنا `8816489` روی `fix/server-retrieval`. کاربر صریحاً بررسی روی همین شاخه را خواست؛ شاخهٔ تازه ساخته نشد. دامنهٔ اصلاح، مسیرهای فعال `Project.Web.AndroidAppsProject`، سرویس‌های مربوط و ابزار تست واکشی است.

## یافته‌ها و اصلاحات

| مسیر / متد | شاهد و اثر قبلی | نتیجه |
|---|---|---|
| `CronJobService`، هر پنج متد | ثبت singleton در Program و دریافت سرویس scoped از root provider؛ با ValidateScopes خطا و بدون آن نگهداری DbContext تا پایان host و اشتراک context بین اجراها | تزریق IServiceScopeFactory، یک CreateAsyncScope برای هر اجرا، await عملیات و await using؛ آزادسازی در موفقیت و خطا، scope مستقل برای اجراهای هم‌زمان |
| `IPConfigController.DeleteInactive` | Task حذف‌ها بدون await؛ پاسخ موفق زودهنگام و امکان ادامهٔ SQL پس از بسته‌شدن request scope یا عملیات هم‌زمان روی context مشترک | انتظار ترتیبی برای هر حذف؛ خطای حذف مشاهده می‌شود و حذف بعدی شروع نمی‌شود |
| `DomainsController.DeleteInactiveDomain` و `DomainService.Remove` | Task سرویس و Taskهای repository انتظار کشیده نمی‌شدند؛ repository فعلی برخی از آن‌ها را همگام انجام می‌دهد، پس وقوع use-after-dispose در مسیر فعلی Domain اثبات نشده است | await در تمام زنجیره؛ شکل پاسخ موفق و شرط انتخاب دامنهٔ غیرفعال حفظ شده است |
| `Program` و `ApplicationServicesRegistration` | LoggerFactory ساخته‌شده با new خارج از مالکیت container بود | mapper از ILoggerFactory متعلق به host استفاده می‌کند؛ پایان request آن را آزاد نمی‌کند، پایان provider آن را آزاد می‌کند |
| `Program.UseSerilog` | logger سراسری با dispose پیش‌فرض و بدون CloseAndFlush ثبت شده بود | dispose:true مالکیت shutdown را به provider می‌دهد؛ مطابق XML بستهٔ Serilog.Extensions.Hosting 10.0.0، CloseAndFlush هنگام dispose provider اجرا می‌شود |
| ابزار `RetrievalDatabase` | شکست Seed قبل از انتقال مالکیت به caller می‌توانست context/cache را بدون آزادسازی بگذارد؛ خطای DisposeAsync context نیز از Dispose cache جلوگیری می‌کرد | cleanup در شکست initialization و finally برای cache |

exceptionهای دو job مربوط به ChangeDomain/ChangeSubDomain مانند قبل ثبت و بلعیده می‌شوند؛ سه متد دیگر خطا را منتشر می‌کنند. امضاهای ICronJobService، زمان‌بندی، سیاست retry و رفتار DNS تغییر نکرده‌اند. نگهداری وضعیت recovery، جلوگیری از هم‌پوشانی jobها و کنترل DNS همچنان موضوع‌های مستقل‌اند.

## مالکیت درست در مسیر واکشی

`ConfigurePersistenceServices`، ApplicationDbContext و repositoryها را scoped ثبت می‌کند. repository مالک context تزریق‌شده نیست و نباید آن را دستی Dispose کند؛ عمر context با request یا scope اجرای job تمام می‌شود. در تست SQL واقعی، repository تا قبل از پایان scope قابل‌استفاده بود، query بدون tracking انجام شد، اتصال عمداً باز تا پایان scope باز ماند و DisposeAsync scope آن را بست؛ دسترسی بعدی به context با ObjectDisposedException رد شد. بسته‌شدن اتصال منطقی با نگهداری اتصال فیزیکی در pool سازگار است.

کلاینت‌های محلی Cloudflare در کد فعال با using و await داخل همان محدوده استفاده می‌شوند؛ streamهای upload نیز await using دارند. این دو مورد بازبینی کد شده‌اند، نه آزمایش شبکهٔ زنده. IServiceProvider، IMemoryCache و ILoggerFactory تزریق‌شده در سرویس مصرف‌کننده دستی آزاد نشده‌اند؛ مالک آن‌ها container است.

## تست‌های اجراشده

۱۵ تست جدید پیش از اصلاح اجرا شدند و هر ۱۵ شکست خوردند: jobها در provider با ValidateScopes نمی‌توانستند وابستگی scoped دریافت کنند، و اکشن‌های حذف زود تمام می‌شدند یا خطا را مشاهده نمی‌کردند. بعد از اصلاح همان ۱۵ تست موفق شدند.

پوشش نهایی افزوده‌شده ۱۸ تست است:

- ۱۱ تست job: هر پنج مسیر در موفقیت/خطا و دو اجرای هم‌زمان؛ resource بدل فقط IAsyncDisposable دارد و پایان DisposeAsync با یک Task کنترل می‌شود تا انتظار واقعی برای آزادسازی نیز ثابت شود.
- ۴ تست اکشن حذف: انتظار، ترتیب، توقف در خطا و بدنهٔ پاسخ موفق قبلی.
- ۱ تست DomainService: فقط دامنهٔ غیرفعال حذف می‌شود؛ removal و save واقعاً انتظار کشیده می‌شوند.
- ۱ تست mapper: ILoggerFactory میزبان استفاده می‌شود و فقط با پایان provider آزاد می‌شود.
- ۱ تست repository با SQL Server یک‌بارمصرف: مالکیت context و بسته‌شدن اتصال با DisposeAsync scope.

اجرای محلی نهایی: ۴۹ تست در Project.LegacyApi.Tests و ۱۵ تست در Project.ServerRetrieval.Tests، مجموع ۶۴ تست .NET موفق، بدون skipped. ۷ تست JavaScript قبلی نیز موفق بودند. هر ۲۸ snapshot نسبت به `8816489` بایت‌به‌بایت ثابت‌اند؛ تست metadata همان ۱۲۱ اکشن و ۴۴ DTO را تأیید می‌کند. build هر هشت پروژه پس از تغییر نهایی Program موفق بود: ۲۵ warning و صفر error؛ هشدارهای قبلی از جمله advisory وابستگی‌ها همچنان باقی‌اند.

```bash
dotnet test RazdanehCMS.sln --no-restore -m:2 --logger trx
node --test tests/frontend/server-retrieval.test.cjs
dotnet build RazdanehCMS.sln --no-restore -m:2
```

## اثر قابل‌مشاهده و محدودیت

route، DTO، casing، نوع فیلد و بدنهٔ موفق قبلی حفظ شده‌اند. درخواست حذف اکنون تا پایان واقعی کار باز می‌ماند؛ شکست async به مسیر خطای موجود می‌رود و موفقیت کاذب برنمی‌گردد. تضمین یکسان‌ماندن پاسخ موفق کاذب یا زمان پاسخ قبلی ادعا نمی‌شود.

Program اصلی، seed، Hangfire واقعی و درخواست Cloudflare اجرا نشدند. shutdown کامل فرآیند، شکست startup پیش از ساخت host، بار طولانی‌مدت و cancellation عملیاتی آزمایش نشده‌اند؛ تغییر Serilog با قرارداد بسته و build بررسی شده است. cleanup شکست Seed نیز بازبینی کد شده و fault injection مجزا ندارد. این شواهد اثبات نبود همهٔ memory leakهای برنامه نیستند.

Web.Admin قدیمی net6 و ارسال‌کننده‌های غیرفعال Mail/SMS در مسیر فعلی اجرا ثبت نمی‌شوند و این گام آن‌ها را فعال یا بازنویسی نکرد. `SmsSender` همچنان RestClient و CancellationTokenSource دستی با مالکیت ناقص دارد؛ پیش از فعال‌سازی آن باید جدا اصلاح و با transport آزمایشی بررسی شود.

rollback با revert commit همین اصلاح ممکن است و migration یا تبدیل داده لازم ندارد. هیچ وابستگی یا snapshot قرارداد عوض نشده است.
