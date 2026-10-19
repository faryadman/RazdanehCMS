# استقرار و آزمون jobهای بومی

مبنای شاخهٔ `jobs`: master در `657019e`. جایگزینی Hangfire به درخواست کاربر انجام شده است؛ تغییر تولید یا اجرای DNS زنده انجام نشده. [معماری و سیاست اجرا](../architecture/native-job-engine.md) مرجع رفتار این گام است.

## راه‌اندازی روی دیتابیس موجود

۱. backup و آخرین migration اعمال‌شده را بررسی کنید. [اسکریپت افزایشی](../../scripts/sql/native-job-scheduling.sql) از `20260923234510_updateServerZoneId` تا `20261019211502_AddNativeJobScheduling` تولید شده است؛ idempotent بودن به معنی جبران migrationهای قبلیِ اعمال‌نشده نیست. اسکریپت را با مسیر اتصال مصوب همان محیط اجرا کنید؛ مقدار اتصال وارد مستندات یا chat نشود.

۲. هنگام cutover تمام نمونه‌های نسخهٔ دارای Hangfire را متوقف کنید و پایان کارهای درحال‌اجرایشان را بررسی کنید. قفل جدید مانع اجرای engine قدیمی که به آن متعهد نیست نمی‌شود؛ rolling deployment با دو engine فعال انجام نشود.

۳. نسخهٔ جدید را ابتدا با `NativeJobs__Enabled=false` بالا بیاورید. مهاجرت خودکار در startup وجود ندارد؛ schema باید پیش از استفاده از worker/پنل آماده باشد. این مهاجرت فقط JobSchedules و JobExecutions را می‌سازد و داده‌های Jobs را تغییر نمی‌دهد.

۴. در `/Admin/Job` با نقش مدیر تنظیمات را بررسی کنید. جدیدترین Id هر نوع، حتی غیرفعال، مرجع است. خاموش‌کردن قدیمی Hangfire گاهی در Jobs ذخیره نمی‌شد؛ وضعیت واقعی خاموش/روشن در cutover باید توسط مالک تأیید شود. تنظیمات نامعتبر در پنل متوقف نمایش داده می‌شوند. اگر هیچ رکوردی برای یک نوع وجود نداشته باشد، پیش‌فرض در دیتابیس ایجاد می‌شود: فقط logDeleterJob با ۱۰ و CheckZoneId با ۴۵ فعال‌اند.

۵. پس از آماده‌بودن تنظیمات و پایان نسخهٔ قبلی، `NativeJobs__Enabled=true` را در تنظیمات محیط قرار دهید و نمونهٔ جدید را راه‌اندازی کنید. jobهای فعال از دیتابیس خوانده می‌شوند و دیگر به ثبت در حافظه وابسته نیستند. پارامترهای میزبان اختیاری‌اند: `NativeJobs__PollSeconds` پیش‌فرض ۵، معتبر ۱ تا ۶۰؛ `NativeJobs__ExecutionTimeoutMinutes` پیش‌فرض ۱۵، معتبر ۱ تا ۱۴۴۰. دوره، فعال‌بودن و آستانه‌های هر job در Jobs ذخیره‌اند، نه appsettings.

۶. شروع، پایان، موعد بعدی، شمارندهٔ موفقیت و سابقه را در پنل کنترل کنید. اجرای دستی یک درخواست پایدار است و تأیید JSON آن به معنی پایان عملیات نیست. تنظیماتِ اجرای جاری snapshot هستند؛ ویرایش فعال برای اجرای بعدی است و غیرفعال‌سازی لغو cooperative درخواست می‌کند.

برای اجرای tooling از SDK محیط و `ApplicationDbContextDesignFactory` استفاده می‌شود؛ Program، seed و worker در تولید migration اجرا نمی‌شوند. factory در نبود `RAZDANEH_DESIGN_CONNECTION_STRING` به نام دیتابیس صرفاً طراحی در localhost محدود است. برای تغییر واقعی دیتابیس از اتصال مصوب محیط یا secret store استفاده کنید؛ مقدار نمونهٔ طراحی اتصال تولید نیست.

