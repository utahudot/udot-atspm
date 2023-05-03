using ATSPM.Application.Configuration;
using ATSPM.Application.Repositories;
using ATSPM.Application.Services;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Data;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using ATSPM.Domain.Extensions;
using ATSPM.EventLogUtility;
using ATSPM.EventLogUtility.Commands;
using ATSPM.Infrastructure.Converters;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Repositories;
using ATSPM.Infrastructure.Services.ControllerDecoders;
using ATSPM.Infrastructure.Services.ControllerDownloaders;
using ATSPM.Infrastructure.Services.HostedServices;
using ATSPM.Infrastructure.Services.SignalControllerLoggers;
using Google.Api;
using Google.Cloud.Diagnostics.Common;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks.Dataflow;
using ATSPM.Application.Analysis;
using ATSPM.Data.Enums;
using System.Security.Cryptography.X509Certificates;
using Google.Protobuf.Reflection;
using ATSPM.Application;
using System.Collections.Generic;

//Random r = new Random();

//var list = new List<ControllerEventLog>();

//for(int i = 0; i <= 150; i++)
//{
//    list.Add(new ControllerEventLog() { SignalId = "1001", EventCode = i, EventParam = r.Next(1, 9), Timestamp = DateTime.Now });
//}


//var broadcast = new BroadcastBlock<IEnumerable<ControllerEventLog>>(null);

//var FilteredPreemptionData = new FilteredPreemptionData();
//var FilteredIndicationData = new FilteredIndicationData();
//var FilteredDetectorData = new FilteredDetectorData();
//var FilteredPedPhases = new FilteredPedPhases();
//var FilteredTerminationStatus = new FilteredTerminationStatus();
//var FilteredTerminations = new FilteredTerminations();
//var FilteredSplitsData = new FilteredSplitsData();
//var FilteredPhaseIntervalChanges = new FilteredPhaseIntervalChanges();
//var FilteredCallStatus = new FilteredCallStatus();
//var FilteredPedCalls = new FilteredPedCalls();
//var FilteredPedPhaseData = new FilteredPedPhaseData();
//var FilteredTimingActuationData = new FilteredTimingActuationData();

//broadcast.LinkTo(FilteredPreemptionData);
//broadcast.LinkTo(FilteredIndicationData);
//broadcast.LinkTo(FilteredDetectorData);
//broadcast.LinkTo(FilteredPedPhases);
//broadcast.LinkTo(FilteredTerminationStatus);
//broadcast.LinkTo(FilteredTerminations);
//broadcast.LinkTo(FilteredSplitsData);
//broadcast.LinkTo(FilteredPhaseIntervalChanges);
//broadcast.LinkTo(FilteredCallStatus);
//broadcast.LinkTo(FilteredPedCalls);
//broadcast.LinkTo(FilteredPedPhaseData);
//broadcast.LinkTo(FilteredTimingActuationData);

//var result = new ActionBlock<IEnumerable<ControllerEventLog>>(a =>
//{
//    Console.WriteLine($"-----------------------------------------------------");
//    foreach (var item in a)
//    {
//        Console.WriteLine($"{item.EventCode}");
//    }
//    Console.WriteLine($"-----------------------------------------------------");
//});

//FilteredPreemptionData.LinkTo(result);
//FilteredIndicationData.LinkTo(result);
//FilteredDetectorData.LinkTo(result);
//FilteredPedPhases.LinkTo(result);
//FilteredTerminationStatus.LinkTo(result);
//FilteredTerminations.LinkTo(result);
//FilteredSplitsData.LinkTo(result);
//FilteredPhaseIntervalChanges.LinkTo(result);
//FilteredCallStatus.LinkTo(result);
//FilteredPedCalls.LinkTo(result);
//FilteredPedPhaseData.LinkTo(result);
//FilteredTimingActuationData.LinkTo(result);

//broadcast.Post(list);


var list = new List<ControllerEventLog>();

//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-03 17:46:47.1000000"), EventCode = 105, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-03 17:46:47.1000000"), EventCode = 102, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-03 17:46:54.3000000"), EventCode = 111, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-03 17:46:54.3000000"), EventCode = 104, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-03 17:51:11.9000000"), EventCode = 105, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-03 17:51:11.9000000"), EventCode = 102, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-03 17:51:17.7000000"), EventCode = 107, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-03 17:51:19.2000000"), EventCode = 104, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-03 17:51:21.2000000"), EventCode = 111, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-03 18:01:45.7000000"), EventCode = 105, EventParam = 6 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-03 18:01:45.7000000"), EventCode = 102, EventParam = 6 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-03 18:02:05.0000000"), EventCode = 107, EventParam = 6 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-03 18:02:23.0000000"), EventCode = 104, EventParam = 6 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-03 18:02:23.2000000"), EventCode = 111, EventParam = 6 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-07 10:33:06.2000000"), EventCode = 105, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-07 10:33:06.2000000"), EventCode = 102, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-07 10:33:12.5000000"), EventCode = 104, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-07 10:33:13.6000000"), EventCode = 107, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-07 10:33:16.7000000"), EventCode = 111, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-09 23:56:41.2000000"), EventCode = 105, EventParam = 6 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-09 23:56:41.2000000"), EventCode = 102, EventParam = 6 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-09 23:56:47.4000000"), EventCode = 107, EventParam = 6 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-09 23:57:19.4000000"), EventCode = 104, EventParam = 6 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-09 23:57:19.6000000"), EventCode = 111, EventParam = 6 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-10 10:13:47.5000000"), EventCode = 105, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-10 10:13:47.5000000"), EventCode = 102, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-10 10:13:52.8000000"), EventCode = 104, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-10 10:13:53.4000000"), EventCode = 107, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-10 10:13:56.9000000"), EventCode = 111, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-12 14:07:27.8000000"), EventCode = 105, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-12 14:07:27.8000000"), EventCode = 102, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-12 14:07:43.2000000"), EventCode = 107, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-12 14:07:56.0000000"), EventCode = 104, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-12 14:07:56.2000000"), EventCode = 111, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-14 06:09:14.5000000"), EventCode = 105, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-14 06:09:14.5000000"), EventCode = 102, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-14 06:09:20.7000000"), EventCode = 104, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-14 06:09:21.1000000"), EventCode = 107, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-14 06:09:24.2000000"), EventCode = 111, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-16 20:22:37.9000000"), EventCode = 105, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-16 20:22:37.9000000"), EventCode = 102, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-16 20:22:43.5000000"), EventCode = 107, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-16 20:22:44.1000000"), EventCode = 104, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-16 20:22:46.6000000"), EventCode = 111, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-16 23:59:44.2000000"), EventCode = 105, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-16 23:59:44.2000000"), EventCode = 102, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-16 23:59:50.8000000"), EventCode = 107, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-16 23:59:52.5000000"), EventCode = 104, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-16 23:59:53.9000000"), EventCode = 111, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-20 11:19:45.6000000"), EventCode = 105, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-20 11:19:45.6000000"), EventCode = 102, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-20 11:19:52.9000000"), EventCode = 107, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-20 11:19:53.8000000"), EventCode = 104, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-20 11:19:56.4000000"), EventCode = 111, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-20 11:20:03.6000000"), EventCode = 105, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-20 11:20:03.6000000"), EventCode = 102, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-20 11:20:10.3000000"), EventCode = 107, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-20 11:20:10.8000000"), EventCode = 104, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-20 11:20:13.4000000"), EventCode = 111, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-20 11:22:32.6000000"), EventCode = 105, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-20 11:22:32.6000000"), EventCode = 102, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-20 11:22:39.8000000"), EventCode = 111, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-20 11:22:39.8000000"), EventCode = 104, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-20 11:41:13.7000000"), EventCode = 105, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-20 11:41:13.7000000"), EventCode = 102, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-20 11:41:19.9000000"), EventCode = 111, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-20 11:41:19.9000000"), EventCode = 104, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-20 23:01:33.2000000"), EventCode = 105, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-20 23:01:33.2000000"), EventCode = 102, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-20 23:01:39.5000000"), EventCode = 104, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-20 23:01:39.9000000"), EventCode = 107, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-20 23:01:43.0000000"), EventCode = 111, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-23 19:21:59.8000000"), EventCode = 105, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-23 19:21:59.8000000"), EventCode = 102, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-23 19:22:06.0000000"), EventCode = 111, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-23 19:22:06.0000000"), EventCode = 104, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-23 19:33:54.8000000"), EventCode = 105, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-23 19:33:54.8000000"), EventCode = 102, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-23 19:34:00.1000000"), EventCode = 111, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-23 19:34:00.1000000"), EventCode = 104, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-27 10:09:02.1000000"), EventCode = 105, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-27 10:09:02.1000000"), EventCode = 102, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-27 10:09:07.3000000"), EventCode = 111, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-27 10:09:07.3000000"), EventCode = 104, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-29 15:05:42.5000000"), EventCode = 105, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-29 15:05:42.5000000"), EventCode = 102, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-29 15:05:49.8000000"), EventCode = 111, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-29 15:05:49.8000000"), EventCode = 104, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-30 19:55:47.2000000"), EventCode = 105, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-30 19:55:47.2000000"), EventCode = 102, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-30 19:55:57.4000000"), EventCode = 111, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2020-06-30 19:55:57.4000000"), EventCode = 104, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2021-07-05 14:58:48.3000000"), EventCode = 105, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2021-07-05 14:58:48.3000000"), EventCode = 102, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2021-07-05 14:58:54.5000000"), EventCode = 107, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2021-07-05 14:59:00.5000000"), EventCode = 104, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2021-07-05 14:59:00.7000000"), EventCode = 111, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2021-07-16 21:07:28.4000000"), EventCode = 105, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2021-07-16 21:07:28.4000000"), EventCode = 102, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "6826", Timestamp = DateTime.Parse("2021-07-16 21:07:43.6000000"), EventCode = 107, EventParam = 4 });

