# راهنمای توسعه ModernCRM

این راهکار با .NET 8 و رویکرد Tactical DDD ساخته شده است. هر تغییر باید مالکیت bounded context و جهت وابستگی لایه‌ها را حفظ کند.

## ساختار معماری

- `Auth` مالک کاربر، اعتبارنامه، نقش، claim و tenant است.
- `CRM` مالک Account، Contact، Ticket و Opportunity است و فقط یک projection از کاربر را نگه می‌دارد.
- `SharedKernel` برای building blockهای عمومی و قراردادهای integration است؛ منطق اختصاصی یک context را به آن منتقل نکنید.
- در هر context، Domain مستقل‌ترین لایه است؛ Application از Domain، و Infrastructure/API از Application و Domain استفاده می‌کنند.
- ارتباط Auth و CRM فقط از طریق integration eventها انجام شود، نه reference مستقیم به مدل یا دیتابیس context دیگر.

## الگوی تغییرات

- قوانین کسب‌وکار و transitionها را داخل aggregate/value object قرار دهید.
- controllerها را نازک نگه دارید و orchestration را به handlerهای Application بسپارید.
- برای قابلیت جدید، الگوی موجود Command/Query، Handler، DTO، Repository و Controller را دنبال کنید.
- handler یا سرویس جدید را در `Program.cs` سرویس میزبان ثبت کنید.
- عملیات messaging باید idempotent باشد و سازگاری outbox/inbox، retry و saga را حفظ کند.
- DbContext فقط در Infrastructure نگهداری و ثبت شود؛ API صرفاً repository و Unit of Work را مصرف کند.
- تغییر schema باید migration و snapshot مربوط به DbContext مالک در Infrastructure را به‌روزرسانی کند.
- جداسازی tenant را حفظ کنید و داده CRM را بدون tenant معتبر query یا mutate نکنید.

## فرمان‌های اصلی

```powershell
dotnet restore ModernCRM.sln --configfile NuGet.config
dotnet build ModernCRM.sln --no-restore
dotnet run --project src/BoundedContexts/Auth/ModernCRM.Auth.Api
dotnet run --project src/BoundedContexts/CRM/ModernCRM.Crm.Api
dotnet run --project src/Presentation/ModernCRM.Web
```

پورت‌های پیش‌فرض به‌ترتیب Auth برابر `9041`، CRM برابر `9040` و Web برابر `9050` هستند. اجرای کامل به SQL Server و RabbitMQ نیاز دارد. migration خودکار فقط با `Database__ApplyMigrationsOnStartup=true` فعال می‌شود و در حالت عادی باید migration در مرحله deployment اجرا شود.

## بررسی پیش از تحویل

1. ابتدا تغییرات محدودتر را build یا تست کنید.
2. سپس کل `ModernCRM.sln` را build کنید.
3. در تغییرات API، authorization، validation و قالب خطای Problem Details را بررسی کنید.
4. در تغییرات UI، وضعیت loading، خطا و authentication را بررسی کنید.
5. اگر وابستگی زیرساختی مانع تست runtime شد، آن را صریح گزارش کنید.

## امنیت و تنظیمات

- secret، رمز عبور، connection string واقعی یا endpoint تولیدی جدید را commit نکنید.
- برای مقادیر حساس جدید از environment variable یا .NET User Secrets استفاده کنید.
- حداقل متغیرهای محیطی runtime شامل `ConnectionStrings__DefaultConnection`، `Jwt__Secret` و در محیط‌های دارای احراز هویت RabbitMQ شامل `RabbitMQ__Password` است. برای seed اولیه نیز `Seed__AdminPassword` را خارج از مخزن تنظیم کنید.
- مقادیر حساس موجود در appsettings را در خروجی، مستندات یا log تکرار نکنید و در اولین فرصت rotate و خارج از مخزن نگهداری کنید.
- تنظیمات issuer، audience و signing key مربوط به JWT باید میان Auth و CRM هماهنگ بماند.

برای راهنمای عملیاتی کامل‌تر Codex از skill محلی `$modern-crm-development` در `.codex/skills/modern-crm-development` استفاده کنید.