## قطع اجرا و بازگشت

| کد ثبت‌شده | معنی / اقدام |
|---|---|
| `execution_failed` | handler شکست خورده؛ چهار job تغییردهنده نیازمند بررسی اثرند |
| `process_interrupted` | Running بدون صاحب پس از restart شناسایی شده است |
| `connection_lost` | اتصال صاحب قفل از دست رفته؛ اجرای فعال لغو و نتیجه نامطمئن ثبت شده است |
| `configuration_changed` | غیرفعال‌سازی یا حذف تنظیمات، لغو اجرای فعال را درخواست کرده است |
| `execution_timeout` | مهلت تعیین‌شده تمام شده؛ task رها نمی‌شود و تا پایان واقعی قفل نگه داشته می‌شود |
| `shutdown` | میزبان در حال توقف بوده است |

در NeedsReview، dispatch جدید متوقف می‌شود. ابتدا مطمئن شوید فرآیند قبلی دیگر فعالیت ندارد و اثر SQL/Cloudflare بررسی شده؛ سپس با POST محافظت‌شده در پنل بررسی را ثبت کنید. این کار همان execution را retry نمی‌کند. هیچ دکمهٔ retry کور برای DNS اضافه نشده است.

برای rollback ابتدا NativeJobs را غیرفعال و همهٔ نمونه‌های جدید را متوقف کنید و هر Running/NeedsReview را بررسی کنید؛ سپس فقط یک engine قدیمی را، در صورت تصمیم به بازگشت، فعال کنید. دو جدول افزایشی را نگه دارید تا history حفظ شود؛ برای بازگشت کد نیازی به Down یا حذف داده نیست. زمان‌بندی حافظه‌ای Hangfire بعد از restart ممکن است به بازثبت تنظیمات نیاز داشته باشد؛ این محدودیت نسخهٔ قدیمی است.

## شواهد آزمون

پیش از جابه‌جایی business از JobController، ۷ تست قواعد سن/تعداد/درصد شکست دامنه و شرط آمار/سن IP روی کد قبلی موفق بودند؛ پس از انتقال، همان حالت‌ها روی JobHandler کنترل می‌شوند. جداگانه، race ناشی از tracking قدیمیِ EF بین خواندن فرم و ثبت درخواست اجرای هم‌زمان با تست SQL شکست خورد؛ پس از تازه‌خواندن entityهای متعلق به scheduler در هر عملیات، تست موفق شد. تغییر مربوط به همان جدول‌هاست و tracking مربوط به Identity و business پاک نمی‌شود.

تست‌های `Project.ServerRetrieval.Tests/NativeJobSqlTests` در SQL Server یک‌بارمصرف پوشش می‌دهند:

