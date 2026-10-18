# رگرسیون API در اولین پاک‌سازی

شاخه: `feature/preserve-api-contracts-cleanup`، از `8791b1a` بدون تغییر master یا شاخهٔ audit. درخواست جدید کاربر مجوز این گام محدود است؛ جایگزینی Hangfire، بازطراحی DNS/دیتابیس و تغییر پاسخ‌ها در این گام نیست.

## baseline و اجرای واقعی

ابتدا بدون تغییر هیچ فایل محصول، host آزمایشی با کنترلرهای واقعی، binding، filter، middleware خطا و Newtonsoft فعلی ساخته شد. سرویس‌های کاربردی substitute هستند؛ entrypoint و seed حساب، SQL، Cloudflare و Hangfire اجرا نمی‌شوند. ۲۵ سناریوی HTTP و metadata مربوط به ۱۲۱ اکشن، ۴۴ DTO و گزینه‌های MVC در ۲۸ فایل JSON ثبت و در commit `90a1b1a` جدا از refactor قرار گرفتند.

۳۲ تست شامل snapshotها و چهار بررسی forwarding ورودی/پیش‌فرض/جلوگیری از write نامعتبر، قبل از refactor موفق شدند. پس از استخراج registration و حذف کدهای بی‌مصرف، همان ۳۲ تست با صفر failure و صفر skipped گذشتند. ۲۸ فایل baseline با SHA256 مقایسه شدند و تغییر بایتی ندارند. یک اجرای نهایی روی solution با host جدید مبتنی بر HostBuilder نیز ۳۲ تست موفق داشت.

دامنهٔ بررسی: status، content-type، نام و ترتیب property، مقدار/null، رشتهٔ Config، enum عددی، آرایه و خطاهای malformed JSON، ورودی نامعتبر، not-found و exception با دادهٔ ساختگی. نتیجهٔ actual-before با actual-after مقایسه شده است؛ snapshotها از معماری مطلوب حدس زده نشده‌اند و runtime تست آن‌ها را بازنویسی نمی‌کند.

```bash
dotnet restore RazdanehCMS.sln
dotnet test RazdanehCMS.sln --no-restore -m:2
dotnet build RazdanehCMS.sln --no-restore -m:2
```

SDK محلی: 10.0.402. workflow مربوط به همین تست‌ها اضافه شده؛ نتیجهٔ اجرای GitHub Actions باید از run واقعی خوانده شود و با موفقیت محلی یکی فرض نشود. warningهای قبلی dependency هنوز وجود دارند.

## تغییر معماری

- `Configuration/LegacyAndroidMvcConfiguration.cs` محل registration قرارداد MVC قدیمی است؛ برنامه و تست از همان registration استفاده می‌کنند. تغییر تنظیم null، enum، casing یا routing با baseline مقایسه می‌شود.
- IMemoryCache بلااستفاده از constructor دو کنترلر عمومی حذف شد؛ method signature اکشن‌ها ثابت است.
- شش کلاس job خالی، JobNames بدون مصرف، IJobManager بدون implementation فعال و JobManager کامنت‌شده حذف شدند؛ engine فعال در CronJobService/JobController حفظ شده است.
- DapperQueryService و interface آن فقط به هم و یک registration بدون مصرف ارجاع داشتند و حذف شدند. Dapper package حفظ شد، چون CronJobInfoController هنوز آن را مصرف می‌کند. حذف این سرویس تمام credentialهای باقی‌ماندهٔ سامانه را اصلاح نمی‌کند.

یک تصحیح ممیزی هم ثبت شد: GetWithFilter در interface مقدار پیش‌فرض۰ و در implementation مقدار۱ دارد؛ کنترلر از interface استفاده می‌کند و۰ می‌فرستد. این رفتار با تست مشخص ثبت شد و کد آن تغییر نکرد.

## محدودیت و بازگشت

این suite جای آزمون query واقعی، انتخاب سرور، auth پنل، DNS، job recovery یا کل pipeline startup و reverse proxy را نمی‌گیرد. نمونهٔ واقعی Android نداریم و سازگاری صددرصد تولید ادعا نمی‌شود. سرویس مستقل WebApi تغییر نکرده و در build solution بررسی می‌شود؛ پاسخ‌های آن در این suite HTTP نیستند.

rollback این گام بازگرداندن commit پاک‌سازی و نگه‌داشتن commit تست است؛ migration، تغییر داده یا عملیات DNS ندارد. گام بعدی باید policy انتخاب سرور یا validator JSON را با تست رفتاری و corpus معتبر، در PR جدا بررسی کند؛ اصلاح امنیت و semantics قدیمی به این تغییر پنهان اضافه نشود.