//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-03 17:46:47.1000000"), EventCode = 105, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-03 17:46:47.1000000"), EventCode = 102, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-03 17:46:54.3000000"), EventCode = 111, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-03 17:46:54.3000000"), EventCode = 104, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-03 17:51:11.9000000"), EventCode = 105, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-03 17:51:11.9000000"), EventCode = 102, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-03 17:51:17.7000000"), EventCode = 107, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-03 17:51:19.2000000"), EventCode = 104, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-03 17:51:21.2000000"), EventCode = 111, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-03 18:01:45.7000000"), EventCode = 105, EventParam = 6 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-03 18:01:45.7000000"), EventCode = 102, EventParam = 6 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-03 18:02:05.0000000"), EventCode = 107, EventParam = 6 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-03 18:02:23.0000000"), EventCode = 104, EventParam = 6 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-03 18:02:23.2000000"), EventCode = 111, EventParam = 6 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-07 10:33:06.2000000"), EventCode = 105, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-07 10:33:06.2000000"), EventCode = 102, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-07 10:33:12.5000000"), EventCode = 104, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-07 10:33:13.6000000"), EventCode = 107, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-07 10:33:16.7000000"), EventCode = 111, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-09 23:56:41.2000000"), EventCode = 105, EventParam = 6 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-09 23:56:41.2000000"), EventCode = 102, EventParam = 6 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-09 23:56:47.4000000"), EventCode = 107, EventParam = 6 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-09 23:57:19.4000000"), EventCode = 104, EventParam = 6 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-09 23:57:19.6000000"), EventCode = 111, EventParam = 6 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-10 10:13:47.5000000"), EventCode = 105, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-10 10:13:47.5000000"), EventCode = 102, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-10 10:13:52.8000000"), EventCode = 104, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-10 10:13:53.4000000"), EventCode = 107, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-10 10:13:56.9000000"), EventCode = 111, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-12 14:07:27.8000000"), EventCode = 105, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-12 14:07:27.8000000"), EventCode = 102, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-12 14:07:43.2000000"), EventCode = 107, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-12 14:07:56.0000000"), EventCode = 104, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-12 14:07:56.2000000"), EventCode = 111, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-14 06:09:14.5000000"), EventCode = 105, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-14 06:09:14.5000000"), EventCode = 102, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-14 06:09:20.7000000"), EventCode = 104, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-14 06:09:21.1000000"), EventCode = 107, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-14 06:09:24.2000000"), EventCode = 111, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-16 20:22:37.9000000"), EventCode = 105, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-16 20:22:37.9000000"), EventCode = 102, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-16 20:22:43.5000000"), EventCode = 107, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-16 20:22:44.1000000"), EventCode = 104, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-16 20:22:46.6000000"), EventCode = 111, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-16 23:59:44.2000000"), EventCode = 105, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-16 23:59:44.2000000"), EventCode = 102, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-16 23:59:50.8000000"), EventCode = 107, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-16 23:59:52.5000000"), EventCode = 104, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-16 23:59:53.9000000"), EventCode = 111, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-20 11:19:45.6000000"), EventCode = 105, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-20 11:19:45.6000000"), EventCode = 102, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-20 11:19:52.9000000"), EventCode = 107, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-20 11:19:53.8000000"), EventCode = 104, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-20 11:19:56.4000000"), EventCode = 111, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-20 11:20:03.6000000"), EventCode = 105, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-20 11:20:03.6000000"), EventCode = 102, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-20 11:20:10.3000000"), EventCode = 107, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-20 11:20:10.8000000"), EventCode = 104, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-20 11:20:13.4000000"), EventCode = 111, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-20 11:22:32.6000000"), EventCode = 105, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-20 11:22:32.6000000"), EventCode = 102, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-20 11:22:39.8000000"), EventCode = 111, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-20 11:22:39.8000000"), EventCode = 104, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-20 11:41:13.7000000"), EventCode = 105, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-20 11:41:13.7000000"), EventCode = 102, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-20 11:41:19.9000000"), EventCode = 111, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-20 11:41:19.9000000"), EventCode = 104, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-20 23:01:33.2000000"), EventCode = 105, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-20 23:01:33.2000000"), EventCode = 102, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-20 23:01:39.5000000"), EventCode = 104, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-20 23:01:39.9000000"), EventCode = 107, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-20 23:01:43.0000000"), EventCode = 111, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-23 19:21:59.8000000"), EventCode = 105, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-23 19:21:59.8000000"), EventCode = 102, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-23 19:22:06.0000000"), EventCode = 111, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-23 19:22:06.0000000"), EventCode = 104, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-23 19:33:54.8000000"), EventCode = 105, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-23 19:33:54.8000000"), EventCode = 102, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-23 19:34:00.1000000"), EventCode = 111, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-23 19:34:00.1000000"), EventCode = 104, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-27 10:09:02.1000000"), EventCode = 105, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-27 10:09:02.1000000"), EventCode = 102, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-27 10:09:07.3000000"), EventCode = 111, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-27 10:09:07.3000000"), EventCode = 104, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-29 15:05:42.5000000"), EventCode = 105, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-29 15:05:42.5000000"), EventCode = 102, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-29 15:05:49.8000000"), EventCode = 111, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-29 15:05:49.8000000"), EventCode = 104, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-30 19:55:47.2000000"), EventCode = 105, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-30 19:55:47.2000000"), EventCode = 102, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-30 19:55:57.4000000"), EventCode = 111, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2020-06-30 19:55:57.4000000"), EventCode = 104, EventParam = 3 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2021-07-05 14:58:48.3000000"), EventCode = 105, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2021-07-05 14:58:48.3000000"), EventCode = 102, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2021-07-05 14:58:54.5000000"), EventCode = 107, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2021-07-05 14:59:00.5000000"), EventCode = 104, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2021-07-05 14:59:00.7000000"), EventCode = 111, EventParam = 5 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2021-07-16 21:07:28.4000000"), EventCode = 105, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2021-07-16 21:07:28.4000000"), EventCode = 102, EventParam = 4 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("2021-07-16 21:07:43.6000000"), EventCode = 107, EventParam = 4 });

