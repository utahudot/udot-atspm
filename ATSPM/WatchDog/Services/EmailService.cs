using ATSPM.Data.Models;
using ATSPM.Data.Models.ConfigurationModels;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using WatchDog.Models;

namespace WatchDog.Services
{
    public class EmailService
    {
        private readonly ILogger<EmailService> logger;
        private readonly IMailService mailService;

        public EmailService(
            ILogger<EmailService> logger,
            IMailService mailService)
        {
            this.logger = logger;
            this.mailService = mailService;
        }



        public async Task SendAllEmails(
            EmailOptions options,
            List<WatchDogLogEvent> eventsContainer,
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
            await SendRegionEmails(options, eventsContainer, Locations, users, regions, userRegions, logsFromPreviousDay);
            await SendJurisdictionEmails(options, eventsContainer, Locations, users, jurisdictions, userJurisdictions, logsFromPreviousDay);
            await SendAreaEmails(options, eventsContainer, Locations, users, areas, userAreas, logsFromPreviousDay);
            var otherUsers = users.Where(u => !userRegions.Any(ur => ur.UserId == u.Id)
                                              && !userJurisdictions.Any(uj => uj.UserId == u.Id)
                                              && !userAreas.Any(ua => ua.UserId == u.Id)).ToList();
            await SendAdminEmail(options, eventsContainer, Locations, "All Locations", otherUsers, logsFromPreviousDay);

        }

        private async Task SendAdminEmail(EmailOptions options, List<WatchDogLogEvent> eventsContainer, List<Location> locations, string v, List<ApplicationUser> users, List<WatchDogLogEvent> logsFromPreviousDay)
        {
            var subject = $"All Locations ATSPM Alerts for {options.ScanDate.ToShortDateString()}";
            var emailBody = await CreateEmailBody(options, eventsContainer, locations, logsFromPreviousDay);
            await mailService.SendEmailAsync(options.DefaultEmailAddress, users, subject, emailBody);
        }

