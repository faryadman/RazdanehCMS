# نمودارهای اجرایی هدف

این نمودارها طراحی پیشنهادی‌اند. startup موجود در [ممیزی معماری](../audit/01-architecture-report.md) شرح داده شده است.

## startup و DI

```mermaid
flowchart TD
    S["خواندن تنظیمات بدون افشای secret"] --> V["اعتبارسنجی گزینه‌ها"]
    V --> D["ثبت scoped store و use case"]
    D --> A["ثبت DNS و formatter legacy"]
    A --> H["ترتیب auth و مسیرهای admin/legacy"]
    H --> W["worker disabled مگر ownership مصوب"]
    W --> R["readiness بدون side effect"]
```

## درخواست و نوشتن دیتابیس

```mermaid
sequenceDiagram
    participant U as مدیر
    participant C as MVC
    participant A as Use case
    participant V as Validator
    participant DB as SQL
    U->>C: POST و token و expectedVersion
    C->>A: فرمان و actor
    A->>V: JSON و schema و قواعد
    alt نامعتبر یا بدون مجوز
        A-->>C: خطای فارسی؛ بدون write
        C-->>U: حفظ input
    else معتبر
        A->>DB: transaction و rowversion و audit
        DB-->>A: commit یا conflict
        A-->>C: نتیجهٔ typed
        C-->>U: پیام پس از commit
    end
```

## پاسخ اندروید و خطا

```mermaid
flowchart LR
    Q["Legacy request"] --> L["پارامتر و route قدیمی"]
    L --> A["Use case"]
    A --> DTO["Legacy DTO"]
    DTO --> J["Newtonsoft و قالب ثابت"]
    J --> C["Android"]
    A --> E["Error mapping legacy"]
    E --> C
    E --> Log["log پاک‌سازی‌شده و correlation id"]
```

## اجرای job

```mermaid
stateDiagram-v2
    [*] --> Pending
    Pending --> Running: claim اتمیک و lease منبع
    Pending --> Cancelled: لغو پیش از شروع
    Running --> Completed: ثبت نتیجه
    Running --> Failed: خطای قطعی
    Running --> Cancelled: لغو امن
    Running --> Reconcile: crash یا اثر بیرونی نامعلوم
    Reconcile --> Completed: مشاهدهٔ اثر قبلی
    Reconcile --> Failed: شکست قطعی
    Failed --> Pending: retry مجاز و شناسهٔ عملیات ثابت
```

Reconcile مرحلهٔ داخلی بازیابی است و می‌تواند در UI به وضعیت خطای نیازمند بررسی نگاشت شود؛ دقیقاً شش status خواسته‌شدهٔ محصول در native-job-engine تفکیک شده‌اند. Disabled وضعیت تعریف job است. نمودار کامل تغییر domain در [مدیریت دامنه](domain-management.md) و validation در [تنظیمات سرور](server-configuration.md) آمده است.