//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:0.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:2.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:6.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:7.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:7.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:10.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:11.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:12.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:13.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:14.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:15.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:16.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:17.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:20.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:20.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:21.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:23.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:34.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:34.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:37.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:40.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:40.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:41.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:44.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:46.0"), EventCode = 1, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:46.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:48.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:50.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:52.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:54.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:00:57.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:3.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:5.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:8.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:10.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:10.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:10.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:12.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:12.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:14.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:14.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:17.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:20.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:21.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:22.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:22.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:23.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:24.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:24.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:27.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:28.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:29.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:29.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:30.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:31.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:33.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:37.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:42.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:43.7"), EventCode = 8, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:44.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:44.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:44.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:47.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:48.8"), EventCode = 9, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:48.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:51.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:52.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:54.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:55.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:55.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:01:58.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:1.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:2.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:3.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:3.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:6.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:7.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:7.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:9.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:9.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:10.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:12.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:12.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:15.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:15.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:17.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:18.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:19.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:19.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:20.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:22.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:23.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:23.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:26.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:26.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:27.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:27.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:30.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:31.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:32.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:33.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:34.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:34.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:36.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:36.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:37.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:39.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:40.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:41.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:42.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:43.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:43.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:44.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:46.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:47.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:48.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:50.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:52.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:54.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:56.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:02:56.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:0.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:0.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:1.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:2.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:3.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:3.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:4.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:5.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:6.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:7.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:7.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:8.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:9.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:9.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:11.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:11.7"), EventCode = 1, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:11.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:12.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:12.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:12.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:13.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:13.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:16.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:16.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:18.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:21.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:23.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:24.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:25.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:29.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:30.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:30.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:34.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:35.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:35.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:37.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:39.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:39.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:40.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:40.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:40.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:41.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:41.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:42.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:44.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:47.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:48.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:49.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:51.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:52.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:54.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:55.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:56.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:03:58.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:0.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:1.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:3.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:7.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:8.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:10.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:12.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:13.7"), EventCode = 8, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:13.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:14.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:16.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:18.8"), EventCode = 9, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:19.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:20.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:21.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:23.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:24.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:26.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:27.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:34.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:34.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:40.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:42.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:45.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:51.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:51.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:52.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:53.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:53.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:55.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:57.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:04:59.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:3.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:3.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:4.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:6.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:6.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:11.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:11.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:12.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:13.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:14.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:16.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:17.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:19.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:20.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:20.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:21.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:22.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:26.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:27.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:28.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:29.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:30.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:31.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:33.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:36.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:38.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:46.0"), EventCode = 1, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:46.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:52.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:54.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:55.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:05:58.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:1.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:2.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:3.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:5.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:7.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:7.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:7.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:8.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:9.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:10.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:11.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:11.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:14.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:16.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:17.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:21.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:21.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:23.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:25.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:28.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:29.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:30.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:33.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:33.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:37.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:38.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:40.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:41.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:42.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:43.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:43.7"), EventCode = 8, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:44.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:47.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:47.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:48.8"), EventCode = 9, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:50.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:50.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:51.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:52.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:54.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:56.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:57.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:58.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:06:59.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:6.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:10.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:11.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:11.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:12.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:12.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:15.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:17.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:17.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:19.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:20.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:21.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:32.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:32.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:34.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:37.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:37.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:37.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:38.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:39.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:43.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:44.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:50.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:50.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:52.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:52.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:57.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:58.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:59.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:07:59.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:1.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:1.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:3.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:4.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:5.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:6.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:10.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:12.0"), EventCode = 1, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:17.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:18.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:19.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:26.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:26.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:27.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:29.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:29.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:30.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:31.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:31.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:31.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:32.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:32.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:34.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:35.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:37.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:38.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:42.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:42.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:42.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:44.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:45.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:46.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:47.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:48.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:49.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:49.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:51.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:53.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:54.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:55.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:55.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:58.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:08:59.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:0.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:0.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:1.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:2.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:3.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:6.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:7.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:11.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:12.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:13.7"), EventCode = 8, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:14.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:16.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:18.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:18.8"), EventCode = 9, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:20.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:25.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:26.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:29.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:29.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:30.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:32.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:32.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:35.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:35.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:36.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:37.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:37.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:38.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:39.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:39.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:41.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:41.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:41.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:42.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:45.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:48.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:49.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:50.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:52.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:52.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:53.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:54.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:54.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:55.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:56.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:56.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:57.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:09:59.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:0.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:1.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:1.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:2.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:2.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:11.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:11.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:12.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:13.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:16.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:16.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:16.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:17.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:17.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:19.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:20.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:21.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:24.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:27.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:27.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:30.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:32.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:35.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:41.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:42.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:43.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:43.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:45.9"), EventCode = 1, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:48.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:52.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:52.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:53.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:54.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:55.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:57.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:59.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:10:59.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:11:2.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:11:2.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:11:2.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:11:4.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:11:6.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:11:7.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:11:8.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:11:11.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:11:13.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:11:15.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:11:17.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:11:17.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:11:18.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:11:20.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:11:24.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:11:25.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:11:28.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:11:31.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:11:43.7"), EventCode = 8, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:11:46.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:11:48.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:11:48.8"), EventCode = 9, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:11:49.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:11:49.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:11:54.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:11:54.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:11:57.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:0.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:0.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:5.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:5.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:7.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:7.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:15.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:17.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:17.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:17.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:21.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:21.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:21.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:23.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:26.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:26.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:29.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:29.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:32.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:33.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:34.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:37.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:38.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:40.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:41.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:41.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:42.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:43.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:44.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:45.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:47.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:49.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:50.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:52.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:55.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:55.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:55.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:55.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:57.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:58.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:12:59.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:0.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:0.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:3.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:3.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:12.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:13.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:15.1"), EventCode = 1, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:17.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:17.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:19.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:23.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:24.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:27.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:28.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:28.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:29.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:29.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:31.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:33.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:34.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:36.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:37.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:38.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:40.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:41.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:44.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:45.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:46.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:48.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:48.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:49.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:51.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:52.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:53.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:53.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:53.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:55.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:57.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:57.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:58.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:13:59.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:0.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:1.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:2.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:3.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:4.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:4.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:5.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:7.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:8.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:10.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:11.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:12.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:12.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:13.7"), EventCode = 8, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:14.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:15.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:17.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:18.8"), EventCode = 9, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:19.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:21.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:22.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:22.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:23.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:25.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:25.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:26.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:26.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:31.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:32.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:34.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:37.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:37.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:37.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:39.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:40.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:42.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:54.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:55.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:56.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:56.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:56.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:57.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:57.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:14:58.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:0.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:1.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:5.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:5.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:7.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:7.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:8.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:9.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:10.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:10.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:12.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:13.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:13.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:14.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:17.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:17.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:19.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:21.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:21.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:22.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:22.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:23.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:23.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:25.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:26.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:26.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:26.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:27.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:28.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:29.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:31.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:32.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:32.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:34.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:34.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:36.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:37.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:38.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:39.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:41.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:42.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:42.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:45.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:45.7"), EventCode = 1, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:47.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:50.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:51.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:52.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:53.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:53.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:54.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:55.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:55.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:56.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:58.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:59.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:15:59.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:0.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:1.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:2.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:2.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:2.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:3.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:4.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:5.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:6.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:8.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:9.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:11.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:15.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:16.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:17.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:17.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:19.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:20.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:22.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:25.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:26.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:26.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:27.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:30.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:31.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:33.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:34.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:36.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:38.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:39.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:40.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:42.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:42.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:43.7"), EventCode = 8, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:46.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:48.8"), EventCode = 9, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:48.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:50.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:50.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:51.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:53.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:54.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:55.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:56.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:56.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:57.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:16:58.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:17:1.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:17:5.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:17:6.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:17:7.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:17:29.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:17:32.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:17:32.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:17:33.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:17:34.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:17:35.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:17:36.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:17:36.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:17:39.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:17:43.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:17:43.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:17:49.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:17:50.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:17:52.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:17:52.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:17:52.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:17:54.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:17:57.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:17:58.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:2.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:15.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:16.0"), EventCode = 1, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:22.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:23.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:24.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:24.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:25.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:26.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:28.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:30.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:31.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:34.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:37.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:38.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:39.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:40.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:43.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:44.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:45.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:46.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:46.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:46.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:48.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:50.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:52.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:53.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:54.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:55.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:55.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:58.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:59.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:18:59.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:0.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:0.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:1.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:3.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:4.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:6.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:6.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:7.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:7.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:8.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:10.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:11.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:12.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:13.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:13.7"), EventCode = 8, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:14.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:15.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:16.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:18.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:18.8"), EventCode = 9, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:19.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:19.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:20.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:20.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:23.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:23.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:23.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:34.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:37.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:38.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:39.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:41.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:42.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:45.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:46.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:47.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:49.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:49.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:49.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:52.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:53.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:55.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:57.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:57.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:19:59.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:0.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:2.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:2.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:4.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:4.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:4.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:7.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:9.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:9.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:11.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:12.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:13.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:14.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:15.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:15.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:17.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:17.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:20.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:22.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:22.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:28.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:30.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:31.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:32.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:32.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:33.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:33.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:38.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:41.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:45.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:46.0"), EventCode = 1, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:46.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:51.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:52.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:52.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:52.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:53.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:56.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:57.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:58.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:20:59.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:0.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:3.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:4.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:5.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:7.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:7.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:10.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:11.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:11.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:11.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:12.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:13.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:18.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:18.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:20.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:21.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:22.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:23.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:24.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:24.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:28.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:30.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:30.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:31.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:35.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:37.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:37.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:38.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:40.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:42.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:43.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:43.7"), EventCode = 8, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:48.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:48.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:48.8"), EventCode = 9, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:51.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:54.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:55.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:21:59.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:0.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:1.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:2.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:2.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:6.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:6.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:6.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:8.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:9.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:11.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:14.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:18.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:18.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:19.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:20.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:20.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:21.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:22.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:23.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:23.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:27.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:28.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:31.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:37.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:37.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:40.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:40.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:42.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:43.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:43.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:45.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:45.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:47.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:47.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:48.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:48.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:54.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:56.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:58.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:59.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:22:59.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:0.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:1.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:1.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:2.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:2.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:5.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:5.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:12.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:15.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:16.0"), EventCode = 1, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:18.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:19.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:25.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:25.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:27.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:27.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:33.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:34.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:38.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:39.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:40.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:40.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:42.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:42.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:43.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:44.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:44.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:45.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:46.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:47.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:48.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:49.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:51.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:55.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:23:56.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:1.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:6.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:13.7"), EventCode = 8, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:15.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:16.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:17.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:18.8"), EventCode = 9, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:19.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:19.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:21.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:25.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:26.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:30.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:30.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:34.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:35.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:40.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:41.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:41.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:42.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:44.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:44.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:47.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:47.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:52.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:52.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:56.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:56.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:57.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:57.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:58.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:24:59.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:1.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:2.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:5.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:8.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:9.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:10.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:10.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:11.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:11.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:13.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:15.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:16.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:19.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:20.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:21.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:22.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:23.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:25.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:25.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:27.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:28.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:28.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:29.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:30.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:31.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:31.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:32.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:32.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:33.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:35.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:37.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:38.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:39.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:39.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:40.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:43.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:44.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:44.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:45.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:46.0"), EventCode = 1, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:47.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:48.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:49.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:49.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:51.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:51.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:52.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:53.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:54.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:55.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:57.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:57.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:25:58.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:0.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:4.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:5.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:7.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:8.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:8.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:9.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:11.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:11.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:13.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:15.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:16.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:19.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:20.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:20.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:21.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:22.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:23.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:24.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:24.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:25.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:28.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:29.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:34.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:34.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:35.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:35.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:36.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:40.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:40.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:40.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:42.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:43.7"), EventCode = 8, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:48.8"), EventCode = 9, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:49.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:51.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:52.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:52.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:54.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:55.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:56.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:57.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:58.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:26:59.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:0.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:1.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:1.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:4.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:4.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:5.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:5.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:6.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:8.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:9.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:10.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:12.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:16.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:21.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:21.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:23.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:23.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:23.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:24.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:24.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:27.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:28.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:32.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:32.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:34.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:34.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:35.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:35.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:40.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:46.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:48.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:49.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:27:56.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:2.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:3.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:5.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:6.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:8.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:9.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:10.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:11.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:11.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:12.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:13.6"), EventCode = 1, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:14.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:15.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:15.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:16.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:17.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:17.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:18.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:20.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:21.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:22.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:26.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:26.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:26.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:27.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:29.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:30.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:31.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:31.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:32.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:35.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:35.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:36.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:37.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:37.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:37.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:40.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:41.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:41.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:42.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:42.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:43.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:44.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:44.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:45.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:48.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:48.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:51.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:52.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:57.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:58.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:28:59.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:0.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:0.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:1.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:2.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:3.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:3.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:5.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:6.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:6.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:7.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:8.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:8.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:9.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:10.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:10.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:11.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:11.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:13.7"), EventCode = 8, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:14.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:14.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:15.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:18.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:18.8"), EventCode = 9, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:19.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:19.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:20.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:21.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:22.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:26.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:27.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:27.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:29.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:30.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:31.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:32.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:33.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:41.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:44.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:45.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:47.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:48.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:52.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:52.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:53.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:53.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:54.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:29:56.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:0.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:0.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:0.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:1.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:4.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:4.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:5.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:6.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:7.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:8.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:10.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:10.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:11.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:12.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:12.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:13.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:15.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:15.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:18.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:19.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:21.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:22.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:24.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:25.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:26.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:28.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:29.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:31.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:36.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:36.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:38.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:38.4"), EventCode = 1, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:39.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:40.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:40.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:42.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:43.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:43.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:45.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:46.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:46.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:47.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:48.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:49.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:52.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:54.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:55.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:57.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:58.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:30:58.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:0.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:0.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:2.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:2.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:5.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:9.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:10.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:12.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:13.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:16.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:17.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:19.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:19.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:21.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:22.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:23.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:24.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:26.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:27.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:27.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:29.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:30.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:32.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:33.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:34.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:35.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:38.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:43.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:43.7"), EventCode = 8, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:44.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:45.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:45.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:47.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:48.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:48.8"), EventCode = 9, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:51.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:53.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:54.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:55.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:56.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:31:58.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:0.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:1.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:1.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:1.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:6.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:7.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:8.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:9.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:9.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:12.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:12.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:15.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:17.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:18.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:19.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:21.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:22.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:32.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:32.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:34.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:36.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:39.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:42.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:43.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:44.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:45.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:46.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:47.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:50.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:51.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:56.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:57.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:57.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:32:58.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:33:15.0"), EventCode = 1, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:33:15.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:33:23.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:33:24.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:33:28.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:33:29.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:33:32.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:33:35.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:33:40.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:33:43.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:33:44.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:33:44.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:33:46.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:33:48.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:33:50.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:33:51.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:33:57.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:33:58.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:34:0.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:34:3.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:34:4.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:34:6.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:34:6.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:34:7.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:34:9.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:34:11.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:34:12.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:34:13.7"), EventCode = 8, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:34:17.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:34:18.8"), EventCode = 9, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:34:32.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:34:32.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:34:33.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:34:35.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:34:38.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:34:39.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:34:42.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:34:45.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:34:54.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:34:58.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:0.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:0.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:1.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:1.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:4.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:6.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:8.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:10.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:14.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:19.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:19.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:21.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:23.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:28.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:30.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:31.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:32.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:39.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:41.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:42.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:42.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:44.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:46.0"), EventCode = 1, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:47.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:49.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:49.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:54.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:58.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:35:59.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:0.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:2.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:7.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:9.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:9.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:10.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:11.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:14.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:19.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:19.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:25.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:25.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:25.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:26.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:30.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:30.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:30.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:34.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:38.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:41.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:43.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:43.7"), EventCode = 8, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:44.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:45.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:47.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:48.8"), EventCode = 9, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:52.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:36:54.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:1.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:1.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:4.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:5.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:6.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:7.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:10.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:13.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:15.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:16.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:17.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:20.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:20.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:20.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:22.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:25.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:27.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:31.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:33.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:39.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:41.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:42.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:44.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:44.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:46.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:49.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:53.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:57.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:37:57.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:4.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:5.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:9.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:10.5"), EventCode = 1, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:11.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:16.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:17.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:28.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:28.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:30.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:31.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:32.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:33.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:34.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:34.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:36.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:37.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:37.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:40.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:41.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:42.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:43.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:45.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:45.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:46.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:48.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:51.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:51.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:54.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:54.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:38:57.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:0.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:0.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:1.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:3.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:4.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:7.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:8.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:9.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:9.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:13.7"), EventCode = 8, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:17.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:17.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:18.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:18.8"), EventCode = 9, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:19.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:20.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:24.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:26.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:27.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:28.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:29.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:30.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:32.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:32.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:34.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:36.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:37.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:37.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:37.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:38.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:40.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:40.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:42.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:43.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:50.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:54.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:54.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:54.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:55.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:56.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:57.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:39:58.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:40:0.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:40:1.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:40:3.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:40:3.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:40:4.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:40:8.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:40:12.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:40:12.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:40:16.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:40:17.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:40:17.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:40:18.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:40:26.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:40:37.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:40:38.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:40:42.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:40:46.0"), EventCode = 1, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:40:48.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:40:49.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:40:51.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:40:53.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:40:56.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:40:56.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:40:57.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:1.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:1.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:1.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:2.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:4.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:5.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:6.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:7.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:8.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:9.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:12.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:14.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:15.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:16.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:18.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:18.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:21.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:21.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:26.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:31.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:31.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:31.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:33.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:34.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:35.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:37.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:37.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:39.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:39.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:40.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:40.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:41.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:41.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:41.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:43.7"), EventCode = 8, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:43.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:44.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:44.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:45.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:46.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:46.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:48.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:48.8"), EventCode = 9, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:51.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:52.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:52.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:53.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:53.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:56.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:41:59.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:0.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:0.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:2.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:5.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:6.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:9.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:13.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:15.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:17.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:17.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:19.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:21.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:22.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:23.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:24.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:25.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:26.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:28.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:31.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:33.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:34.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:36.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:38.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:40.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:41.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:42.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:43.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:44.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:45.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:45.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:46.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:47.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:48.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:50.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:51.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:53.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:54.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:55.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:59.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:42:59.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:1.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:4.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:6.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:7.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:11.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:13.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:15.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:16.0"), EventCode = 1, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:19.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:20.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:21.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:22.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:23.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:24.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:26.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:26.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:28.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:33.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:34.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:35.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:38.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:39.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:43.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:44.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:45.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:45.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:49.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:52.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:53.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:54.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:57.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:58.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:43:59.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:44:2.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:44:4.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:44:6.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:44:9.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:44:10.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:44:12.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:44:12.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:44:12.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:44:13.7"), EventCode = 8, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:44:13.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:44:18.8"), EventCode = 9, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:44:20.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:44:20.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:44:24.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:44:24.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:44:27.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:44:27.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:44:30.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:44:30.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:44:48.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:44:49.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:44:54.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:44:55.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:44:56.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:3.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:5.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:6.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:7.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:8.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:10.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:12.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:12.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:13.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:15.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:17.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:19.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:20.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:20.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:28.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:28.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:30.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:37.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:37.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:40.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:46.0"), EventCode = 1, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:47.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:52.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:52.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:53.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:54.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:55.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:59.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:45:59.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:2.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:3.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:5.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:5.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:6.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:7.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:10.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:12.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:15.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:19.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:20.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:22.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:23.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:25.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:26.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:31.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:36.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:37.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:38.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:40.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:42.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:43.7"), EventCode = 8, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:45.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:46.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:47.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:48.8"), EventCode = 9, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:55.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:56.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:57.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:58.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:46:58.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:47:1.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:47:4.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:47:4.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:47:6.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:47:7.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:47:14.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:47:22.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:47:23.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:47:24.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:47:25.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:47:27.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:47:32.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:47:34.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:47:34.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:47:37.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:47:39.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:47:41.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:47:42.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:47:43.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:47:44.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:47:47.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:47:48.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:47:49.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:47:50.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:47:52.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:47:57.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:1.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:2.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:4.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:6.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:8.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:8.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:9.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:11.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:11.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:12.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:12.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:15.6"), EventCode = 1, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:17.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:18.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:18.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:21.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:23.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:24.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:25.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:25.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:25.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:28.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:30.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:30.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:31.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:35.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:36.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:37.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:39.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:39.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:40.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:42.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:42.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:45.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:46.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:47.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:47.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:48.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:49.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:49.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:52.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:54.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:57.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:58.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:48:59.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:0.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:2.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:3.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:5.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:5.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:6.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:6.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:6.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:7.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:8.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:9.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:9.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:9.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:10.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:11.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:13.7"), EventCode = 8, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:14.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:15.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:18.8"), EventCode = 9, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:20.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:22.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:24.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:29.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:30.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:34.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:34.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:40.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:41.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:42.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:46.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:46.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:49.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:49:52.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:4.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:4.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:8.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:12.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:12.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:12.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:13.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:14.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:16.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:16.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:17.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:18.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:19.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:22.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:25.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:26.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:26.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:28.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:29.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:29.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:32.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:33.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:33.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:36.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:37.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:41.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:42.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:43.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:43.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:43.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:44.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:46.0"), EventCode = 1, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:46.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:47.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:47.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:48.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:49.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:50.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:51.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:53.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:54.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:50:58.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:1.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:6.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:8.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:9.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:12.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:12.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:12.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:12.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:15.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:15.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:16.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:16.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:17.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:17.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:19.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:21.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:22.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:22.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:23.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:25.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:25.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:26.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:27.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:28.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:29.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:32.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:33.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:35.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:35.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:41.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:42.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:43.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:43.7"), EventCode = 8, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:44.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:44.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:45.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:45.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:46.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:46.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:48.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:48.8"), EventCode = 9, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:51.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:54.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:54.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:55.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:56.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:57.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:57.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:58.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:51:59.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:0.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:1.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:2.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:3.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:4.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:7.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:7.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:7.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:8.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:9.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:10.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:15.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:15.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:16.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:17.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:19.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:21.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:24.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:24.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:25.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:25.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:27.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:29.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:30.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:36.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:36.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:37.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:38.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:39.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:41.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:43.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:45.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:45.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:49.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:50.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:51.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:52.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:57.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:58.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:58.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:52:59.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:0.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:2.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:3.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:3.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:5.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:6.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:6.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:9.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:11.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:13.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:13.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:14.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:16.0"), EventCode = 1, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:17.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:24.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:26.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:27.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:28.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:30.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:30.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:31.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:34.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:35.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:36.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:38.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:38.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:40.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:41.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:42.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:45.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:49.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:49.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:50.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:51.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:53.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:53.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:55.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:55.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:58.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:59.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:53:59.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:54:2.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:54:3.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:54:3.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:54:8.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:54:10.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:54:11.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:54:13.7"), EventCode = 8, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:54:15.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:54:17.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:54:17.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:54:17.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:54:18.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:54:18.8"), EventCode = 9, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:54:19.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:54:26.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:54:26.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:54:29.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:54:34.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:54:35.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:54:36.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:54:48.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:54:54.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:54:56.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:54:59.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:54:59.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:0.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:5.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:5.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:9.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:14.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:17.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:20.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:24.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:24.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:25.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:26.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:32.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:33.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:37.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:38.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:41.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:42.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:43.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:43.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:45.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:46.0"), EventCode = 1, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:46.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:47.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:47.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:49.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:53.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:55.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:55.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:59.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:55:59.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:0.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:0.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:1.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:2.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:5.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:6.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:6.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:7.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:11.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:12.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:22.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:25.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:27.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:29.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:31.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:35.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:35.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:39.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:41.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:42.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:43.7"), EventCode = 8, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:44.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:45.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:48.8"), EventCode = 9, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:49.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:53.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:53.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:55.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:56:58.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:57:2.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:57:4.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:57:13.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:57:15.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:57:17.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:57:17.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:57:18.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:57:20.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:57:20.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:57:22.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:57:23.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:57:25.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:57:30.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:57:30.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:57:33.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:57:33.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:57:33.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:57:35.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:57:36.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:57:44.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:57:45.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:57:46.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:57:46.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:57:47.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:57:49.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:12.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:13.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:13.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:13.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:14.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:16.0"), EventCode = 1, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:20.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:22.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:22.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:23.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:25.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:30.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:30.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:32.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:32.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:38.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:41.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:44.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:44.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:45.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:49.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:51.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:53.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:54.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:55.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:56.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:57.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:58.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:58.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:58:59.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:1.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:2.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:4.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:5.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:5.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:8.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:8.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:9.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:10.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:12.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:13.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:13.7"), EventCode = 8, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:16.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:18.8"), EventCode = 9, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:20.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:22.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:24.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:24.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:27.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:32.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:33.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:34.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:35.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:36.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:39.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:45.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:55.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 8:59:59.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 9:00:4.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 9:00:5.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 9:00:6.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 9:00:9.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 9:00:10.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 9:00:12.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 9:00:13.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 9:00:16.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 9:00:17.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 9:00:29.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 9:00:31.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 9:00:38.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 9:00:41.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 9:00:41.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 9:00:41.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 9:00:46.0"), EventCode = 1, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 9:00:46.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 9:00:47.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 9:00:47.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 9:00:50.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 9:00:50.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 9:00:53.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 9:00:53.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 9:00:53.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 9:00:54.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 9:00:55.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "7115", Timestamp = DateTime.Parse("4/17/2023 9:00:58.7"), EventCode = 82, EventParam = 2 });









