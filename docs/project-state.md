# وضعیت پروژه

## اصلاح واکشی و repository دامنه — ۲۰۲۶-۱۰-۲۰، تهران

- مبنا `486b038`، روی همان `jobs`: فهرست غیرفعال، query انتخاب دامنه، خطای گمراه‌کنندهٔ ChangeDomain، ZoneId سرور و lookup پایهٔ مصرف‌شده اصلاح شدند. خواندن و شرط‌ها در repository، با AsNoTracking، ترتیب مشخص و TOP(1) انجام می‌شوند.
- تولید ۱۱٬۸۸۱٬۳۷۶ رشته برای یک نام DNS با ساخت مستقیم یک نام جایگزین شد؛ helper، فیلدها و queryهای بلااستفاده حذف شدند. ثبت زون شرطی است و ویرایش/غیرفعال‌سازی/تغییر نام هم‌زمان مدیر را بازنویسی نمی‌کند. حذف غیرفعال‌ها یک statement است.
- هر ۱۴۵ تست .NET (۷۴ unit/HTTP/قرارداد + ۷۱ SQL) و ۱۳ JavaScript موفق، صفر skipped؛ ۲۸ snapshot و DTOها نسبت به `657019e` ثابت‌اند. build نهایی هشت پروژه با ۳ warning قبلی و صفر error و بررسی EF بدون pending model change موفق بود.
- [گزارش دامنه](testing/domain-retrieval.md) رفتار pool، پایهٔ سرور موجود و محدودیت‌های DNS و پاک‌سازی فیزیکی را توضیح می‌دهد. هیچ SQL/DNS تولید اجرا نشده است؛ خطای گزارش‌شده به معنی اثبات وضعیت فعلی داده‌های تولید نیست.

مرحلهٔ فعلی: جایگزینی Hangfire با worker بومی .NET روی شاخهٔ `jobs`، از master در `657019e`، به درخواست صریح کاربر. PR #11 قبلی توسط مالک merge شده است. مرجع این گام: [موتور job](architecture/native-job-engine.md) و [آزمون و cutover](testing/native-jobs.md).

## ساخت خودکار جدول‌های جاب — ۲۰۲۶-۱۰-۱۹ UTC

- مبنا `801f992`؛ کاربر صریحاً خواست جدول گمشده خودکار ساخته شود. سیاست گام قبلیِ انتظار برای migration دستی با `SqlJobSchema.EnsureCreatedAsync` جایگزین شد: زیر قفل configuration و همان transaction، فقط جدول گمشدهٔ جاب و indexهایش از تعریف migration موجود ساخته می‌شوند. Jobs و جدول‌های موجود حفظ می‌شوند؛ migrationهای تاریخی اجرا نمی‌شوند.
- worker فعال در اولین claim و پنل در اولین دسترسی این مسیر را اجرا می‌کنند. ردیف migration جدید فقط در EF history موجود ثبت می‌شود؛ history نامعلوم baseline نمی‌شود. در خطای دسترسی، 503 مشخص و تلاش مجدد باقی است؛ روی schema آماده DDL لازم نیست.
- شواهد استفادهٔ قبلی و از‌دست‌رفتن جدول، marker با `schema_recreated` و NeedsReview می‌سازند؛ دادهٔ حذف‌شده قابل‌بازیابی نیست و dispatch تا review متوقف می‌ماند. [ADR-014](architecture/decisions.md) و [راهنما](testing/native-jobs.md) مرجع رفتار جدیدند.
- سه حالت ساخت خودکار پیش از اصلاح شکست خوردند؛ ۱۳ حالت هدفمند پس از اصلاح موفق شدند. اجرای کامل: ۱۲۱ تست .NET (۷۴ unit/HTTP/قرارداد + ۴۷ SQL) و ۱۳ JavaScript موفق، صفر skipped. build هشت پروژه با ۱۳ warning قبلی و صفر error؛ EF بدون pending model change و هر ۲۸ snapshot نسبت به `657019e` ثابت.
- هم‌زمانی دو instance، ساخت اولیه با/بدون EF history، حفظ دادهٔ جدول باقی‌مانده، خطای واقعی مجوز SQL و بازیابی با grant و ادامه پس از revoke DDL، rollback با cancellation وسط ساخت و اجرای IHostedService واقعی با business بدل تست شدند. SQL/DNS تولید، startup اصلی، seed و deployment اجرا نشده‌اند.
- تغییر تازهٔ مالک در `91e6536` برای pin بستهٔ Pkcs به 10.0.13 در پنج پروژه با merge `e35559b` حفظ شد. پس از restore، هر ۱۲۱ تست .NET روی نتیجهٔ ترکیب‌شده دوباره موفق شدند؛ build نهایی هشت پروژه با ۳ warning و صفر error، EF بدون تغییر مدل و snapshotها ثابت‌اند. نتیجهٔ ۱۳ تست JavaScript همین گام نیز موفق است؛ فایل‌های frontend در merge تغییر نکردند.

