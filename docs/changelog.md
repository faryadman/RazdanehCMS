# تغییرات

## ۲۰۲۶-۱۰-۱۹ UTC — ساخت خودکار جدول‌های جاب به درخواست کاربر

- ساخت جدول گمشدهٔ JobSchedules/JobExecutions و indexها زیر قفل SQL و transaction مشترک، با تعریف همان migration موجود؛ حفظ Jobs و دادهٔ جدول موجود.
- ثبت فقط migration جدید در history موجود؛ بازیابی جدول ازدست‌رفته با marker و توقف dispatch تا review، پیام مشخص در نبود مجوز ساخت و تلاش مجدد پس از اصلاح دسترسی.
- ۱۲۱ تست .NET و ۱۳ JavaScript موفق، صفر skipped؛ build هشت پروژه با صفر error، EF بدون تغییر مدل و ۲۸ snapshot ثابت. تست SQL واقعی برای هم‌زمانی، rollback با cancellation، حفظ داده و مجوز DDL؛ هیچ DB تولید تغییر نکرد.
- تغییر تازهٔ مالک برای Pkcs 10.0.13 در پنج پروژه حفظ شد؛ restore و هر ۱۲۱ تست .NET روی merge نیز موفق و build نهایی با ۳ warning و صفر error است.

## ۲۰۲۶-۱۰-۱۹ UTC — مدیریت نبود جدول‌های زمان‌بند

- بررسی وجود هر دو جدول پیش از تغییر Jobs؛ پیام مشخص migration در پنل، پاسخ 503 مدیریت job و لاگ زمان‌بند.
- راهنمای رفع `Invalid object name 'JobSchedules'` با اسکریپت SQL و فرمان‌های EF/PMC روی اتصال همان محیط؛ schema در startup خودکار تغییر نمی‌کند.
- پنج حالت خطا پیش از اصلاح ناموفق و سپس موفق شدند؛ بازیابی با EF و اسکریپت واقعی SQL، تکرار اسکریپت و حفظ تنظیمات قبلی بررسی شد. کل ۱۱۳ تست .NET و ۱۳ JavaScript موفق؛ snapshotها ثابت، build و بررسی مدل EF موفق. هیچ دیتابیس تولید تغییر نکرد.

## ۲۰۲۶-۱۰-۱۹ UTC — jobهای بومی در شاخهٔ jobs

- جایگزینی Hangfire با BackgroundService و ذخیرهٔ زمان‌بندی، درخواست دستی و history در SQL؛ تنظیمات Jobs موجود حفظ می‌شوند.
- قفل مشترک session بین همهٔ jobها و instanceها، scope و cancellation، توقف اجرای نامطمئن برای بررسی و نبود retry کور DNS.
- پنل شش job با نقش مدیر و POST/CSRF، رفع ذخیره‌نشدن disable و خواندن نسخهٔ قدیمی؛ حذف service/repositoryها و بسته‌های قدیمیِ بدون مصرف.
- پاک‌سازی batch لاگ تا watermark، انتقال لغو به عملیات job و حذف sync-over-async از مسیر اجرایی IP/دامنه.
- migration فقط دو جدول جدید می‌سازد؛ cutover و محدودیت‌ها در [راهنما](testing/native-jobs.md). هیچ production/DNS زنده تغییر نکرد.
- هر ۱۰۷ تست .NET و ۱۳ تست JavaScript در اجرای محلی نهایی موفق شدند؛ ۲۸ snapshot بدون بازتولید حفظ شدند. metadata فقط تغییر مصوب مدیریت job را مستثنا می‌کند. build و تطابق مدل/migration موفق و اسکریپت SQL آماده است.

## ۲۰۲۶-۱۰-۱۹ UTC — اصلاح عمر منابع روی همان شاخه