//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:56:48.8"), EventCode = 9, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:56:49.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:56:53.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:56:53.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:56:55.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:56:58.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:57:2.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:57:4.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:57:13.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:57:15.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:57:17.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:57:17.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:57:18.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:57:20.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:57:20.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:57:22.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:57:23.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:57:25.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:57:30.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:57:30.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:57:33.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:57:33.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:57:33.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:57:35.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:57:36.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:57:44.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:57:45.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:57:46.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:57:46.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:57:47.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:57:49.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:12.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:13.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:13.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:13.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:14.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:16.0"), EventCode = 1, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:20.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:22.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:22.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:23.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:25.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:30.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:30.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:32.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:32.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:38.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:41.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:44.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:44.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:45.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:49.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:51.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:53.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:54.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:55.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:56.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:57.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:58.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:58.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:58:59.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:1.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:2.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:4.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:5.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:5.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:8.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:8.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:9.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:10.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:12.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:13.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:13.7"), EventCode = 8, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:16.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:18.8"), EventCode = 9, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:20.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:22.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:24.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:24.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:27.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:32.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:33.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:34.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:35.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:36.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:39.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:45.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:55.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 8:59:59.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:00:4.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:00:5.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:00:6.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:00:9.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:00:10.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:00:12.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:00:13.4"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:00:16.8"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:00:17.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:00:29.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:00:31.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:00:38.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:00:41.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:00:41.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:00:41.6"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:00:46.0"), EventCode = 1, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:00:46.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:00:47.0"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:00:47.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:00:50.2"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:00:50.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:00:53.1"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:00:53.3"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:00:53.7"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:00:54.9"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:00:55.5"), EventCode = 82, EventParam = 2 });
//list.Add(new ControllerEventLog() { SignalId = "1001", Timestamp = DateTime.Parse("4/17/2023 9:00:58.7"), EventCode = 82, EventParam = 2 });






