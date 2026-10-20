namespace Project.Application.Jobs;

public sealed class JobSchemaNotReadyException() : Exception(
    $"ساخت خودکار جدول‌های زمان‌بند انجام نشد. وجود جدول Jobs و مجوزهای دسترسی، CREATE TABLE و ALTER روی schema را در همان دیتابیس اتصال برنامه بررسی کنید؛ یا مهاجرت {MigrationId} را دستی اعمال کنید. راهنما: docs/testing/native-jobs.md")
{
    public const string MigrationId = "20261019211502_AddNativeJobScheduling";
}
