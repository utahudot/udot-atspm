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
using FluentFTP;
using FluentFTP.Rules;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Utah.Gov.Udot.PipelineManager;

namespace ATSPM.Infrasturcture.Services.ControllerDownloaders
{
    public class SFTPSignalControllerDownloader : ControllerDownloaderBase
    {

        public SFTPSignalControllerDownloader(ILogger<SFTPSignalControllerDownloader> log, IServiceProvider serviceProvider, IOptionsSnapshot<SignalControllerDownloaderConfiguration> options) : base(log, serviceProvider, options) { }

        #region Properties

        public override SignalControllerType ControllerType => SignalControllerType.Cobalt;

        #endregion

        #region Methods

        protected override async IAsyncEnumerable<FileInfo> ExecutionTask(Signal parameter, IProgress<ControllerDownloadProgress> progress = null, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            var connectionInfo = new ConnectionInfo
                (parameter.Ipaddress, 
                parameter.ControllerType.UserName, 
                new PasswordAuthenticationMethod(parameter.ControllerType.UserName, parameter.ControllerType.Password));

            using (var client = new SftpClient(connectionInfo))
            {
                try
                {
                    client.Connect();
                }
                catch (SshException e)
                {
                    _log.LogDebug(new EventId(Convert.ToInt32(parameter.SignalId)), e, "Exception connecting to {ip}", parameter.Ipaddress);

                    progress?.Report(new ControllerDownloadProgress(e));
                }
                catch (SocketException e)
                {
                    _log.LogDebug(new EventId(Convert.ToInt32(parameter.SignalId)), e, "Exception connecting to {ip}", parameter.Ipaddress);

                    progress?.Report(new ControllerDownloadProgress(e));
                }

                if (client.IsConnected)
                {
                    var files = client.ListDirectory("/opt/econolite/set1").Where(f => f.FullName.Contains(".dat")).ToList();

                    foreach (var file in files)
                    {
                        var fileInfo = new FileInfo(Path.Combine(_options.LocalPath, parameter.SignalId, file.Name));
                        fileInfo.Directory.Create();

                        using (FileStream fileStream = fileInfo.Create())
                        {
                            try
                            {
                                await Task.Factory.FromAsync(client.BeginDownloadFile(file.FullName, fileStream), client.EndDownloadFile);
                            }
                            catch (SshException e)
                            {
                                _log.LogDebug(new EventId(Convert.ToInt32(parameter.SignalId)), e, "Exception downloading file {file}", file.FullName);

                                progress?.Report(new ControllerDownloadProgress(e, files.IndexOf(file) + 1, files.Count));
                            }
                            catch (IOException e)
                            {
                                _log.LogDebug(new EventId(Convert.ToInt32(parameter.SignalId)), e, "Exception downloading file {file}", file.FullName);

                                progress?.Report(new ControllerDownloadProgress(e, files.IndexOf(file) + 1, files.Count));
                            }

                            if (fileInfo.Exists)
                            {
                                _log.LogInformation(new EventId(Convert.ToInt32(parameter.SignalId)), "Downloaded {file}", fileInfo.FullName);

                                progress?.Report(new ControllerDownloadProgress(fileInfo, files.IndexOf(file) + 1, files.Count));

                                yield return fileInfo;
                            }
                        }
                    }

                    client.Disconnect();
                }
            }
        }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}
