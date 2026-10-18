# برنامهٔ مهاجرت بدون حذف داده

هیچ مرحلهٔ SQL این سند اجرا نشده است. migration روی تولید، restore یا DNS نیازمند کار اجرایی جدا و تأیید baseline است.

۱. از schema، داده، identity، mapping DNS، version اپ و کلیدهای حفاظت داده backup رمزگذاری‌شده با مالک، checksum و مدت نگهداری تهیه شود. restore واقعی در SQL ایزوله و زمان بازگردانی اندازه‌گیری شود. وجود فایل backup به‌تنهایی آزمون restore نیست.

۲. schema واقعی و __EFMigrationsHistory با snapshot و migrationها تطبیق داده شود. duplicate، orphan، null، JSON نامعتبر، timezone نامعلوم و ناسازگاری ZoneId فقط با query خواندنی شمارش شوند. دادهٔ مشکوک در staging با شناسهٔ اصلی، مقدار raw، hash و دلیل نگهداری شود؛ حذف یا تصحیح حدسی ممنوع.

۳. ستون/جدول/index افزایشی با نام و FK صریح ساخته شود. ستون قدیمی باقی بماند. unique فقط بعد از تعیین تکلیف تعارض و index با زمان/فضای کافی ساخته شود. طول رشته کاهش نیابد مگر صحت تمام داده اثبات شود.

۴. backfill دسته‌ای با checkpoint، transaction کوتاه و idempotency اجرا شود. group token غیرعددی، domain مبهم، زمان local بدون منطقه و JSON ناشناخته وارد quarantine شوند. raw موجود دست‌نخورده بماند. برای پول، precision و جمع کل قبل/بعد کنترل شود.

۵. dual-write با transaction داخل SQL و outbox برای اثر بیرونی؛ نسخهٔ قدیمی تا زمان پشتیبانی dual-write فعال نشود. shadow-read خروجی جدید را با projection legacy مقایسه کند. شناسه، تعداد، hash fieldهای اصلی، null، روابط و خروجی API برای ورودی معین برابر باشد. اختلاف باید صفر یا تک‌تک مصوب باشد.

۶. مسیر خواندن به‌تدریج با feature flag عوض شود؛ Android همچنان همان DTO، status، casing، Config string و semantics group/domain را ببیند. یک writer authoritative باشد؛ موتور قدیمی و جدید DNS هم‌زمان فعال نشوند.

۷. rollback برنامه با flag و بازگشت خواندن قدیمی انجام شود؛ expansion schema حذف نشود. دادهٔ نوشته‌شده بعد از cutover با log عملیات reconciliation شود. restore کور SQL ممکن است writes جدید را از بین ببرد و DNS را برنمی‌گرداند. compensation DNS فقط برای record owned همان operation و پس از بررسی وضعیت فعلی انجام شود.

۸. contraction و حذف فایل/ستون/جدول خارج scope این طرح اولیه است؛ فقط پس از دورهٔ مشاهده، تأیید مصرف‌کنندگان و backup قابل‌بازیابی در PR جدا بررسی شود.

دروازهٔ پذیرش: restore قابل‌اجرا، شمارش/hash برابر، نبود orphan جدید، fixture legacy سبز، قطع ارتباط وسط batch، اجرای دوباره، race و rollback روی دادهٔ پس از cutover آزمایش شوند. هیچ تضمین zero data loss بدون اجرای این بررسی‌ها اعلام نشود.
