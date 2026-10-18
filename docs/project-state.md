# وضعیت پروژه

مرحلهٔ فعلی: اولین پاک‌سازی محدود معماری با درخواست کاربر، روی `feature/preserve-api-contracts-cleanup` از baseline `8791b1a`. شاخهٔ audit و master در این گام تغییر نکرده‌اند. این مجوز به معنی تصویب یا اجرای تمام بازطراحی نیست.

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

وضعیت انتشار feature و شمارهٔ Draft PR پس از push ثبت می‌شود. PR ممیزی #6 به master و PR پیاده‌سازی به شاخهٔ audit هدف می‌گیرد تا تغییرات کد جدا بررسی شوند.
