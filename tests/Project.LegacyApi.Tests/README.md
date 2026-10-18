# محافظ قراردادهای قدیمی API

۲۸ snapshot این پوشه پیش از refactor، از کد محصول در `8791b1a` و دادهٔ کاملاً ساختگی گرفته شدند: ۲۵ پاسخ HTTP، فهرست ۱۲۱ اکشن MVC، schemaهای ۴۴ DTO و گزینه‌های MVC/JSON. suite شامل ۳۲ تست است. snapshotها به‌طور خودکار هنگام تست ساخته یا به‌روزرسانی نمی‌شوند.

```bash
dotnet test tests/Project.LegacyApi.Tests/Project.LegacyApi.Tests.csproj
```

host آزمایشی controller، binding، filter، middleware خطا و serializer واقعی را اجرا می‌کند؛ تمام application serviceهای مورد استفاده جایگزین کنترل‌شده دارند. Program اصلی اجرا نمی‌شود و SQL، Cloudflare، Hangfire و seed حساب کاربری در host ثبت نشده‌اند. این suite تغییرات قرارداد HTTP را می‌سنجد، نه query دیتابیس، انتخاب سرور، DNS، job و کل pipeline استقرار.

تاریخ، traceId، فرهنگ، IP و User-Agent آزمایشی ثابت‌اند. تنها whitespace JSON نادیده گرفته می‌شود؛ ترتیب property/array، نام، عدد/رشته، null، content-type و status مقایسه می‌شوند. نمونه‌ها ترافیک واقعی Android نیستند؛ سازگاری کامل تولید همچنان به نمونه‌های واقعی پاک‌سازی‌شده نیاز دارد.

اگر تست شکست خورد، ابتدا تغییر behavior را بررسی کنید؛ baseline را صرفاً برای سبزشدن تست جایگزین نکنید. هر تغییر مصوب snapshot باید دلیل، اثر Android و بازبینی جداگانه داشته باشد. رفتارهای ناامن موجود، مانند Delete با GET و جزئیات exception، در این مرحله فقط ثبت شده‌اند؛ اصلاح آن‌ها تصمیم امنیت/سازگاری جدا می‌خواهد.
