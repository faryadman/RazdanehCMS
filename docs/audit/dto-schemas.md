# فهرست propertyهای DTO

استخراج با ContractResolver واقعی MVC از اسمبلی Application است؛ مقادیر تولید استفاده نشده‌اند. نوع CLR بیان شده؛ Nullable یعنی CLR می‌تواند null بپذیرد، نه تضمین تولید null. stringهای nullable-disabled هم بالقوه null هستند. enum عددی، DateTime به شکل ISO طبق Kind و UpdatedAtFormatted رشتهٔ شمسی است. Required ورودی با nullable خروجی متفاوت است. نوع‌های generic باز و compiler-generated در استخراج خودکار نیستند؛ DatatableResponse<T> دارای sEcho، recordsTotal، recordsFiltered و data از نوع object است و T در property مصرف نشده و در کنترلرهای قدیمی کاربرد دارد.

Task، entity مستقیم Test/IPConfig و anonymous resultها DTO این namespace نیستند؛ inventory و تحلیل دیتابیس برای آن‌ها مکمل‌اند.

## `Project.Application.DTOs.ApiLog.ApiLogDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `id` | `Int32` | خیر |
| `appSettingId` | `Int32` | خیر |
| `serverId` | `Int32` | خیر |
| `connectionStatus` | `Project.Domain.Enums.ConnectionStatus` | خیر |
| `updatedAt` | `DateTime` | خیر |
| `updatedAtFormatted` | `String` | بله |

## `Project.Application.DTOs.AppSetting.AppSettingDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `id` | `Int32` | خیر |
| `apiRoute` | `String` | بله |
| `appTitle` | `String` | بله |
| `interAC` | `String` | بله |
| `interAD` | `String` | بله |
| `interSP` | `String` | بله |
| `interPreOne` | `String` | بله |
| `interPreTwo` | `String` | بله |
| `splashTimeOut` | `Int32` | خیر |
| `isAdmobActive` | `Boolean` | خیر |
| `isSplashActive` | `Boolean` | خیر |
| `storeVersion` | `String` | بله |
| `isUpdateForce` | `Boolean` | خیر |
| `responseTimeout` | `Int32` | خیر |
| `retryNumber` | `Int32` | خیر |
| `shareUi` | `String` | بله |
| `checkIp` | `Boolean` | خیر |
| `url` | `String` | بله |
| `appId` | `String` | بله |
| `bannerId` | `String` | بله |
| `groupsThatAppIsJoinedIn` | `String` | بله |
| `serverApiRoute` | `String` | بله |
| `sendRandomServer` | `Boolean` | خیر |
| `isLogAllowed` | `Boolean` | خیر |
| `isAdServerAllowed` | `Boolean` | خیر |
| `timerCounter` | `Int64` | خیر |
| `isGdprAllowed` | `Boolean` | خیر |
| `isAcAllowed` | `Boolean` | خیر |
| `isSpAllowed` | `Boolean` | خیر |
| `isTimeToStopAllowed` | `Boolean` | خیر |
| `timeToStop` | `Int32` | خیر |

## `Project.Application.DTOs.AppSetting.CreateAppSettingDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `apiRoute` | `String` | بله |
| `storeVersion` | `String` | بله |
| `appTitle` | `String` | بله |

## `Project.Application.DTOs.AppSetting.EditAppSettingDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `apiRoute` | `String` | بله |
| `appTitle` | `String` | بله |
| `interAC` | `String` | بله |
| `interAD` | `String` | بله |
| `interSP` | `String` | بله |
| `interPreOne` | `String` | بله |
| `interPreTwo` | `String` | بله |
| `splashTimeOut` | `Int32` | خیر |
| `isAdmobActive` | `Boolean` | خیر |
| `isSplashActive` | `Boolean` | خیر |
| `storeVersion` | `String` | بله |
| `isUpdateForce` | `Boolean` | خیر |
| `responseTimeout` | `Int32` | خیر |
| `retryNumber` | `Int32` | خیر |
| `shareUi` | `String` | بله |
| `checkIp` | `Boolean` | خیر |
| `url` | `String` | بله |
| `appId` | `String` | بله |
| `bannerId` | `String` | بله |
| `sendRandomServer` | `Boolean` | خیر |
| `isLogAllowed` | `Boolean` | خیر |
| `isAdServerAllowed` | `Boolean` | خیر |
| `timerCounter` | `Int64` | خیر |
| `isGdprAllowed` | `Boolean` | خیر |
| `isAcAllowed` | `Boolean` | خیر |
| `isSpAllowed` | `Boolean` | خیر |
| `isTimeToStopAllowed` | `Boolean` | خیر |
| `timeToStop` | `Int32` | خیر |

