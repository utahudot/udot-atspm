#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services.DeviceDownloaders/DeviceDownloader.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this resource except in compliance with the License.
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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using Utah.Udot.Atspm.Common;
using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.Infrastructure.Services.DeviceDownloaders
{
    ///<inheritdoc cref="IDeviceDownloader"/>
    public class DeviceDownloader : ExecutableServiceWithProgressAsyncBase<Device, Tuple<Device, FileInfo>, ControllerDownloadProgress>, IDeviceDownloader
    {
        #region Fields

        private readonly IEnumerable<IDownloaderClient> _clients;
        protected readonly ILogger _log;
        protected readonly DeviceDownloaderConfiguration _options;

        #endregion

        ///<inheritdoc/>
        public DeviceDownloader(IEnumerable<IDownloaderClient> clients, ILogger<IDeviceDownloader> log, IOptionsSnapshot<DeviceDownloaderConfiguration> options) : base(true)
        {
            _clients = clients;
            _log = log;
            _options = options?.Get(GetType().Name) ?? options?.Value;
        }

        #region Methods

        //public override void Initialize()
        //{
        //}

        /// <summary>
        /// Generates a pattern for folder structure used for temp event log storage
        /// </summary>
        /// <param name="value"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public virtual Uri GenerateLocalFilePath(Device value, Uri resource)
        {
            //var path = Path.Combine
            //    (_options.BasePath,
            //    $"{value.Location?.LocationIdentifier} - {value.Location?.PrimaryName}",
            //    value.DeviceType.ToString(),
            //    value.Ipaddress.ToString());


            //    Path.GetFileName(resource));

            //return result;

            return resource;
        }

        ///<inheritdoc/>
        public override bool CanExecute(Device value)
        {
            return value.LoggingEnabled && value?.DeviceConfiguration?.Protocol != TransportProtocols.Unknown;
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidDeviceIpAddressException"></exception>
        /// <exception cref="ExecuteException"></exception>
        public override async IAsyncEnumerable<Tuple<Device, FileInfo>> Execute(Device parameter, IProgress<ControllerDownloadProgress> progress = null, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter), $"Parameter can not be null");

            var client = _clients.FirstOrDefault(w => parameter.DeviceConfiguration.Protocol == w.Protocol);

            //if (CanExecute(parameter) && !cancelToken.IsCancellationRequested)
            if (CanExecute(parameter))
            {
                var logMessages = new DeviceDownloaderLogMessages(_log, parameter);

                if (!IPAddress.TryParse(parameter.Ipaddress, out IPAddress ipaddress) && !ipaddress.IsValidIpAddress())
                {
                    if (_options.Ping ? !await ipaddress.PingIpAddressAsync() : true)
                    {
                        logMessages.InvalidDeviceIpAddressException(ipaddress);

                        throw new InvalidDeviceIpAddressException(parameter);
                    }
                }

                var deviceIdentifier = parameter?.DeviceIdentifier;
                var user = parameter?.DeviceConfiguration?.UserName;
                var password = parameter?.DeviceConfiguration?.Password;
                var path = parameter?.DeviceConfiguration?.Path;
                var query = parameter?.DeviceConfiguration?.Query;
                var connectionTimeout = parameter?.DeviceConfiguration?.ConnectionTimeout ?? 2000;
                var operationTimeout = parameter?.DeviceConfiguration?.OperationTimeout ?? 2000;

                using (client)
                {
                    try
                    {
                        var connection = new IPEndPoint(ipaddress, parameter.DeviceConfiguration.Port);
                        var credentials = new NetworkCredential(user, password, ipaddress.ToString());

                        logMessages.ConnectingToHostMessage(deviceIdentifier, ipaddress);

                        await client.ConnectAsync(connection, credentials, connectionTimeout, operationTimeout, null, cancelToken);
                    }
                    catch (DownloaderClientConnectionException e)
                    {
                        logMessages.ConnectingToHostException(deviceIdentifier, ipaddress, e);
                    }
                    catch (OperationCanceledException e)
                    {
                        logMessages.OperationCancelledException(deviceIdentifier, ipaddress, e);
                    }

                    if (client.IsConnected)
                    {
                        logMessages.ConnectedToHostMessage(deviceIdentifier, ipaddress);

                        IEnumerable<Uri> resources = new List<Uri>();

                        try
                        {
                            logMessages.GettingsResourcesListMessage(deviceIdentifier, ipaddress, path);

                            resources = await client.ListResourcesAsync(path, cancelToken, query);
                        }
                        catch (DownloaderClientListResourcesException e)
                        {
                            logMessages.ResourceListingException(deviceIdentifier, ipaddress, path, e);
                        }
                        catch (DownloaderClientConnectionException e)
                        {
                            logMessages.NotConnectedToHostException(deviceIdentifier, ipaddress, e);
                        }

                        int total = resources.Count();
                        int current = 0;

                        logMessages.ResourceListingMessage(total, deviceIdentifier, ipaddress);

                        foreach (var resource in resources)
                        {
                            var locatlPath = GenerateLocalFilePath(parameter, resource);
                            FileInfo downloadedFile = null;

                            try
                            {
                                logMessages.DownloadingResourceMessage(resource, deviceIdentifier, ipaddress);

                                downloadedFile = await client.DownloadResourceAsync(locatlPath, resource, cancelToken);
                                current++;
                            }
                            catch (DownloaderClientDownloadResourceException e)
                            {
                                logMessages.DownloadResourceException(resource, deviceIdentifier, ipaddress, e);
                            }
                            catch (DownloaderClientConnectionException e)
                            {
                                logMessages.NotConnectedToHostException(deviceIdentifier, ipaddress, e);
                            }
                            catch (OperationCanceledException e)
                            {
                                logMessages.OperationCancelledException(deviceIdentifier, ipaddress, e);
                            }

                            if (_options.DeleteRemoteFile)
                            {
                                try
                                {
                                    logMessages.DeletingResourceMessage(resource, deviceIdentifier, ipaddress);

                                    await client.DeleteResourceAsync(resource, cancelToken);
                                }
                                catch (DownloaderClientDeleteResourceException e)
                                {
                                    logMessages.DeleteResourceException(resource, deviceIdentifier, ipaddress, e);
                                }
                                catch (OperationCanceledException e)
                                {
                                    logMessages.OperationCancelledException(deviceIdentifier, ipaddress, e);
                                }

                                logMessages.DeletedResourceMessage(resource, deviceIdentifier, ipaddress);
                            }

                            //HACK: don't know why files aren't downloading without throwing an error
                            if (downloadedFile != null)
                            {
                                logMessages.DownloadedResourceMessage(resource, deviceIdentifier, ipaddress);

                                progress?.Report(new ControllerDownloadProgress(downloadedFile, current, total));

                                yield return Tuple.Create(parameter, downloadedFile);
                            }
                            else
                            {
                                _log.LogWarning(new EventId(Convert.ToInt32(deviceIdentifier)), "File failed to download on {Location} file name: {file}", deviceIdentifier, resource);
                            }
                        }

                        logMessages.DownloadedResourcesMessage(current, total, deviceIdentifier, ipaddress);

                        try
                        {
                            logMessages.DisconnectingFromHostMessage(deviceIdentifier, ipaddress);

                            await client.DisconnectAsync(cancelToken);
                        }
                        catch (DownloaderClientConnectionException e)
                        {
                            logMessages.DisconnectingFromHostException(deviceIdentifier, ipaddress, e);
                        }
                        catch (OperationCanceledException e)
                        {
                            logMessages.OperationCancelledException(deviceIdentifier, ipaddress, e);
                        }
                    }
                }
            }
            else
            {
                throw new ExecuteException();
            }
        }

        #endregion

        ///<inheritdoc/>
        protected override void DisposeManagedCode()
        {
            foreach (var client in _clients)
            {
                client.Dispose();
            }
        }
    }










    public class StringObjectParser //: IFormattable
    {
        public StringObjectParser(object obj, string query)
        {
            Obj = obj;
            Query = query;
        }

        public object Obj { get; set; }

        public string Query { get; set; }

        public string ParseQuery()
        {
            var output = Query.Split('[', ']').ToArray();

            var builder = new StringBuilder();

            foreach (var o in output)
            {
                var result = o;

                //Console.WriteLine($"o: {o}");

                if (o.StartsWith("DateTime"))
                {
                    result = o.Replace("DateTime", "");


                    //o.Split(':', 2);






                    //HACK: update this!
                    builder.AppendFormat("{0" + result + "}", DateTime.Now.AddHours(-2));













                }

                else if (o.StartsWith(Obj.GetType().Name))
                {
                    var length = Obj.GetType().Name.Length;
                    result = o.Remove(0, length);

                    //result = o.Replace(Obj.GetType().Name, "");

                    builder.AppendFormat(new TestFormatProvider(), "{0" + result + "}", Obj);
                }

                else
                {
                    builder.Append(result);
                }
            }

            return builder.ToString();
        }

        public override string? ToString()
        {
            return ParseQuery();
        }
    }













    public class TestFormatProvider : IFormatProvider
    {
        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(ICustomFormatter))
            {
                return new CapitalisationCustomFormatter();
            }

            Console.WriteLine($"formatType: {formatType}");

            return null;
        }
    }

    public class CapitalisationCustomFormatter : ICustomFormatter
    {
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            Console.WriteLine($"format: {format} - arg: {arg}");

            if (arg.HasProperty(format))
                return arg.GetPropertyValueString(format);

            return format;
        }
    }

    public static class PropertyExtensions
    {
        public static string GetPropertyValueString(this object obj, string propertyName)
        {
            if (obj.HasProperty(propertyName))
            {
                string value = obj.GetType().GetProperty(propertyName).GetValue(obj, null).ToString();

                return value;
            }

            throw new ArgumentException(propertyName, "propertyName");
        }
    }
}
