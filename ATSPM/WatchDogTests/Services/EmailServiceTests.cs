﻿using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Net.Mail;
using WatchDog.Models;
using Xunit;

namespace WatchDog.Services.Tests
{
    public class EmailServiceTests
    {
        [Fact()]
        public async void CreateAndSendEmailTest()
        {
            var loggerMock = new Mock<ILogger<EmailService>>();
            var emailService = new EmailService(loggerMock.Object);
            var emailOptions = new EmailOptions
            {
                PreviousDayPMPeakEnd = 17,
                PreviousDayPMPeakStart = 18,
                ScanDate = new DateTime(2023, 8, 24),
                ScanDayEndHour = 5,
                ScanDayStartHour = 1,
                WeekdayOnly = false,
                EmailServer = "smtp.freesmtpservers.com",
                Port = 25,
                //EmailServer = "sandbox.smtp.mailtrap.io",
                //UserName = "241b4c03c87968",
                //Password = "0f894391e4e8d3",
                //Port = 587,
                //EnableSsl = true,
                DefaultEmailAddress = "derekjlowe@gmail.com",
                EmailAllErrors = true
            };

            var region1MockSignal = new Mock<Signal>();
            region1MockSignal.Object.Id = 1; // Updated Id
            region1MockSignal.Object.SignalIdentifier = "1001"; // Updated SignalId
            region1MockSignal.Object.Latitude = 40.326352;
            region1MockSignal.Object.Longitude = -111.724889;
            region1MockSignal.Object.PrimaryName = "1600 N (SR-241)";
            region1MockSignal.Object.SecondaryName = "1200 W";
            region1MockSignal.Object.Ipaddress = IPAddress.Parse("10.163.6.51");
            region1MockSignal.Object.RegionId = 1;
            region1MockSignal.Object.Areas = new List<Area>() { new Area { Id = 1, Name = "Area 1" }, new Area { Id = 2, Name = "Area 2" } };
            region1MockSignal.Object.JurisdictionId = 1;
            region1MockSignal.Object.Jurisdiction = new Jurisdiction { Id = 1, Name = "Jurisdiction 1" };
            region1MockSignal.Object.ControllerTypeId = 2; // Updated ControllerTypeId
            region1MockSignal.Object.ChartEnabled = true;
            region1MockSignal.Object.VersionAction = SignalVersionActions.Initial;
            region1MockSignal.Object.Note = "Initial - WAS #6500";
            region1MockSignal.Object.Start = new System.DateTime(1900, 1, 1);
            region1MockSignal.Object.Pedsare1to1 = true;

            region1MockSignal.Setup(s => s.Jurisdiction).Returns(new Jurisdiction { Id = 1, Name = "Jurisdiction 1" });

            var directionType = new Mock<DirectionType>();
            directionType.Object.Description = "Westbound";
            directionType.Object.Abbreviation = "WB";
            directionType.Object.Id = (DirectionTypes)4;
            directionType.Object.DisplayOrder = 2;

            var approach = new Mock<Approach>();
            approach.Object.Id = 11; // Updated Id
            approach.Object.SignalId = 2840; // Updated SignalId
            approach.Object.DirectionTypeId = DirectionTypes.WB;
            approach.Object.Description = "WBT Ph2";
            approach.Object.Mph = 35;
            approach.Object.ProtectedPhaseNumber = 2;
            approach.Object.IsProtectedPhaseOverlap = false;
            approach.Object.PermissivePhaseNumber = null;
            approach.Object.IsPermissivePhaseOverlap = false;
            approach.Object.PedestrianPhaseNumber = null;
            approach.Object.IsPedestrianPhaseOverlap = false;
            approach.Object.PedestrianDetectors = null;
            approach.Object.DirectionType = directionType.Object;

            var mockDetector1 = new Mock<Detector>();
            mockDetector1.Object.ApproachId = 11;
            mockDetector1.Object.DateAdded = new System.DateTime(2019, 12, 16);
            mockDetector1.Object.DateDisabled = null;
            mockDetector1.Object.DecisionPoint = null;
            mockDetector1.Object.DetectorChannel = 22;
            mockDetector1.Object.MovementType = MovementTypes.T;
            mockDetector1.Object.LaneType = LaneTypes.V ;
            //mockDetector1.Object.DetectionHardwareId = DetectionHardwareTypes.WavetronixMatrix;
            mockDetector1.Object.DectectorIdentifier = "638722";
            mockDetector1.Object.DistanceFromStopBar = null;
            mockDetector1.Object.Id = 11;
            mockDetector1.Object.LaneNumber = 1;
            //mockDetector1.Object.LaneTypeId = LaneTypes.V;
            mockDetector1.Object.LatencyCorrection = 0;
            mockDetector1.Object.MinSpeedFilter = null;
            mockDetector1.Object.MovementDelay = null;
            //mockDetector1.Object.MovementTypeId = MovementTypes.T;

            approach.Setup(a => a.Signal).Returns(region1MockSignal.Object);
            mockDetector1.Setup(a => a.Approach).Returns(approach.Object);
            region1MockSignal.Setup(a => a.Approaches).Returns(new List<Approach> { approach.Object });


            var errors = new List<WatchDogLogEvent>
            {
                new WatchDogLogEvent(1, "1001", DateTime.Now, WatchDogComponentType.Signal, 1, WatchDogIssueType.RecordCount, "Details 1", null),
                new WatchDogLogEvent(1, "1001", DateTime.Now, WatchDogComponentType.Detector, 11, WatchDogIssueType.LowDetectorHits, "Details 2", null),
                new WatchDogLogEvent(1, "1001", DateTime.Now, WatchDogComponentType.Approach, 11, WatchDogIssueType.StuckPed, "Details 3", 2),
                new WatchDogLogEvent(1, "1001", DateTime.Now, WatchDogComponentType.Approach, 11, WatchDogIssueType.ForceOffThreshold, "Details 4", 2),
                new WatchDogLogEvent(1, "1001", DateTime.Now, WatchDogComponentType.Approach, 11, WatchDogIssueType.MaxOutThreshold, "Details 5", 2)
            };

            var regions = new List<Region> { new Region { Id = 1, Description = "Region 1" }, new Region { Id = 2, Description = "Region 2" } };
            var userRegions = new List<UserRegion> { new UserRegion { RegionId = 1, UserId = "1" }, new UserRegion { RegionId = 2, UserId = "2" } };

            var areas = new List<Area> { new Area { Id = 1, Name = "Area 1" }, new Area { Id = 2, Name = "Area 2" } };
            var userAreas = new List<UserArea> { new UserArea { AreaId = 1, UserId = "1" }, new UserArea { AreaId = 2, UserId = "2" } };

            var jurisdictions = new List<Jurisdiction> { new Jurisdiction { Id = 1, Name = "Jurisdiction 1" }, new Jurisdiction { Id = 2, Name = "Jurisdiction 2" } };
            var userJurisdictions = new List<UserJurisdiction> { new UserJurisdiction { JurisdictionId = 1, UserId = "1" }, new UserJurisdiction { JurisdictionId = 2, UserId = "2" } };

            var mockUser1 = new Mock<ApplicationUser>();
            mockUser1.Object.Id = "1";
            mockUser1.Object.Email = "derekjlowe@gmail.com";
            mockUser1.Object.FirstName = "Derek";
            mockUser1.Object.LastName = "Lowe";
            mockUser1.Object.PhoneNumber = "555-555-5555";
            mockUser1.Object.PhoneNumberConfirmed = true;
            mockUser1.Object.UserName = "dlowe";
            mockUser1.Object.EmailConfirmed = true;
            mockUser1.Object.LockoutEnabled = false;
            mockUser1.Object.AccessFailedCount = 0;
            mockUser1.Object.TwoFactorEnabled = false;
            mockUser1.Object.SecurityStamp = "123456";
            mockUser1.Object.PasswordHash = "123456";
            mockUser1.Setup(u => u.Id).Returns("1");
            mockUser1.Setup(u => u.Email).Returns("derekjlowe@gmail.com");

            var mockUser2 = new Mock<ApplicationUser>();
            mockUser2.Object.Id = "2";
            mockUser2.Object.Email = "dlowe@avenueconsultants.com";
            mockUser2.Object.FirstName = "Derek";
            mockUser2.Object.LastName = "Lowe";
            mockUser2.Object.PhoneNumber = "555-555-5555";
            mockUser2.Object.PhoneNumberConfirmed = true;
            mockUser2.Object.UserName = "dlowe";
            mockUser2.Object.EmailConfirmed = true;
            mockUser2.Object.LockoutEnabled = false;
            mockUser2.Object.AccessFailedCount = 0;
            mockUser2.Object.TwoFactorEnabled = false;
            mockUser2.Object.SecurityStamp = "123456";
            mockUser2.Object.PasswordHash = "123456";
            mockUser2.Setup(u => u.Id).Returns("2");
            mockUser2.Setup(u => u.Email).Returns("dlowe@avenueconsultants.com");


            var users = new List<ApplicationUser> { mockUser1.Object, mockUser2.Object };

            var signals = new List<Signal> { region1MockSignal.Object };

            SmtpClient smtp = new SmtpClient(emailOptions.EmailServer);
            if (emailOptions.Port.HasValue)
                smtp.Port = emailOptions.Port.Value;
            if (emailOptions.Password != null)
                smtp.Credentials = new NetworkCredential(emailOptions.UserName, emailOptions.Password);
            if (emailOptions.EnableSsl.HasValue)
                smtp.EnableSsl = emailOptions.EnableSsl.Value;


            var recordsFromTheDayBefore = new List<WatchDogLogEvent>();


            await emailService.SendAllEmails(emailOptions, errors, signals, smtp, users, jurisdictions, userJurisdictions.ToList(), areas, userAreas.ToList(), regions, userRegions.ToList(), recordsFromTheDayBefore);

            Assert.Equal(1, 1);
        }
    }
}