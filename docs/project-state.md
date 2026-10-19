# وضعیت پروژه

مرحلهٔ فعلی: اصلاح واکشی و بازبینی عمر منابع روی `fix/server-retrieval`. کاربر پس از رفع واکشی، بررسی Dispose را صریحاً روی همین شاخه خواست. گزارش‌ها: [واکشی](testing/server-retrieval.md) و [عمر منابع](testing/resource-lifetime.md).

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
