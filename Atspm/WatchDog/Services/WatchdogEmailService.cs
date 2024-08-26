#region license
// Copyright 2024 Utah Departement of Transportation
// for WatchDog - WatchDog.Services/WatchdogEmailService.cs
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

using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Net.Mail;
using System.Text;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.WatchDog.Models;

namespace Utah.Udot.Atspm.WatchDog.Services
{
    public class WatchdogEmailService
    {
        private readonly ILogger<WatchdogEmailService> logger;
        private readonly IEmailService mailService;

        public WatchdogEmailService(
            ILogger<WatchdogEmailService> logger,
            IEmailService mailService)
        {
            this.logger = logger;
            this.mailService = mailService;
        }



        public async Task SendAllEmails(
            EmailOptions options,
            List<WatchDogLogEvent> newErrors,
            List<WatchDogLogEvent> dailyRecurringErrors,
            List<WatchDogLogEvent> recurringErrors,
            List<Location> Locations,
            List<ApplicationUser> users,
            List<Jurisdiction> jurisdictions,
            List<UserJurisdiction> userJurisdictions,
            List<Area> areas,
            List<UserArea> userAreas,
            List<Region> regions,
            List<UserRegion> userRegions,
            List<WatchDogLogEvent> logsFromPreviousDay)
        {
            await SendRegionEmails(options, newErrors, dailyRecurringErrors, recurringErrors, Locations, users, regions, userRegions, logsFromPreviousDay);
            await SendJurisdictionEmails(options, newErrors, dailyRecurringErrors, recurringErrors, Locations, users, jurisdictions, userJurisdictions, logsFromPreviousDay);
            await SendAreaEmails(options, newErrors, dailyRecurringErrors, recurringErrors, Locations, users, areas, userAreas, logsFromPreviousDay);
            var otherUsers = users.Where(u => !userRegions.Any(ur => ur.UserId == u.Id)
                                              && !userJurisdictions.Any(uj => uj.UserId == u.Id)
                                              && !userAreas.Any(ua => ua.UserId == u.Id)).ToList();
            await SendAdminEmail(options, newErrors, dailyRecurringErrors, recurringErrors, Locations, "All Locations", otherUsers, logsFromPreviousDay);

        }

        private async Task SendAdminEmail(
            EmailOptions options,
            List<WatchDogLogEvent> newErrors,
            List<WatchDogLogEvent> dailyRecurringErrors,
            List<WatchDogLogEvent> recurringErrors,
            List<Location> locations,
            string v,
            List<ApplicationUser> users,
            List<WatchDogLogEvent> logsFromPreviousDay)
        {
            var subject = $"All Locations ATSPM Alerts for {options.ScanDate.ToShortDateString()}";
            var emailBody = await CreateEmailBody(options, newErrors, dailyRecurringErrors, recurringErrors
                , locations, logsFromPreviousDay);

            //await mailService.SendEmailAsync(options.DefaultEmailAddress, users, subject, emailBody);

            await mailService.SendEmailAsync(new MailAddress(options.DefaultEmailAddress), users.GetMailingAddresses(), subject, emailBody, true);

        }

        private async Task SendJurisdictionEmails(
            EmailOptions options,
            List<WatchDogLogEvent> newErrors,
            List<WatchDogLogEvent> dailyRecurringErrors,
            List<WatchDogLogEvent> recurringErrors,
            List<Location> Locations,
            List<ApplicationUser> users,
            List<Jurisdiction> jurisdictions,
            List<UserJurisdiction> userJurisdictions,
            List<WatchDogLogEvent> logsFromPreviousDay)
        {
            foreach (var jurisdiction in jurisdictions)
            {
                var userIdsByJurisdiction = userJurisdictions.Where(uj => uj.JurisdictionId == jurisdiction.Id).ToList();
                if (userIdsByJurisdiction.IsNullOrEmpty())
                    continue;
                var usersByJurisdiction = users.Where(u => userIdsByJurisdiction.Any(uj => uj.UserId == u.Id)).ToList();
                var LocationsByJurisdiction = Locations.Where(s => s.JurisdictionId == jurisdiction.Id).ToList();
                var subject = $"{jurisdiction.Name} ATSPM Alerts for {options.ScanDate.ToShortDateString()}";
                if (!userIdsByJurisdiction.IsNullOrEmpty() && !LocationsByJurisdiction.IsNullOrEmpty())
                {
                    var emailBody = await CreateEmailBody(
                        options,
                        newErrors, dailyRecurringErrors, recurringErrors,
                        LocationsByJurisdiction,
                        logsFromPreviousDay);
                    //await mailService.SendEmailAsync(options.DefaultEmailAddress, usersByJurisdiction, subject, emailBody);

                    await mailService.SendEmailAsync(new MailAddress(options.DefaultEmailAddress), usersByJurisdiction.GetMailingAddresses(), subject, emailBody, true);
                }
            }
        }

