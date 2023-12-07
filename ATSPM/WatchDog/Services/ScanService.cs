using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using WatchDog.Models;

namespace WatchDog.Services
{
    public class ScanService
    {
        private readonly ISignalRepository signalRepository;
        private readonly IWatchDogLogEventRepository watchDogLogEventRepository;
        private readonly IRegionsRepository regionsRepository;
        private readonly IJurisdictionRepository jurisdictionRepository;
        private readonly IAreaRepository areaRepository;
        private readonly IUserRegionRepository userRegionRepository;
        private readonly IUserJurisdictionRepository userJurisdictionRepository;
        private readonly IUserAreaRepository userAreaRepository;

        private readonly UserManager<ApplicationUser> userManager;
        private readonly WatchDogLogService logService;
        private readonly EmailService emailService;
        private readonly ILogger<ScanService> logger;

        public ScanService(
            ISignalRepository signalRepository,
            IWatchDogLogEventRepository watchDogEventRepository,
            IWatchDogLogEventRepository watchDogLogEventRepository,
            IRegionsRepository regionsRepository,
            IJurisdictionRepository jurisdictionRepository,
            IAreaRepository areaRepository,
            IUserRegionRepository userRegionRepository,
            IUserJurisdictionRepository userJurisdictionRepository,
            IUserAreaRepository userAreaRepository,
            UserManager<ApplicationUser> userManager,
            WatchDogLogService logService,
            EmailService emailService,
            ILogger<ScanService> logger)
        {
            this.signalRepository = signalRepository;
            this.watchDogLogEventRepository = watchDogLogEventRepository;
            this.regionsRepository = regionsRepository;
            this.jurisdictionRepository = jurisdictionRepository;
            this.areaRepository = areaRepository;
            this.userRegionRepository = userRegionRepository;
            this.userJurisdictionRepository = userJurisdictionRepository;
            this.userAreaRepository = userAreaRepository;
            this.userManager = userManager;
            this.logService = logService;
            this.emailService = emailService;
            this.logger = logger;
        }
        public async void StartScan(
            LoggingOptions loggingOptions,
            EmailOptions emailOptions)
        {
            //need a version of this that gets the signal version for date of the scan
            var signals = signalRepository.GetLatestVersionOfAllSignals().ToList();
            var errors = await logService.GetWatchDogIssues(loggingOptions, signals);
            if (emailOptions != null)
            {
                SmtpClient smtp = new SmtpClient(emailOptions.EmailServer);
                if (emailOptions.Port.HasValue)
                    smtp.Port = emailOptions.Port.Value;
                if (emailOptions.Password != null)
                    smtp.Credentials = new NetworkCredential(emailOptions.UserName, emailOptions.Password);
                if (emailOptions.EnableSsl.HasValue)
                    smtp.EnableSsl = emailOptions.EnableSsl.Value;

                var regions = regionsRepository.GetList().ToList();
                var userRegions = await userRegionRepository.GetAllAsync();

                var areas = areaRepository.GetList().ToList();
                var userAreas = await userAreaRepository.GetAllAsync();

                var jurisdictions = jurisdictionRepository.GetList().ToList();
                var userJurisdictions = await userJurisdictionRepository.GetAllAsync();

                var users = GetUsersWithWatchDogClaim();


                var recordsFromTheDayBefore = new List<WatchDogLogEvent>();
                if (!emailOptions.EmailAllErrors)
                {
                    //List<WatchDogEvent> RecordsFromTheDayBefore = new List<WatchDogEvent>();
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
                    errors,
                    signals,
                    smtp,
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




    }

    //public class EventsContainer
    //{
    //    public ConcurrentBag<WatchDogLogEvent> ForceOffErrors = new ConcurrentBag<WatchDogLogEvent>();
    //    public ConcurrentBag<WatchDogLogEvent> LowHitCountErrors = new ConcurrentBag<WatchDogLogEvent>();
    //    public ConcurrentBag<WatchDogLogEvent> MaxOutErrors = new ConcurrentBag<WatchDogLogEvent>();
    //    public ConcurrentBag<WatchDogLogEvent> MissingRecords = new ConcurrentBag<WatchDogLogEvent>();
    //    public ConcurrentBag<WatchDogLogEvent> CannotFtpFiles = new ConcurrentBag<WatchDogLogEvent>();
    //    public List<WatchDogLogEvent> RecordsFromTheDayBefore = new List<WatchDogLogEvent>();
    //    public ConcurrentBag<Signal> SignalsNoRecords = new ConcurrentBag<Signal>();
    //    public ConcurrentBag<Signal> SignalsWithRecords = new ConcurrentBag<Signal>();
    //    public ConcurrentBag<WatchDogLogEvent> StuckPedErrors = new ConcurrentBag<WatchDogLogEvent>();
    //}


}