        private async Task SendJurisdictionEmails(
            EmailOptions options,
            List<WatchDogLogEvent> eventsContainer,
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
                        eventsContainer,
                        LocationsByJurisdiction,
                        logsFromPreviousDay);
                    await mailService.SendEmailAsync(options.DefaultEmailAddress, usersByJurisdiction, subject, emailBody);
                }
            }
        }

        private async Task SendAreaEmails(
            EmailOptions options,
            List<WatchDogLogEvent> eventsContainer,
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
                        eventsContainer,
                        LocationsByArea,
                        logsFromPreviousDay);
                    await mailService.SendEmailAsync(options.DefaultEmailAddress, usersByArea, subject, emailBody);
                }
            }
        }
        private async Task SendRegionEmails(
            EmailOptions options,
            List<WatchDogLogEvent> eventsContainer,
            List<Location> Locations,
            List<ApplicationUser> users,
            List<ATSPM.Data.Models.Region> regions,
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
                        eventsContainer,
                        LocationsByRegion,
                        logsFromPreviousDay);
                    await mailService.SendEmailAsync(options.DefaultEmailAddress, usersByRegion, subject, emailBody);
                }
            }
        }

        public async Task<string> CreateEmailBody(
            EmailOptions options,
            List<WatchDogLogEvent> eventsContainer,
            List<Location> Locations,
            List<WatchDogLogEvent> logsFromPreviousDay)
        {
            List<WatchDogLogEvent> missingErrorsLogs, forceErrorsLogs, maxErrorsLogs, countErrorsLogs, stuckpedErrorsLogs, configurationErrorsLogs, unconfiguredDetectorErrorsLogs;
            GetEventsByIssueType(eventsContainer, out missingErrorsLogs, out forceErrorsLogs, out maxErrorsLogs, out countErrorsLogs, out stuckpedErrorsLogs, out configurationErrorsLogs, out unconfiguredDetectorErrorsLogs);
            return GetFormattedMessageBody(options, Locations, logsFromPreviousDay, missingErrorsLogs, forceErrorsLogs, maxErrorsLogs, countErrorsLogs, stuckpedErrorsLogs, configurationErrorsLogs, unconfiguredDetectorErrorsLogs);

        }

        private string GetFormattedMessageBody(
            EmailOptions options,
            List<Location> Locations,
            List<WatchDogLogEvent> logsFromPreviousDay,
            List<WatchDogLogEvent> missingErrorsLogs,
            List<WatchDogLogEvent> forceErrorsLogs,
            List<WatchDogLogEvent> maxErrorsLogs,
            List<WatchDogLogEvent> countErrorsLogs,
            List<WatchDogLogEvent> stuckpedErrorsLogs,
            List<WatchDogLogEvent> configurationErrorsLogs,
            List<WatchDogLogEvent> unconfiguredDetectorErrorsLogs
            )
        {
            var body = "<style>\r\n  .atspm-table {\r\n    border: solid 2px #DDEEEE;\r\n    border-collapse: collapse;\r\n    border-spacing: 0;\r\n    font: normal 14px Roboto, sans-serif;\r\n  }\r\n\r\n  .atspm-table thead th {\r\n    background-color: #DDEFEF;\r\n    border: solid 1px #DDEEEE;\r\n    color: #336B6B;\r\n    padding: 10px;\r\n    text-align: left;\r\n    text-shadow: 1px 1px 1px #fff;\r\n  }\r\n\r\n  .atspm-table tbody td {\r\n    border: solid 1px #DDEEEE;\r\n    color: #333;\r\n    padding: 10px;\r\n    text-shadow: 1px 1px 1px #fff;\r\n  }\r\n</style>";


            //var ftpErrors = SortAndAddToMessage(Locations, eventsContainer.CannotFtpFiles, options);
            if (missingErrorsLogs.Any())
            {
                body += $"<table class='atspm-table'><thead><tr><th colspan='3'>The following Locations had too few records in the database on {options.ScanDate.Date.ToShortDateString()}</th></tr>";
                body += $"<tr><thead><th>Location</th><th>Location Description</th><th>Issue Details</th></thead></tr></thead>";
                body += SortAndAddToMessage(Locations, missingErrorsLogs, options, logsFromPreviousDay);
                body += "</table></br>";
            }
            else
            {
                body += $"<table class='atspm-table'><thead><tr><th colspan='3'>No new missing record errors were found on {options.ScanDate.Date.ToShortDateString()}</th></tr>";
                body += $"<tr><th>Location</th><th>Location Description</th><th>Issue Details</th></tr><thead>";
                body += "</table></br>";
            }

            if (forceErrorsLogs.Any())
            {
                body += $"<table class='atspm-table'><thead><tr><th colspan='4'>The following Locations had too many force off occurrences between {options.ScanDayStartHour}:00 and {options.ScanDayEndHour}:00</th></tr>";
                body += $"<tr><th>Location</th><th>Location Description</th><th>Phase</th><th>Issue Details</th></tr></thead>";
                body += SortAndAddToMessage(Locations, forceErrorsLogs, options, logsFromPreviousDay);
                body += "</table></br>";
            }
            else
            {
                body += $"<table class='atspm-table'><thead><tr><th colspan='4'>No new force off errors were found between {options.ScanDayStartHour}:00 and {options.ScanDayEndHour}:00</th></tr>";
                body += $"<tr><th>Location</th><th>Location Description</th><th>Phase</th><th>Issue Details</th></tr></thead>";
                body += "</table></br>";
            }

            if (maxErrorsLogs.Any())
            {
                body += $"<table class='atspm-table'><thead><tr><th colspan='4'>The following Locations had too many max out occurrences between {options.ScanDayStartHour}:00 and {options.ScanDayEndHour}:00</th></tr>";
                body += $"<tr><th>Location</th><th>Location Description</th><th>Phase</th><th>Issue Details</th></tr></thead>";
                body += SortAndAddToMessage(Locations, maxErrorsLogs, options, logsFromPreviousDay);
                body += "</table></br>";
            }
            else
            {
                body += $"<table class='atspm-table'><thead><tr><th colspan='4'>No new max out errors were found between {options.ScanDayStartHour}:00 and {options.ScanDayEndHour}:00</th></tr>";
                body += $"<tr><th>Location</th><th>Location Description</th><th>Phase</th><th>Issue Details</th></tr><thead>";
                body += "</table></br>";
            }

            if (countErrorsLogs.Any())
            {
                body += $"<table class='atspm-table'><thead><tr><th colspan='4'>The following Locations had unusually low advanced detection counts on {options.ScanDate.Date.ToShortDateString()}";
                body += $"{options.PreviousDayPMPeakStart}:00 and {options.PreviousDayPMPeakEnd}:00</th></tr>";
                body += $"<tr><th>Location</th><th>Location Description</th><th>Detector Id</th><th>Issue Details</th></tr><thead>";
                body += SortAndAddToMessage(Locations, countErrorsLogs, options, logsFromPreviousDay);
                body += "</table></br>";
            }
            else
            {
                body += $"<table class='atspm-table'><thead><tr><th colspan='4'>No new low advanced detection count errors on {options.ScanDate.Date.ToShortDateString()}";
                body += $"{options.PreviousDayPMPeakStart}:00 and {options.PreviousDayPMPeakEnd}:00</th></tr>";
                body += $"<tr><th>Location</th><th>Location Description</th><th>Issue Details</th></tr><thead>";
                body += "</table></br>";
            }
            if (stuckpedErrorsLogs.Any())
            {
                body += $"<table class='atspm-table'><thead><tr><th colspan='4'>The following Locations have high pedestrian activation occurrences between {options.ScanDayStartHour}:00 and {options.ScanDayEndHour}:00</th></tr>";
                body += $"<tr><th>Location</th><th>Location Description</th><th>Phase</th><th>Issue Details</th></tr><thead>";
                body += SortAndAddToMessage(Locations, stuckpedErrorsLogs, options, logsFromPreviousDay);
                body += "</table></br>";
            }
            else
            {
                body += $"<table class='atspm-table'><thead><tr><th colspan='4'>No new high pedestrian activation errors between {options.ScanDayStartHour}:00 and {options.ScanDayEndHour}:00: \n";
                body += $"<tr><th>Location</th><th>Location Description</th><th>Phase</th><th>Issue Details</th></tr><thead>";
                body += "</table></br>";
            }
            if (configurationErrorsLogs.Any())
            {
                body += $"<table class='atspm-table'><thead><tr><th colspan='4'>The following Locations have unconfigured approaches</th></tr>";
                body += $"<tr><th>Location</th><th>Location Description</th><th>Phase</th><th>Issue Details</th></tr><thead>";
                body += SortAndAddToMessage(Locations, configurationErrorsLogs, options, logsFromPreviousDay);
                body += "</table></br>";
            }
            else
            {
                body += $"<table class='atspm-table'><thead><tr><th colspan='4'>No new unconfigured approaches \n";
                body += $"<tr><th>Location</th><th>Location Description</th><th>Phase</th><th>Issue Details</th></tr><thead>";
                body += "</table></br>";
            }
            if (unconfiguredDetectorErrorsLogs.Any())
            {
                body += $"<table class='atspm-table'><thead><tr><th colspan='4'>The following Locations have unconfigured detectors</th></tr>";
                body += $"<tr><th>Location</th><th>Location Description</th><th>Detector Id</th><th>Issue Details</th></tr><thead>";
                body += SortAndAddToMessage(Locations, unconfiguredDetectorErrorsLogs, options, logsFromPreviousDay);
                body += "</table></br>";
            }
            else
            {
                body += $"<table class='atspm-table'><thead><tr><th colspan='4'>No new unconfigured detectors \n";
                body += $"<tr><th>Location</th><th>Location Description</th><th>Detector Id</th><th>Issue Details</th></tr><thead>";
                body += "</table></br>";
            }

            return body;

            //if (eventsContainer.CannotFtpFiles.Count > 0 && ftpErrors != "")
            //{
            //    message.Body += " \n --The following Locations have had FTP problems.  central was not able to delete the file on the controller between  " +
            //                    options.ScanDayStartHour + ":00 and " +
            //                    options.ScanDayEndHour + ":00: \n";
            //    message.Body += ftpErrors;
            //}
            //else
            //{
            //    message.Body += "\n --No new controllers had problems FTPing files from the controller between " +
            //                    options.ScanDayStartHour + ":00 and " +
            //                    options.ScanDayEndHour + ":00: \n";
            //}
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
            missingErrorsLogs = eventsContainer.Where(e => e.IssueType == ATSPM.Data.Enums.WatchDogIssueType.RecordCount).ToList();
            forceErrorsLogs = eventsContainer.Where(e => e.IssueType == ATSPM.Data.Enums.WatchDogIssueType.ForceOffThreshold).ToList();
            maxErrorsLogs = eventsContainer.Where(e => e.IssueType == ATSPM.Data.Enums.WatchDogIssueType.MaxOutThreshold).ToList();
            countErrorsLogs = eventsContainer.Where(e => e.IssueType == ATSPM.Data.Enums.WatchDogIssueType.LowDetectorHits).ToList();
            stuckpedErrorsLogs = eventsContainer.Where(e => e.IssueType == ATSPM.Data.Enums.WatchDogIssueType.StuckPed).ToList();
            configurationErrorsLogs = eventsContainer.Where(e => e.IssueType == ATSPM.Data.Enums.WatchDogIssueType.UnconfiguredApproach).ToList();
            unconfiguredDetectorErrorsLogs = eventsContainer.Where(e => e.IssueType == ATSPM.Data.Enums.WatchDogIssueType.UnconfiguredDetector).ToList();
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
                return String.Empty;
            }
            var sortedLocations = Locations.OrderBy(x => x.LocationIdentifier).ToList();
            var errorMessage = "";
            foreach (var Location in sortedLocations)
            {
                foreach (var error in issues.Where(e => e.locationId == Location.Id))
                {
                    if (options.EmailAllErrors || !logsFromPreviousDay.Contains(error))
                    {
                        //   Add to email if it was not failing yesterday
                        errorMessage += $"<tr><td>{error.locationIdentifier}</td><td>{Location.PrimaryName} & {Location.SecondaryName}</td>";

                        if (error.Phase > 0)
                        {
                            errorMessage += $"<td>{error.Phase}</td>";
                        }
                        if (error.ComponentType == ATSPM.Data.Enums.WatchDogComponentType.Detector)
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