#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices/WatchdogEmailService.cs
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
using System.Net.Mail;
using System.Text;
using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices
{
    public class WatchdogEmailService : IWatchdogEmailService
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
            WatchdogEmailOptions options,
            List<WatchDogLogEventWithCountAndDate> newErrors,
            List<WatchDogLogEventWithCountAndDate> dailyRecurringErrors,
            List<WatchDogLogEventWithCountAndDate> recurringErrors,
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
            WatchdogEmailOptions options,
            List<WatchDogLogEventWithCountAndDate> newErrors,
            List<WatchDogLogEventWithCountAndDate> dailyRecurringErrors,
            List<WatchDogLogEventWithCountAndDate> recurringErrors,
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
            WatchdogEmailOptions options,
            List<WatchDogLogEventWithCountAndDate> newErrors,
            List<WatchDogLogEventWithCountAndDate> dailyRecurringErrors,
            List<WatchDogLogEventWithCountAndDate> recurringErrors,
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
            WatchdogEmailOptions options,
            List<WatchDogLogEventWithCountAndDate> newErrors,
            List<WatchDogLogEventWithCountAndDate> dailyRecurringErrors,
            List<WatchDogLogEventWithCountAndDate> recurringErrors,
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

                    await mailService.SendEmailAsync(new MailAddress(options.DefaultEmailAddress), usersByArea.GetMailingAddresses(), subject, emailBody, true);
                }
            }
        }
        private async Task SendRegionEmails(
            WatchdogEmailOptions options,
            List<WatchDogLogEventWithCountAndDate> newErrors,
            List<WatchDogLogEventWithCountAndDate> dailyRecurringErrors,
            List<WatchDogLogEventWithCountAndDate> recurringErrors,
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

                    await mailService.SendEmailAsync(new MailAddress(options.DefaultEmailAddress), usersByRegion.GetMailingAddresses(), subject, emailBody, true);
                }
            }
        }

        public async Task<string> CreateEmailBody(
            WatchdogEmailOptions options,
            List<WatchDogLogEventWithCountAndDate> newErrors,
            List<WatchDogLogEventWithCountAndDate> dailyRecurringErrors,
            List<WatchDogLogEventWithCountAndDate> recurringErrors,
            List<Location> locations,
            List<WatchDogLogEvent> logsFromPreviousDay)
        {
            var emailBodyBuilder = new StringBuilder();

            // Process New Errors
            emailBodyBuilder.Append(ProcessErrorList("New Errors", $"Errors that occurred on {options.ScanDate.ToString("M/d/yyyy")} that have not occured within the last 12 months.", newErrors, options, locations, logsFromPreviousDay, false, false));

            // Process Daily Recurring Errors
            emailBodyBuilder.Append(ProcessErrorList("Daily Recurring Errors", $"Errors that occurred on {options.ScanDate.ToString("M/d/yyyy")} that also occurred the day prior to the proccessing date.", dailyRecurringErrors, options, locations, logsFromPreviousDay, true, true));

            // Process Recurring Errors
            emailBodyBuilder.Append(ProcessErrorList("Recurring Errors", $"Errors that occurred on {options.ScanDate.ToString("M/d/yyyy")} that did not occur on the day prior to processing but have occured at least one time in the last 12 months.", recurringErrors, options, locations, logsFromPreviousDay, true, false));

            return emailBodyBuilder.ToString();
        }

        public string ProcessErrorList(
            string errorTitle,
            string errorSubHeader,
            List<WatchDogLogEventWithCountAndDate> errors,
            WatchdogEmailOptions options,
            List<Location> locations,
            List<WatchDogLogEvent> logsFromPreviousDay,
            bool includeErrorCounts,
            bool includeConsecutive)
        {
            if (errors == null || !errors.Any())
            {
                return $"<h2>{errorTitle}</h2><h4>{errorSubHeader}</h4><p>No errors found for this category.</p>";
            }

            // Categorize errors
            GetEventsByIssueType(errors, out var missingErrorsLogs, out var forceErrorsLogs, out var maxErrorsLogs,
                out var countErrorsLogs, out var stuckPedErrorsLogs, out var configurationErrorsLogs,
                out var unconfiguredDetectorErrorsLogs);

            var bodyBuilder = new StringBuilder();

            // Define the CSS style
            var body = "<style>\r\n" +
                       ".shaded-header {\r\n  " +
                       "    background-color: #09549c; \r\n  " +
                       "    color: #ffff; /* Text color */\r\n  " +
                       "    padding: 10px; /* Padding around the text */\r\n  " +
                       "    border-radius: 5px; /* Rounded corners */" +
                       "    font: 24px bold Roboto, sans-serif}\r\n" +
                       "  .atspm-table {\r\n" +
                       "    border: solid 2px #09549c;\r\n" +
                       "    border-collapse: collapse;\r\n" +
                       "    border-spacing: 0;\r\n" +
                       "    font: normal 14px Roboto, sans-serif;\r\n" +
                       "  }\r\n\r\n" +
                       ".atspm-table tbody td {\r\n   " +
                       "     border: solid 1px #cbe4fc;\r\n    " +
                       "    padding: 10px;\r\n  }\r\n\r\n  " +
                       "h3 {\r\n     " +
                       "    font: bold 16px Roboto, sans-serif;\r\n     " +
                       "    color: #09549c;\r\n  }" +
                       "</style>";

            bodyBuilder.Append(body);
            bodyBuilder.Append($"<h2 class='shaded-header'>{errorTitle} ({errorSubHeader})</h2>");
            var locationDictionary = locations.ToDictionary(l => l.Id, l => l);
            // Build HTML sections for each error type
            bodyBuilder.Append(BuildErrorSection("Missing Records Errors", $"The following Locations had too few records in the database on {options.ScanDate.Date.ToShortDateString()}", missingErrorsLogs, locationDictionary, options, logsFromPreviousDay, includeErrorCounts, includeConsecutive));
            bodyBuilder.Append(BuildErrorSection("Force Off Errors", $"The following Locations had too many force off occurrences between {options.ScanDayStartHour}:00 and {options.ScanDayEndHour}:00", forceErrorsLogs, locationDictionary, options, logsFromPreviousDay, includeErrorCounts, includeConsecutive));
            bodyBuilder.Append(BuildErrorSection("Max Out Errors", $"The following Locations had too many max out occurrences between {options.ScanDayStartHour}:00 and {options.ScanDayEndHour}:00", maxErrorsLogs, locationDictionary, options, logsFromPreviousDay, includeErrorCounts, includeConsecutive));
            bodyBuilder.Append(BuildErrorSection("Low Detection Count Errors", $"The following Locations had unusually low advanced detection counts on {options.ScanDate.Date.ToShortDateString()}", countErrorsLogs, locationDictionary, options, logsFromPreviousDay, includeErrorCounts, includeConsecutive));
            bodyBuilder.Append(BuildErrorSection("High Pedestrian Activation Errors", $"The following Locations have high pedestrian activation occurrences between {options.ScanDayStartHour}:00 and {options.ScanDayEndHour}:00", stuckPedErrorsLogs, locationDictionary, options, logsFromPreviousDay, includeErrorCounts, includeConsecutive));
            bodyBuilder.Append(BuildErrorSection("Unconfigured Approaches Errors", "", configurationErrorsLogs, locationDictionary, options, logsFromPreviousDay, includeErrorCounts, includeConsecutive));
            bodyBuilder.Append(BuildErrorSection("Unconfigured Detectors Errors", "", unconfiguredDetectorErrorsLogs, locationDictionary, options, logsFromPreviousDay, includeErrorCounts, includeConsecutive));

            return bodyBuilder.ToString();
        }

        public string BuildErrorSection(
            string sectionTitle,
            string sectionTimeDescription,
            List<WatchDogLogEventWithCountAndDate> errorLogs,
            Dictionary<int, Location> locations,
            WatchdogEmailOptions options,
            List<WatchDogLogEvent> logsFromPreviousDay,
            bool includeErrorCounts,
            bool includeConsecutive)
        {
            var sectionBuilder = new StringBuilder();

            if (errorLogs != null && errorLogs.Any())
            {
                sectionBuilder.Append($"<h3>{sectionTitle}</h3>");
                sectionBuilder.Append("<table class='atspm-table'>");
                sectionBuilder.Append("<thead><tr>");

                if (sectionTimeDescription.Length > 0)
                {
                    sectionBuilder.Append($"<p>{sectionTimeDescription}</p>");
                }

                // Define table headers based on error type
                var headers = GetTableHeadersForErrorType(sectionTitle, includeErrorCounts, includeConsecutive);
                foreach (var header in headers)
                {
                    sectionBuilder.Append($"<th>{header}</th>");
                }

                sectionBuilder.Append("</tr></thead><tbody>");

                // Build table rows
                sectionBuilder.Append(GetMessage(locations, errorLogs, options, logsFromPreviousDay, includeErrorCounts, includeConsecutive));

                sectionBuilder.Append("</tbody></table><br/>");
            }
            else
            {
                sectionBuilder.Append($"<h3>{sectionTitle}</h3>");
                sectionBuilder.Append("<p>No errors found for this category.</p>");
            }

            return sectionBuilder.ToString();
        }

        public static List<string> GetTableHeadersForErrorType(string sectionTitle, bool includeErrorCounts, bool includeConsecutive)
        {
            // Base headers
            var headers = sectionTitle switch
            {
                "Missing Records Errors" => new List<string> { "Location", "Location Description", "Issue Details", "Date of First Occurrence" },
                "Force Off Errors" or "Max Out Errors" or "High Pedestrian Activation Errors" or "Unconfigured Approaches Errors" =>
                    new List<string> { "Location", "Location Description", "Phase", "Issue Details", "Date of First Occurrence" },
                "Low Detection Count Errors" or "Unconfigured Detectors Errors" =>
                    new List<string> { "Location", "Location Description", "Detector Id", "Issue Details", "Date of First Occurrence" },
                _ => new List<string> { "Location", "Location Description", "Issue Details", "Date of First Occurrence" }
            };

            // Add Error Counts if the flag is true
            if (includeErrorCounts)
            {
                headers.Insert(headers.Count - 1, "Error Count");
            }
            if (includeConsecutive)
            {
                headers.Insert(headers.Count - 1, "Consecutive Occurrence Count");
            }

            return headers;
        }


        public static void GetEventsByIssueType(
            List<WatchDogLogEventWithCountAndDate> eventsContainer,
            out List<WatchDogLogEventWithCountAndDate> missingErrorsLogs,
            out List<WatchDogLogEventWithCountAndDate> forceErrorsLogs,
            out List<WatchDogLogEventWithCountAndDate> maxErrorsLogs,
            out List<WatchDogLogEventWithCountAndDate> countErrorsLogs,
            out List<WatchDogLogEventWithCountAndDate> stuckpedErrorsLogs,
            out List<WatchDogLogEventWithCountAndDate> configurationErrorsLogs,
            out List<WatchDogLogEventWithCountAndDate> unconfiguredDetectorErrorsLogs
        )
        {
            // Handle null container
            eventsContainer ??= new List<WatchDogLogEventWithCountAndDate>();

            // Categorize events by IssueType
            missingErrorsLogs = eventsContainer
                .Where(e => e?.IssueType == WatchDogIssueTypes.RecordCount)
                .ToList();

            forceErrorsLogs = eventsContainer
                .Where(e => e?.IssueType == WatchDogIssueTypes.ForceOffThreshold)
                .ToList();

            maxErrorsLogs = eventsContainer
                .Where(e => e?.IssueType == WatchDogIssueTypes.MaxOutThreshold)
                .ToList();

            countErrorsLogs = eventsContainer
                .Where(e => e?.IssueType == WatchDogIssueTypes.LowDetectorHits)
                .ToList();

            stuckpedErrorsLogs = eventsContainer
                .Where(e => e?.IssueType == WatchDogIssueTypes.StuckPed)
                .ToList();

            configurationErrorsLogs = eventsContainer
                .Where(e => e?.IssueType == WatchDogIssueTypes.UnconfiguredApproach)
                .ToList();

            unconfiguredDetectorErrorsLogs = eventsContainer
                .Where(e => e?.IssueType == WatchDogIssueTypes.UnconfiguredDetector)
                .ToList();
        }



        public string GetMessage(
            Dictionary<int, Location> locationDictionary,
            List<WatchDogLogEventWithCountAndDate> issues,
            WatchdogEmailOptions options,
            List<WatchDogLogEvent> logsFromPreviousDay,
            bool includeErrorCounts,
            bool includeConsecutive)
        {
            if (locationDictionary is null || issues is null || options is null || logsFromPreviousDay is null)
            {
                logger.LogError("Null parameter passed to GetMessage");
                return string.Empty;
            }
            var errorMessage = "";
            foreach (var error in issues)
            {
                if (options.EmailAllErrors || !logsFromPreviousDay.Contains(error))
                {
                    if (locationDictionary.ContainsKey(error.LocationId))
                    {
                        //   Add to email if it was not failing yesterday
                        errorMessage += $"<tr><td>{error.LocationIdentifier}</td><td>{locationDictionary[error.LocationId].PrimaryName} & {locationDictionary[error.LocationId].SecondaryName}</td>";

                        if (error.Phase > 0)
                        {
                            errorMessage += $"<td>{error.Phase}</td>";
                        }
                        if (error.ComponentType == WatchDogComponentTypes.Detector)
                        {
                            errorMessage += $"<td>{error.ComponentId}</td>";
                        }

                        errorMessage += $"<td>{error.Details}</td>";
                        if (includeErrorCounts)
                        {
                            errorMessage += $"<td>{error.EventCount}</td>";
                        }
                        if (includeConsecutive)
                        {
                            errorMessage += $"<td>{error.ConsecutiveOccurenceCount}</td>";
                        }
                        errorMessage += $"<td>{error.DateOfFirstInstance}</td></tr>";
                    }
                }
            }

            return errorMessage;
        }


    }
}