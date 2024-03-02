using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using WatchDog.Models;

namespace WatchDog.Services
{
    public class ScanService
    {
        private readonly ILocationRepository LocationRepository;
        private readonly IWatchDogLogEventRepository watchDogLogEventRepository;
        private readonly IRegionsRepository regionsRepository;
        private readonly IJurisdictionRepository jurisdictionRepository;
        private readonly IAreaRepository areaRepository;
        private readonly IUserRegionRepository userRegionRepository;
        private readonly IUserJurisdictionRepository userJurisdictionRepository;
        private readonly IUserAreaRepository userAreaRepository;

        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly WatchDogLogService logService;
        private readonly EmailService emailService;
        private readonly ILogger<ScanService> logger;

        public ScanService(
            ILocationRepository LocationRepository,
            IWatchDogLogEventRepository watchDogLogEventRepository,
            IRegionsRepository regionsRepository,
            IJurisdictionRepository jurisdictionRepository,
            IAreaRepository areaRepository,
            IUserRegionRepository userRegionRepository,
            IUserJurisdictionRepository userJurisdictionRepository,
            IUserAreaRepository userAreaRepository,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            WatchDogLogService logService,
            EmailService emailService,
            ILogger<ScanService> logger)
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
        }
        public async Task StartScan(
            LoggingOptions loggingOptions,
            EmailOptions emailOptions,
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

            //SmtpClient smtp = new SmtpClient();
            //if (emailOptions != null)
            //{
            //    switch (emailOptions.EmailType)
            //    {
            //        case "smtp":
            //            await smtp.ConnectAsync(emailOptions.EmailServer, emailOptions.Port.Value, emailOptions.EnableSsl.Value ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
            //            await smtp.AuthenticateAsync(emailOptions.UserName, emailOptions.Password);
            //            break;
            //        case "googleApi":
            //            // Use OAuth2. For Gmail, you need to obtain the OAuth2 token beforehand.
            //            // This example assumes you have the OAuth2 token stored in emailOptions.Password for simplicity.
            //            var oauth2 = new SaslMechanismOAuth2(emailOptions.UserName, emailOptions.Password);
            //            await smtp.ConnectAsync(emailOptions.EmailServer, emailOptions.Port.Value, SecureSocketOptions.Auto);
            //            await smtp.AuthenticateAsync(oauth2);
            //            break;
            //        default:
            //            throw new InvalidOperationException("Unsupported email type specified in EmailOptions.");
            //    }
            //}


            var regions = regionsRepository.GetList().ToList();
            var userRegions = userRegionRepository.GetList();

            var areas = areaRepository.GetList().ToList();
            var userAreas = userAreaRepository.GetList().ToList();

            var jurisdictions = jurisdictionRepository.GetList().ToList();
            var userJurisdictions = userJurisdictionRepository.GetList();

            var users = await GetUsersWithWatchDogClaimAsync();


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


        public void SaveErrorLogs(List<WatchDogLogEvent> errors)
        {
            watchDogLogEventRepository.AddRange(errors);
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