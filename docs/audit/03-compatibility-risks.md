# خطرهای سازگاری اندروید

هیچ نمونهٔ واقعی request/response، سورس APK، تنظیمات proxy تولید یا آمار endpointهای مصرف‌شده ارائه نشده است. ماتریس کد و شش بررسی serializer، اثبات سازگاری کامل نیستند. هر ردیف زیر شرط انتشار دارد.

| تغییر داخلی پیشنهادی | خطر بیرونی | روش حفظ قرارداد | شرط قبول و بازگشت |
|---|---|---|---|
| جداسازی DTO از entity و حذف AutoMapper از Domain | casing، null، فیلد محاسبه‌شده و عدد enum | `LegacyAndroidAdapter` با DTO و تنظیم serializer ثابت | مقایسهٔ snapshot تمام شاخه‌های پاسخ؛ بازگشت adapter قدیمی |
| JSON معتبر و نسخه‌دار | ردشدن Configهای معتبر قدیمی یا تغییر رشته | شناسایی format، نگهداری رشتهٔ اصلی و فیلدهای ناشناخته؛ ابتدا گزارش‌گیری shadow | fixture واقعی تأییدشده؛ rollback validator با عدم حذف داده |
| اصلاح انتخاب سرور، cache و log | تغییر ترتیب/تنوع/دامنه یا زمان مشاهدهٔ ویرایش | policy نسخه‌دار پشت قرارداد قبلی؛ زمان و random قابل‌کنترل در تست | trace ورودی‌های یکسان و سناریوی fallback؛ flag برگشت |
| گروه‌بندی relational | تغییر گروه‌ها یا سرور قابل‌دسترسی | dual-read و تطبیق رشتهٔ underscore با join؛ خروجی رشته‌ای قدیمی حفظ شود | diff مجموعه‌ها، شمارش و orphan صفر یا تعیین‌تکلیف؛ بازگشت خواندن قدیمی |
| UTC و decimal | زمان/عدد متفاوت برای client | تبدیل فقط داخل storage، formatter قدیمی در adapter | حدهای timezone و عدد بزرگ؛ raw قدیمی حفظ شود |
| جایگزینی Hangfire | تأخیر یا اجرای تکراری rotation | ابتدا disabled، هماهنگی shared resource و فعال‌کردن تک‌به‌تک | restart، crash و overlap؛ فقط یک موتور writer |
| DNS scoped و history | تغییر host، SNI، address و هدف CNAME | adapter Config همان path و رشتهٔ خروجی را تولید کند؛ تغییر خارجی مرحله‌ای | mock zone با رکورد نامرتبط و تست شکست میانی؛ compensation هدفمند |
| احراز هویت مسیرهای خطرناک | کلاینت قدیمی ممکن است مسیر write را صدا بزند | ابتدا شناسایی مصرف از log پاک‌سازی‌شده؛ جداسازی control plane و legacy data plane | تأیید مصرف‌کننده؛ credential جدید را نمی‌توان به APK ثابت تحمیل کرد |
| pagination و بازطراحی UI | تبدیل array به envelope یا حذف رکوردهای مورد انتظار | endpoint نسخه‌دار admin؛ مسیر قدیمی array و ترتیب را نگه دارد | snapshot و تست صفحهٔ آخر؛ بازگشت خواندن قبلی |
| خطای فارسی استاندارد | تغییر پیام/HTTP یا ProblemDetails | پروفایل خطای legacy حفظ؛ متن جدید فقط مسیر جدید/تأییدشده | malformed body، validation و not-found؛ snapshot بایت و status |

امنیت و سازگاری ممکن است تعارض واقعی داشته باشند: anonymous write خطرناک را نباید برای همیشه حفظ کرد؛ همچنین نمی‌توان بدون شواهد اعلام کرد بستن آن برای Android بی‌اثر است. تصمیم جداسازی مسیر، کنترل لبه و زمان‌بندی محدودسازی باید با مالک محصول ثبت شود؛ تا آن زمان هیچ تغییر runtime در این PR انجام نشده است.

Golden master پیشنهادی: درخواست، method، header و body پاک‌سازی‌شده، status، content-type، headerهای وابسته به client، JSON و رشتهٔ داخلی Config ذخیره شود. فیلدهای زمان/random با clock و random تزریق‌شده در زیرساخت تست کنترل شوند؛ حذف عمومی تاریخ و Config از مقایسه مجاز نیست. برای هر case علاوه بر snapshot، معنای host/address و JSON دوباره بررسی شود.