## `Project.Application.DTOs.AppSetting.MinimalAppSettingDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `id` | `Int32` | خیر |
| `apiRoute` | `String` | بله |
| `storeVersion` | `String` | بله |
| `appTitle` | `String` | بله |
| `updatedAt` | `DateTime` | خیر |
| `updatedAtFormatted` | `String` | بله |

## `Project.Application.DTOs.Base.BaseDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `id` | `Int32` | خیر |
| `updatedAt` | `DateTime` | خیر |
| `updatedAtFormatted` | `String` | بله |

## `Project.Application.DTOs.Base.PaginateDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `start` | `Nullable[Int32]` | بله |
| `length` | `Nullable[Int32]` | بله |

## `Project.Application.DTOs.Base.SeoBaseDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `id` | `Int32` | خیر |
| `keywords` | `String` | بله |
| `updatedAt` | `DateTime` | خیر |
| `updatedAtFormatted` | `String` | بله |

## `Project.Application.DTOs.Base.SeoUpsertDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `keywords` | `String` | بله |
| `id` | `Nullable[Int32]` | بله |

## `Project.Application.DTOs.Base.UpsertDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `id` | `Nullable[Int32]` | بله |

## `Project.Application.DTOs.BlackList.BlackListDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `id` | `Int32` | خیر |
| `serverId` | `Int32` | خیر |
| `server` | `Server.ServerDTO` | بله |
| `ip` | `String` | بله |
| `updatedAt` | `DateTime` | خیر |
| `updatedAtFormatted` | `String` | بله |

## `Project.Application.DTOs.BlackList.CreateBlackListDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `serverId` | `Int32` | خیر |
| `ip` | `String` | بله |

## `Project.Application.DTOs.CronJobInfo.CronJobInfoDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `id` | `Int32` | خیر |
| `timer` | `Int32` | خیر |
| `executedCount` | `Int32` | خیر |
| `lastExecutionDate` | `DateTime` | خیر |
| `lastExecutionDateToString` | `String` | بله |
| `updatedAt` | `DateTime` | خیر |
| `updatedAtFormatted` | `String` | بله |

## `Project.Application.DTOs.CronJobInfo.UpdateCronJobInfoDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `timer` | `Int32` | خیر |

## `Project.Application.DTOs.Datatable.Base.DatatableInput`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `id` | `Nullable[Int32]` | بله |
| `isActive` | `Boolean` | خیر |

## `Project.Application.DTOs.Datatable.Base.FiltersFromRequestDataTable`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `length` | `Int32` | خیر |
| `start` | `Int32` | خیر |
| `sortColumn` | `String` | بله |
| `sortColumnDirection` | `String` | بله |
| `sortColumnIndex` | `String` | بله |
| `draw` | `String` | بله |
| `searchValue` | `String` | بله |

## `Project.Application.DTOs.Datatable.TicketDatatableInput`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `id` | `Nullable[Int32]` | بله |
| `isActive` | `Boolean` | خیر |

## `Project.Application.DTOs.Datatable.UserDatatableInput`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `name` | `String` | بله |
| `id` | `Nullable[Int32]` | بله |
| `isActive` | `Boolean` | خیر |

## `Project.Application.DTOs.Domain.CreateDomainDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `domainName` | `String` | بله |
| `fileName` | `String` | بله |
| `zoneId` | `String` | بله |

## `Project.Application.DTOs.Domain.DomainDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `id` | `Int32` | خیر |
| `domainName` | `String` | بله |
| `fileName` | `String` | بله |
| `zoneId` | `String` | بله |
| `isActive` | `Boolean` | خیر |
| `isDeleted` | `Boolean` | خیر |
| `updatedAt` | `DateTime` | خیر |
| `updatedAtFormatted` | `String` | بله |

## `Project.Application.DTOs.Domain.ZoneIdDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `zoneId` | `String` | بله |

## `Project.Application.DTOs.GeneralServerDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `serverDTO` | `Server.ServerDTO` | بله |
| `serverAdDTO` | `Server.ServerDTO` | بله |
| `appSettingDTO` | `AppSetting.AppSettingDTO` | بله |

## `Project.Application.DTOs.Group.CreateGroupDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `title` | `String` | بله |
| `isAd` | `Boolean` | خیر |

## `Project.Application.DTOs.Group.EditGroupDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `title` | `String` | بله |
| `itemId` | `Int32` | خیر |

