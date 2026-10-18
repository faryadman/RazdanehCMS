# قراردادهای موجود API

این ماتریس از کد و metadata اسمبلی ساخته شده است؛ مصرف واقعی توسط Android اثبات نشده. هیچ درخواست تولید یا عملیات تغییردهنده اجرا نشده است. [فهرست اکشن‌ها](endpoint-inventory.md) تمام ۱۲۱ اکشن MVC میزبان اصلی، پارامترها، binding، محدودیت verb و خروجی کدی را پوشش می‌دهد؛ [فهرست schemaها](dto-schemas.md) نام JSON و نوع همهٔ DTOهای Application را دارد. این سه سند با هم ماتریس هستند.

## پروفایل مشترک میزبان اصلی

MVC از Newtonsoft با camelCase و ReferenceLoopHandling.Ignore استفاده می‌کند؛ enum converter string فعال نیست. شش بررسی محلی serializer با تنظیمات ثبت‌شدهٔ MVC این موارد را تأیید کرد: status عددی، config رشته، null باقی می‌ماند، zoneId قابل null، ConnectionStatus عددی و BadRequest->HTTP400. یک بررسی مرزی اضافی شکست UpdatedAtFormatted برای تاریخ پیش‌فرض را بازتولید کرد. این بررسی‌ها pipeline HTTP، DB و client را تست نکردند.

پروفایل `R<T>`: شیء `{"status":1,"message":"با موفقیت انجام شد","errors":[],"data":...}`، HTTP200. `Status`: Failed=0/500، Succeed=1/200، NotFound=2/404، BadRequest=3/400، Unauthorized=4/401، Forbidden=5/403. data در Response<string>(Succeed) برابر null است، نه رشتهٔ success و نه حذف property. تغییر به ProblemDetails شکستن قرارداد است.

ExceptionMiddleware خطاهای BadRequest/Validation را 400، NotFound را 404 و UnauthorizedAccess را 401 می‌دهد؛ default exception هم به 400 تبدیل می‌شود و Message/InnerException وارد errors می‌شوند. `[ApiController]` ممکن است پیش از filter به malformed body یا model validation پاسخ ValidationProblemDetails استاندارد بدهد؛ اولویت دقیق رفتار تمام خطاها در HTTP تست نشده است. نام `R` برای پاسخ موفق به معنی یکسان‌بودن تمام خطاها نیست. 404 routing و خطای خارج middleware نیز می‌تواند HTML/status page باشد.

DateTimeها JSON ISO-8601 هستند؛ UTC با Z و زمان unspecified ممکن است بدون offset بیاید. UpdatedAtFormatted رشتهٔ شمسی محاسبه‌شده است. اعداد JSON عدد هستند، enum عدد صحیح، TimerCounter از نوع Int64 و nullableهای entity لزوماً در DTO nullable نیستند. سقف امن عدد JavaScript با Int64 یکسان نیست. ترتیب propertyهای BaseDTO با JsonProperty(Order) تعیین شده؛ ترتیب بایت JSON فقط پس از capture واقعی معیار شود.

## APIهای اندروید در میزبان اصلی

همهٔ مسیرهای این جدول در کد anonymous هستند؛ cookie/JWT اجباری ندارند. status موفق 200 و خطاها طبق پروفایل بالا، مگر خلافش در ردیف آمده باشد. پارامتر path الزامی است و source آن با ActionDescriptor بررسی شد. برای هر endpoint آینده، URL، verb، ورودی و DTO legacy ثابت می‌ماند؛ تغییر امنیتی باید طبق [ریسک‌ها](03-compatibility-risks.md) تصمیم‌گیری شود.

