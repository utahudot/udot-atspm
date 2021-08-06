using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Threading.Channels;
using ControllerLogger.Models;
using ControllerLogger.Data;
using System.Text.Json;
using ControllerLogger.Helpers;
using System.Diagnostics;
using ControllerLogger.Configuration;

namespace ControllerLogger.ServiceHosts
{
    public class PipelineBackgroundServiceTest : BackgroundService
    {
        private readonly ILogger _log;
        private readonly IServiceProvider _serviceProvider;
        protected readonly IOptions<ControllerFTPSettings> _options;

        public PipelineBackgroundServiceTest(ILogger<PipelineBackgroundServiceTest> log, IServiceProvider serviceProvider, IOptions<ControllerFTPSettings> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Progress<PipelineProgress> stepOneProgress = new Progress<PipelineProgress>(p => Console.WriteLine($"StepOneProgress: {p}"));
            Progress<PipelineProgress> stepTwoProgress = new Progress<PipelineProgress>(p => Console.WriteLine($"stepTwoProgress: {p}"));
            Progress<PipelineProgress> stepThreeProgress = new Progress<PipelineProgress>(p => Console.WriteLine($"stepThreeProgress: {p}"));
            //Progress<PipelineProgress> setpFourProgress = new Progress<PipelineProgress>(p => Console.WriteLine($"setpFourProgress: {p}"));




            Task<int> TestSelector(int input) => input switch
            {
                1 => Task4(1),
                2 => Task4(2),
                3 => Task4(3),
                4 => Task4(4),
                _ => Task1(0),
            };

            var plt = new PipelineTask<int, int>(null, i => true);
            plt.Selector = i => TestSelector(i);

            //await plt.Execute(2);



            #region pipelinetaskgroup

            var taskGroup = new PipelineTaskGroup<int, int>();

            taskGroup.AddTask(i => Task1(i), i => i > 1);
            taskGroup.AddTask(i => Task1(i), i => i > 5);
            taskGroup.AddTask(i => Task3(i), i => i < 1);
            taskGroup.AddTask(i => Task4(i), i => i == 100);

            //await taskGroup.Execute(2);

            #endregion

            #region pipelinemanager

            //add pipeline manager
            PipelineManager plm = new PipelineManager(stoppingToken);

            //add steps
            plm.AddStep<int, int>("StepOne", async i => await Task1(i), i => true, i => true);
            plm.AddStep<int, int>("StepTwo", async i => await Task2(i), i => true, i => true);
            plm.AddStep<int, int>("StepThree", async i => await Task3(i), i => true, i => true);
            //plm.AddStep<int, int>("StepFour", async i => await Task4(i), i => true, i => true);

            //add pipes
            plm.AddPipe<int>("PipeOne");
            plm.AddPipe<int>("PipeTwo");
            plm.AddPipe<int>("PipeThree");
            plm.AddPipe<int>("PipeFour");

            if (plm.Pipes["PipeFour"] is PipelinePipe<int> writer)
            {
                foreach (int value in Enumerable.Range(1, 800))
                {
                    await writer.Writer.WriteAsync(value);
                }
            }

            //connect pipes
            plm["StepOne"].Input = plm.Pipes["PipeFour"];
            plm["StepOne"].Output = plm.Pipes["PipeTwo"];

            plm["StepTwo"].Input = plm.Pipes["PipeFour"];
            plm["StepTwo"].Output = plm.Pipes["PipeTwo"];

            plm["StepThree"].Input = plm.Pipes["PipeTwo"];
            plm["StepThree"].Output = plm.Pipes["PipeThree"];

            //plm["StepFour"].Input = plm.Pipes["PipeThree"];
            //plm["StepFour"].Output = plm.Pipes["PipeFour"];

            await plm.Execute(null);

            #endregion
        }

        public async Task<int> Task1(int input)
        {
            Random rnd = new Random();
            //await Task.Delay(TimeSpan.FromSeconds(rnd.Next(1, 5)));

            _log.LogInformation($"{nameof(Task1)} is complete {input}");

            return input * 100;
        }

        public async Task<int> Task2(int input)
        {
            Random rnd = new Random();
            //await Task.Delay(TimeSpan.FromSeconds(rnd.Next(1, 5)));

            _log.LogInformation($"{nameof(Task2)} is complete {input}");
            return input * 200;
        }

        public async Task<int> Task3(int input)
        {
            Random rnd = new Random();
            //await Task.Delay(TimeSpan.FromSeconds(rnd.Next(1, 5)));

            _log.LogInformation($"{nameof(Task3)} is complete {input}");
            return input * 300;
        }

        public async Task<int> Task4(int input)
        {
            Random rnd = new Random();
            //await Task.Delay(TimeSpan.FromSeconds(rnd.Next(1, 5)));

            _log.LogInformation($"{nameof(Task4)} is complete {input}");
            return input * 400;
        }
    }
}
