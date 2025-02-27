#region license
// Copyright 2025 Utah Departement of Transportation
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
        private readonly WatchDogLogService logService;
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
            WatchDogLogService logService,
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
            this.logService = logService;
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
            //need a version of this that gets the Location version for date of the scan
            var locations = LocationRepository.GetLatestVersionOfAllLocations(emailOptions.ScanDate).ToList();
            var errors = new List<WatchDogLogEvent>();
            if (watchDogLogEventRepository.GetList().Where(e => e.Timestamp == emailOptions.ScanDate).Any())
            {
                errors = watchDogLogEventRepository.GetList().Where(e => e.Timestamp == emailOptions.ScanDate).ToList();
            }
            else
            {
                errors = await logService.GetWatchDogIssues(loggingOptions, locations, cancellationToken);
                SaveErrorLogs(errors);
            }

            var regions = regionsRepository.GetList().ToList();
            var userRegions = userRegionRepository.GetList();

            var areas = areaRepository.GetList().ToList();
            var userAreas = userAreaRepository.GetList().ToList();

            var jurisdictions = jurisdictionRepository.GetList().ToList();
            var userJurisdictions = userJurisdictionRepository.GetList();

            var users = await GetUsersWithWatchDogClaimAsync();

            var filteredErrors = ignoreEventService.GetFilteredWatchDogEventsForEmail(errors, emailOptions.ScanDate);
            var (newErrors, dailyRecurringErrors, recurringErrors) = segmentedErrorsService.GetSegmentedErrors(filteredErrors, emailOptions);


            if (!emailOptions.WeekdayOnly || emailOptions.WeekdayOnly && emailOptions.ScanDate.DayOfWeek != DayOfWeek.Saturday &&
               emailOptions.ScanDate.DayOfWeek != DayOfWeek.Sunday)
            {
                var recordsFromTheDayBefore = new List<WatchDogLogEvent>();
                if (!emailOptions.EmailAllErrors)
                {
                    //compare to error log to see if this was failing yesterday
                    if (emailOptions.WeekdayOnly && emailOptions.ScanDate.DayOfWeek == DayOfWeek.Monday)
                        recordsFromTheDayBefore =
                            watchDogLogEventRepository.GetList(w => w.Timestamp >= emailOptions.ScanDate.AddDays(-3) &&
                                w.Timestamp < emailOptions.ScanDate.AddDays(-2)).ToList();
                    else
                        recordsFromTheDayBefore =
                            watchDogLogEventRepository.GetList(w => w.Timestamp >= emailOptions.ScanDate.AddDays(-1) &&
                                w.Timestamp < emailOptions.ScanDate).ToList();
                }


                await emailService.SendAllEmails(
                    emailOptions,
                    newErrors,
                    dailyRecurringErrors,
                    recurringErrors,
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

    }



}