using ATSPM.Application.Common;
using ATSPM.Application.Configuration;
using ATSPM.Application.Extensions;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Data.Models;
using FluentFTP;
using FluentFTP.Rules;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ATSPM.Infrasturcture.Services.ControllerDownloaders
{
    public class FTPSignalControllerDownloader : ControllerDownloaderBase
    {
        //public FTPSignalControllerDownloader(IFTPDownloaderClient client, ILogger<FTPSignalControllerDownloader> log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> options) : base(client, log, options) { }
        public FTPSignalControllerDownloader(IFTPDownloaderClient client, ILogger<FTPSignalControllerDownloader> log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> options) : base(client, log, options) { }

        #region Properties

        public override int ControllerType => 1;

        public override string[] FileFilters { get; set; } = new string[] { "dat", "datZ" };

        #endregion

        #region Methods

        protected async IAsyncEnumerable<FileInfo> ExecutionTask(Signal parameter, IProgress<ControllerDownloadProgress> progress = null, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            using FtpClient client = new FtpClient(parameter.Ipaddress.ToString());
            {
                client.Credentials = new NetworkCredential(parameter.ControllerType.UserName, parameter.ControllerType.Password);
                //TODO: replace this with options setting
                client.ConnectTimeout = 1000;
                client.ReadTimeout = 1000;
                if (parameter.ControllerType.ActiveFtp)
                {
                    client.DataConnectionType = FtpDataConnectionType.AutoActive;
                }

                //all results go into this list
                List<FtpResult> results = new List<FtpResult>();

                try
                {
                    var profile = await client?.AutoConnectAsync(cancelToken);

                    _log.LogInformation(new EventId(Convert.ToInt32(parameter.SignalId)), $"Connected to Controller: {parameter.ControllerType.Description} - {profile?.Host} - {profile?.DataConnection} - {profile?.SocketPollInterval} - {profile?.RetryAttempts} - {profile?.Timeout}");
                }
                catch (Exception e)
                {
                    progress?.Report(new ControllerDownloadProgress(e));

                    _log.LogDebug(new EventId(Convert.ToInt32(parameter.SignalId)), e, "FTP Connection Error {ip}", parameter.Ipaddress);
                }

                if (client.IsConnected)
                {
                    if (await client.DirectoryExistsAsync(parameter.ControllerType.Ftpdirectory, cancelToken))
                    {
                        var rules = new List<FtpRule> { new FtpFileExtensionRule(true, new List<string> { "dat", "datZ" }) };

                        Progress<FtpProgress> ftpProgress = new Progress<FtpProgress>(p => Console.WriteLine($"FTP Progress: {p.Progress} - {p.FileIndex}/{p.FileCount} - {p.LocalPath}"));

                        try
                        {
                            results = await client.DownloadDirectoryAsync(Path.Combine(_options.LocalPath, parameter.SignalId), parameter.ControllerType.Ftpdirectory, FtpFolderSyncMode.Update, FtpLocalExists.Overwrite, FtpVerify.None, rules, ftpProgress, cancelToken);
                        }
                        catch (Exception e)
                        {
                            progress?.Report(new ControllerDownloadProgress(e));

                            _log.LogDebug(new EventId(Convert.ToInt32(parameter.SignalId)), e, "FTP Download Error {ip}", parameter.Ipaddress);
                        }
                    }

                    if (results.Count > 0)
                    {
                        //process results
                        foreach (FtpResult r in results)
                        {
                            if (r.IsSuccess && r.IsDownload && !r.IsSkippedByRule)
                            {
                                var file = new FileInfo(r.LocalPath);

                                progress?.Report(new ControllerDownloadProgress(file));

                                _log.LogInformation(new EventId(Convert.ToInt32(parameter.SignalId)), r.Exception, "Success: file:{file} downloaded:{Downloaded} failed:{Failed} skipped:{skipped} success:{success}", r.Name, r.IsDownload, r.IsFailed, r.IsSkipped, r.IsSuccess);

                                yield return file;
                            }

                            if (r.IsFailed)
                            {
                                progress?.Report(new ControllerDownloadProgress(file: null));

                                _log.LogWarning(new EventId(Convert.ToInt32(parameter.SignalId)), r.Exception, "Failed: file:{file} downloaded:{Downloaded} failed:{Failed} skipped:{skipped} success:{success}", r.Name, r.IsDownload, r.IsFailed, r.IsSkipped, r.IsSuccess);
                            }

                            if (r.Exception != null)
                            {
                                if (r.Exception.InnerException != null)
                                {
                                    progress?.Report(new ControllerDownloadProgress(r.Exception.InnerException));

                                    r.Exception.InnerException.LogE(LogLevel.Error);
                                }
                                else
                                {
                                    progress?.Report(new ControllerDownloadProgress(r.Exception));

                                    r.Exception.LogE(LogLevel.Error);
                                }
                            }
                        }
                    }

                    await client.DisconnectAsync(cancelToken);
                }
            }
        }

        #endregion
    }
}