        private async Task SendAreaEmails(
            EmailOptions options,
            List<WatchDogLogEvent> newErrors,
            List<WatchDogLogEvent> dailyRecurringErrors,
            List<WatchDogLogEvent> recurringErrors,
            List<Location> Locations,
            List<ApplicationUser> users,
            List<Area> areas,
            List<UserArea> userAreas,
            List<WatchDogLogEvent> logsFromPreviousDay)
        {
            foreach (var area in areas)
            {
                var userIdsByArea = userAreas.Where(ua => ua.AreaId == area.Id).ToList();
                if (userIdsByArea.IsNullOrEmpty())
                    continue;
                var usersByArea = users.Where(u => userIdsByArea.Any(ua => ua.UserId == u.Id)).ToList();
                var LocationsByArea = Locations.Where(s => s.Areas.Select(a => a.Id).Contains(area.Id)).ToList();
                var subject = $"{area.Name} ATSPM Alerts for {options.ScanDate.ToShortDateString()}";
                if (!userIdsByArea.IsNullOrEmpty() && !LocationsByArea.IsNullOrEmpty())
                {
                    var emailBody = await CreateEmailBody(
                        options,
                        newErrors,
                        dailyRecurringErrors,
                        recurringErrors,
                        LocationsByArea,
                        logsFromPreviousDay);
                    //await mailService.SendEmailAsync(options.DefaultEmailAddress, usersByArea, subject, emailBody);

                    await mailService.SendEmailAsync(new MailAddress(options.DefaultEmailAddress), usersByArea.GetMailingAddresses(), subject, emailBody, true);
                }
            }
        }
        private async Task SendRegionEmails(
            EmailOptions options,
            List<WatchDogLogEvent> newErrors,
            List<WatchDogLogEvent> dailyRecurringErrors,
            List<WatchDogLogEvent> recurringErrors,
            List<Location> Locations,
            List<ApplicationUser> users,
            List<Region> regions,
            List<UserRegion> userRegions,
            List<WatchDogLogEvent> logsFromPreviousDay)
        {
            foreach (var region in regions)
            {
                var userIdsByRegion = userRegions.Where(ur => ur.RegionId == region.Id).ToList();
                if (userIdsByRegion.IsNullOrEmpty())
                    continue;
                var usersByRegion = users.Where(u => userIdsByRegion.Any(ur => ur.UserId == u.Id)).ToList();
                var LocationsByRegion = Locations.Where(s => s.RegionId == region.Id).ToList();
                var subject = $"{region.Description} ATSPM Alerts for {options.ScanDate.ToShortDateString()}";
                if (!userIdsByRegion.IsNullOrEmpty() && !LocationsByRegion.IsNullOrEmpty())
                {
                    var emailBody = await CreateEmailBody(
                        options,
                        newErrors,
                        dailyRecurringErrors,
                        recurringErrors,
                        LocationsByRegion,
                        logsFromPreviousDay);
                    //await mailService.SendEmailAsync(options.DefaultEmailAddress, usersByRegion, subject, emailBody);

                    await mailService.SendEmailAsync(new MailAddress(options.DefaultEmailAddress), usersByRegion.GetMailingAddresses(), subject, emailBody, true);
                }
            }
        }

