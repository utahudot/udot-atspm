using ATSPM.Application.Configuration;
using ATSPM.Application.Enums;
using ATSPM.Application.Extensions;
using ATSPM.Application.Models;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Domain.BaseClasses;
using ATSPM.Domain.Common;
using ATSPM.Domain.Exceptions;
using ATSPM.Domain.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
    public class MaxTimeSignalControllerDownloader : ControllerDownloaderBase
    {

        public MaxTimeSignalControllerDownloader(ILogger<MaxTimeSignalControllerDownloader> log, IServiceProvider serviceProvider, IOptions<SignalControllerDownloaderConfiguration> options) : base(log, serviceProvider, options) { }

        #region Properties

        public override SignalControllerType ControllerType => SignalControllerType.MaxTime;

        #endregion

        #region Methods

        public override void Initialize()
        {
        }

        protected override async Task<DirectoryInfo> ExecutionTask(Signal parameter, CancellationToken cancelToken = default, IProgress<int> progress = null)
        {
            //return directory
            DirectoryInfo dir = null;

            //TODO: replace this with options setting
            using (HttpClient client = new HttpClient() { Timeout = TimeSpan.FromSeconds(1) })
            {
                var builder = new UriBuilder("http", parameter.Ipaddress, 80, "v1/asclog/xml/full");
                //builder.Query = "since=09-19-2021 23:59:59.9";
                builder.Query = $"since={new DateTime(2021, 9, 19):MM-dd-yyyy} 00:00:00.0";

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/xml"));

                _log.LogInformation(new EventId(Convert.ToInt32(parameter.SignalId)), "Uri: {uri}", builder.Uri);

                try
                {
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

                            _log.LogInformation(new EventId(Convert.ToInt32(parameter.SignalId)), "Download Succeeded {path}", Path.Combine(dir.FullName, $"{parameter.SignalId}_{DateTime.Now.Ticks}.xml"));
                        }
                        catch (Exception e)
                        {
                            e.LogE();

                            return await Task.FromException<DirectoryInfo>(e);
                        }
                    }
                    else
                    {
                        _log.LogWarning(new EventId(Convert.ToInt32(parameter.SignalId)), "Download Failed {response}", response.StatusCode);

                        return await Task.FromException<DirectoryInfo>(new Exception($"Download Failed {response.StatusCode}"));
                    }
                }
                catch (Exception e)
                {
                    e.LogE();

                    return await Task.FromException<DirectoryInfo>(e);
                }
            }

            return dir;
        }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}
