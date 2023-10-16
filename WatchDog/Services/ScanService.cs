using ATSPM.Application.Repositories;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using WatchDog.Models;

namespace WatchDog.Services
{
    public class ScanService
    {
        private readonly ISignalRepository signalRepository;
        private readonly IWatchDogLogEventRepository watchDogEventRepository;
        // private readonly UserManager<ApplicationUser> userManager;
        private readonly WatchDogLogService logService;
        private readonly EmailService emailService;
        private readonly ILogger<ScanService> logger;

        public ScanService(
            ISignalRepository signalRepository,
            IWatchDogLogEventRepository watchDogEventRepository,
            //UserManager<ApplicationUser> userManager,
            WatchDogLogService logService,
            EmailService emailService,
            ILogger<ScanService> logger)
        {
            this.signalRepository = signalRepository;
            this.watchDogEventRepository = watchDogEventRepository;
            //this.userManager = userManager;
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
                    smtp.Credentials = new NetworkCredential(emailOptions.FromEmailAddress, emailOptions.Password);
                if (emailOptions.EnableSsl.HasValue)
                    smtp.EnableSsl = emailOptions.EnableSsl.Value;
                await emailService.CreateAndSendEmail(emailOptions, errors, signals, smtp);
            }
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
