# نقشهٔ راه

مرحلهٔ فعلی: ممیزی در Draft PR #6، پاک‌سازی اولیه در Draft PR #7 و اصلاح واکشی پنل در شاخهٔ `fix/server-retrieval`. نگاشت ناقص، بارگیری گروه و رندر پنل با تست SQL واقعی اصلاح شده‌اند. بررسی Dispose روی همین شاخه نیز scope مستقل job، انتظار کامل حذف و مالکیت logger را اصلاح کرده است؛ [شواهد](testing/resource-lifetime.md). امنیت، validator JSON و بازطراحی‌های پرخطر همچنان بازند؛ این گام تکمیل کل Iteration 1 یا Iteration 5 نیست.

| اولویت / مرحله | خروجی و scope | پیش‌نیاز پذیرش / rollback |
|---|---|---|
| P0 — همین ممیزی | معماری، ماتریس API، امنیت، دیتابیس، jobs/DNS/JSON و طرح migration؛ docs-only | بررسی شواهد و محدودیت‌ها؛ بدون merge خودکار |
| P0 — Iteration 1 | کنترل دسترسی خطرناک، secret handling، جلوگیری از DNS unrelated، baseline contract | تعیین مصرف legacy، fixture واقعی، SQL/Cloudflare fake؛ flag و بازگشت محدودسازی |
| P1 — Iteration 2 | validator format-aware در backend/frontend برای تمام writeها | corpus واقعی و تست صفر write در شکست، حفظ input و snapshot Android |
| P1 — Iteration 3 | SQL job store، lease، history و native worker disabled؛ مهاجرت job به job | crash/restart/multi-instance/cancel؛ ownership یک writer و rollback |
| P1 — Iteration 4 | assignment/history و provider محدود DNS | fake zone مشترک، failure میانی، reconciling و snapshot Config؛ compensation هدفمند |
| P2 — Iteration 5 | روابط app/group، query projection، cache و concurrency | backup restore، dual-read parity، plan SQL و حفظ داده؛ old reader |
| P2 — Iteration 6 | پنل RTL، pagination، errors، loading و فرم امن | browser fixture، auth/CSRF/XSS و عدم تغییر API legacy |
| P1 — Iteration 7 | integration و reliability نهایی | همهٔ gateها، دادهٔ حجیم، failure/recovery و مشاهدهٔ تدریجی |

هر iteration می‌تواند چند PR کوچک داشته باشد؛ امنیت را برای پایان برنامه عقب نیندازید. بسته‌های دارای advisory با owner dependency و تست رگرسیون در Iteration 1 بررسی شوند؛ نسخه‌ها برای سبزشدن تست کور تغییر نکنند.

اطلاعات لازم برای baseline بعدی: نمونهٔ پاک‌سازی‌شدهٔ ترافیک Android و formatهای Config، فهرست endpointهای مصرف‌شده، schema/migration history واقعی با دسترسی خواندنی مجاز، سیاست retention و مالکیت DNS، معنای interval/cron و timezoneهای تاریخی. secret در chat درخواست نشود؛ زیرساخت آزمایشی از secret store جدا استفاده کند.