- مرجع‌بودن آخرین رکورد غیرفعال، توقف JSON خراب و حفظ نسخه‌های قدیمی Jobs.
- ذخیره و ویرایش تنظیمات غیرفعال، restart provider، درخواست دستی پایدار، ادغام تکرارها و تقدم درخواست قدیمی‌تر.
- دو provider/worker و دو job تغییردهندهٔ متفاوت؛ حداکثر یک handler فعال زیر قفل واقعی SQL.
- coalesce موعدهای عقب‌افتاده و ماندگاری موعد آینده از ساعت SQL UTC.
- Running رهاشده برای rotation، پاک‌سازی لاگ و zone، توقف dispatch همهٔ jobها، تأیید بررسی ثبت‌شده و عدم replay شناسهٔ قبلی؛ session از‌دست‌رفته به معنی پایان قطعی task قبلی فرض نمی‌شود.
- شکست با کد پاک‌سازی‌شده، جلوگیری از retry کور، لغو با غیرفعال‌سازی و ثابت‌ماندن snapshot در ویرایش فعال.
- ویرایش درجا در دیتابیس و قطع واقعی session صاحب قفل با KILL فقط در container همان تست.
- اجرای واقعی NativeJobWorker از registration مربوط به IHostedService و توقف تمیز آن؛ business بدل است و Cloudflare واقعی فراخوانی نمی‌شود.
- اعمال migration روی schema موجود و history قبلیِ شبیه‌سازی‌شده؛ Jobs قبلی بدون تغییر می‌ماند. این آزمون بازپخش تمام migrationهای تاریخی از دیتابیس خالی یا اثبات سازگاری همهٔ نسخه‌های تولید نیست.
- حذف بیش از یک batch لاگ، نبود بارگیری entityها و حفظ لاگی که بعد از watermark وارد شده است.
- scope مدیریتیِ دوباره‌استفاده‌شده، خواندن تازهٔ وضعیت و حذف درخواست دستی هنگام غیرفعال‌سازی.

تست‌های unit/HTTP شامل مرز ساعت/سال/کبیسه، تنظیمات نامعتبر، نبود آمار، cancellation، انتظار برای task و DisposeAsync در موفقیت/خطا، timeout و shutdown با handler غیرهمکار هستند. پنل واقعی Razor با TestServer و احراز هویت آزمایشی بررسی شده: 401 بدون ورود، 403 برای غیرمدیر، رد POST بدون CSRF، خواندن token/cookie از HTML و ذخیره/درخواست موفق با token معتبر. همهٔ mutationهای job نقش admin و POST/CSRF دارند.

تست‌های Node برای فرم جدید شامل ارسال FormData، جلوگیری از submit هم‌زمان، خطای شبکه، نمایش امن متن خطا، redirect ورود و تأیید بررسی‌اند؛ این‌ها بدل DOM هستند و مرورگر کامل محسوب نمی‌شوند.

```bash
dotnet restore RazdanehCMS.sln
dotnet test RazdanehCMS.sln --no-restore -m:2 --logger trx
node --test tests/frontend/*.test.cjs
dotnet build RazdanehCMS.sln --no-restore -m:2
dotnet ef migrations has-pending-model-changes --project CMS/Project.Persistence --startup-project CMS/Project.Web.AndroidAppsProject --context ApplicationDbContext
```

نتیجهٔ محلی نهایی: ۷۲ تست unit/HTTP/قرارداد و ۳۵ تست SQL، مجموع ۱۰۷ تست .NET، به‌علاوهٔ ۱۳ تست JavaScript؛ همگی موفق، صفر skipped. اعتبارسنجی رکوردهای فعالِ قدیمیِ بدون دوره/آستانه هم پوشش دارد تا از شروع عملیات با پیش‌فرض ناخواسته جلوگیری شود. هر ۲۸ snapshot نسبت به `657019e` بدون تغییر بایتی است. metadata قدیمی فقط برای تغییرهای مشخص مدیریت job مقایسهٔ تعدیل‌شده دارد؛ بدنه‌های HTTP، schema هر ۴۴ DTO و تنظیمات JSON همان baseline هستند. build هشت پروژه بدون error و بررسی EF بدون pending model change موفق بود؛ اسکریپت idempotent نیز تولید و scope آن دو جدول جدید کنترل شد. هشدارهای قبلی dependency/build همچنان باقی‌اند.

Program اصلی، seed مدیر، DNS/SQL تولید، Cloudflare زنده، مرورگر کامل، deployment واقعی، network partition دوطرفهٔ provider و kill سیستم‌عامل هنگام call بیرونی تست نشده‌اند. advisoryهای قبلی Pkcs خارج این اصلاح‌اند. این آزمون‌ها جای بازبینی نسخه/compatibility level دیتابیس واقعی و cutover تک‌engine را نمی‌گیرند.
