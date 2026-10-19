# وضعیت پروژه

مرحلهٔ فعلی: اصلاح واکشی فهرست/جزئیات پنل روی `fix/server-retrieval` از `f25783f`. کاربر مشکل را به پنل محدود کرد. مستندات تفصیلی در [گزارش واکشی](testing/server-retrieval.md) است.

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

دامنهٔ اصلاح: کد محصول فقط registration و وابستگی/کلاس بدون مصرف است. DTO، route، action، serializer settings، منطق انتخاب سرور، JobController/CronJobService، DNS و schema دیتابیس تغییر نکرده‌اند. هیچ SQL یا DNS تولید اجرا نشده است.

## موارد باز

سازگاری کامل Android بدون نمونهٔ واقعی قابل‌ادعا نیست. تست‌های SQL، منطق انتخاب سرور، DNS، job recovery و کل startup/proxy اجرا نشده‌اند. کنترل دسترسی عملیات خطرناک، deletion گستردهٔ DNS، seed ثابت، credentialهای باقی‌مانده، عدم validation JSON و advisoryهای Pkcs همچنان بازند؛ حذف Dapper بدون مصرف آن‌ها را حل نمی‌کند.

گام بعد باید در PR مستقل با تست رفتاری باشد: اعتبارسنجی format-aware JSON با نمونهٔ معتبر یا استخراج policy انتخاب سرور بدون تغییر قرارداد. مسائل امنیتی بحرانی باید با تصمیم صریح دربارهٔ مصرف legacy اولویت داشته باشند. rollback این پاک‌سازی بازگرداندن commit refactor و حفظ commit تست است؛ مهاجرت داده ندارد.

شاخهٔ feature با commit تست `90a1b1a` و commit پاک‌سازی `94f97d7` push شده و [Draft PR پیاده‌سازی #7](https://github.com/faryadman/RazdanehCMS/pull/7) ایجاد شده است. PR ممیزی #6 به master و PR پیاده‌سازی #7 به شاخهٔ audit هدف می‌گیرد تا تغییرات کد جدا بررسی شوند. هیچ‌کدام merge نشده‌اند. workflow روی pull_request و push به master اجرا می‌شود تا push ویژگی و PR یک تست را دوبار اجرا نکنند؛ وضعیت نهایی CI از GitHub خوانده شود.
