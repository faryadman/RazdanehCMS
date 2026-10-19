namespace Project.Application.Jobs;

public sealed class JobSchemaNotReadyException() : Exception(
    $"جدول‌های زمان‌بند آماده نیستند یا دسترسی به آن‌ها وجود ندارد. مهاجرت {MigrationId} را روی همان دیتابیس اتصال برنامه اعمال و دسترسی جدول‌ها را بررسی کنید. راهنما: docs/testing/native-jobs.md")
{
    public const string MigrationId = "20261019211502_AddNativeJobScheduling";
}