var detectorEvent = new DetectorEvent() {
    Detector = new Detector() { DistanceFromStopBar = 340, LatencyCorrection = 1.2, Approach = new Approach() { Mph = 45 } },
    EventLogs = list 
};

var IdentifyandAdjustVehicleActivations = new IdentifyandAdjustVehicleActivations();


List<Vehicle> vehicles = IdentifyandAdjustVehicleActivations.ExecuteAsync(detectorEvent).Result.ToList();


var redCycles = GetRedCycles(list);

foreach (var cycleGroup in redCycles)
{
    foreach (var cycle in cycleGroup)
    {
        Console.WriteLine($"{cycle}");
    }
}


//foreach (var v in vehicles)
//{
//    var cycle = redCycles.FirstOrDefault(f => v.TimeStamp >= f.StartTime && v.TimeStamp <= f.EndTime);

//    v.RedToRedCycle = cycle;

//    Console.WriteLine($"{v}");
//}


//var test1 = new CalculateDwellTime();

//var action = new ActionBlock<IEnumerable<PreempDetailValueBase>>(a =>
//{
//    Console.WriteLine($"{a.Count()}------------------------------------------------------------------------");

//    foreach (var item in a)
//    {
//        Console.WriteLine($"{item.GetType().Name} - {item.SignalId} - {item.PreemptNumber} - {item.Start} - {item.End} - {item.Seconds}");
//    }
//});

