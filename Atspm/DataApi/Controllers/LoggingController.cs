#region license
// Copyright 2024 Utah Departement of Transportation
// for DataApi - Utah.Udot.Atspm.DataApi.Controllers/LoggingController.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Services;
using Utah.Udot.Atspm.ValueObjects;
using Utah.Udot.ATSPM.Infrastructure.Workflows;

namespace Utah.Udot.Atspm.DataApi.Controllers
{
    /// <summary>
    /// logging controller
    /// for querying raw device log data
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class LoggingController : ControllerBase
    {
        private readonly IDeviceRepository _repository;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger _log;
        private const string DownloadWindowDateTimeFormat = "yyyy-MM-dd'T'HH:mm:sszzz";

        /// <inheritdoc/>
        public LoggingController(IDeviceRepository deviceRepository, IServiceScopeFactory serviceScopeFactory, ILogger<LoggingController> log)
        {
            _repository = deviceRepository;
            _serviceScopeFactory = serviceScopeFactory;
            _log = log;
        }

        /// <summary>
        /// Synchronizes event logs for the requested devices.
        /// </summary>
        [HttpPost("SyncDeviceEvents")]
        [Authorize(Policy = "CanEditLocationConfigurations")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<DeviceEventDownload>>> SyncDeviceEventsAsync([FromBody] SyncDeviceEventsRequest request, CancellationToken cancellationToken)
        {
            var (deviceIdList, validationError) = ValidateDeviceIds(request);
            if (validationError != null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid deviceIds",
                    Detail = validationError,
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var devices = _repository.GetList().Where(device => device.LoggingEnabled == true && deviceIdList.Contains(device.Id)).ToList();

            List<DeviceEventDownload> devicesEventDownload = new List<DeviceEventDownload>();

            if (devices == null || !devices.Any())
                return devicesEventDownload;

            var semaphore = new SemaphoreSlim(3); // allow 3 concurrent workflows
            var tasks = new List<Task<DeviceEventDownload>>();

            foreach (var device in devices)
            {
                await semaphore.WaitAsync(cancellationToken);

                var task = Task.Run(async () =>
                {
                    try
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var downloadWindow = GetDownloadWindow(device);
                            var runtimeDevice = CreateRuntimeDeviceForDownloadWindow(device, downloadWindow);
                            await VerifyDeviceConnectionAsync(scope.ServiceProvider, runtimeDevice, cancellationToken);

                            var eventLogRepository = scope.ServiceProvider.GetRequiredService<IEventLogRepository>();

                            List<CompressedEventLogBase> eventsPreWorkflowInitial = await eventLogRepository
                                .GetData(device.Location.LocationIdentifier, downloadWindow.Start, downloadWindow.End, device.Id).ToListAsync();
                            var eventsPreWorkflow = eventsPreWorkflowInitial
                                .SelectMany(s => s.Data)
                                .ToList();

                            var workflow = new DeviceEventLogWorkflow(scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>(), 50000, 1, cancellationToken);

                            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                            await workflow.Input.SendAsync(runtimeDevice, cancellationToken);
                            workflow.Input.Complete();
                            await Task.WhenAll(workflow.Steps.Select(s => s.Completion));

                            List<CompressedEventLogBase> eventsPostWorkflowInitial = await eventLogRepository
                                .GetData(device.Location.LocationIdentifier, downloadWindow.Start, downloadWindow.End, device.Id).ToListAsync();

                            var eventsPostWorkflow = eventsPostWorkflowInitial
                                .SelectMany(s => s.Data)
                                .ToList();

                            return new DeviceEventDownload
                            {
                                DeviceId = device.Id,
                                Ipaddress = device.Ipaddress,
                                DeviceType = device.DeviceType,
                                BeforeWorkflowEventCount = eventsPreWorkflow.Count,
                                AfterWorkflowEventCount = eventsPostWorkflow.Count,
                                ChangeInEventCount = eventsPostWorkflow.Count - eventsPreWorkflow.Count
                            };
                        }
                    }
                    catch (Exception e)
                    {
                        _log.LogError("An error was thrown " + e);
                        return new DeviceEventDownload
                        {
                            DeviceId = device.Id,
                            Ipaddress = device.Ipaddress,
                            DeviceType = device.DeviceType,
                            BeforeWorkflowEventCount = -1,
                            AfterWorkflowEventCount = -1,
                            ChangeInEventCount = -1
                        };
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                tasks.Add(task);
            }

            var results = await Task.WhenAll(tasks);
            devicesEventDownload.AddRange(results);

            //devicesEventDownload.AddRange(results);
            return devicesEventDownload;
        }

        private static (HashSet<int> DeviceIds, string? Error) ValidateDeviceIds(SyncDeviceEventsRequest? request)
        {
            if (request?.DeviceIds == null)
                return (new HashSet<int>(), "deviceIds is required.");

            var deviceIds = request.DeviceIds.Distinct().ToHashSet();
            if (!deviceIds.Any())
                return (deviceIds, "At least one device id is required.");

            var invalidDeviceIds = deviceIds.Where(w => w <= 0).ToList();
            if (invalidDeviceIds.Any())
                return (deviceIds, $"Device ids must be positive integers. Invalid values: {string.Join(", ", invalidDeviceIds)}.");

            return (deviceIds, null);
        }

        private static async Task VerifyDeviceConnectionAsync(IServiceProvider services, Device device, CancellationToken cancellationToken)
        {
            var deviceConfiguration = device.DeviceConfiguration
                ?? throw new InvalidOperationException($"Device {device.Id} does not have a device configuration.");

            var client = services.GetServices<IDownloaderClient>()
                .FirstOrDefault(w => w.Protocol == deviceConfiguration.Protocol)
                ?? throw new InvalidOperationException($"No downloader client is registered for {deviceConfiguration.Protocol}.");

            if (!IPAddress.TryParse(device.Ipaddress, out var ipaddress) || ipaddress == null)
                throw new InvalidOperationException($"Device {device.Id} has an invalid IP address: {device.Ipaddress}");

            var connection = new IPEndPoint(ipaddress, deviceConfiguration.Port);
            var credentials = new NetworkCredential(deviceConfiguration.UserName, deviceConfiguration.Password, ipaddress.ToString());
            var connectionTimeout = deviceConfiguration.ConnectionTimeout;
            var operationTimeout = deviceConfiguration.OperationTimeout;
            var props = deviceConfiguration.ConnectionProperties?.ToDictionary(k => k.Key, k => k.Value.ToString());

            await client.ConnectAsync(connection, credentials, connectionTimeout, operationTimeout, props, cancellationToken);

            if (!client.IsConnected)
                throw new InvalidOperationException($"Device {device.Id} at {device.Ipaddress} did not connect.");

            await client.DisconnectAsync(cancellationToken);
        }

        private static (DateTime Start, DateTime End) GetDownloadWindow(Device device)
        {
            var end = RoundToMinute(DateTime.Now).AddMinutes(-(device.DeviceConfiguration?.LoggingOffset ?? 0));

            return (end.AddHours(-12), end);
        }

        private static Device CreateRuntimeDeviceForDownloadWindow(Device device, (DateTime Start, DateTime End) downloadWindow)
        {
            var runtimeDevice = (Device)device.Clone();
            runtimeDevice.DeviceConfiguration = CreateRuntimeDeviceConfiguration(device.DeviceConfiguration, downloadWindow);

            return runtimeDevice;
        }

        private static DeviceConfiguration? CreateRuntimeDeviceConfiguration(DeviceConfiguration? deviceConfiguration, (DateTime Start, DateTime End) downloadWindow)
        {
            if (deviceConfiguration == null)
                return null;

            var runtimeDeviceConfiguration = (DeviceConfiguration)deviceConfiguration.Clone();
            runtimeDeviceConfiguration.Path = RenderDownloadWindowTokens(deviceConfiguration.Path, downloadWindow);
            runtimeDeviceConfiguration.Query = deviceConfiguration.Query?
                .Select(query => RenderDownloadWindowTokens(query, downloadWindow))
                .ToArray();

            return runtimeDeviceConfiguration;
        }

        private static string RenderDownloadWindowTokens(string value, (DateTime Start, DateTime End) downloadWindow)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return Regex.Replace(value, @"\[(StartTime|EndTime)[^\]]*\]", match =>
            {
                var tokenTime = string.Equals(match.Groups[1].Value, "StartTime", StringComparison.Ordinal)
                    ? downloadWindow.Start
                    : downloadWindow.End;

                return new DateTimeOffset(tokenTime).ToString(DownloadWindowDateTimeFormat);
            });
        }

        private static DateTime RoundToMinute(DateTime value)
        {
            return value.AddTicks(-(value.Ticks % TimeSpan.TicksPerMinute));
        }
    }

    public class SyncDeviceEventsRequest
    {
        public IEnumerable<int>? DeviceIds { get; set; }
    }
}
