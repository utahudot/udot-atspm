using ATSPM.Application.Configuration;
using ATSPM.Application.Services;
using ATSPM.Data.Models.EventLogModels;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Services.ControllerDecoders;
using ATSPM.Infrastructure.Services.ControllerDownloaders;
using ATSPM.Infrastructure.Services.DownloaderClients;
using Google.Cloud.Diagnostics.Common;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using ATSPM.Domain.Configuration;
using ATSPM.Domain.Services;
using ATSPM.Infrastructure.Services.EmailServices;
using Microsoft.OpenApi.Writers;

namespace ATSPM.LocationControllerLogger
{
    class Program
    {
        static async Task Main(string[] args)
        {

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((h, l) =>
                {
                    //l.SetMinimumLevel(LogLevel.None);

                    //TODO: add a GoogleLogger section
                    //LoggingServiceOptions GoogleOptions = h.Configuration.GetSection("GoogleLogging").Get<LoggingServiceOptions>();
                    //TODO: remove this to an extension method
                    //DOTNET_ENVIRONMENT = Development,GOOGLE_APPLICATION_CREDENTIALS = M:\My Drive\ut-udot-atspm-dev-023438451801.json
                    //if (h.Configuration.GetValue<bool>("UseGoogleLogger"))
                    //{

                    //https://cloud.google.com/dotnet/docs/reference/Google.Cloud.Diagnostics.Common/latest

                    l.AddGoogle(new LoggingServiceOptions
                    {
                        ProjectId = "1022556126938",
                        //ProjectId = "869261868126",
                        ServiceName = AppDomain.CurrentDomain.FriendlyName,
                        Version = Assembly.GetEntryAssembly().GetName().Version.ToString(),
                        Options = LoggingOptions.Create(LogLevel.Information, AppDomain.CurrentDomain.FriendlyName)
                    });
                    //}
                })
                .ConfigureServices((h, s) =>
                {
                    //s.AddGoogleErrorReporting(new ErrorReportingServiceOptions() {
                    //    ProjectId = "1022556126938",
                    //    ServiceName = "ErrorReporting",
                    //    Version = "1.1",
                    //});

                    s.AddLogging();

                    s.AddAtspmDbContext(h);

                    //background services
                    //s.AddHostedService<LoggerBackgroundService>();

                    //repositories
                    s.AddAtspmEFConfigRepositories();
                    s.AddAtspmEFEventLogRepositories();
                    s.AddAtspmEFAggregationRepositories();

                    //s.AddTransient<IFileTranscoder, JsonFileTranscoder>();
                    //s.AddTransient<IFileTranscoder, ParquetFileTranscoder>();
                    //s.AddTransient<IFileTranscoder, CompressedJsonFileTranscoder>();

                    ////downloader clients
                    s.AddTransient<IHTTPDownloaderClient, HttpDownloaderClient>();
                    s.AddTransient<IFTPDownloaderClient, FluentFTPDownloaderClient>();
                    s.AddTransient<ISFTPDownloaderClient, SSHNetSFTPDownloaderClient>();

                    //downloaders
                    //s.AddScoped<IDeviceDownloader, DeviceFtpDownloader>();
                    //s.AddScoped<IDeviceDownloader, CobaltLocationControllerDownloader>();
                    //s.AddScoped<IDeviceDownloader, MaxTimeLocationControllerDownloader>();
                    //s.AddScoped<IDeviceDownloader, EOSSignalControllerDownloader>();
                    //s.AddScoped<IDeviceDownloader, NewCobaltLocationControllerDownloader>();
                    s.AddScoped<IDeviceDownloader, DeviceFtpDownloader>();
                    s.AddScoped<IDeviceDownloader, DeviceSftpDownloader>();
                    s.AddScoped<IDeviceDownloader, DeviceHttpDownloader>();
                    //s.AddScoped<IDeviceDownloader, DeviceSnmpDownloader>();

                    //decoders
                    s.AddScoped<ILocationControllerDecoder<IndianaEvent>, ASCLocationControllerDecoder>();
                    //s.AddScoped<ILocationControllerDecoder<IndianaEvent>, MaxTimeLocationControllerDecoder>();

                    //LocationControllerDataFlow
                    //s.AddScoped<ILocationControllerLoggerService, CompressedLocationControllerLogger>();
                    //s.AddScoped<ILocationControllerLoggerService, LegacyLocationControllerLogger>();

                    //controller logger configuration
                    s.Configure<LocationControllerLoggerConfiguration>(h.Configuration.GetSection(nameof(LocationControllerLoggerConfiguration)));

                    //downloader configurations
                    //s.ConfigureSignalControllerDownloaders(h);
                    //s.Configure<SignalControllerDownloaderConfiguration>(nameof(DeviceFtpDownloader), h.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(DeviceFtpDownloader)}"));
                    //s.Configure<SignalControllerDownloaderConfiguration>(nameof(CobaltLocationControllerDownloader), h.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(CobaltLocationControllerDownloader)}"));
                    //s.Configure<SignalControllerDownloaderConfiguration>(nameof(MaxTimeLocationControllerDownloader), h.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(MaxTimeLocationControllerDownloader)}"));
                    //s.Configure<SignalControllerDownloaderConfiguration>(nameof(EOSSignalControllerDownloader), h.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(EOSSignalControllerDownloader)}"));
                    //s.Configure<SignalControllerDownloaderConfiguration>(nameof(NewCobaltLocationControllerDownloader), h.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(NewCobaltLocationControllerDownloader)}"));

                    //decoder configurations
                    //s.Configure<SignalControllerDecoderConfiguration>(nameof(ASCLocationControllerDecoder), h.Configuration.GetSection($"{nameof(SignalControllerDecoderConfiguration)}:{nameof(ASCLocationControllerDecoder)}"));
                    //s.Configure<SignalControllerDecoderConfiguration>(nameof(MaxTimeLocationControllerDecoder), h.Configuration.GetSection($"{nameof(SignalControllerDecoderConfiguration)}:{nameof(MaxTimeLocationControllerDecoder)}"));

                    //s.Configure<FileRepositoryConfiguration>(h.Configuration.GetSection("FileRepositoryConfiguration"));

                    s.AddHostedService<TestService>();




                    s.AddTransient<IEmailService, SendGridEmailService>();



                    s.PostConfigureAll<SignalControllerDownloaderConfiguration>(o =>
                    {
                        o.LocalPath = "C:\\temp2";
                        o.PingControllerToVerify = false;
                        o.ConnectionTimeout = 3000;
                        o.ReadTimeout = 3000;
                        o.DeleteFile = false;
                    });

                    s.PostConfigureAll<EmailConfiguration>(o =>
                    {
                        o.Key = ***REMOVED***;
                    });


                })

                //.UseConsoleLifetime()
                .Build();

            //host.Services.PrintHostInformation();

            //await host.RunAsync();
            //await host.StartAsync();
            //await host.StopAsync();



            using (var scope = host.Services.CreateScope())
            {
                var email = scope.ServiceProvider.GetService<IEmailService>();

                var to = new List<MailAddress>() {
                    { new MailAddress("christianbaker@utah.gov", "Christian Baker") },
                    { new MailAddress("beatnikthedan@hotmail.com", "Christian Baker")}};

                var result = await email.SendEmailAsync(
                    new MailAddress("AtspmWatchdog@utah.gov", "Atspm Watchdog"),
                    to,
                    "this is the test subject",
                    "this is the test body",
                    false,
                    MailPriority.Low);

                Console.WriteLine(result);
            }




            Console.ReadLine();
        }
    }