- scope مستقل و آزادسازی async برای هر اجرای CronJobService singleton؛ رفع نگهداری سرویس‌های scoped در root.
- انتظار کامل برای حذف‌های IP/Domain، استفادهٔ mapper از LoggerFactory میزبان و واگذاری dispose Serilog به host.
- آزادسازی context/cache تست در شکست initialization؛ افزودن ۱۸ تست رگرسیون مربوط به lifetime.
- ۶۴ تست .NET و ۷ تست JavaScript موفق؛ ۲۸ snapshot ثابت. شواهد، اثر مشاهده‌پذیر حذف و محدودیت‌ها در [گزارش Dispose](testing/resource-lifetime.md).
- مشاهدهٔ mergeهای انجام‌شده توسط مالک و بسته‌شدن PR #8؛ حفظ master جدید `2d8d1ba` روی همین fix با merge `8501f1b` و آماده‌سازی Draft PR تازه به master.

## ۲۰۲۶-۱۰-۱۹ — اصلاح واکشی پنل

- رفع فیلدهای ناقص در فهرست/آمار و بارگیری گروه در repository جزئیات؛ بدون تغییر DTO و route.
- حفظ HTTP400 برای فیلتر برنامهٔ ناموجود، همراه خطای فارسی کنترل‌شده به‌جای NullReference.
- رندر مقاوم برای JSON/گروه غایب، نمایش امن متن و استفادهٔ فرم از جزئیات تازه.
- افزودن ۱۴ تست SQL Server و ۷ تست JavaScript؛ همراه ۳۲ تست قدیمی، همگی محلی موفق و build بدون error.
- جزئیات، تغییر مقادیر ناقص و محدودیت‌ها در [گزارش واکشی](testing/server-retrieval.md) ثبت شد.
- انتشار commit `3ccb76e` در [Draft PR #8](https://github.com/faryadman/RazdanehCMS/pull/8) با base شاخهٔ PR #7؛ هیچ merge به master انجام نشد.

## ۲۰۲۶-۱۰-۱۸ — اولین پاک‌سازی محدود

- ایجاد feature branch از ممیزی؛ ثبت baseline تست قبل از refactor در `90a1b1a`.
- افزودن ۳۲ تست قرارداد و workflow؛ ۲۸ snapshot قبل/بعد بدون تغییر.
- جداسازی registration MVC/JSON، حذف وابستگی بلااستفادهٔ دو کنترلر و ۱۱ فایل Job/Dapper بدون مصرف؛ بستهٔ Dapper به دلیل مصرف فعال دیگر باقی ماند.
- build solution موفق؛ تست کامل SQL/DNS/Android تولید ادعا نمی‌شود.
- ایجاد Draft PR ممیزی [#6](https://github.com/faryadman/RazdanehCMS/pull/6) پس از رفع دسترسی API؛ هیچ merge انجام نشد.
- انتشار commitهای `90a1b1a` و `94f97d7` و ایجاد [Draft PR پاک‌سازی #7](https://github.com/faryadman/RazdanehCMS/pull/7) با base شاخهٔ audit؛ CI روی PR و push master اجرا می‌شود.
- تصحیح توضیح default filter لیست سرور در گزارش؛ بدون تغییر behavior.

## ۲۰۲۶-۱۰-۱۸ — ممیزی اولیه

- افزودن AGENTS.md و اسناد ممیزی، قرارداد API/schema، امنیت و دیتابیس.
- افزودن طرح هدف، نمودارها، native job engine، DNS، JSON، جریان داده و پنل RTL.
- افزودن برنامهٔ migration، تصمیم‌ها، نقشهٔ راه و روش تست با تفکیک اجرای واقعی و برنامهٔ آینده.
- بدون تغییر source برنامه، package، migration، تنظیمات runtime و رفتار تولید؛ بدون حذف فایل/جدول و بدون اجرای DNS زنده.
- build و serializer بررسی شدند؛ تست کامل Android/SQL/worker هنوز اجرا نشده است.

- انتشار مستندات روی audit با commit اولیهٔ `3f462a1`؛ ایجاد Draft PR با Forbidden از GitHub GraphQL مسدود شد. متن PR ذخیره و domain موردنیاز به draft شبکه افزوده شد؛ اتصال پس از اعمال تنظیمات هنوز بررسی نشده است.