## اصلاح نبود جدول زمان‌بند — ۲۰۲۶-۱۰-۱۹ UTC

- مبنا `9f7e877` روی همان شاخهٔ `jobs` و Draft PR #12؛ گزارش کاربر: `Invalid object name 'JobSchedules'`. نبود هرکدام/هردو جدول در SQL آزمایشی با سه تست ناموفق بازتولید شد؛ دیتابیس کاربر بررسی یا تغییر داده نشده است.
- `SqlJobStore.WithConfigurationLockAsync` پیش از تغییر Jobs وجود هر دو جدول را بررسی می‌کند؛ خطای مشخص با نام migration می‌دهد. پنل و endpointهای مدیریت job پاسخ 503 قابل‌فهم دارند و worker با پیام راه‌اندازی منتظر می‌ماند. schema خودکار ساخته نمی‌شود؛ پاسخ‌های موفق Android ثابت‌اند.
- پس از اصلاح، هر ۱۱۳ تست .NET (۷۴ unit/HTTP/قرارداد + ۳۹ SQL) و ۱۳ JavaScript موفق، صفر skipped؛ build هشت پروژه با ۱۳ warning قبلی و صفر error، EF بدون pending model change و ۲۸ snapshot نسبت به `657019e` بدون تغییر.
- migration EF، اجرای دوبارهٔ اسکریپت واقعی SQL، حفظ تنظیمات قبلی و بازیابی همان store سپس اجرای worker با business بدل آزمایش شدند. [راهنمای رفع خطا](testing/native-jobs.md) فرمان EF/PMC و الزام هدف‌گیری همان دیتابیس برنامه را توضیح می‌دهد. اعمال روی دیتابیس واقعی و cutover هنوز انجام نشده است.

## گام jobها — ۲۰۲۶-۱۰-۱۹ UTC

- تنظیمات فعلی Jobs حفظ و جدول‌های افزایشی JobSchedules/JobExecutions اضافه شدند؛ آخرین نسخه، حتی غیرفعال، مرجع است. migration و اسکریپت idempotent برای بررسی آماده‌اند؛ هیچ DB تولید تغییر نکرده است.
- NativeJobWorker، قفل session مشترک SQL برای تمام jobها/instanceها، scope مستقل، ماندگاری موعد و درخواست دستی و history پیاده شدند؛ Hangfire و ثبت‌های حافظه‌ای آن حذف شدند.
- قطع اتصال یا اجرای رهاشدهٔ هر شش نوع، و شکست/لغو چهار job تغییردهنده، به بررسی انسانی می‌روند؛ retry کور ندارند. فعال‌سازی از دیتابیس، تنظیمات معتبر، cron دقیقه‌ای UTC و coalesce پس از restart حفظ شده‌اند.
- پنل مدیر برای شش job، تنظیمات، اجرای دستی و review دارد؛ mutationها POST و CSRF هستند. قالب API Android و ۲۸ snapshot قدیمی تغییر نکرده؛ تغییر مصوب محدود به مدیریت job مستند است.
- ۷ تست قواعد قدیمی پیش از انتقال موفق شدند. تست race مربوط به EF tracking ابتدا شکست خورد و پس از اصلاح موفق شد. هر ۱۰۷ تست .NET (۷۲ unit/HTTP + ۳۵ SQL) و ۱۳ JavaScript در اجرای نهایی موفق شدند؛ صفر skipped. build هشت پروژه بدون error و تطابق EF model/migration موفق؛ اسکریپت SQL افزایشی آماده است. نتیجهٔ CI از run واقعی PR بررسی شود.
- cutover باید با توقف همهٔ نسخه‌های Hangfire و اعمال migration انجام شود؛ دو engine هم‌زمان فعال نشوند. دامنهٔ قفل، jobهای engine جدید است؛ بازطراحی منطق DNS و هماهنگ‌سازی همهٔ writeهای مستقیم خارج این گام‌اند.