## `Project.Application.DTOs.Group.GroupDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `id` | `Int32` | خیر |
| `title` | `String` | بله |
| `isAd` | `Boolean` | خیر |
| `updatedAt` | `DateTime` | خیر |
| `updatedAtFormatted` | `String` | بله |

## `Project.Application.DTOs.Group.GroupsByAppDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `title` | `String` | بله |
| `doTheyHaveRelation` | `Boolean` | خیر |
| `isAd` | `Boolean` | خیر |
| `id` | `Int32` | خیر |
| `createdBy` | `String` | بله |
| `updatedBy` | `String` | بله |
| `createdAt` | `DateTime` | خیر |
| `updatedAt` | `DateTime` | خیر |
| `isActive` | `Boolean` | خیر |

## `Project.Application.DTOs.IP.CreateIpDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `id` | `Int32` | خیر |
| `ip` | `String` | بله |
| `tcp` | `String` | بله |
| `userAgent` | `String` | بله |

## `Project.Application.DTOs.IP.IpDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `id` | `Int32` | خیر |
| `ip` | `String` | بله |
| `tcp` | `String` | بله |
| `updatedAt` | `DateTime` | خیر |
| `updatedAtFormatted` | `String` | بله |

## `Project.Application.DTOs.IpConfig.IPConfigDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `ip` | `String` | بله |
| `fileName` | `String` | بله |
| `isActive` | `Boolean` | خیر |
| `isDeleted` | `Boolean` | خیر |

## `Project.Application.DTOs.Job.CreateJobDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `email` | `String` | بله |
| `apiKey` | `String` | بله |
| `jobName` | `String` | بله |
| `jobConfig` | `String` | بله |
| `isActive` | `Nullable[Boolean]` | بله |
| `jobPeriodTime` | `Nullable[Int32]` | بله |
| `jobExpireMinuteTime` | `Nullable[Int32]` | بله |

## `Project.Application.DTOs.Job.DomainJob.CreateDomainJobDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `failConnectionCount` | `Nullable[Int32]` | بله |
| `failConnectionPercent` | `Nullable[Int32]` | بله |
| `jobPeriodTime` | `Nullable[Int32]` | بله |
| `jobExpireMinuteTime` | `Nullable[Int32]` | بله |
| `isActiveJob` | `Boolean` | خیر |
| `email` | `String` | بله |
| `apiKey` | `String` | بله |

## `Project.Application.DTOs.Job.HangfireDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `id` | `String` | بله |
| `cron` | `String` | بله |
| `lastExecTime` | `String` | بله |
| `nextExecTime` | `String` | بله |
| `createdAt` | `String` | بله |

## `Project.Application.DTOs.Job.JobDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `id` | `Int32` | خیر |
| `email` | `String` | بله |
| `apiKey` | `String` | بله |
| `jobName` | `String` | بله |
| `jobConfig` | `String` | بله |
| `isActive` | `Boolean` | خیر |
| `jobPeriodTime` | `Nullable[Int32]` | بله |
| `jobExpireMinuteTime` | `Nullable[Int32]` | بله |

## `Project.Application.DTOs.OperatorIdentification.CreateOperatorIdentificationDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `text` | `String` | بله |
| `operator` | `Project.Domain.Enums.Operator` | خیر |
| `isIsp` | `Boolean` | خیر |

## `Project.Application.DTOs.OperatorIdentification.OperatorIdentificationDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `id` | `Int32` | خیر |
| `text` | `String` | بله |
| `operator` | `Project.Domain.Enums.Operator` | خیر |
| `operatorName` | `String` | بله |
| `isIsp` | `Boolean` | خیر |
| `updatedAt` | `DateTime` | خیر |
| `updatedAtFormatted` | `String` | بله |

## `Project.Application.DTOs.PaginateDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `pageNumber` | `Int32` | خیر |
| `pageDataCount` | `Int32` | خیر |
| `dataCount` | `Int32` | خیر |

## `Project.Application.DTOs.Server.CreateServerDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `serverName` | `String` | بله |
| `location` | `String` | بله |
| `ip` | `String` | بله |
| `config` | `String` | بله |
| `configKey` | `String` | بله |
| `configValue` | `String` | بله |
| `currentDomainValue` | `String` | بله |
| `groupId` | `Int32` | خیر |
| `isAvailable` | `Boolean` | خیر |
| `isForIrancell` | `Boolean` | خیر |
| `isForHamraheAvval` | `Boolean` | خیر |
| `isAd` | `Boolean` | خیر |
| `isNewDomain` | `Boolean` | خیر |
| `domainDateTime` | `DateTime` | خیر |
| `zoneId` | `String` | بله |
| `zoneIdLastSynced` | `Nullable[DateTime]` | بله |

