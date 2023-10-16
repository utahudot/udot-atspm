using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using WatchDog.Models;

namespace WatchDog.Services
{
    public class EmailService
    {
        private readonly IWatchDogLogEventRepository watchDogLogEventRepository;
        private readonly IRegionsRepository regionsRepository;
        private readonly IJurisdictionRepository jurisdictionRepository;
        private readonly IAreaRepository areaRepository;
        private readonly IUserRegionRepository userRegionRepository;
        private readonly IUserJurisdictionRepository userJurisdictionRepository;
        private readonly IUserAreaRepository userAreaRepository;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<EmailService> logger;

        public EmailService(IWatchDogLogEventRepository watchDogLogEventRepository,
            IRegionsRepository regionsRepository,
            IJurisdictionRepository jurisdictionRepository,
            IAreaRepository areaRepository,
            IUserRegionRepository userRegionRepository,
            IUserJurisdictionRepository userJurisdictionRepository,
            IUserAreaRepository userAreaRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<EmailService> logger)
        {
            this.watchDogLogEventRepository = watchDogLogEventRepository;
            this.regionsRepository = regionsRepository;
            this.jurisdictionRepository = jurisdictionRepository;
            this.areaRepository = areaRepository;
            this.userRegionRepository = userRegionRepository;
            this.userJurisdictionRepository = userJurisdictionRepository;
            this.userAreaRepository = userAreaRepository;
            this.userManager = userManager;
            this.logger = logger;
        }

        public List<ApplicationUser> GetUsersWithWatchDogClaim()
        {
            // Define the claim type and value you want to search for
            const string claimType = "Admin:WatchDog";

            // Get all users
            var allUsers = userManager.Users.ToList();

            // Filter users with the specified claim
            var usersWithWatchDogClaim = allUsers
                .Where(user => userManager.GetClaimsAsync(user).Result
                    .Any(claim => claim.Type == claimType))
                .ToList();

            return usersWithWatchDogClaim;
        }

        public async Task SendAllEmails(
            EmailOptions options,
            List<WatchDogLogEvent> eventsContainer,
            List<Signal> signals,
            SmtpClient smtp)
        {
            var users = GetUsersWithWatchDogClaim();
            await SendRegionEmails(options, eventsContainer, signals, smtp, users);
            await SendJurisdictionEmails(options, eventsContainer, signals, smtp, users);
            await SendAreaEmails(options, eventsContainer, signals, smtp, users);

        }

        private async Task SendJurisdictionEmails(EmailOptions options, List<WatchDogLogEvent> eventsContainer, List<Signal> signals, SmtpClient smtp, List<ApplicationUser> users)
        {
            var jurisdictions = jurisdictionRepository.GetList().ToList();
            var userJurisdictions = await userJurisdictionRepository.GetAllAsync();
            foreach (var jurisdiction in jurisdictions)
            {
                var userIdsByJurisdiction = userJurisdictions.Where(uj => uj.JurisdictionId == jurisdiction.Id).ToList();
                var usersByJurisdiction = users.Where(u => userIdsByJurisdiction.Any(uj => uj.UserId == u.Id)).ToList();
                var signalsByJurisdiction = signals.Where(s => s.JurisdictionId == jurisdiction.Id).ToList();
                CreateAndSendEmail(options, eventsContainer, signalsByJurisdiction, smtp, jurisdiction.Name, usersByJurisdiction);
            }
        }
        private async Task SendAreaEmails(EmailOptions options, List<WatchDogLogEvent> eventsContainer, List<Signal> signals, SmtpClient smtp, List<ApplicationUser> users)
        {
            var areas = areaRepository.GetList().ToList();
            var userAreas = await userAreaRepository.GetAllAsync();
            foreach (var area in areas)
            {
                var userIdsByArea = userAreas.Where(ua => ua.AreaId == area.Id).ToList();
                var usersByArea = users.Where(u => userIdsByArea.Any(ua => ua.UserId == u.Id)).ToList();
                var signalsByArea = signals.Where(s => s.RegionId == area.Id).ToList();
                CreateAndSendEmail(options, eventsContainer, signalsByArea, smtp, area.Name, usersByArea);
            }
        }
        private async Task SendRegionEmails(EmailOptions options, List<WatchDogLogEvent> eventsContainer, List<Signal> signals, SmtpClient smtp, List<ApplicationUser> users)
        {
            var regions = regionsRepository.GetList().ToList();
            var userRegions = await userRegionRepository.GetAllAsync();
            foreach (var region in regions)
            {
                var userIdsByRegion = userRegions.Where(ur => ur.RegionId == region.Id).ToList();
                var usersByRegion = users.Where(u => userIdsByRegion.Any(ur => ur.UserId == u.Id)).ToList();
                var signalsByRegion = signals.Where(s => s.RegionId == region.Id).ToList();
                CreateAndSendEmail(options, eventsContainer, signalsByRegion, smtp, region.Description, usersByRegion);
            }
        }

