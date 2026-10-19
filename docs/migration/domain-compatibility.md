# حفظ سازگاری دامنه با Android

Android source و capture واقعی موجود نیست. فرض مصرف SNI، Host، address و CurrentDomainValue از مسیر کد است و باید با مالک محصول و نمونهٔ پاک‌سازی‌شده تأیید شود.

| تغییر | حفظ خروجی قدیمی | آزمون و rollout |
|---|---|---|
| assignment/history افزایشی | Id سرور، رشتهٔ Config و propertyهای DTO ثابت؛ assignment جدید جزئیات داخلی است | دادهٔ قدیمی -> adapter -> snapshot برابر |
| مالکیت record و zone | نام/هدف CNAME فقط برای همان سرور تغییر کند؛ unrelated حفظ شود | fake zone مشترک؛ دو سرور و callback دستی/زمان‌بندی هم‌زمان |
| parser نام صحیح | اصلاح تجزیهٔ دامنه بدون تغییر بی‌اعلان SNI/Host قبلی | نام دو‌بخشی، چندسطحی، IDN و suffix چند‌بخشی؛ موارد مبهم quarantine |
| انتقال SDK به Infrastructure | status و error envelope legacy از mapping ثابت عبور کنند | timeout، 429، success=false و شکست پس از side effect |
| UTC و rotation deterministic | تاریخ و انتخاب سرور قابل‌مشاهده به adapter قدیمی سپرده شود | کنترل زمان و random؛ اختلاف آگاهانه فقط با تصویب رفتار |

پیش از فعال‌سازی: inventory خواندنی رکوردهای owned، backup، reconciliation SQL/Cloudflare، dry-run بدون SDK نوشتنی و diff plan. سپس برای subset محدود با پرچم فعال شود؛ تمام گروه‌های Android تا پایان validation مسیر قدیمی داشته باشند. در rollback writer جدید drain شود، assignment قبلی فقط پس از بررسی سلامت هدف و ownership بازگردد. بازگردانی کور کل zone ممنوع است.