| URL و verb | query/body | پاسخ و رفتار فعلی | ترتیب/پیش‌فرض/خطر |
|---|---|---|---|
| GET `/Api/GetGeneralApp/{apiRoute}/{isp}/{Operator}` | سه string در path؛ body ندارد | R<GeneralServerDTO> با appSettingDTO، serverDTO، serverAdDTO | دو انتخاب مستقل؛ نداشتن سرور تبلیغی می‌تواند کل پاسخ را خطا کند؛ بالا |
| GET `/Api/AppSetting/{apiRoute}` | string path | R<AppSettingDTO>؛ نبود app ممکن است data:null شود | cache مشترک؛ serverApiRoute از AppTitle ساخته می‌شود و ISP/operator ندارد؛ بالا |
| GET `/Api/GetServer/{apiRoute}/{isp}/{Operator}` | string path | R<ServerDTO>؛ عادی | active/available/non-ad، فیلتر operator، سپس انتخاب؛ بالا |
| GET `/Api/GetAdServer/{apiRoute}/{isp}/{Operator}` | string path | R<ServerDTO>؛ تبلیغی | active/available/ad؛ بالا |
| POST `/Api/SuccessServerLog` | JSON AddServerLogDTO | R<string> با data:null؛ IP ورودی با IP سرور و وضعیت با Successful عوض می‌شود | validation و duplicate semantics جدا تست شود؛ بالا |
| POST `/Api/FailedServerLog` | JSON AddServerLogDTO | R<string>؛ status Failed در log، نه status پاسخ | lookup cache از نوع DTO ولی Set مقدار ServerDTO می‌گذارد؛ dedupe قابل اتکا نیست؛ بالا |
| POST `/Api/FailedServer` | JSON AddServerLogDTO | اول write log، سپس R<GeneralServerDTO> با appSettingDTO:null و دو سرور | شکست انتخاب پس از write به معنی rollback log نیست؛ بالا |
| GET `/Api/ServerLogs?serverId=` | int query، default CLR=0 در نبود مقدار؛ nonnumeric خطای binding | R<List<ServerLogDTO>>؛ IP/UserId و اپراتور | active، Id نزولی، بدون pagination؛ بالا |
| GET `/Api/AddIp?tcp=` | int query؛ User-Agent header و RemoteIpAddress | R<string>؛ شاخهٔ Invalid TCP با شیء status=3/message عملاً بعد از bind int نامحتمل است | write با GET؛ IP موجود حذف و در همان call بازدرج نمی‌شود؛ بالا |
| GET `/Api/ListIp` | ندارد | R<List<IpDTO>> | active و Id نزولی، cache پنج دقیقه، بدون pagination؛ بالا |
| هر verb `/api/Servers/List` | groupId:int?، appId:int?، isAd:bool query | R<List<ServerDTO>> | filter پیش‌فرض سرویس ۱ -> available؛ Id صعودی؛ projection برخی فیلدها را مقداردهی نمی‌کند |
| هر verb `/api/Servers/Detail?id=` | int query | R<ServerDTO>، inactive/missing ->404 | mapping کامل‌تر از List؛ بالا |
| هر verb `/api/Servers/Create` | JSON CreateServerDTO | R<string>، ایجاد مستقیم | Required ولی بدون JSON schema؛ IsAvailable=true، IsNewDomain=false، DomainDateTime=UtcNow |
| هر verb `/api/Servers/Edit` | JSON EditServerDTO | R<string>، ویرایش مستقیم | ItemId، تاریخ ورودی با UtcNow جایگزین می‌شود؛ missing ممکن است400 exception شود |
| هر verb `/api/Servers/Delete?id=` | int query | R<string>؛ soft-delete، نبود id هم موفق | متد HTTP محدود نشده؛ خطر امنیتی بحرانی |
| هر verb `/api/BlackList/Create` | JSON CreateBlackListDTO(serverId:int, ip:string) | R<string>؛ duplicate ->400 با errors | server باید Detail شود؛ unique دیتابیس ندارد، race ممکن است |
| هر verb `/api/BlackList/GetData?serverId=` | int? query؛ نبود ->همه | R<List<BlackListDTO>> | active، Id نزولی، بدون pagination |
| هر verb `/Test/Index` و `/Test` | ندارد | JSON entityهای AppSettings بدون R | ToList، بدون active filter/order؛ propertyهای entity با DTO تفاوت دارند |
| هر verb `/LogDeleter/Index` و `/LogDeleter` | ندارد | HTTP200 و JSON boolean true پس از حذف log | constructor تعریف job را عوض می‌کند؛ destructive و anonymous |

«هر verb» یعنی metadata هیچ HttpMethodActionConstraint ندارد؛ این به معنی موفقیت هر verb/body نیست: ApiController می‌تواند نبود/نوع body را 400/415 کند و pipeline ممکن است HEAD را متفاوت پردازش کند. lowercasing تولید URL در registration فعال است؛ نمونه‌ها با casing کد آمده‌اند. mapping نهایی reverse proxy و base path استقرار در اختیار نیست.

## شکل داده‌های حساس به سازگاری

- GeneralServerDTO: `serverDTO`، `serverAdDTO`، `appSettingDTO`؛ پسوند DTO به `Dto` تغییر نکرده است.
- ServerDTO: config یک JSON string تو‌در‌توست؛ serverName، location، ip، configKey/configValue، currentDomainValue، zoneId/zoneIdLastSynced، isAd/isNewDomain/isAvailable، group/groupId، isForIrancell/isForHamraheAvval، domainDateTime و چهار شیء آمار، به‌علاوهٔ id/updatedAt/updatedAtFormatted.
- آمار: count/successCount/failCount اعداد int؛ در projection وقتی log واجد شرایط نیست، شیء می‌تواند null باشد. group در Detail بسته به navigation بارگیری‌شده می‌تواند null باشد.
- AddServerLogDTO: serverId:int، userId:string Required، ip/isp/city/org/country/operator/apiRoute:string و connectionStatus عددی؛ ConnectionStatus Successful=0 و Failed=1. Operator در log خروجی Irancell=0، HamraheAvval=1 و Unknown=2؛ نام اپراتور رشتهٔ محاسبه‌شده است.
- AppSettingDTO دارای propertyهای acronym مثل `interAC`، `interAD`، `isGdprAllowed` و timerCounter:Int64 است. فهرست کامل و nullability CLR در schema همراه است؛ stringهای reference در پروژهٔ nullable-disabled می‌توانند null باشند و Required تضمین خروجی نیست.
- IsLogAllowed، IsAdServerAllowed و BlackList در انتخاب جاری GetByApp به‌صورت gate صریح مصرف نشده‌اند؛ وجود فیلد به معنی enforce آن نیست.

