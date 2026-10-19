# نقشهٔ راه

مرحلهٔ فعلی: پس از merge ممیزی #6، پاک‌سازی #9 و واکشی/lifetime در #11 توسط مالک، کاربر پیاده‌سازی jobهای بهتر و کنارگذاشتن Hangfire را خواست. شاخهٔ `jobs` شامل worker بومی، ذخیرهٔ پایدار SQL، قفل بین instanceها، history، پنل مدیریت و migration افزایشی است؛ [طرح اجراشده](architecture/native-job-engine.md). این گام به معنی تکمیل بازطراحی مالکیت DNS، validator JSON یا هماهنگ‌سازی تمام writeهای خارج از job نیست.

| اولویت / مرحله | خروجی و scope | پیش‌نیاز پذیرش / rollback |
|---|---|---|
| P0 — همین ممیزی | معماری، ماتریس API، امنیت، دیتابیس، jobs/DNS/JSON و طرح migration؛ docs-only | بررسی شواهد و محدودیت‌ها؛ بدون merge خودکار |
| P0 — Iteration 1 | کنترل دسترسی خطرناک، secret handling، جلوگیری از DNS unrelated، baseline contract | تعیین مصرف legacy، fixture واقعی، SQL/Cloudflare fake؛ flag و بازگشت محدودسازی |
| P1 — Iteration 2 | validator format-aware در backend/frontend برای تمام writeها | corpus واقعی و تست صفر write در شکست، حفظ input و snapshot Android |
| P1 — Iteration 3 | پیاده‌شده روی jobs: SQL store/history، session lock و native worker؛ cutover تولید هنوز انجام نشده | تست crash/restart/multi-instance/cancel موفق؛ توقف engine قدیمی، migration و بازبینی تنظیمات پیش از فعال‌سازی تولید |
| P1 — Iteration 4 | assignment/history و provider محدود DNS | fake zone مشترک، failure میانی، reconciling و snapshot Config؛ compensation هدفمند |
| P2 — Iteration 5 | روابط app/group، query projection، cache و concurrency | backup restore، dual-read parity، plan SQL و حفظ داده؛ old reader |
| P2 — Iteration 6 | پنل RTL، pagination، errors، loading و فرم امن | browser fixture، auth/CSRF/XSS و عدم تغییر API legacy |
| P1 — Iteration 7 | integration و reliability نهایی | همهٔ gateها، دادهٔ حجیم، failure/recovery و مشاهدهٔ تدریجی |

هر iteration می‌تواند چند PR کوچک داشته باشد؛ امنیت را برای پایان برنامه عقب نیندازید. بسته‌های دارای advisory با owner dependency و تست رگرسیون در Iteration 1 بررسی شوند؛ نسخه‌ها برای سبزشدن تست کور تغییر نکنند.

اطلاعات لازم برای baseline بعدی: نمونهٔ پاک‌سازی‌شدهٔ ترافیک Android و formatهای Config، فهرست endpointهای مصرف‌شده، schema/migration history واقعی با دسترسی خواندنی مجاز، سیاست retention و مالکیت DNS، معنای interval/cron و timezoneهای تاریخی. secret در chat درخواست نشود؛ زیرساخت آزمایشی از secret store جدا استفاده کند.
