# مهاجرت تدریجی از Hangfire

۱. از ثبت‌ها و Jobs خروجی پاک‌سازی‌شده و backup بگیرید؛ رفتار جدول [موتور وظایف](../architecture/native-job-engine.md) را با fixture SQL و Cloudflare fake baseline کنید. کلاس خالی Job معیار رفتار نیست.

۲. در شاخهٔ مستقل engine و persistence افزایشی را بسازید، پیش‌فرض disabled. بدون side effect فقط due slot و candidate را محاسبه و با اجرای قبلی مقایسه کند؛ dry-run نباید SDK نوشتنی را resolve کند.

۳. shared lock و history را پیش از تغییر ownership آماده کنید. یک owner version برای هر job/resource داشته باشید که هر دو مسیر manual و scheduler آن را رعایت کنند. Hangfire قدیمی بدون این قرارداد در کنار writer جدید قابل‌اعتماد نیست.

۴. ابتدا job خواندنی/zone-check و سپس jobهای تغییر state با ترتیب مبتنی بر ریسک مهاجرت کنند. schedule cron فعلی از جمله `*/45` را به interval واقعی تبدیل نکنید مگر تغییر معنای زمان‌بندی مصوب شود.

۵. معادل Hangfire را disable کنید، درحال‌اجراها را drain و persisted ownership را اتمیک به engine جدید بدهید؛ سپس فعال کنید. یک ScheduledFor/OperationId نباید از هر دو موتور اجرا شود.

۶. restart، crash، overlap، cancellation، آمار و خروجی Config را بسنجید. پاک‌کردن logها سیاست کسب‌وکار مستقل می‌خواهد؛ بازتولید حذف همهٔ logها صرفاً برای برابری موتور ایمن نیست و باید به‌عنوان اصلاح رفتار تصویب شود.

۷. rollback: writer جدید متوقف و drain، آثار نامعلوم reconcile، سپس ownership به قدیم منتقل شود. history/ستون جدید حفظ شوند. ثبت مجدد قدیم بدون هماهنگی ممنوع.

۸. پس از گذشت دورهٔ مشاهده و تأیید همهٔ jobها، package، dashboard، registration و کد controller Hangfire در PR جدا حذف شود؛ داده و history حفظ شود. این ممیزی هیچ بسته‌ای را حذف نکرده است.
