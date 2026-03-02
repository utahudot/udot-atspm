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
            if (options.EmailPmErrors || options.EmailAmErrors)
            {
                string emailScanDatesString = BuildEmailScanDatesShortString(options);
                var subject = $"All Locations ATSPM Alerts for {emailScanDatesString}";
                var emailBody = await CreateEmailBody(options, newErrors, dailyRecurringErrors, recurringErrors
                    , locations, logsFromPreviousDay);

                await mailService.SendEmailAsync(new MailAddress(options.DefaultEmailAddress), users.GetMailingAddresses(), subject, emailBody, true);
            }

            //This will send the ramp email.
            if (options.EmailRampErrors)
            {
                var emailScanDatesString = BuildEmailScanDatesShortString(options, true);
                var rampsubject = $"All Ramp ATSPM Alerts for {emailScanDatesString}";
                var rampEmailBody = await CreateEmailBody(options, newErrors, dailyRecurringErrors, recurringErrors
                    , locations, logsFromPreviousDay, true);


                await mailService.SendEmailAsync(new MailAddress(options.DefaultEmailAddress), users.GetMailingAddresses(), rampsubject, rampEmailBody, true);
            }

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

                string emailScanDatesString = BuildEmailScanDatesShortString(options);
                //This is the ramp email.
                if (jurisdiction.Name.ToLower().Contains("ramp"))
                {
                    emailScanDatesString = BuildEmailScanDatesShortString(options, true);
                    var subject = $"{jurisdiction.Name} ATSPM Ramp Alerts for {emailScanDatesString}";
                    if (!userIdsByJurisdiction.IsNullOrEmpty() && !LocationsByJurisdiction.IsNullOrEmpty())
                    {
                        var emailBody = await CreateEmailBody(
                            options,
                            newErrors, dailyRecurringErrors, recurringErrors,
                            LocationsByJurisdiction,
                            logsFromPreviousDay, true);

                        await mailService.SendEmailAsync(new MailAddress(options.DefaultEmailAddress), usersByJurisdiction.GetMailingAddresses(), subject, emailBody, true);
                    }
                }
                else if (options.EmailPmErrors || options.EmailAmErrors)
                {

                    var subject = $"{jurisdiction.Name} ATSPM Alerts for {emailScanDatesString}";
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
            if (!(options.EmailPmErrors || options.EmailAmErrors))
            {
                return;
            }
            foreach (var area in areas)
            {
                var userIdsByArea = userAreas.Where(ua => ua.AreaId == area.Id).ToList();
                if (userIdsByArea.IsNullOrEmpty())
                    continue;
                var usersByArea = users.Where(u => userIdsByArea.Any(ua => ua.UserId == u.Id)).ToList();
                var LocationsByArea = Locations.Where(s => s.Areas.Select(a => a.Id).Contains(area.Id)).ToList();
                string emailScanDatesString = BuildEmailScanDatesShortString(options);
                var subject = $"{area.Name} ATSPM Alerts for {emailScanDatesString}";
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
            if (!(options.EmailPmErrors || options.EmailAmErrors))
            {
                return;
            }
            foreach (var region in regions)
            {
                var userIdsByRegion = userRegions.Where(ur => ur.RegionId == region.Id).ToList();
                if (userIdsByRegion.IsNullOrEmpty())
                    continue;
                var usersByRegion = users.Where(u => userIdsByRegion.Any(ur => ur.UserId == u.Id)).ToList();
                var LocationsByRegion = Locations.Where(s => s.RegionId == region.Id).ToList();
                string emailScanDatesString = BuildEmailScanDatesShortString(options);
                var subject = $"{region.Description} ATSPM Alerts for {emailScanDatesString}";
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
            List<WatchDogLogEvent> logsFromPreviousDay,
            bool rampEmail = false)
        {
            var emailBodyBuilder = new StringBuilder();
            string emailScanDatesString = BuildEmailScanDatesString(options, rampEmail);

            // Process New Errors
            emailBodyBuilder.Append(ProcessErrorList("New Errors", $"Errors that occurred on {emailScanDatesString} that have not occured within the last 12 months.", newErrors, options, locations, logsFromPreviousDay, false, false, rampEmail));

            // Process Daily Recurring Errors
            emailBodyBuilder.Append(ProcessErrorList("Daily Recurring Errors", $"Errors that occurred on {emailScanDatesString} that also occurred the day prior to the proccessing date.", dailyRecurringErrors, options, locations, logsFromPreviousDay, true, true, rampEmail));

            // Process Recurring Errors
            emailBodyBuilder.Append(ProcessErrorList("Recurring Errors", $"Errors that occurred on {emailScanDatesString} that did not occur on the day prior to processing but have occured at least one time in the last 12 months.", recurringErrors, options, locations, logsFromPreviousDay, true, false, rampEmail));

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
            bool includeConsecutive,
            bool rampEmail = false)
        {
            if (errors == null || !errors.Any())
            {
                return $"<h2>{errorTitle}</h2><h4>{errorSubHeader}</h4><p>No errors found for this category.</p>";
            }
            var emailAllErrors = options.EmailAllErrors;

            // Categorize errors
            GetEventsByIssueType(errors, out var missingErrorsLogs, out var forceErrorsLogs, out var maxErrorsLogs,
                out var countErrorsLogs, out var stuckPedErrorsLogs, out var configurationErrorsLogs,
                out var unconfiguredDetectorErrorsLogs, out var rampDetectorThresholdErrorsLogs, out var rampMainlineErrorsLogs);

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
            if (options.EmailRampErrors && rampEmail)
            {
                var emailScanDate = options.RampMissedDetectorHitsStartScanDate.Date.ToShortDateString();
                bodyBuilder.Append(BuildErrorSection("Ramp Mainline Errors", $"The following Ramps had too many records failure in the database on {emailScanDate}",
                    rampMainlineErrorsLogs, locationDictionary, emailAllErrors, logsFromPreviousDay, includeErrorCounts, includeConsecutive));
                bodyBuilder.Append(BuildErrorSection("Ramp Detectors Threshold Errors", $"The following Ramps encountered errors above the set threshold on {emailScanDate} between {options.RampDetectorStartHour}:00 and {options.RampDetectorEndHour}:00",
                    rampDetectorThresholdErrorsLogs, locationDictionary, emailAllErrors, logsFromPreviousDay, includeErrorCounts, includeConsecutive));
            }
            if (options.EmailPmErrors && !rampEmail)
            {
                var emailScanDate = options.PmScanDate.Date.ToShortDateString();
                bodyBuilder.Append(BuildErrorSection("Missing Records Errors", $"The following Locations had too few records in the database on {emailScanDate}",
                    missingErrorsLogs, locationDictionary, emailAllErrors, logsFromPreviousDay, includeErrorCounts, includeConsecutive));
                bodyBuilder.Append(BuildErrorSection("Low Detection Count Errors", $"The following Locations had unusually low advanced detection counts on {emailScanDate}",
                    countErrorsLogs, locationDictionary, emailAllErrors, logsFromPreviousDay, includeErrorCounts, includeConsecutive));
                bodyBuilder.Append(BuildErrorSection("Unconfigured Approaches Errors", $"The following Approaches flagged as unconfigured on {emailScanDate}",
                    configurationErrorsLogs, locationDictionary, emailAllErrors, logsFromPreviousDay, includeErrorCounts, includeConsecutive));
                bodyBuilder.Append(BuildErrorSection("Unconfigured Detectors Errors", $"The following Detectors flagged as unconfigured on {emailScanDate}",
                    unconfiguredDetectorErrorsLogs, locationDictionary, emailAllErrors, logsFromPreviousDay, includeErrorCounts, includeConsecutive));
            }
            if (options.EmailAmErrors && !rampEmail)
            {
                var emailScanDate = options.RampMissedDetectorHitsStartScanDate.Date.ToShortDateString();
                bodyBuilder.Append(BuildErrorSection("Force Off Errors", $"The following Locations had too many force off occurrences between {options.AmStartHour}:00 and {options.AmEndHour}:00 on {emailScanDate}",
                    forceErrorsLogs, locationDictionary, emailAllErrors, logsFromPreviousDay, includeErrorCounts, includeConsecutive));
                bodyBuilder.Append(BuildErrorSection("Max Out Errors", $"The following Locations had too many max out occurrences between {options.AmStartHour}:00 and {options.AmEndHour}:00 on {emailScanDate}",
                    maxErrorsLogs, locationDictionary, emailAllErrors, logsFromPreviousDay, includeErrorCounts, includeConsecutive));
                bodyBuilder.Append(BuildErrorSection("High Pedestrian Activation Errors", $"The following Locations have high pedestrian activation occurrences between {options.AmStartHour}:00 and {options.AmEndHour}:00 on {emailScanDate}",
                    stuckPedErrorsLogs, locationDictionary, emailAllErrors, logsFromPreviousDay, includeErrorCounts, includeConsecutive));
            }

            return bodyBuilder.ToString();
        }

        public string BuildErrorSection(
            string sectionTitle,
            string sectionTimeDescription,
            List<WatchDogLogEventWithCountAndDate> errorLogs,
            Dictionary<int, Location> locations,
            bool emailAllErrors,
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
                sectionBuilder.Append(GetMessage(locations, errorLogs, emailAllErrors, logsFromPreviousDay, includeErrorCounts, includeConsecutive));

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
            out List<WatchDogLogEventWithCountAndDate> unconfiguredDetectorErrorsLogs,
            out List<WatchDogLogEventWithCountAndDate> rampDetectorThresholdErrorsLogs,
            out List<WatchDogLogEventWithCountAndDate> rampMainlineErrorsLogs
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

            rampDetectorThresholdErrorsLogs = eventsContainer
                .Where(e => e?.IssueType == WatchDogIssueTypes.LowRampDetectorHits)
                .ToList();

            rampMainlineErrorsLogs = eventsContainer
                .Where(e => e?.IssueType == WatchDogIssueTypes.RampMissedDetectorHits)
                .ToList();
        }



        public string GetMessage(
            Dictionary<int, Location> locationDictionary,
            List<WatchDogLogEventWithCountAndDate> issues,
            bool emailAllErrors,
            List<WatchDogLogEvent> logsFromPreviousDay,
            bool includeErrorCounts,
            bool includeConsecutive)
        {
            if (locationDictionary is null || issues is null || logsFromPreviousDay is null)
            {
                logger.LogError("Null parameter passed to GetMessage");
                return string.Empty;
            }
            var errorMessage = "";
            foreach (var error in issues)
            {
                if (emailAllErrors || !logsFromPreviousDay.Contains(error))
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

        private string BuildScanDateString(
            WatchdogEmailOptions emailOptions,
            IEnumerable<WatchdogScanType> scanTypes)
        {
            var parts = new List<string>();

            foreach (var scanType in scanTypes)
            {
                switch (scanType)
                {
                    case WatchdogScanType.Pm when emailOptions.EmailPmErrors:
                        parts.Add($"PM {emailOptions.PmScanDate:M/d/yyyy}");
                        break;

                    case WatchdogScanType.Am when emailOptions.EmailAmErrors:
                        parts.Add($"AM {emailOptions.AmScanDate:M/d/yyyy}");
                        break;

                    case WatchdogScanType.Ramp when emailOptions.EmailRampErrors:
                        parts.Add($"Ramp {emailOptions.RampMissedDetectorHitsStartScanDate:M/d/yyyy}");
                        break;
                }
            }

            return string.Join(", ", parts);
        }

        private string BuildScanDateShortString(
            WatchdogEmailOptions emailOptions,
            IEnumerable<WatchdogScanType> scanTypes)
        {
            var parts = new List<string>();

            foreach (var scanType in scanTypes)
            {
                switch (scanType)
                {
                    case WatchdogScanType.Pm when emailOptions.EmailPmErrors:
                        parts.Add($"PM {emailOptions.PmScanDate.Date.ToShortDateString()}");
                        break;

                    case WatchdogScanType.Am when emailOptions.EmailAmErrors:
                        parts.Add($"AM {emailOptions.AmScanDate.Date.ToShortDateString()}");
                        break;

                    case WatchdogScanType.Ramp when emailOptions.EmailRampErrors:
                        parts.Add($"Ramp {emailOptions.RampMissedDetectorHitsStartScanDate.Date.ToShortDateString()}");
                        break;
                }
            }

            return string.Join(", ", parts);
        }

        private string BuildEmailScanDatesString(
            WatchdogEmailOptions options,
            bool rampEmail = false)
        {
            if (options.EmailRampErrors && rampEmail)
            {
                return BuildScanDateString(
                    options,
                    new[] { WatchdogScanType.Ramp });
            }

            if (options.EmailPmErrors && options.EmailAmErrors)
            {
                return BuildScanDateString(
                    options,
                    new[] { WatchdogScanType.Pm, WatchdogScanType.Am });
            }

            if (options.EmailPmErrors)
            {
                return BuildScanDateString(
                    options,
                    new[] { WatchdogScanType.Pm });
            }

            if (options.EmailAmErrors)
            {
                return BuildScanDateString(
                    options,
                    new[] { WatchdogScanType.Am });
            }

            return string.Empty;
        }

        private string BuildEmailScanDatesShortString(
            WatchdogEmailOptions options,
            bool rampEmail = false)
        {
            if (options.EmailRampErrors && rampEmail)
            {
                return BuildScanDateShortString(
                    options,
                    new[] { WatchdogScanType.Ramp });
            }

            if (options.EmailPmErrors && options.EmailAmErrors)
            {
                return BuildScanDateShortString(
                    options,
                    new[] { WatchdogScanType.Pm, WatchdogScanType.Am });
            }

            if (options.EmailPmErrors)
            {
                return BuildScanDateShortString(
                    options,
                    new[] { WatchdogScanType.Pm });
            }

            if (options.EmailAmErrors)
            {
                return BuildScanDateShortString(
                    options,
                    new[] { WatchdogScanType.Am });
            }

            return string.Empty;
        }
    }
}