        public async Task<string> CreateEmailBody(
            EmailOptions options,
            List<WatchDogLogEvent> newErrors,
            List<WatchDogLogEvent> dailyRecurringErrors,
            List<WatchDogLogEvent> recurringErrors,
            List<Location> locations,
            List<WatchDogLogEvent> logsFromPreviousDay)
        {
            var emailBodyBuilder = new StringBuilder();

            // Process New Errors
            emailBodyBuilder.Append(ProcessErrorList("New Errors", newErrors, options, locations, logsFromPreviousDay));

            // Process Daily Recurring Errors
            emailBodyBuilder.Append(ProcessErrorList("Daily Recurring Errors", dailyRecurringErrors, options, locations, logsFromPreviousDay));

            // Process Recurring Errors
            emailBodyBuilder.Append(ProcessErrorList("Recurring Errors", recurringErrors, options, locations, logsFromPreviousDay));

            return emailBodyBuilder.ToString();
        }

        private string ProcessErrorList(
            string errorTitle,
            List<WatchDogLogEvent> errors,
            EmailOptions options,
            List<Location> locations,
            List<WatchDogLogEvent> logsFromPreviousDay)
        {
            if (errors == null || !errors.Any())
            {
                return $"<h2>{errorTitle}</h2><p>No errors found for this category.</p>";
            }

            // Categorize errors
            GetEventsByIssueType(errors, out var missingErrorsLogs, out var forceErrorsLogs, out var maxErrorsLogs,
                out var countErrorsLogs, out var stuckPedErrorsLogs, out var configurationErrorsLogs,
                out var unconfiguredDetectorErrorsLogs);

            var bodyBuilder = new StringBuilder();
            bodyBuilder.Append($"<h2>{errorTitle}</h2>");

            // Build HTML sections for each error type
            bodyBuilder.Append(BuildErrorSection("Missing Records Errors", missingErrorsLogs, locations, options, logsFromPreviousDay));
            bodyBuilder.Append(BuildErrorSection("Force Off Errors", forceErrorsLogs, locations, options, logsFromPreviousDay));
            bodyBuilder.Append(BuildErrorSection("Max Out Errors", maxErrorsLogs, locations, options, logsFromPreviousDay));
            bodyBuilder.Append(BuildErrorSection("Low Detection Count Errors", countErrorsLogs, locations, options, logsFromPreviousDay));
            bodyBuilder.Append(BuildErrorSection("High Pedestrian Activation Errors", stuckPedErrorsLogs, locations, options, logsFromPreviousDay));
            bodyBuilder.Append(BuildErrorSection("Unconfigured Approaches Errors", configurationErrorsLogs, locations, options, logsFromPreviousDay));
            bodyBuilder.Append(BuildErrorSection("Unconfigured Detectors Errors", unconfiguredDetectorErrorsLogs, locations, options, logsFromPreviousDay));

            return bodyBuilder.ToString();
        }

        private string BuildErrorSection(
            string sectionTitle,
            List<WatchDogLogEvent> errorLogs,
            List<Location> locations,
            EmailOptions options,
            List<WatchDogLogEvent> logsFromPreviousDay)
        {
            var sectionBuilder = new StringBuilder();

            if (errorLogs != null && errorLogs.Any())
            {
                sectionBuilder.Append($"<h3>{sectionTitle}</h3>");
                sectionBuilder.Append("<table class='atspm-table'>");
                sectionBuilder.Append("<thead><tr>");

                // Define table headers based on error type
                var headers = GetTableHeadersForErrorType(sectionTitle);
                foreach (var header in headers)
                {
                    sectionBuilder.Append($"<th>{header}</th>");
                }

                sectionBuilder.Append("</tr></thead><tbody>");

                // Build table rows
                sectionBuilder.Append(SortAndAddToMessage(locations, errorLogs, options, logsFromPreviousDay));

                sectionBuilder.Append("</tbody></table><br/>");
            }
            else
            {
                sectionBuilder.Append($"<h3>{sectionTitle}</h3>");
                sectionBuilder.Append("<p>No errors found for this category.</p>");
            }

            return sectionBuilder.ToString();
        }