//test1.LinkTo(action, new DataflowLinkOptions() { PropagateCompletion = true });

//test1.Post(list);
//test1.Complete();

//test1.ExecuteAsync(list);

//var original = DateTime.Parse("4/17/2023 8:00:0.2");
//var adjusted = AtspmMath.AdjustTimeStamp(original, 45, 340, 1.2);

//Console.WriteLine($"original: {original:yyyy-MM-dd'T'HH:mm:ss.f}");
//Console.WriteLine($"adjusted: {adjusted:yyyy-MM-dd'T'HH:mm:ss.f}");
//Console.WriteLine($"difference: {adjusted - original}");



//PreemptionDetailsWorkflow PreemptionDetailsWorkflow = new PreemptionDetailsWorkflow();

//await foreach (var item in PreemptionDetailsWorkflow.Execute(list, default, default))
//{
//    Console.WriteLine($"{item}");
//}


//Console.WriteLine($"{PreemptionDetailsWorkflow.IsInitialized}");

//ArrivalOnRed(list);

Console.ReadLine();

IEnumerable<IEnumerable<RedToRedCycle>> GetRedCycles(IEnumerable<ControllerEventLog> logs)
{
    var preFilter = logs.Where(l => l.EventCode == 1 || l.EventCode == 8 || l.EventCode == 9)
        .OrderBy(o => o.Timestamp)
        .GroupBy(g => g.SignalId)
        .SelectMany(s => s.GroupBy(g => g.EventParam).Select(s => s)
        .Where((w, i) => i <= w.Count() - 3 && w.EventCode == 9 && w[i + 1].EventCode == 1 && w[i + 2].EventCode == 8 && w[i + 3].EventCode == 9)
            .Select((s, i) => new { s, i = w.IndexOf(s) })
            .Select(s => w.Skip(s.i).Take(4))
            .Select(s => new RedToRedCycle()
            {
                StartTime = s.ElementAt(0).Timestamp,
                EndTime = s.ElementAt(3).Timestamp,
                GreenEvent = s.ElementAt(1).Timestamp,
                YellowEvent = s.ElementAt(2).Timestamp,
                Phase = s.ElementAt(0).EventParam,
                EventLogs = logs.Where(l => l.EventCode != 1 && l.EventCode != 8 && l.EventCode != 9).ToList()
            }));

    return preFilter;
    //foreach (var signal in preFilter)
    //{
    //    var result = signal.Where((w, i) => i <= signal.Count - 3 && w.EventCode == 9 && signal[i + 1].EventCode == 1 && signal[i + 2].EventCode == 8 && signal[i + 3].EventCode == 9)
    //        .Select((s, i) => new { s, i = signal.IndexOf(s) })
    //        .Select(s => signal.Skip(s.i).Take(4))
    //        .Select(s => new RedToRedCycle()
    //        {
    //            StartTime = s.ElementAt(0).Timestamp,
    //            EndTime = s.ElementAt(3).Timestamp,
    //            GreenEvent = s.ElementAt(1).Timestamp,
    //            YellowEvent = s.ElementAt(2).Timestamp,
    //            Phase = s.ElementAt(0).EventParam,
    //            EventLogs = logs.Where(l => l.EventCode != 1 && l.EventCode != 8 && l.EventCode != 9).ToList()
    //        });

    //    yield return result;
    //}

    //var result = preFilter.Where((w, i) => i <= preFilter.Count - 3 && w.EventCode == 9 && preFilter[i + 1].EventCode == 1 && preFilter[i + 2].EventCode == 8 && preFilter[i + 3].EventCode == 9)
    //    .Select((s, i) => new { s, i = preFilter.IndexOf(s) })
    //    .Select(s => preFilter.Skip(s.i).Take(4))
    //    .Select(s => new RedToRedCycle() 
    //    { 
    //        StartTime = s.ElementAt(0).Timestamp, 
    //        EndTime = s.ElementAt(3).Timestamp,
    //        GreenEvent = s.ElementAt(1).Timestamp, 
    //        YellowEvent = s.ElementAt(2).Timestamp,
    //        Phase = s.ElementAt(0).EventParam,
    //        EventLogs = logs.Where(l => l.EventCode != 1 && l.EventCode != 8 && l.EventCode != 9).ToList()
    //    });

    //return result;
}

