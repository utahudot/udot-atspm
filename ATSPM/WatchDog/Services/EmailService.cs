using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Net.Mail;
using WatchDog.Models;

namespace WatchDog.Services
{
    public class EmailService
    {
        private readonly ILogger<EmailService> logger;

        public EmailService(
            ILogger<EmailService> logger)
        {
            this.logger = logger;
        }



        public async Task SendAllEmails(
            EmailOptions options,
            List<WatchDogLogEvent> eventsContainer,
            List<Location> signals,
            SmtpClient smtp,
            List<ApplicationUser> users,
            List<Jurisdiction> jurisdictions,
            List<UserJurisdiction> userJurisdictions,
            List<Area> areas,
            List<UserArea> userAreas,
            List<Region> regions,
            List<UserRegion> userRegions,
            List<WatchDogLogEvent> logsFromPreviousDay)
        {
            await SendRegionEmails(options, eventsContainer, signals, smtp, users, regions, userRegions, logsFromPreviousDay);
            await SendJurisdictionEmails(options, eventsContainer, signals, smtp, users, jurisdictions, userJurisdictions, logsFromPreviousDay);
            await SendAreaEmails(options, eventsContainer, signals, smtp, users, areas, userAreas, logsFromPreviousDay);
            await CreateAndSendEmail(options, eventsContainer, signals, smtp, "All Signals", users, logsFromPreviousDay);

        }