        private static List<string> GetTableHeadersForErrorType(string sectionTitle)
        {
            switch (sectionTitle)
            {
                case "Missing Records Errors":
                    return new List<string> { "Location", "Location Description", "Issue Details" };
                case "Force Off Errors":
                case "Max Out Errors":
                case "High Pedestrian Activation Errors":
                case "Unconfigured Approaches Errors":
                    return new List<string> { "Location", "Location Description", "Phase", "Issue Details" };
                case "Low Detection Count Errors":
                case "Unconfigured Detectors Errors":
                    return new List<string> { "Location", "Location Description", "Detector Id", "Issue Details" };
                default:
                    return new List<string> { "Location", "Location Description", "Issue Details" };
            }
        }

        private static void GetEventsByIssueType(
            List<WatchDogLogEvent> eventsContainer,
            out List<WatchDogLogEvent> missingErrorsLogs,
            out List<WatchDogLogEvent> forceErrorsLogs,
            out List<WatchDogLogEvent> maxErrorsLogs,
            out List<WatchDogLogEvent> countErrorsLogs,
            out List<WatchDogLogEvent> stuckpedErrorsLogs,
            out List<WatchDogLogEvent> configurationErrorsLogs,
            out List<WatchDogLogEvent> unconfiguredDetectorErrorsLogs
            )
        {
            missingErrorsLogs = eventsContainer.Where(e => e.IssueType == Data.Enums.WatchDogIssueTypes.RecordCount).ToList();
            forceErrorsLogs = eventsContainer.Where(e => e.IssueType == Data.Enums.WatchDogIssueTypes.ForceOffThreshold).ToList();
            maxErrorsLogs = eventsContainer.Where(e => e.IssueType == Data.Enums.WatchDogIssueTypes.MaxOutThreshold).ToList();
            countErrorsLogs = eventsContainer.Where(e => e.IssueType == Data.Enums.WatchDogIssueTypes.LowDetectorHits).ToList();
            stuckpedErrorsLogs = eventsContainer.Where(e => e.IssueType == Data.Enums.WatchDogIssueTypes.StuckPed).ToList();
            configurationErrorsLogs = eventsContainer.Where(e => e.IssueType == Data.Enums.WatchDogIssueTypes.UnconfiguredApproach).ToList();
            unconfiguredDetectorErrorsLogs = eventsContainer.Where(e => e.IssueType == Data.Enums.WatchDogIssueTypes.UnconfiguredDetector).ToList();
        }


        private string SortAndAddToMessage(
            List<Location> Locations,
            List<WatchDogLogEvent> issues,
            EmailOptions options,
            List<WatchDogLogEvent> logsFromPreviousDay)
        {
            if (Locations is null || issues is null || options is null || logsFromPreviousDay is null)
            {
                logger.LogError("Null parameter passed to SortAndAddToMessage");
                return string.Empty;
            }
            var sortedLocations = Locations.OrderBy(x => x.LocationIdentifier).ToList();
            var errorMessage = "";
            foreach (var Location in sortedLocations)
            {
                foreach (var error in issues.Where(e => e.LocationId == Location.Id))
                {
                    if (options.EmailAllErrors || !logsFromPreviousDay.Contains(error))
                    {
                        //   Add to email if it was not failing yesterday
                        errorMessage += $"<tr><td>{error.LocationIdentifier}</td><td>{Location.PrimaryName} & {Location.SecondaryName}</td>";

                        if (error.Phase > 0)
                        {
                            errorMessage += $"<td>{error.Phase}</td>";
                        }
                        if (error.ComponentType == Data.Enums.WatchDogComponentTypes.Detector)
                        {
                            errorMessage += $"<td>{error.ComponentId}</td>";
                        }

                        errorMessage += $"<td>{error.Details}</td></tr>";
                    }
                }
            }

            return errorMessage;
        }


    }
}