## سابقهٔ واکشی و lifetime

وضعیت تازهٔ remote در همین گام: مالک مخزن ممیزی #6 و پاک‌سازی #9 را merge کرده؛ PRهای #7 و #8 بسته و شاخهٔ feature حذف شده‌اند. master در `2d8d1ba` با merge `8501f1b` به همین fix وارد شد تا تغییرات تازهٔ مالک حفظ شوند؛ انتشار ادامه با Draft PR تازه به master است. گزارش تاریخی PRهای زیر وضعیت زمان اجرای همان گام را نشان می‌دهد.

## ادامهٔ گام — بررسی Dispose، ۲۰۲۶-۱۰-۱۹ UTC

- مبنا `8816489`؛ مشکل دریافت scoped از root در CronJobService singleton و پایان زودهنگام درخواست‌های حذف با ۱۵ تست ناموفق بازتولید شد؛ همان تست‌ها پس از اصلاح موفق شدند.
- هر job یک async scope مستقل دارد؛ زنجیرهٔ حذف‌های پنل await می‌شود؛ mapper از ILoggerFactory میزبان استفاده می‌کند؛ Serilog با dispose:true ثبت شده و cleanup ابزار SQL در شکست initialization محافظ دارد.
- ۱۸ تست جدید: ۱۷ تست scope/async/mapper و یک تست SQL واقعی برای بسته‌شدن اتصال و آزادسازی context. نتیجه: ۶۴ تست .NET و ۷ تست JavaScript موفق، صفر skipped؛ ۲۸ snapshot قرارداد قبلی بدون تغییر بایتی.
- پس از حفظ master، تمام ۶۴+۷ تست و بررسی ۲۸ snapshot دوباره موفق شدند؛ build نهایی incremental هشت پروژه با ۱۳ warning و صفر error موفق شد. build پیش از merge ۲۵ warning داشت؛ advisoryها و هشدارهای قبلی به‌عنوان بخشی از این اصلاح تغییر نکردند.
- قالب پاسخ موفق ثابت است؛ عملیات حذف تا پایان واقعی کار انتظار می‌کشد و خطای async را به مسیر خطای موجود می‌رساند. زمان‌بندی و سیاست exception jobها ثابت‌اند؛ shutdown کامل فرآیند و DNS زنده تست نشده‌اند.
- commit اصلاح lifetime برابر `aecf75f` است؛ همان شاخه ادامه دارد، ولی PR #8 قبلاً توسط مالک بسته شده و باز نمی‌شود. ادامه در Draft PR تازه به master قابل‌بازبینی است؛ هیچ push/merge به master توسط این گام انجام نشده. محدودیت‌ها، بخش غیرفعال SMS و rollback در گزارش عمر منابع ثبت شده‌اند.

## گام جاری — ۲۰۲۶-۱۰-۱۹

