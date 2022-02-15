using ATSPM.Application.Common;
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
using System.Runtime.CompilerServices;
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

        public MaxTimeSignalControllerDownloader(ILogger<MaxTimeSignalControllerDownloader> log, IServiceProvider serviceProvider, IOptionsSnapshot<SignalControllerDownloaderConfiguration> options) : base(log, serviceProvider, options) { }

        #region Properties

        public override SignalControllerType ControllerType => SignalControllerType.MaxTime;

        #endregion

        #region Methods

        public override void Initialize()
        {
        }

        protected override async IAsyncEnumerable<FileInfo> ExecutionTask(Signal parameter, IProgress<ControllerDownloadProgress> progress = null, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            //TODO: replace this with options setting
            //using (HttpClient client = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) })
            using (HttpClient client = new HttpClient())
            {
                var builder = new UriBuilder("http", parameter.Ipaddress, 80, "v1/asclog/xml/full")
                {
                    //Query = "since=09-19-2021 23:59:59.9",
                    //Query = $"since={new DateTime(2021, 9, 19):MM-dd-yyyy} 00:00:00.0",
                    Query = $"since={DateTime.Now:MM-dd-yyyy} 00:00:00.0"
                };

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/xml"));

                HttpResponseMessage response = new HttpResponseMessage();

                try
                {
                    response = await client.GetAsync(builder.Uri, cancelToken);

                    _log.LogDebug(new EventId(Convert.ToInt32(parameter.SignalId)), "Uri: {uri} : {controllertype}", builder.Uri, parameter.ControllerType.ControllerTypeId);
                }
                catch (Exception e)
                {
                    progress?.Report(new ControllerDownloadProgress(e));

                    _log.LogDebug(new EventId(Convert.ToInt32(parameter.SignalId)), e, "Exception connecting to {ip}", parameter.Ipaddress);
                }

                if (response.IsSuccessStatusCode && response?.Content != null)
                {
                    string data = await response.Content.ReadAsStringAsync();

                    FileInfo file = new FileInfo(Path.Combine(_options.LocalPath, parameter.SignalId, $"{parameter.SignalId}_{DateTime.Now.Ticks}.xml"));
                    
                    XmlDocument xml = new XmlDocument();

                    try
                    {
                        xml.LoadXml(data);

                        xml.Save(file.FullName);
                    }
                    catch (XmlException e)
                    {
                        progress?.Report(new ControllerDownloadProgress(e));

                        _log.LogDebug(new EventId(Convert.ToInt32(parameter.SignalId)), e, "Exception downloading XML Document");
                    }

                    if (file.Exists)
                    {
                        progress?.Report(new ControllerDownloadProgress(file));

                        _log.LogInformation(new EventId(Convert.ToInt32(parameter.SignalId)), "Download Succeeded {path}", file.FullName);

                        yield return file;
                    }   
                }
            }
        }

        //public override void Dispose()
        //{
        //    //throw new NotImplementedException();
        //}

        #endregion
    }
}