        private async Task SendJurisdictionEmails(
            EmailOptions options,
            List<WatchDogLogEvent> eventsContainer,
            List<Location> signals,
            SmtpClient smtp,
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
                var signalsByJurisdiction = signals.Where(s => s.JurisdictionId == jurisdiction.Id).ToList();
                if (!userIdsByJurisdiction.IsNullOrEmpty() && !signalsByJurisdiction.IsNullOrEmpty())
                    CreateAndSendEmail(options, eventsContainer, signalsByJurisdiction, smtp, jurisdiction.Name, usersByJurisdiction, logsFromPreviousDay);
            }
        }
        private async Task SendAreaEmails(
            EmailOptions options,
            List<WatchDogLogEvent> eventsContainer,
            List<Location> signals,
            SmtpClient smtp,
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
                var signalsByArea = signals.Where(s => s.RegionId == area.Id).ToList();
                if (!userIdsByArea.IsNullOrEmpty() && !signalsByArea.IsNullOrEmpty())
                    CreateAndSendEmail(options, eventsContainer, signalsByArea, smtp, area.Name, usersByArea, logsFromPreviousDay);
            }
        }
        private async Task SendRegionEmails(
            EmailOptions options,
            List<WatchDogLogEvent> eventsContainer,
            List<Location> signals,
            SmtpClient smtp,
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
                var signalsByRegion = signals.Where(s => s.RegionId == region.Id).ToList();
                if (!userIdsByRegion.IsNullOrEmpty() && !signalsByRegion.IsNullOrEmpty())
                    CreateAndSendEmail(options, eventsContainer, signalsByRegion, smtp, region.Description, usersByRegion, logsFromPreviousDay);
            }
        }

        public async Task CreateAndSendEmail(
            EmailOptions options,
            List<WatchDogLogEvent> eventsContainer,
            List<Location> signals,
            SmtpClient smtp,
            string areaSpecificMessage,
            List<ApplicationUser> users,
            List<WatchDogLogEvent> logsFromPreviousDay)
        {

            var message = new MailMessage();
            SetupMailMessage(options, areaSpecificMessage, users, message);
            List<WatchDogLogEvent> missingErrorsLogs, forceErrorsLogs, maxErrorsLogs, countErrorsLogs, stuckpedErrorsLogs, configurationErrorsLogs, unconfiguredDetectorErrorsLogs;
            GetEventsByIssueType(eventsContainer, out missingErrorsLogs, out forceErrorsLogs, out maxErrorsLogs, out countErrorsLogs, out stuckpedErrorsLogs, out configurationErrorsLogs, out unconfiguredDetectorErrorsLogs);
            AddMessageBody(options, signals, logsFromPreviousDay, message, missingErrorsLogs, forceErrorsLogs, maxErrorsLogs, countErrorsLogs, stuckpedErrorsLogs, configurationErrorsLogs, unconfiguredDetectorErrorsLogs);
            SendMessage(message, smtp);
        }

        private void AddMessageBody(
            EmailOptions options,
            List<Location> signals,
            List<WatchDogLogEvent> logsFromPreviousDay,
            MailMessage message,
            List<WatchDogLogEvent> missingErrorsLogs,
            List<WatchDogLogEvent> forceErrorsLogs,
            List<WatchDogLogEvent> maxErrorsLogs,
            List<WatchDogLogEvent> countErrorsLogs,
            List<WatchDogLogEvent> stuckpedErrorsLogs,
            List<WatchDogLogEvent> configurationErrorsLogs,
            List<WatchDogLogEvent> unconfiguredDetectorErrorsLogs
            )
        {
            message.Body += "<style>\r\n  .atspm-table {\r\n    border: solid 2px #DDEEEE;\r\n    border-collapse: collapse;\r\n    border-spacing: 0;\r\n    font: normal 14px Roboto, sans-serif;\r\n  }\r\n\r\n  .atspm-table thead th {\r\n    background-color: #DDEFEF;\r\n    border: solid 1px #DDEEEE;\r\n    color: #336B6B;\r\n    padding: 10px;\r\n    text-align: left;\r\n    text-shadow: 1px 1px 1px #fff;\r\n  }\r\n\r\n  .atspm-table tbody td {\r\n    border: solid 1px #DDEEEE;\r\n    color: #333;\r\n    padding: 10px;\r\n    text-shadow: 1px 1px 1px #fff;\r\n  }\r\n</style>";

            //var ftpErrors = SortAndAddToMessage(signals, eventsContainer.CannotFtpFiles, options);
            if (missingErrorsLogs.Any())
            {
                message.Body += $"<table class='atspm-table'><thead><tr><th colspan='3'>The following signals had too few records in the database on {options.ScanDate.Date.ToShortDateString()}</th></tr>";
                message.Body += $"<tr><thead><th>Signal</th><th>Signal Description</th><th>Issue Details</th></thead></tr></thead>";
                message.Body += SortAndAddToMessage(signals, missingErrorsLogs, options, logsFromPreviousDay);
                message.Body += "</table></br>";
            }
            else
            {
                message.Body += $"<table class='atspm-table'><thead><tr><th colspan='3'>No new missing record errors were found on {options.ScanDate.Date.ToShortDateString()}</th></tr>";
                message.Body += $"<tr><th>Signal</th><th>Signal Description</th><th>Issue Details</th></tr><thead>";
                message.Body += "</table></br>";
            }

            if (forceErrorsLogs.Any())
            {
                message.Body += $"<table class='atspm-table'><thead><tr><th colspan='4'>The following signals had too many force off occurrences between {options.ScanDayStartHour}:00 and {options.ScanDayEndHour}:00</th></tr>";
                message.Body += $"<tr><th>Signal</th><th>Signal Description</th><th>Phase</th><th>Issue Details</th></tr></thead>";
                message.Body += SortAndAddToMessage(signals, forceErrorsLogs, options, logsFromPreviousDay);
                message.Body += "</table></br>";
            }
            else
            {
                message.Body += $"<table class='atspm-table'><thead><tr><th colspan='4'>No new force off errors were found between {options.ScanDayStartHour}:00 and {options.ScanDayEndHour}:00</th></tr>";
                message.Body += $"<tr><th>Signal</th><th>Signal Description</th><th>Phase</th><th>Issue Details</th></tr></thead>";
                message.Body += "</table></br>";
            }

            if (maxErrorsLogs.Any())
            {
                message.Body += $"<table class='atspm-table'><thead><tr><th colspan='4'>The following signals had too many max out occurrences between {options.ScanDayStartHour}:00 and {options.ScanDayEndHour}:00</th></tr>";
                message.Body += $"<tr><th>Signal</th><th>Signal Description</th><th>Phase</th><th>Issue Details</th></tr></thead>";
                message.Body += SortAndAddToMessage(signals, maxErrorsLogs, options, logsFromPreviousDay);
                message.Body += "</table></br>";
            }
            else
            {
                message.Body += $"<table class='atspm-table'><thead><tr><th colspan='4'>No new max out errors were found between {options.ScanDayStartHour}:00 and {options.ScanDayEndHour}:00</th></tr>";
                message.Body += $"<tr><th>Signal</th><th>Signal Description</th><th>Phase</th><th>Issue Details</th></tr><thead>";
                message.Body += "</table></br>";
            }

            if (countErrorsLogs.Any())
            {
                message.Body += $"<table class='atspm-table'><thead><tr><th colspan='4'>The following signals had unusually low advanced detection counts on {options.ScanDate.Date.ToShortDateString()}";
                message.Body += $"{options.PreviousDayPMPeakStart}:00 and {options.PreviousDayPMPeakEnd}:00</th></tr>";
                message.Body += $"<tr><th>Signal</th><th>Signal Description</th><th>Detector Id</th><th>Issue Details</th></tr><thead>";
                message.Body += SortAndAddToMessage(signals, countErrorsLogs, options, logsFromPreviousDay);
                message.Body += "</table></br>";
            }
            else
            {
                message.Body += $"<table class='atspm-table'><thead><tr><th colspan='4'>No new low advanced detection count errors on {options.ScanDate.Date.ToShortDateString()}";
                message.Body += $"{options.PreviousDayPMPeakStart}:00 and {options.PreviousDayPMPeakEnd}:00</th></tr>";
                message.Body += $"<tr><th>Signal</th><th>Signal Description</th><th>Issue Details</th></tr><thead>";
                message.Body += "</table></br>";
            }
            if (stuckpedErrorsLogs.Any())
            {
                message.Body += $"<table class='atspm-table'><thead><tr><th colspan='4'>The following signals have high pedestrian activation occurrences between {options.ScanDayStartHour}:00 and {options.ScanDayEndHour}:00</th></tr>";
                message.Body += $"<tr><th>Signal</th><th>Signal Description</th><th>Phase</th><th>Issue Details</th></tr><thead>";
                message.Body += SortAndAddToMessage(signals, stuckpedErrorsLogs, options, logsFromPreviousDay);
                message.Body += "</table></br>";
            }
            else
            {
                message.Body += $"<table class='atspm-table'><thead><tr><th colspan='4'>No new high pedestrian activation errors between {options.ScanDayStartHour}:00 and {options.ScanDayEndHour}:00: \n";
                message.Body += $"<tr><th>Signal</th><th>Signal Description</th><th>Phase</th><th>Issue Details</th></tr><thead>";
                message.Body += "</table></br>";
            }
            if (configurationErrorsLogs.Any())
            {
                message.Body += $"<table class='atspm-table'><thead><tr><th colspan='4'>The following signals have unconfigured approaches</th></tr>";
                message.Body += $"<tr><th>Signal</th><th>Signal Description</th><th>Phase</th><th>Issue Details</th></tr><thead>";
                message.Body += SortAndAddToMessage(signals, configurationErrorsLogs, options, logsFromPreviousDay);
                message.Body += "</table></br>";
            }
            else
            {
                message.Body += $"<table class='atspm-table'><thead><tr><th colspan='4'>No new unconfigured approaches \n";
                message.Body += $"<tr><th>Signal</th><th>Signal Description</th><th>Phase</th><th>Issue Details</th></tr><thead>";
                message.Body += "</table></br>";
            }
            if (unconfiguredDetectorErrorsLogs.Any())
            {
                message.Body += $"<table class='atspm-table'><thead><tr><th colspan='4'>The following signals have unconfigured detectors</th></tr>";
                message.Body += $"<tr><th>Signal</th><th>Signal Description</th><th>Detector Id</th><th>Issue Details</th></tr><thead>";
                message.Body += SortAndAddToMessage(signals, unconfiguredDetectorErrorsLogs, options, logsFromPreviousDay);
                message.Body += "</table></br>";
            }
            else
            {
                message.Body += $"<table class='atspm-table'><thead><tr><th colspan='4'>No new unconfigured detectors \n";
                message.Body += $"<tr><th>Signal</th><th>Signal Description</th><th>Detector Id</th><th>Issue Details</th></tr><thead>";
                message.Body += "</table></br>";
            }
            //if (eventsContainer.CannotFtpFiles.Count > 0 && ftpErrors != "")
            //{
            //    message.Body += " \n --The following signals have had FTP problems.  central was not able to delete the file on the controller between  " +
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

        private void SetupMailMessage(EmailOptions options, string areaSpecificMessage, List<ApplicationUser> users, MailMessage message)
        {
            try
            {
                foreach (var user in users)
                    message.To.Add(user.Email);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding users to email");
                throw;
            }
            message.To.Add(options.DefaultEmailAddress);
            message.Subject = $"{areaSpecificMessage} ATSPM Alerts for {options.ScanDate.ToShortDateString()}";
            message.From = new MailAddress(options.DefaultEmailAddress);
            message.IsBodyHtml = true;
        }

        private void SendMessage(MailMessage message, SmtpClient smtp)
        {

            if (smtp is null)
            {
                throw new ArgumentNullException(nameof(smtp));
            }
            if (message is null)
            {
                logger.LogError("Message object cannot be null");
            }
            else
            {
                try
                {
                    logger.LogInformation($"Sending Email to: {message.To} \nMessage text: {message.Body} \n");
                    smtp.Send(message);
                    Thread.Sleep(5000);
                    logger.LogInformation($"Email Sent Successfully to: {message.To}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message);
                }
            }
        }


        private string SortAndAddToMessage(
            List<Location> signals,
            List<WatchDogLogEvent> issues,
            EmailOptions options,
            List<WatchDogLogEvent> logsFromPreviousDay)
        {
            if (signals is null || issues is null || options is null || logsFromPreviousDay is null)
            {
                logger.LogError("Null parameter passed to SortAndAddToMessage");
                return String.Empty;
            }
            var sortedSignals = signals.OrderBy(x => x.LocationIdentifier).ToList();
            var errorMessage = "";
            foreach (var signal in sortedSignals)
            {
                foreach (var error in issues.Where(e => e.locationId == signal.Id))
                {
                    if (options.EmailAllErrors || !logsFromPreviousDay.Contains(error))
                    {
                        //   Add to email if it was not failing yesterday
                        errorMessage += $"<tr><td>{error.locationIdentifier}</td><td>{signal.PrimaryName} & {signal.SecondaryName}</td>";

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