- بازتولید با SQL Server واقعی یک‌بارمصرف: فیلدهای ناقص فهرست/آمار، گروه null جزئیات و خطای null برای appId ناموجود تأیید شد؛ در اجرای اولیه ۶ تست از ۱۱ تست شکست خوردند.
- projection مشترک و کامل، read اختصاصی repository با Include گروه و AsNoTracking، مسیر مشترک Detail عددی/رشته‌ای و BadRequest کنترل‌شده برای appId نامعتبر اضافه شد.
- رندر پنل در برابر JSON خراب و گروه/مسیر غایب مقاوم شد؛ متن‌های HTML escape می‌شوند؛ فرم ویرایش اطلاعات تازهٔ Detail را مصرف می‌کند و رندر درج اضافه ندارد.
- نتیجهٔ محلی: ۱۴ تست SQL + ۳۲ تست قرارداد قبلی + ۷ تست JavaScript موفق؛ build هشت پروژه با صفر error. snapshotهای قبلی تغییر نکرده‌اند. commit اصلاح `3ccb76e` منتشر و [Draft PR #8](https://github.com/faryadman/RazdanehCMS/pull/8) با base شاخهٔ پاک‌سازی PR #7 ایجاد شد. CI این suiteها را هم اجرا می‌کند؛ نتیجهٔ نهایی remote در checks همان PR بررسی شود.
- ساختار و نام/type فیلدهای API حفظ شده؛ فیلدهای ناقص اکنون مقدار واقعی دارند. کد HTTP400 برای appId ناموجود حفظ و متن خطای null فارسی شده است. DB/DNS تولید، migration، انتخاب سرور Android و worker تغییر نکرده‌اند.

## سابقهٔ پاک‌سازی قبلی

پاک‌سازی محدود معماری روی `feature/preserve-api-contracts-cleanup` از `8791b1a` انجام شد و در Draft PR #7 قرار دارد؛ master و audit در گام جاری تغییر نمی‌کنند.

ممیزی اولیه در commitهای `3f462a1` و `8791b1a` روی شاخهٔ `audit/architecture-security-android-compat-20261017` منتشر شد. با رفع دسترسی GitHub API، [Draft PR ممیزی #6](https://github.com/faryadman/RazdanehCMS/pull/6) ساخته شد؛ merge انجام نشده است.

## گام انجام‌شده

- commit `90a1b1a`: ثبت تست قبل از تغییر محصول؛ ۲۵ پاسخ HTTP ساختگی، ۱۲۱ اکشن، ۴۴ schema DTO و گزینه‌های MVC در ۲۸ snapshot؛ ۳۲ تست قبل از refactor موفق.
- انتقال registration MVC/JSON به `LegacyAndroidMvcConfiguration`، حذف IMemoryCache بدون مصرف از دو کنترلر و حذف ۱۱ فایل خالی/بدون مصرف Job و Dapper.
- بعد از refactor: همان ۳۲ تست موفق، صفر skipped و تمام snapshotها بدون تغییر بایتی. build هفت پروژهٔ solution شامل تست‌ها موفق، ۱۱ warning و صفر error در اجرای نهایی incremental.
- workflow رگرسیون اضافه شده؛ موفقیت CI فقط با نتیجهٔ واقعی GitHub اعلام شود. جزئیات در [رگرسیون API](testing/legacy-api-regression.md).
- تصحیح گزارش ممیزی: default پارامتر filter در interface صفر است و همان به GetWithFilter فرستاده می‌شود؛ رفتار محصول تغییر نکرده است.

دامنهٔ پاک‌سازی قبلی: کد محصول فقط registration و وابستگی/کلاس بدون مصرف بود. DTO، route، action، serializer settings، منطق انتخاب سرور، JobController/CronJobService، DNS و schema دیتابیس در آن گام تغییر نکردند. هیچ SQL یا DNS تولید اجرا نشد. تغییر بعدی lifetime در بخش جاری بالای همین سند ثبت شده است.

## موارد باز

سازگاری کامل Android بدون نمونهٔ واقعی قابل‌ادعا نیست. تست SQL یک‌بارمصرف برای واکشی و lifetime اجرا شده است؛ SQL تولید، منطق انتخاب سرور Android، DNS زنده، job recovery و کل startup/proxy آزمایش نشده‌اند. کنترل دسترسی عملیات خطرناک، deletion گستردهٔ DNS، seed ثابت، credentialهای باقی‌مانده، عدم validation JSON و advisoryهای Pkcs همچنان بازند؛ حذف Dapper بدون مصرف آن‌ها را حل نمی‌کند.

گام بعد باید در PR مستقل با تست رفتاری باشد: اعتبارسنجی format-aware JSON با نمونهٔ معتبر یا استخراج policy انتخاب سرور بدون تغییر قرارداد. مسائل امنیتی بحرانی باید با تصمیم صریح دربارهٔ مصرف legacy اولویت داشته باشند. rollback این پاک‌سازی بازگرداندن commit refactor و حفظ commit تست است؛ مهاجرت داده ندارد.

شاخهٔ feature با commit تست `90a1b1a` و commit پاک‌سازی `94f97d7` push شده و [Draft PR پیاده‌سازی #7](https://github.com/faryadman/RazdanehCMS/pull/7) ایجاد شده است. PR ممیزی #6 به master و PR پیاده‌سازی #7 به شاخهٔ audit هدف می‌گیرد تا تغییرات کد جدا بررسی شوند. هیچ‌کدام merge نشده‌اند. workflow روی pull_request و push به master اجرا می‌شود تا push ویژگی و PR یک تست را دوبار اجرا نکنند؛ وضعیت نهایی CI از GitHub خوانده شود.