## `Project.Application.DTOs.Server.EditServerDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `itemId` | `Int32` | خیر |
| `serverName` | `String` | بله |
| `location` | `String` | بله |
| `ip` | `String` | بله |
| `config` | `String` | بله |
| `configKey` | `String` | بله |
| `configValue` | `String` | بله |
| `currentDomainValue` | `String` | بله |
| `isForIrancell` | `Boolean` | خیر |
| `isForHamraheAvval` | `Boolean` | خیر |
| `isNewDomain` | `Boolean` | خیر |
| `domainDateTime` | `DateTime` | خیر |
| `zoneId` | `String` | بله |
| `zoneIdLastSynced` | `Nullable[DateTime]` | بله |

## `Project.Application.DTOs.Server.ServerDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `id` | `Int32` | خیر |
| `serverName` | `String` | بله |
| `location` | `String` | بله |
| `ip` | `String` | بله |
| `config` | `String` | بله |
| `configKey` | `String` | بله |
| `configValue` | `String` | بله |
| `currentDomainValue` | `String` | بله |
| `zoneId` | `String` | بله |
| `zoneIdLastSynced` | `Nullable[DateTime]` | بله |
| `isAd` | `Boolean` | خیر |
| `isNewDomain` | `Boolean` | خیر |
| `isAvailable` | `Boolean` | خیر |
| `group` | `Group.GroupDTO` | بله |
| `groupId` | `Int32` | خیر |
| `isForIrancell` | `Boolean` | خیر |
| `isForHamraheAvval` | `Boolean` | خیر |
| `domainDateTime` | `DateTime` | خیر |
| `allLogsStatistics` | `ServerLog.ServerLogStatistics` | بله |
| `irancellLogsStatistics` | `ServerLog.ServerLogStatistics` | بله |
| `hamraheAvvalLogsStatistics` | `ServerLog.ServerLogStatistics` | بله |
| `unknownLogsStatistics` | `ServerLog.ServerLogStatistics` | بله |
| `updatedAt` | `DateTime` | خیر |
| `updatedAtFormatted` | `String` | بله |

## `Project.Application.DTOs.Server.ServerWithLogsDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `id` | `Int32` | خیر |
| `serverName` | `String` | بله |
| `location` | `String` | بله |
| `ip` | `String` | بله |
| `config` | `String` | بله |
| `configKey` | `String` | بله |
| `configValue` | `String` | بله |
| `group` | `String` | بله |
| `logs` | `List`1[ServerLog.ServerLogDTO]` | بله |
| `updatedAt` | `DateTime` | خیر |
| `updatedAtFormatted` | `String` | بله |

## `Project.Application.DTOs.ServerLog.AddServerLogDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `serverId` | `Int32` | خیر |
| `userId` | `String` | بله |
| `ip` | `String` | بله |
| `isp` | `String` | بله |
| `city` | `String` | بله |
| `org` | `String` | بله |
| `country` | `String` | بله |
| `operator` | `String` | بله |
| `apiRoute` | `String` | بله |
| `connectionStatus` | `Project.Domain.Enums.ConnectionStatus` | خیر |

## `Project.Application.DTOs.ServerLog.ServerLogDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `id` | `Int32` | خیر |
| `serverId` | `Int32` | خیر |
| `userId` | `String` | بله |
| `ip` | `String` | بله |
| `isp` | `String` | بله |
| `city` | `String` | بله |
| `org` | `String` | بله |
| `country` | `String` | بله |
| `operator` | `Project.Domain.Enums.Operator` | خیر |
| `operatorName` | `String` | بله |
| `connectionStatus` | `Project.Domain.Enums.ConnectionStatus` | خیر |
| `updatedAt` | `DateTime` | خیر |
| `updatedAtFormatted` | `String` | بله |

## `Project.Application.DTOs.ServerLog.ServerLogStatistics`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `count` | `Int32` | خیر |
| `successCount` | `Int32` | خیر |
| `failCount` | `Int32` | خیر |

## `Project.Application.DTOs.ServerLog.ServerLogStatisticsDTO`

| نام JSON | نوع CLR | امکان null |
|---|---|---|
| `allLogsStatistics` | `ServerLog.ServerLogStatistics` | بله |
| `irancellLogsStatistics` | `ServerLog.ServerLogStatistics` | بله |
| `hamraheAvvalLogsStatistics` | `ServerLog.ServerLogStatistics` | بله |
| `unknownLogsStatistics` | `ServerLog.ServerLogStatistics` | بله |
