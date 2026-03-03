#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices/ScanService.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.Atspm.Repositories;

namespace Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices
{
    public class ScanService
    {
        private readonly ILocationRepository LocationRepository;
        private readonly IWatchDogEventLogRepository watchDogLogEventRepository;
        private readonly IRegionsRepository regionsRepository;
        private readonly IJurisdictionRepository jurisdictionRepository;
        private readonly IAreaRepository areaRepository;
        private readonly IUserRegionRepository userRegionRepository;
        private readonly IUserJurisdictionRepository userJurisdictionRepository;
        private readonly IUserAreaRepository userAreaRepository;
        private readonly IWatchDogIgnoreEventService ignoreEventService;

        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly WatchDogAmLogService logAmService;
        private readonly WatchDogPmLogService logPmService;
        private readonly WatchDogRampLogService logRampService;
        private readonly IWatchdogEmailService emailService;
        private readonly SegmentedErrorsService segmentedErrorsService;
        private readonly ILogger<ScanService> logger;

        public ScanService(
            ILocationRepository LocationRepository,
            IWatchDogEventLogRepository watchDogLogEventRepository,
            IRegionsRepository regionsRepository,
            IJurisdictionRepository jurisdictionRepository,
            IAreaRepository areaRepository,
            IUserRegionRepository userRegionRepository,
            IUserJurisdictionRepository userJurisdictionRepository,
            IUserAreaRepository userAreaRepository,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            WatchDogAmLogService logAmService,
            WatchDogPmLogService logPmService,
            WatchDogRampLogService logRampService,
            IWatchdogEmailService emailService,
            ILogger<ScanService> logger,
            SegmentedErrorsService segmentedErrorsService,
            IWatchDogIgnoreEventService ignoreEventService)
        {
            this.LocationRepository = LocationRepository;
            this.watchDogLogEventRepository = watchDogLogEventRepository;
            this.regionsRepository = regionsRepository;
            this.jurisdictionRepository = jurisdictionRepository;
            this.areaRepository = areaRepository;
            this.userRegionRepository = userRegionRepository;
            this.userJurisdictionRepository = userJurisdictionRepository;
            this.userAreaRepository = userAreaRepository;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.logAmService = logAmService;
            this.logPmService = logPmService;
            this.logRampService = logRampService;
            this.emailService = emailService;
            this.logger = logger;
            this.segmentedErrorsService = segmentedErrorsService;
            this.ignoreEventService = ignoreEventService;
        }
        public async Task StartScan(
            WatchdogLoggingOptions loggingOptions,
            WatchdogEmailOptions emailOptions,
            CancellationToken cancellationToken)
        {
            DateTime scanDate = emailOptions.EmailAmErrors ? emailOptions.AmScanDate : emailOptions.EmailPmErrors
                ? emailOptions.PmScanDate : emailOptions.EmailRampErrors ? emailOptions.RampMissedDetectorHitsStartScanDate
                : throw new InvalidOperationException("No email options are enabled.");

            //need a version of this that gets the Location version for date of the scan
            var locations = LocationRepository.GetLatestVersionOfAllLocations(scanDate).ToList();

            var regions = regionsRepository.GetList().ToList();
            var userRegions = userRegionRepository.GetList();

            var areas = areaRepository.GetList().ToList();
            var userAreas = userAreaRepository.GetList().ToList();

            var jurisdictions = jurisdictionRepository.GetList().ToList(); // only look for ramp in jurisdiction if only .....
            var userJurisdictions = userJurisdictionRepository.GetList();

            var users = await GetUsersWithWatchDogClaimAsync();

            var errors = new List<WatchDogLogEvent>();
            var recordsFromTheDayBefore = new List<WatchDogLogEvent>();

            List<WatchDogLogEventWithCountAndDate> newIssues = new();
            List<WatchDogLogEventWithCountAndDate> dailyRecurringIssues = new();
            List<WatchDogLogEventWithCountAndDate> recurringIssues = new();

            var existingEvents = watchDogLogEventRepository.GetList();

            // PM
            if (emailOptions.EmailPmErrors)
            {
                var scanDateForErrors = emailOptions.PmScanDate;

                var pmErrors = existingEvents.Where(e => e.Timestamp == scanDateForErrors).ToList();

                if (!pmErrors.Any())
                {
                    var pmOptions = BreakOutPmOptions(loggingOptions);
                    pmErrors = await logPmService.GetWatchDogIssues(pmOptions, locations, cancellationToken);
                    SaveErrorLogs(pmErrors);
                }

                errors.AddRange(pmErrors);

                GetErrorsWithHistoricalRecuringData(scanDateForErrors, emailOptions, pmErrors, recordsFromTheDayBefore,
                    newIssues, dailyRecurringIssues, recurringIssues);
            }

            // AM
            if (emailOptions.EmailAmErrors)
            {
                var scanDateForErrors = emailOptions.AmScanDate;

                var amErrors = existingEvents.Where(e => e.Timestamp == scanDateForErrors).ToList();

                if (!amErrors.Any())
                {
                    var amOptions = BreakOutAmOptions(loggingOptions);
                    amErrors = await logAmService.GetWatchDogIssues(amOptions, locations, cancellationToken);
                    SaveErrorLogs(amErrors);
                }

                errors.AddRange(amErrors);

                GetErrorsWithHistoricalRecuringData(scanDateForErrors, emailOptions, amErrors, recordsFromTheDayBefore,
                    newIssues, dailyRecurringIssues, recurringIssues);
            }

            // Ramp
            if (emailOptions.EmailRampErrors)
            {
                var scanDateForErrors = emailOptions.RampMissedDetectorHitsStartScanDate;

                var rampErrors = existingEvents.Where(e => e.Timestamp == scanDateForErrors).ToList();

                if (!rampErrors.Any())
                {
                    var rampOptions = BreakOutRampOptions(loggingOptions);
                    rampErrors = await logRampService.GetWatchDogIssues(rampOptions, locations, cancellationToken);
                    SaveErrorLogs(rampErrors);
                }

                errors.AddRange(rampErrors);

                GetErrorsWithHistoricalRecuringData(scanDateForErrors, emailOptions, rampErrors, recordsFromTheDayBefore,
                    newIssues, dailyRecurringIssues, recurringIssues);
            }

            recordsFromTheDayBefore = recordsFromTheDayBefore.DistinctBy(e => e.Id).ToList();

            await emailService.SendAllEmails(
                    emailOptions,
                    newIssues,
                    dailyRecurringIssues,
                    recurringIssues,
                    locations,
                    users,
                    jurisdictions,
                    userJurisdictions.ToList(),
                    areas,
                    userAreas.ToList(),
                    regions,
                    userRegions.ToList(),
                    recordsFromTheDayBefore);
        }

