using ATSPM.Application.Configuration;
using ATSPM.Application.Enums;
using ATSPM.Application.Extensions;
using ATSPM.Application.Models;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Domain.BaseClasses;
using ATSPM.Domain.Common;
using ATSPM.Domain.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ATSPM.Infrasturcture.Services.ControllerDownloaders
{
    public class MaxTimeSignalControllerDownloader : ServiceObjectBase, ISignalControllerDownloader
    {
        public event EventHandler CanExecuteChanged;

        #region Fields

        private readonly ILogger _log;
        private readonly IServiceProvider _serviceProvider;
        protected readonly IOptions<SignalControllerDownloaderConfiguration> _options;

        #endregion

        public MaxTimeSignalControllerDownloader(ILogger<MaxTimeSignalControllerDownloader> log, IServiceProvider serviceProvider, IOptions<SignalControllerDownloaderConfiguration> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

        #region Properties
        public SignalControllerType ControllerType => SignalControllerType.MaxTime;

        #endregion

        #region Methods

        public override void Initialize()
        {
        }

        #region IPipelineExecute<Tin, Tout>

        public bool CanExecute(Signal value)
        {
            //check valid controller type
            return ((int)ControllerType >> value.ControllerType.ControllerTypeId) == 1

            //check valid ipaddress
            && value.Ipaddress.IsValidIPAddress(_options.Value.PingControllerToVerify);
        }

        public async Task<DirectoryInfo> ExecuteAsync(Signal parameter, CancellationToken cancelToken = default)
        {
            return await ExecuteAsync(parameter, cancelToken, default);
        }

        public async Task<DirectoryInfo> ExecuteAsync(Signal parameter, CancellationToken cancelToken = default, IProgress<int> progress = default)
        {
            //TODO: integrate CancellationToken
            //TODO: write out detailed logs

            //return directory
            DirectoryInfo dir = null;

            //_log.LogDebug(new EventId(Convert.ToInt32(parameter.SignalId)), $"Controller Type: {parameter.ControllerType.Description} - {parameter.ControllerType.Ftpdirectory}");

            if (CanExecute(parameter))
            {
                //TODO: replace this with options setting
                using (HttpClient client = new HttpClient() { Timeout = TimeSpan.FromSeconds(1)})
                {
                    var builder = new UriBuilder("http", "10.209.2.120", 80, "v1/asclog/xml/full");
                    //builder.Query = "since=09-19-2021 23:59:59.9";
                    builder.Query = $"since={new DateTime(2021, 9, 19).ToString("MM-dd-yyyy")} 00:00:00.0";

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/xml"));

                    _log.LogInformation(new EventId(Convert.ToInt32(parameter.SignalId)), "Uri: {uri}", builder.Uri);

                    var response = await client.GetAsync(builder.Uri);

                    if (response.IsSuccessStatusCode && response?.Content != null)
                    {
                        string data = await response.Content.ReadAsStringAsync();

                        XmlDocument xml = new XmlDocument();
                        xml.LoadXml(data);

                        dir = new DirectoryInfo(Path.Combine(_options.Value.LocalPath, parameter.SignalId));
                        dir.Create();

                        try
                        {
                            xml.Save(Path.Combine(dir.FullName, $"{parameter.SignalId}_{DateTime.Now.Ticks}.xml"));

                            _log.LogInformation(new EventId(Convert.ToInt32(parameter.SignalId)), "Download Succeeded! {path}", Path.Combine(dir.FullName, $"{parameter.SignalId}_{DateTime.Now.Ticks}.xml"));
                        }
                        catch (Exception e) when (e.LogE()) { }
                    }
                }
            }
            else
            {
                dir ??= new DirectoryInfo(Path.Combine(_options.Value.LocalPath, "DidNotPassCanExecute", $"{parameter.SignalId} - {parameter.ControllerTypeId}"));
                dir.Create();
            }

            //if (dir != null && !dir.Exists)
                //dir.Create();

            return dir;
        }

        #endregion

        Task IExecuteAsync.ExecuteAsync(object parameter)
        {
            if (parameter is Signal p)
                return Task.Run(() => ExecuteAsync(p, default, default));
            return default;
        }

        bool ICommand.CanExecute(object parameter)
        {
            if (parameter is Signal p)
                return CanExecute(p);
            return default;
        }

        void ICommand.Execute(object parameter)
        {
            if (parameter is Signal p)
                Task.Run(() => ExecuteAsync(p, default, default));
        }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}