IEnumerable<ControllerEventLog[]> ArrivalOnRed(IEnumerable<ControllerEventLog> logs)
{
    var result = new List<ControllerEventLog[]>();
    
    var preFilter = logs.OrderBy(o => o.Timestamp).ToList();
    int i = 0;

    while (i <= preFilter.Count)
    {
        var red = preFilter.FindIndex(i, f => f.EventCode == 9);

        if (red >= 0)
        {
            var green = preFilter.FindIndex(red, f => f.EventCode == 1);
            //var yellow = preFilter.FindIndex(i, f => f.EventCode == 8);

            Console.BackgroundColor = ConsoleColor.Black;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Red {preFilter.ElementAt(red).Timestamp}------------------------------------------------------------------");

            Console.ForegroundColor = ConsoleColor.White;
            foreach (var item in preFilter.GetRange(red, green - red).Where(w => w.EventCode == 82))
            {
                Console.WriteLine($"{item} --- Delay: {(preFilter.ElementAt(green).Timestamp - item.Timestamp).TotalSeconds}");
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Green {preFilter.ElementAt(green).Timestamp}------------------------------------------------------------------\n");

            result.Add(preFilter.GetRange(red, green - red).Where(w => w.EventCode == 82).ToArray());

            i = +green;
        }
        else
        {
            i = preFilter.Count;
        }
    }

    return result;
}






















//var rootCmd = new EventLogCommands();
//var cmdBuilder = new CommandLineBuilder(rootCmd);
//cmdBuilder.UseDefaults();

//cmdBuilder.UseHost(a =>
//{
//    return Host.CreateDefaultBuilder(a)
//    .UseConsoleLifetime()
//    .ConfigureLogging((h, l) =>
//    {
//        //TODO: add a GoogleLogger section
//        //LoggingServiceOptions GoogleOptions = h.Configuration.GetSection("GoogleLogging").Get<LoggingServiceOptions>();
//        //TODO: remove this to an extension method
//        //DOTNET_ENVIRONMENT = Development,GOOGLE_APPLICATION_CREDENTIALS = M:\My Drive\ut-udot-atspm-dev-023438451801.json
//        if (h.Configuration.GetValue<bool>("UseGoogleLogger"))
//        {
//            l.AddGoogle(new LoggingServiceOptions
//            {
//                ProjectId = "1022556126938",
//                //ProjectId = "869261868126",
//                ServiceName = AppDomain.CurrentDomain.FriendlyName,
//                Version = Assembly.GetEntryAssembly().GetName().Version.ToString(),
//                Options = LoggingOptions.Create(LogLevel.Warning, AppDomain.CurrentDomain.FriendlyName)
//            });
//        }
//    })
//    .ConfigureServices((h, s) =>
//    {
//        s.AddLogging();

//        s.AddATSPMDbContext(h);

//        //repositories
//        s.AddScoped<ISignalRepository, SignalEFRepository>();
//        s.AddScoped<IControllerEventLogRepository, ControllerEventLogEFRepository>();
//        //s.AddScoped<IControllerEventLogRepository, ControllerEventLogFileRepository>();

//        //s.AddTransient<IFileTranscoder, JsonFileTranscoder>();
//        //s.AddTransient<IFileTranscoder, ParquetFileTranscoder>();
//        s.AddTransient<IFileTranscoder, CompressedJsonFileTranscoder>();

//        //downloader clients
//        s.AddTransient<IHTTPDownloaderClient, HttpDownloaderClient>();
//        s.AddTransient<IFTPDownloaderClient, FluentFTPDownloaderClient>();
//        s.AddTransient<ISFTPDownloaderClient, SSHNetSFTPDownloaderClient>();

//        //downloaders
//        s.AddScoped<ISignalControllerDownloader, ASC3SignalControllerDownloader>();
//        s.AddScoped<ISignalControllerDownloader, CobaltSignalControllerDownloader>();
//        s.AddScoped<ISignalControllerDownloader, MaxTimeSignalControllerDownloader>();
//        s.AddScoped<ISignalControllerDownloader, EOSSignalControllerDownloader>();
//        s.AddScoped<ISignalControllerDownloader, NewCobaltSignalControllerDownloader>();

//        //decoders
//        s.AddScoped<ISignalControllerDecoder, ASCSignalControllerDecoder>();
//        s.AddScoped<ISignalControllerDecoder, MaxTimeSignalControllerDecoder>();

//        //SignalControllerLogger
//        //s.AddScoped<ISignalControllerLoggerService, CompressedSignalControllerLogger>();
//        s.AddScoped<ISignalControllerLoggerService, LegacySignalControllerLogger>();

//        //controller logger configuration
//        s.Configure<SignalControllerLoggerConfiguration>(h.Configuration.GetSection(nameof(SignalControllerLoggerConfiguration)));

//        //downloader configurations
//        s.ConfigureSignalControllerDownloaders(h);

//        //decoder configurations
//        s.ConfigureSignalControllerDecoders(h);

//        s.Configure<FileRepositoryConfiguration>(h.Configuration.GetSection("FileRepositoryConfiguration"));

//        //command options
//        //if (cmd is ICommandOption<EventLogLoggingConfiguration> cmdOpt)
//        //{
//        //    s.AddSingleton(cmdOpt.GetOptionsBinder());
//        //    s.AddOptions<EventLogLoggingConfiguration>().BindCommandLine();

//        //    var opt = cmdOpt.GetOptionsBinder().CreateInstance(h.GetInvocationContext().BindingContext) as EventLogLoggingConfiguration;

//        //    //s.PostConfigureAll<SignalControllerDownloaderConfiguration>(o => o.LocalPath = opt.Path.FullName);
//        //    //s.PostConfigureAll<SignalControllerDownloaderConfiguration>(o => o.PingControllerToVerify = h.GetInvocationContext().ParseResult.GetValueForArgument(cmd.PingControllerArg));
//        //    //s.PostConfigureAll<SignalControllerDownloaderConfiguration>(o => o.DeleteFile = h.GetInvocationContext().ParseResult.GetValueForArgument(cmd.DeleteLocalFileArg));
//        //}

//        ////hosted services
//        //s.AddHostedService<SignalLoggerUtilityHostedService>();
//        //s.AddHostedService<TestSignalLoggerHostedService>();

//        //s.PostConfigureAll<SignalControllerDownloaderConfiguration>(o => o.LocalPath = s.configurall);
//    });
//},
//h =>
//{
//    var cmd = h.GetInvocationContext().ParseResult.CommandResult.Command;

//    h.ConfigureServices((h, s) =>
//    {
//        if (cmd is ICommandOption opt)
//        {
//            opt.BindCommandOptions(s);
//        }
//    });
//});

//var cmdParser = cmdBuilder.Build();
//await cmdParser.InvokeAsync(args);

public class TestExtractLogHostedService : IHostedService
{
    private readonly ILogger _log;
    private IServiceProvider _serviceProvider;
    private IOptions<EventLogExtractConfiguration> _options;

    public TestExtractLogHostedService(ILogger<TestExtractLogHostedService> log, IServiceProvider serviceProvider, IOptions<EventLogExtractConfiguration> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => Console.WriteLine($"StartAsync Cancelled..."));

        _serviceProvider.PrintHostInformation();

        _log.LogInformation("Extraction Path: {path}", _options.Value.Path);
        _log.LogInformation("Extraction File Formate: {format}", _options.Value.FileFormat);

        try
        {
            using (var scope = _serviceProvider.CreateAsyncScope())
            {
                foreach (var s in _options.Value.Dates)
                {
                    _log.LogInformation("Extracting Event Logs for Date(s): {date}", s.ToString("dd/MM/yyyy"));
                }

                if (_options.Value.Included != null)
                {
                    foreach (var s in _options.Value.Included)
                    {
                        _log.LogInformation("Including Event Logs for Signal(s): {signal}", s);
                    }
                }

                if (_options.Value.Excluded != null)
                {
                    foreach (var s in _options.Value.Excluded)
                    {
                        _log.LogInformation("Excluding Event Logs for Signal(s): {signal}", s);
                    }
                }
            }
        }
        catch (Exception e)
        {

            _log.LogError("Exception: {e}", e);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => Console.WriteLine($"StopAsync Cancelled..."));

        Console.WriteLine();
        Console.WriteLine($"Operation Completed or Cancelled...");

        return Task.CompletedTask;
    }
}

