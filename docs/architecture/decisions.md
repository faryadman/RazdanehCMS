# دفتر تصمیم‌ها

پیشنهادهای بازطراحی عمده همچنان نیازمند بازبینی‌اند. درخواست جدید کاربر مجوز پاک‌سازی تدریجی محدود و محافظت از قرارداد API است؛ اجرای آن در ADR-011 ثبت شده است.

| شناسه | تصمیم و وضعیت | دلیل و پیامد |
|---|---|---|
| ADR-001 | قطعی از درخواست: مرحلهٔ نخست docs-only روی شاخهٔ ممیزی | بدون refactor، SQL مخرب یا DNS زنده؛ implementation بعد از approval |
| ADR-002 | قطعی از درخواست: Android بدون تغییر | DTO/serializer/route/status و semantics observable پشت adapter حفظ؛ نیاز به capture واقعی |
| ADR-003 | پیشنهاد: modular monolith و composition root روشن | کمترین تغییر deployment؛ استخراج سرویس مستقل فقط با نیاز اثبات‌شده |
| ADR-004 | پیشنهاد: حفظ Config raw و profile نسخه‌دار | جلوگیری از رد قالب معتبر قدیمی یا حذف فیلد ناشناخته؛ schema فعلاً قطعی نیست |
| ADR-005 | پیشنهاد: schema افزایشی و dual-read/write کنترل‌شده | حفظ داده و rollback؛ contraction خارج مرحلهٔ اول |
| ADR-006 | پیشنهاد: native worker + SQL durable scheduling/lease/history | PeriodicTimer به‌تنهایی persistence/coordination نیست؛ exactly-once خارجی تضمین‌پذیر نیست |
| ADR-007 | پیشنهاد: مالکیت صریح DNS و state machine تطبیق | حذف گستردهٔ zone و retry خطرناک متوقف شود؛ رکورد موجود خودبه‌خود owned نیست |
| ADR-008 | پیشنهاد: MVC/Razor RTL، endpoint مدیریتی جدید | pagination و امنیت پنل مستقل از قرارداد Android |
| ADR-009 | باز: امنیت write عمومی در برابر legacy | نیاز به شناخت مصرف واقعی؛ credential جدید به APK ثابت تحمیل نشود؛ تصمیم مشترک مالک محصول/امنیت |
| ADR-010 | باز: semantics cron، retention و timezone تاریخی | تبدیل */45 به interval و حذف log تغییر رفتارند؛ نیاز به تصمیم کسب‌وکار |

| ADR-011 | اجراشده در feature: registration واحد MVC قدیمی با snapshotهای قبل از refactor | تغییر داخلی بدون تغییر گزینه‌های serializer؛ حذف ۱۱ فایل بدون مصرف پس از جست‌وجوی ارجاع؛ ۳۲ تست قبل/بعد موفق. سایر بازطراحی‌ها از این تصمیم مجوز نمی‌گیرند. |

تغییر هر تصمیم باید دلیل، اثر بر data/Android، تست، مسیر بازگشت و وضعیت تأیید داشته باشد.
