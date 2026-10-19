# موارد پذیرش JSON

این جدول طراحی تست آینده است؛ هیچ‌یک به‌عنوان تست validator اجراشده گزارش نمی‌شود. validator جدید هنوز وجود ندارد. همهٔ نمونه‌ها باید ساختگی و بدون token/دامنه/IP تولید باشند.

| مورد | انتظار |
|---|---|
| object معتبر برای format TLS+WS با فیلدهای ناشناخته | پذیرش، حفظ optional و متن معنایی |
| قالب legacy معتبر بدون TLS یا با outbound متفاوت | پذیرش فقط در profile مربوط؛ TLS سراسری اجباری نشود |
| `{`، نقل‌قول ناقص، comma نابجا | رد با path/line/column؛ صفر write |
| null، string یا array در root برای profile object | رد root؛ config قبلی سالم |
| outbounds خالی، settings غایب، vnext نوع اشتباه | رد دقیق مسیر فقط در profile نیازمند آن |
| port صفر، منفی، ۶۵۵۳۶، اعشاری یا string | طبق profile رد؛ coercion پنهان ممنوع |
| IPv4/IPv6/hostname/IDN معتبر و hostname ناسالم | parser مناسب همان فیلد، بدون تماس شبکه |
| فیلد ناشناخته و property casing متفاوت | سیاست version-aware و fixture legacy؛ حذف خاموش ممنوع |
| duplicate key، comment، trailing comma | مقایسهٔ Newtonsoft فعلی و System.Text.Json و تصمیم صریح |
| حجم/depth بیش از سقف | رد محدود و پاسخ فارسی؛ مصرف منابع محدود |
| placeholder غایب، تکراری، داخل URL یا متن نامربوط | تبدیل فقط طبق profile و بررسی خروجی کامل |
| ConfigKey یا ConfigValue null/خالی | خطای کنترل‌شده؛ عدم replace ناخواستهٔ همهٔ @ها |
| write دستی، API، import، duplicate، domain و job | validator واحد پیش از save هر مسیر |
| درخواست ویرایش هم‌زمان | conflict بدون overwrite config سالم |
| UI validation شکست می‌خورد | input باقی بماند، هیچ پیام موفق یا close modal |
| خواندن رکورد قدیمی خراب | نمایش حالت نامعتبر/قرنطینه، باقی جدول render شود |

هر تست integration باید قبل/بعد rowversion، مقدار Config و تعداد writeهای fake repository/SQL را بسنجد؛ فقط assertion روی پیام اعتبارسنجی کافی نیست. schema از نمونه‌های واقعی پاک‌سازی‌شده استخراج و fixtureها با مالک format تأیید شوند.
