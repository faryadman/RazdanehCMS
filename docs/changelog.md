# تغییرات

## ۲۰۲۶-۱۰-۱۸ — اولین پاک‌سازی محدود

- ایجاد feature branch از ممیزی؛ ثبت baseline تست قبل از refactor در `90a1b1a`.
- افزودن ۳۲ تست قرارداد و workflow؛ ۲۸ snapshot قبل/بعد بدون تغییر.
- جداسازی registration MVC/JSON، حذف وابستگی بلااستفادهٔ دو کنترلر و ۱۱ فایل Job/Dapper بدون مصرف؛ بستهٔ Dapper به دلیل مصرف فعال دیگر باقی ماند.
- build solution موفق؛ تست کامل SQL/DNS/Android تولید ادعا نمی‌شود.
- ایجاد Draft PR ممیزی [#6](https://github.com/faryadman/RazdanehCMS/pull/6) پس از رفع دسترسی API؛ هیچ merge انجام نشد.
- تصحیح توضیح default filter لیست سرور در گزارش؛ بدون تغییر behavior.

## ۲۰۲۶-۱۰-۱۸ — ممیزی اولیه

- افزودن AGENTS.md و اسناد ممیزی، قرارداد API/schema، امنیت و دیتابیس.
- افزودن طرح هدف، نمودارها، native job engine، DNS، JSON، جریان داده و پنل RTL.
- افزودن برنامهٔ migration، تصمیم‌ها، نقشهٔ راه و روش تست با تفکیک اجرای واقعی و برنامهٔ آینده.
- بدون تغییر source برنامه، package، migration، تنظیمات runtime و رفتار تولید؛ بدون حذف فایل/جدول و بدون اجرای DNS زنده.
- build و serializer بررسی شدند؛ تست کامل Android/SQL/worker هنوز اجرا نشده است.

- انتشار مستندات روی audit با commit اولیهٔ `3f462a1`؛ ایجاد Draft PR با Forbidden از GitHub GraphQL مسدود شد. متن PR ذخیره و domain موردنیاز به draft شبکه افزوده شد؛ اتصال پس از اعمال تنظیمات هنوز بررسی نشده است.
