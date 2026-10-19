# تست یکپارچهٔ واکشی سرور

این suite با SQL Server واقعی در container یک‌بارمصرف، DbContext، repository و سرویس واقعی اجرا می‌شود. یک تست هم درخواست HTTP پنل را تا SQL عبور می‌دهد. هیچ تنظیم اتصال برنامه یا سرویس تولید استفاده نمی‌شود؛ اجرای آن به Docker Linux و امکان دانلود تصویر SQL Server و Ryuk نیاز دارد.

```bash
dotnet test tests/Project.ServerRetrieval.Tests/Project.ServerRetrieval.Tests.csproj
```

تصویر SQL با digest ثابت انتخاب شده، رمز با RandomNumberGenerator فقط در حافظه تولید می‌شود و container پس از suite حذف می‌شود. appsettings و Program محصول اجرا نمی‌شوند؛ migration روی تولید یا دسترسی Cloudflare وجود ندارد. نبود Docker باید باعث شکست روشن تست شود و با skip پنهان جایگزین نشود.

۱۴ تست، فیلتر/ترتیب، نگاشت کامل، گروه، null/empty، آمار، query count، نبود tracking و مسیر HTTP را بررسی می‌کنند. تست‌های سریع قرارداد قدیمی در `Project.LegacyApi.Tests` مستقل‌اند و برای اجرای تنها آن‌ها Docker لازم نیست. جزئیات بازتولید قبل/بعد در `docs/testing/server-retrieval.md` است.