    public class SendGridTest
    {
        public async Task SendEmailAsync()
        {
            //var apiKey = Environment.GetEnvironmentVariable("NAME_OF_THE_ENVIRONMENT_VARIABLE_FOR_YOUR_SENDGRID_KEY");
            var apiKey = ***REMOVED***;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("AtspmWatchdog@utah.gov", "Atspm Watchdog");
            var subject = "Hey Buffalo Bill!";
            var to = new EmailAddress("christianbaker@utah.gov", "Christian Baker");
            //var plainTextContent = "and easy to do anywhere, even with C#";
            var htmlContent = "<strong>and easy to do anywhere, even with C#</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlContent);
            var response = await client.SendEmailAsync(msg);

            Console.WriteLine($"response: {response.StatusCode} - {response.IsSuccessStatusCode}");
        }

        //public void SendSmtpEmail()
        //{
        //    var test = new System.Net.Mail.MailMessage();
        //    var test2 = new System.Net.Mail.MailAddress("christianbaker@utah.gov", "Christian Baker");

        //    Console.WriteLine($"{test2.Address} - {test2.User} - {test2.DisplayName} - {test2.Host}");

        //    var test3 = new System.Net.Mail.SmtpClient();
        //    test3.Credentials = new NetworkCredential

        //}
    }

    //public interface IMailService
    //{
    //    Task SendEmailAsync(MailRequest mailRequest);

    //}

    //public class MailRequest
    //{
    //    public string ToEmail { get; set; }
    //    public string Subject { get; set; }
    //    public string Body { get; set; }
    //    public List<IFormFile> Attachments { get; set; }
    //}

    //public class MailService : IMailService
    //{
    //    private readonly MailSettings _mailSettings;
    //    public MailService(IOptions<MailSettings> mailSettings)
    //    {
    //        _mailSettings = mailSettings.Value;
    //    }
    //    public async Task SendEmailAsync(MailRequest mailRequest)
    //    {
    //        var email = new MimeMessage();
    //        email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
    //        email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
    //        email.Subject = mailRequest.Subject;
    //        var builder = new BodyBuilder();
    //        if (mailRequest.Attachments != null)
    //        {
    //            byte[] fileBytes;
    //            foreach (var file in mailRequest.Attachments)
    //            {
    //                if (file.Length > 0)
    //                {
    //                    using (var ms = new MemoryStream())
    //                    {
    //                        file.CopyTo(ms);
    //                        fileBytes = ms.ToArray();
    //                    }
    //                    builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
    //                }
    //            }
    //        }
    //        builder.HtmlBody = mailRequest.Body;
    //        email.Body = builder.ToMessageBody();
    //        using var smtp = new SmtpClient();
    //        smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
    //        smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
    //        await smtp.SendAsync(email);
    //        smtp.Disconnect(true);
    //    }
    //}

    //public class MailSettings
    //{
    //    public string Mail { get; set; }
    //    public string DisplayName { get; set; }
    //    public string Password { get; set; }
    //    public string Host { get; set; }
    //    public int Port { get; set; }
    //}
}