        public async Task CreateAndSendEmail(
            EmailOptions options,
            List<WatchDogLogEvent> eventsContainer,
            List<Signal> signals,
            SmtpClient smtp,
            string areaSpecificMessage,
            List<ApplicationUser> users)
        {

            var message = new MailMessage();
            foreach (var user in users)
                message.To.Add(user.Email);
            message.To.Add(options.DefaultEmailAddress);
            message.Subject = $"{areaSpecificMessage} ATSPM Alerts for {options.ScanDate.ToShortDateString()}";
            message.From = new MailAddress(options.DefaultEmailAddress);
            var missingErrorsLogs = eventsContainer.Where(e => e.IssueType == ATSPM.Data.Enums.WatchDogIssueType.RecordCount).ToList();
            var forceErrorsLogs = eventsContainer.Where(e => e.IssueType == ATSPM.Data.Enums.WatchDogIssueType.ForceOffThreshold).ToList();
            var maxErrorsLogs = eventsContainer.Where(e => e.IssueType == ATSPM.Data.Enums.WatchDogIssueType.MaxOutThreshold).ToList();
            var countErrorsLogs = eventsContainer.Where(e => e.IssueType == ATSPM.Data.Enums.WatchDogIssueType.LowDetectorHits).ToList();
            var stuckpedErrorsLogs = eventsContainer.Where(e => e.IssueType == ATSPM.Data.Enums.WatchDogIssueType.StuckPed).ToList();
            //var ftpErrors = SortAndAddToMessage(signals, eventsContainer.CannotFtpFiles, options);
            if (missingErrorsLogs.Any())
            {
                message.Body += $" \n --The following signals had too few records in the database on {options.ScanDate.Date.ToShortDateString()}: \n";
                message.Body += SortAndAddToMessage(signals, missingErrorsLogs, options);
            }
            else
            {
                message.Body += $"\n --No new missing record errors were found on {options.ScanDate.Date.ToShortDateString()}: \n";
            }

            if (forceErrorsLogs.Any())
            {
                message.Body += " \n --The following signals had too many force off occurrences between " +
                                options.ScanDayStartHour + ":00 and " +
                                options.ScanDayEndHour + ":00: \n";
                message.Body += SortAndAddToMessage(signals, forceErrorsLogs, options);
            }
            else
            {
                message.Body += "\n --No new force off errors were found between " +
                                options.ScanDayStartHour + ":00 and " +
                                options.ScanDayEndHour + ":00: \n";
            }

            if (maxErrorsLogs.Any())
            {
                message.Body += " \n --The following signals had too many max out occurrences between " +
                                options.ScanDayStartHour + ":00 and " +
                                options.ScanDayEndHour + ":00: \n";
                message.Body += SortAndAddToMessage(signals, maxErrorsLogs, options);
            }
            else
            {
                message.Body += "\n --No new max out errors were found between " +
                                options.ScanDayStartHour + ":00 and " +
                                options.ScanDayEndHour + ":00: \n";
            }

            if (countErrorsLogs.Any())
            {
                message.Body += $" \n --The following signals had unusually low advanced detection counts on {options.ScanDate.Date.ToShortDateString()}: \n";
                message.Body += options.PreviousDayPMPeakStart + ":00 and " +
                                options.PreviousDayPMPeakEnd + ":00: \n";
                message.Body += SortAndAddToMessage(signals, countErrorsLogs, options);
            }
            else
            {
                message.Body += $"\n --No new low advanced detection count errors on {options.ScanDate.Date.ToShortDateString()}: \n";
                message.Body += options.PreviousDayPMPeakStart + ":00 and " +
                                options.PreviousDayPMPeakEnd + ":00: \n";
            }
            if (stuckpedErrorsLogs.Any())
            {
                message.Body += " \n --The following signals have high pedestrian activation occurrences between " +
                                options.ScanDayStartHour + ":00 and " +
                                options.ScanDayEndHour + ":00: \n";
                message.Body += SortAndAddToMessage(signals, stuckpedErrorsLogs, options);
            }
            else
            {
                message.Body += "\n --No new high pedestrian activation errors between " +
                                options.ScanDayStartHour + ":00 and " +
                                options.ScanDayEndHour + ":00: \n";
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

            SendMessage(message, options, smtp);
        }



        private void SendMessage(MailMessage message, EmailOptions options, SmtpClient smtp)
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

        //private void SendMessageUsingCredentials(MailMessage message, EmailOptions options)
        //{
        //    var smtp = options.SmptClient;
        //    try
        //    {
        //        smtp.Credentials = options.GmailCredentials;
        //        smtp.Port = options.Port.Value;
        //        smtp.EnableSsl = options.EnableSsl.Value;

        //        logger.LogInformation($"Sending Email to: {message.To} \nMessage text: {message.Body} \n");
        //        smtp.Send(message);
        //        Thread.Sleep(5000);
        //        logger.LogInformation($"Email Sent Successfully to: {message.To}");
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex.Message);
        //    }
        //}