public class TestSignalLoggerHostedService : IHostedService
{
    private readonly ILogger _log;
    private IServiceProvider _serviceProvider;
    private IOptions<EventLogLoggingConfiguration> _options;

    public TestSignalLoggerHostedService(ILogger<TestSignalLoggerHostedService> log, IServiceProvider serviceProvider, IOptions<EventLogLoggingConfiguration> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => Console.WriteLine($"StartAsync Cancelled..."));

        _serviceProvider.PrintHostInformation();

        _log.LogInformation("Extraction Path: {path}", _options.Value.Path);

        try
        {
            using (var scope = _serviceProvider.CreateAsyncScope())
            {
                foreach (var option in scope.ServiceProvider.GetServices<IOptionsSnapshot<SignalControllerDownloaderConfiguration>>())
                {
                    Console.WriteLine($"------------local path: {option.Value.LocalPath}");
                    Console.WriteLine($"------------ping: {option.Value.PingControllerToVerify}");
                    Console.WriteLine($"------------delete: {option.Value.DeleteFile}");
                }

                if (_options.Value.ControllerTypes != null)
                {
                    foreach (var s in _options.Value.ControllerTypes)
                    {
                        _log.LogInformation("Including Event Logs for Types(s): {type}", s);
                    }
                }

                if (_options.Value.Included != null)
                {
                    foreach (var s in _options.Value.Included)
                    {
                        _log.LogInformation("Including Event Logs for Signal(s): {signal}", s);
                    }
                }

                if (_options.Value.Excluded != null)
                {
                    foreach (var s in _options.Value.Excluded)
                    {
                        _log.LogInformation("Excluding Event Logs for Signal(s): {signal}", s);
                    }
                }
            }
        }
        catch (Exception e)
        {

            _log.LogError("Exception: {e}", e);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => Console.WriteLine($"StopAsync Cancelled..."));

        Console.WriteLine();
        Console.WriteLine($"Operation Completed or Cancelled...");

        return Task.CompletedTask;
    }
}

public class TestSignalInfoHostedService : IHostedService
{
    private readonly ILogger _log;
    private IServiceProvider _serviceProvider;
    private IOptions<EventLogSignalInfoConfiguration> _options;

    public TestSignalInfoHostedService(ILogger<TestSignalInfoHostedService> log, IServiceProvider serviceProvider, IOptions<EventLogSignalInfoConfiguration> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => Console.WriteLine($"StartAsync Cancelled..."));

        _serviceProvider.PrintHostInformation();

        try
        {
            using (var scope = _serviceProvider.CreateAsyncScope())
            {

                if (_options.Value.ControllerTypes != null)
                {
                    foreach (var s in _options.Value.ControllerTypes)
                    {
                        _log.LogInformation("Including Event Logs for Types(s): {type}", s);
                    }
                }

                if (_options.Value.Included != null)
                {
                    foreach (var s in _options.Value.Included)
                    {
                        _log.LogInformation("Including Event Logs for Signal(s): {signal}", s);
                    }
                }

                if (_options.Value.Excluded != null)
                {
                    foreach (var s in _options.Value.Excluded)
                    {
                        _log.LogInformation("Excluding Event Logs for Signal(s): {signal}", s);
                    }
                }
            }
        }
        catch (Exception e)
        {

            _log.LogError("Exception: {e}", e);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => Console.WriteLine($"StopAsync Cancelled..."));

        Console.WriteLine();
        Console.WriteLine($"Operation Completed or Cancelled...");

        return Task.CompletedTask;
    }
}

public class TestAggregationHostedService : IHostedService
{
    private readonly ILogger _log;
    private IServiceProvider _serviceProvider;
    private IOptions<EventLogAggregateConfiguration> _options;

    public TestAggregationHostedService(ILogger<TestAggregationHostedService> log, IServiceProvider serviceProvider, IOptions<EventLogAggregateConfiguration> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => Console.WriteLine($"StartAsync Cancelled..."));

        _serviceProvider.PrintHostInformation();

        try
        {
            using (var scope = _serviceProvider.CreateAsyncScope())
            {
                Console.WriteLine($"------------type: {_options.Value.AggregationType}");
                Console.WriteLine($"------------size: {_options.Value.BinSize}");

                if (_options.Value.Dates != null)
                {
                    foreach (var s in _options.Value.Dates)
                    {
                        _log.LogInformation("Extracting Event Logs for Date(s): {date}", s.ToString("dd/MM/yyyy"));
                    }
                }

                if (_options.Value.Included != null)
                {
                    foreach (var s in _options.Value.Included)
                    {
                        _log.LogInformation("Including Event Logs for Signal(s): {signal}", s);
                    }
                }

                if (_options.Value.Excluded != null)
                {
                    foreach (var s in _options.Value.Excluded)
                    {
                        _log.LogInformation("Excluding Event Logs for Signal(s): {signal}", s);
                    }
                }
            }
        }
        catch (Exception e)
        {

            _log.LogError("Exception: {e}", e);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => Console.WriteLine($"StopAsync Cancelled..."));

        Console.WriteLine();
        Console.WriteLine($"Operation Completed or Cancelled...");

        return Task.CompletedTask;
    }
}