        private void GetErrorsWithHistoricalRecuringData(DateTime scanDate, WatchdogEmailOptions emailOptions, List<WatchDogLogEvent> errors, List<WatchDogLogEvent> recordsFromTheDayBefore, List<WatchDogLogEventWithCountAndDate> newIssues, List<WatchDogLogEventWithCountAndDate> dailyRecurringIssues, List<WatchDogLogEventWithCountAndDate> recurringIssues)
        {
            var filteredErrors = ignoreEventService.GetFilteredWatchDogEventsForEmail(errors, scanDate);
            var (newErrorsScanDate, dailyRecurringErrorsScanDate, recurringErrorsScanDate) = segmentedErrorsService.GetSegmentedErrors(filteredErrors, emailOptions.WeekdayOnly, emailOptions.Sort, scanDate);
            newIssues.AddRange(newErrorsScanDate);
            dailyRecurringIssues.AddRange(dailyRecurringErrorsScanDate);
            recurringIssues.AddRange(recurringErrorsScanDate);

            if (!emailOptions.WeekdayOnly || emailOptions.WeekdayOnly && scanDate.DayOfWeek != DayOfWeek.Saturday &&
               scanDate.DayOfWeek != DayOfWeek.Sunday)
            {

                if (!emailOptions.EmailAllErrors)
                {
                    var recordsFromTheDayBeforeTemp = new List<WatchDogLogEvent>();
                    //compare to error log to see if this was failing yesterday
                    if (emailOptions.WeekdayOnly && scanDate.DayOfWeek == DayOfWeek.Monday)
                        recordsFromTheDayBeforeTemp =
                            watchDogLogEventRepository.GetList(w => w.Timestamp >= scanDate.AddDays(-3) &&
                                w.Timestamp < scanDate.AddDays(-2)).ToList();
                    else
                        recordsFromTheDayBeforeTemp =
                            watchDogLogEventRepository.GetList(w => w.Timestamp >= scanDate.AddDays(-1) &&
                                w.Timestamp < scanDate).ToList();

                    recordsFromTheDayBefore.AddRange(recordsFromTheDayBeforeTemp);
                }
            }
        }

