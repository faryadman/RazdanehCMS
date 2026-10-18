# قراردادهای توسعه

- شناسهٔ انگلیسی معنادار و فارسی خوانا برای مستند و خطای کاربر؛ نام Domain اینترنتی از Domain نرم‌افزار جدا شود.
- controller کوچک، بدون SQL/DNS/زمان‌بندی و منطق انتخاب؛ use case مالک transaction و business policy باشد.
- async سرتاسری و CancellationToken در IO؛ `.Result`، GetAwaiter().GetResult و fire-and-forget روی DbContext ممنوع در کد جدید.
- scoped dependency در scope همان request/execution resolve شود؛ singleton provider ریشه برای DbContext استفاده نشود.
- queryهای مشخص با projection و AsNoTracking برای خواندن؛ patch محدود با version برای write. تغییر entity کامل بدون کنترل concurrency مجاز نیست.
- JSON profile/version مشخص، optional حفظ، اعتبارسنجی backend پیش از تمام writeها؛ parser success به معنی صحت کاربردی نیست.
- timestamp عملیاتی UTC و TimeProvider؛ formatting شمسی فقط presentation یا legacy adapter. integer enum و casing خارجی بی‌اجازه تغییر نکند.
- عملیات DNS فقط record owned با operationId؛ نتیجهٔ false/partial/unknown صریح باشد؛ retry blanket ممنوع.
- secret هیچ‌گاه در DTO پاسخ، log، test snapshot، SQL literal و docs نوشته نشود. خطا با ErrorCode و correlation؛ متن exception خام به کاربر نرود.
- DTO خارجی، ViewModel و entity مسئولیت مستقل دارند؛ interface صرفاً برای مرز زیرساخت، policy یا test seam واقعی.
- هر PR محدود، با آزمون رفتاری و rollback؛ وابستگی/lockfile فقط در PR مصوب و با verification معتبر تغییر کند.
