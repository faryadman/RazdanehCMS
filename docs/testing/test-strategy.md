# راهبرد آزمون و شواهد اجرای ممیزی

## اجراشده در ۲۰۲۶-۱۰-۱۸

| بررسی | نتیجه و دامنه |
|---|---|
| `dotnet build RazdanehCMS.sln --no-restore -m:2` با SDK10.0.402 | موفق؛ ۶ پروژه، ۸ warning و ۰ error در build incremental؛ Web.Admin خارج scope build |
| `dotnet list RazdanehCMS.sln package --vulnerable --include-transitive --format json` | اجرا و advisory دو نسخهٔ Pkcs ثبت شد؛ پاک‌بودن dependencyها ادعا نمی‌شود |
| ساخت MVC ActionDescriptor بدون اجرای Program | ۱۲۱ اکشن میزبان اصلی استخراج شد؛ هیچ سرویس DB/DNS resolve و هیچ host شروع نشد |
| serializer با AddControllers/AddNewtonsoftJson و تنظیم فعلی | ۶ assertion موفق: numeric status، config string، null باقی‌مانده، zoneId null، numeric enum، status400 |
| serializer روی ServerDTO با UpdatedAt پیش‌فرض | JsonSerializationException ناشی از PersianCalendar بازتولید شد؛ case مرزی مشاهده‌شده، نه خرابی همهٔ responseهای واقعی |
| Git read و بررسی baseline | remote branch موجود و هم‌commit master در آغاز؛ diff محصول باید صفر بماند |

probe در پوشهٔ خارجی `/workspace/.setup/audit-probe` اجرا شده است؛ کد محصول و پروژهٔ تست به مخزن افزوده نشد. کد دقیق و روش اجرای دوباره در [سند probe](serialization-probe.md) ثبت شده است. خروجی واقعی serializer با دادهٔ ساختگی در همان پوشه نگهداری شد و contract تولید نیست.

## اجرا‌نشده و محدودیت

Golden-master HTTP/Android، contract با fixture تولید پاک‌سازی‌شده، SQL persistence/restore/migration، auth/authorization HTTP، frontend browser، DNS provider، job concurrency/restart/cancellation و engine جدید تست نشده‌اند. هیچ تست suite در solution نبود؛ build و serializer جای آن را نمی‌گیرند. smokeهای راه‌اندازی محیط در روز قبل به اجرای امروز نسبت داده نشده‌اند. در این ممیزی host برنامه به علت seed و background side effect آغاز نشده است.

## تست‌های لازم پیش از refactor

| سطح | موارد الزامی | زیرساخت و شرط شکست |
|---|---|---|
| contract legacy | همهٔ مسیرهای جدول API، casing/null، enum، status/content-type، خطای binding، ترتیب و Config string | WebApplicationFactory با entrypoint ایمن و سرویس fake؛ fixture واقعی تصدیق‌شده؛ diff غیرمجاز شکست |
| JSON | موارد سند json-validation-cases، تمام writeها، عدم overwrite قبلی | validator unit و use-case integration؛ شمارش write و version |
| domain و DNS | assignment، rotation، رکورد نامرتبط، zone mismatch، Success=false، partial/timeout و retry | درگاه fake با state و failure injection؛ هر unrelated mutation شکست |
| jobs | schedule/interval/cron/DST، دو instance، manual overlap، expired lease، crash، shutdown و cancellation | SQL Server یک‌بارمصرف واقعی؛ fake clock و provider؛ duplicate effect شکست |
| persistence | FK، unique، soft/hard delete، rowversion، query projection، paging و serializer | SQL Server نه EF InMemory برای semantics relational؛ roundtrip و null/date |
| امنیت | anonymous/role/policy، CSRF، cookie/lockout/rate، XSS و upload | host کنترل‌شده و browser؛ هیچ تماس به host تولید |
| مهاجرت | restore، checkpoint/backfill، quarantine، dual-read/write، rollback و آثار پس از cutover | دادهٔ مصنوعی/پاک‌سازی‌شده، شمارش/hash و audit مستقل |

Golden master را از اجرای کور برنامه روی DB تولید نسازید. ابتدا startup seed و registration job در test composition جایگزین شوند؛ این خود کد تست در feature branch مصوب است. handlerهای خالص باید time/random و provider را تزریق بگیرند. fixture واقعی بدون key/userId/IP واقعی در repo؛ نمونهٔ مصنوعی label داشته باشد.

هر اجرا: فرمان، commit، target، زمان، count passed/failed/skipped و exit code ثبت شود. expected failure جدا از pass باشد. صفر test یا حذف assertion پذیرش نیست؛ output فایل به run جاری تعلق داشته باشد. تست‌های DNS زنده و سرور تولید بدون تأیید صریح ممنوع‌اند.