        public void SaveErrorLogs(List<WatchDogLogEvent> errors)
        {
            const int maxRetryAttempts = 3;
            int retryCount = 0;
            int delay = 30000; //(in milliseconds)

            while (retryCount < maxRetryAttempts)
            {
                try
                {
                    // Attempt to save the errors
                    watchDogLogEventRepository.AddRange(errors);

                    // If successful, exit the retry loop
                    return;
                }
                catch (Exception ex)
                {
                    retryCount++;

                    // Log the exception (optional, based on your logging implementation)
                    logger.LogError($"Attempt {retryCount} failed: {ex.Message}");

                    if (retryCount == maxRetryAttempts)
                    {
                        // Throw the exception after max retries
                        throw new Exception("Failed to save error logs after multiple attempts.", ex);
                    }

                    // Exponential backoff delay before retrying
                    logger.LogInformation($"Retrying in {delay} ms...");
                    Thread.Sleep(delay);
                    delay *= 2; // Double the delay time
                }
            }
        }



        public async Task<List<ApplicationUser>> GetUsersWithWatchDogClaimAsync()
        {
            // Define the claim type you are looking for
            const string claimValue = "Watchdog:View";

            var usersWithWatchDogClaim = new List<ApplicationUser>();

            // Get all users
            var allUsers = userManager.Users.ToList();

            foreach (var user in allUsers)
            {
                var userRoles = await userManager.GetRolesAsync(user);

                foreach (var role in userRoles)
                {
                    var roleClaims = await roleManager.GetClaimsAsync(await roleManager.FindByNameAsync(role));

                    if (roleClaims.Any(claim => claim.Value == claimValue))
                    {
                        usersWithWatchDogClaim.Add(user);
                        break; // Once we find the claim in one of the user's roles, no need to check further
                    }
                }
            }

            return usersWithWatchDogClaim;
        }


        private WatchdogAmLoggingOptions BreakOutAmOptions(WatchdogLoggingOptions options)
        {
            return new WatchdogAmLoggingOptions
            {
                AmScanDate = options.AmScanDate,
                AmStartHour = options.AmStartHour,
                AmEndHour = options.AmEndHour,
                ConsecutiveCount = options.ConsecutiveCount,
                MinPhaseTerminations = options.MinPhaseTerminations,
                PercentThreshold = options.PercentThreshold,
                MaximumPedestrianEvents = options.MaximumPedestrianEvents
            };
        }

        private WatchdogPmLoggingOptions BreakOutPmOptions(WatchdogLoggingOptions options)
        {
            return new WatchdogPmLoggingOptions
            {
                PmScanDate = options.PmScanDate,
                PmPeakStartHour = options.PmPeakStartHour,
                PmPeakEndHour = options.PmPeakEndHour,
                RampMainlineStartHour = options.RampMainlineStartHour,
                RampMainlineEndHour = options.RampMainlineEndHour,
                RampStuckQueueStartHour = options.RampStuckQueueStartHour,
                RampStuckQueueEndHour = options.RampStuckQueueEndHour,
                MinimumRecords = options.MinimumRecords,
                LowHitThreshold = options.LowHitThreshold
            };
        }

        private WatchdogRampLoggingOptions BreakOutRampOptions(WatchdogLoggingOptions options)
        {
            return new WatchdogRampLoggingOptions
            {
                RampMissedDetectorHitsStartScanDate = options.RampMissedDetectorHitsStartScanDate,
                RampMissedDetectorHitsEndScanDate = options.RampMissedDetectorHitsEndScanDate,
                RampMissedDetectorHitStartHour = options.RampMissedDetectorHitStartHour,
                RampMissedDetectorHitEndHour = options.RampMissedDetectorHitEndHour,
                RampDetectorStartHour = options.RampDetectorStartHour,
                RampDetectorEndHour = options.RampDetectorEndHour,
                RampMissedEventsThreshold = options.RampMissedEventsThreshold,
                LowHitRampThreshold = options.LowHitRampThreshold
            };
        }








    }



}