ترتیب انتخاب سرور Id نزولی آغاز می‌شود اما خروجی انتخاب random/last log است، نه pagination. last log سراسری و قدیمی‌ترین فعال است؛ random server آخرین ServerId آن را کنار می‌گذارد و fallback index/Id اشتباه دارد. اصلاح آن خروجی legacy را تغییر می‌دهد. cache یک کلید مشترک را گاهی entity و گاهی DTO نگه می‌دارد، TTL یک دقیقه/یک ساعت و invalidation جزئی است؛ freshness قرارداد عملی نامطمئن دارد.

## سرویس مستقل WebApi

این API را نباید جایگزین هم‌قرارداد میزبان اصلی فرض کرد. System.Text.Json و pipeline بدون middleware خطای اختصاصی استفاده می‌شود؛ endpointها auth ندارند. جزئیات parsing/error HTTP در این جلسه اجرا نشده است.

| مسیر / method | ورودی | خروجی فعلی و status | نکته و خطر |
|---|---|---|---|
| GET `/AddIp` | tcpCount:int query؛ RemoteIpAddress و User-Agent | 200 JSON string `success`؛ شاخهٔ BadRequest string `failed` پس از bind int عملاً محدود | query DB، remove/add روی context، **بدون SaveChanges**؛ با `/Api/AddIp?tcp=` متفاوت |
| GET `/api/AppSetting/{apiRoute}` | path string | 200 JSON string برابر apiRoute | هیچ AppSetting از DB نمی‌خواند؛ نه R و نه AppSettingDTO |
| GET `/{apiRoute}` | path string | همان echo | مسیر عمومی تک‌segment؛ route precedence باید حفظ شود |
| POST `/` | apiRoute:string از query inferred، body خوانده نمی‌شود | همان echo؛ نبود پارامتر می‌تواند400 binding بدهد | تغییر به FromBody breaking است |
| PUT `/{apiRoute}` | path string | همان echo | write واقعی ندارد |
| DELETE `/{apiRoute}` | path string | همان echo | حذف واقعی ندارد |
| GET `/weatherforecast` | ندارد | 200 array پنج‌تایی date، temperatureC:int، summary:string و temperatureF:int | DateTime.Now و Random؛ نمونهٔ نمایشی، نه شاهد Android |
| GET `/swagger/v1/swagger.json` و `/swagger/*` | ندارد | سند و UI Swagger | در development دوبار ثبت و بیرون شرط هم فعال؛ قرارداد تجاری نیست |

## پوشش Areas و پنل قدیمی

تمام اکشن‌های Area Admin در فهرست همراه ثبت شده‌اند، شامل callbackهای public با Task که MVC آن‌ها را اکشن می‌بیند: CheckDomainJob، CheckSubDomainJob، CheckDeleteDnsDomainJob و CheckIpConfigJob. Area بودن به معنی Authorize نیست. فقط Groups، IPConfig، Ip، Isp، Operator و CronJobInfo policy نقش admin دارند؛ Account عمدتاً عمومی و Logout GET است. GetStatisticsServer در ServersArea رمز query جداگانه می‌سنجد (نبود متغیر500، mismatch401 با شیء Message)؛ بقیهٔ actionهای آن کنترلر از همین بررسی بهره‌مند نمی‌شوند.

پنل مستقل Web.Admin خارج solution و ناسازگار framework است. قراردادهای کدی آن در inventory جدا آمده‌اند؛ حضور source به معنی route فعال در تولید نیست. default controller Home در میزبان اصلی تعریف نشده و root ممکن است از Razor page پاسخ بگیرد؛ route فرضی Home به‌عنوان endpoint واقعی اختراع نشده است.

Golden-master تولیدی و contract testهای HTTP/DB هنوز ساخته/اجرا نشده‌اند؛ در این مرحله طبق دستور فقط docs commit می‌شود. راهبرد و برنامهٔ اجرای آن در [تست‌ها](../testing/test-strategy.md) آمده است. آیندهٔ تمام مسیرهای قدیمی در adapter باید همان قرارداد باشد؛ هر استثنای امنیتی نیازمند تصمیم و معیار انتشار جداست.
