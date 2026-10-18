# وضعیت پروژه

تاریخ: ۲۰۲۶-۱۰-۱۸. مبنای کد: `68b2acb92bc579f7b2da16ee84b33d857adb8077`. شاخهٔ موجود audit از همان HEAD master استفاده شد. وضعیت اولیهٔ checkout پاک بود.

انجام‌شده: بررسی معماری ۷ پروژه، source و registration، entity/migration، controller/DTO/serialization، jobs/DNS/JSON، query و UI؛ گزارش امنیت و طراحی هدف/rollback/نقشهٔ راه؛ استخراج ۱۲۱ اکشن MVC و schemaهای DTO. فقط AGENTS.md و docs تغییر کرده‌اند.

شواهد اجرا: build شش پروژه موفق؛ audit dependency دارای advisory؛ شش assertion serializer موفق و یک expected failure تاریخ پیش‌فرض بازتولیدشده. جزئیات و probe در docs/testing است. هیچ API تولید، SQL یا DNS تغییردهنده اجرا نشده؛ تست کامل Android، persistence و worker اجرا نشده‌اند.

خطرهای اصلی باز: کنترل دسترسی عملیات سرور/دامنه/job، حذف گستردهٔ DNS، seed با رمز ثابت، credential در source/webroot، عدم validation Config، MemoryStorage و scope نامناسب، نبود هماهنگی و history. هیچ‌یک با این PR اصلاح نشده‌اند.

گام بعد: بازبینی ممیزی و تأیید baseline؛ فراهم‌کردن نمونه‌های پاک‌سازی‌شدهٔ Android/Config و SQL ایزوله؛ سپس Iteration 1 در feature branch مستقل. پیش از refactor خطرناک، contract/behavior suite لازم است. UI و حذف Hangfire فعلاً طرح هستند.

مستندات اصلی با commit `3f462a1` روی شاخهٔ audit ثبت و push شدند. ایجاد Draft PR با `gh pr create --draft` تلاش شد اما POST به `https://api.github.com/graphql` با Forbidden رد شد؛ هیچ شماره یا لینک PR تأییدشده‌ای وجود ندارد. متن آماده در [توضیح PR](audit/pr-description.md) ذخیره است.

دامنهٔ `api.github.com` با حفظ allowance قبلی به draft تنظیمات شبکه افزوده شد؛ save ابزار موفق بود و نیاز به review/save و انتشار محیط در تنظیمات دارد. این فقط draft است و اتصال runtime را اثبات نمی‌کند. پس از اعمال تغییر، درخواست GitHub API و ایجاد Draft PR دوباره بررسی شوند. credential شخصی درخواست نشده؛ دسترسی Git با قابلیت API/PR یکی نیست. master تغییر نکرده است.

فرمان ادامه پس از رفع دسترسی، از ریشهٔ checkout:

```bash
gh pr create --repo faryadman/RazdanehCMS --draft --base master --head audit/architecture-security-android-compat-20261017 --title 'ممیزی معماری، امنیت و سازگاری اندروید؛ فقط مستندات' --body-file docs/audit/pr-description.md
```

پیش از retry پس از تغییر اتصال، PRهای موجود را بررسی کنید تا PR تکراری ساخته نشود. عدم دسترسی به API به معنی نیاز قطعی به token جدید نیست؛ از احراز هویت موجود و تنظیمات محیط استفاده شود.