        private string SortAndAddToMessage(
            List<Signal> signals,
            List<WatchDogLogEvent> issues,
            EmailOptions options)
        {
            var sortedSignals = signals.OrderBy(x => x.SignalIdentifier).ToList();
            var errorMessage = "";
            var recordsFromTheDayBefore = new List<WatchDogLogEvent>();
            if (!options.EmailAllErrors)
            {
                //List<WatchDogEvent> RecordsFromTheDayBefore = new List<WatchDogEvent>();
                //compare to error log to see if this was failing yesterday
                if (options.WeekdayOnly && options.ScanDate.DayOfWeek == DayOfWeek.Monday)
                    recordsFromTheDayBefore =
                        watchDogLogEventRepository.GetList(w => w.Timestamp >= options.ScanDate.AddDays(-3) &&
                            w.Timestamp < options.ScanDate.AddDays(-2)).ToList();
                else
                    recordsFromTheDayBefore =
                        watchDogLogEventRepository.GetList(w => w.Timestamp >= options.ScanDate.AddDays(-1) &&
                            w.Timestamp < options.ScanDate).ToList();
            }
            foreach (var signal in sortedSignals)
            {
                foreach (var error in issues.Where(e => e.SignalId == signal.Id))
                {
                    if (options.EmailAllErrors || !recordsFromTheDayBefore.Contains(error))
                    {
                        //   Add to email if it was not failing yesterday
                        errorMessage += $"{error.SignalIdentifier} - {signal.PrimaryName} & {signal.SecondaryName}";

                        if (error.Phase > 0)
                        {
                            errorMessage += $" - Phase {error.Phase}";
                        }

                        errorMessage += $" ({error.Details})\n";
                    }
                }
            }

            return errorMessage;
        }
    }
}
