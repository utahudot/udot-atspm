using ATSPM.Application.Configuration;
using ATSPM.Application.Enums;
using ATSPM.Application.Extensions;
using ATSPM.Application.Models;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Domain.BaseClasses;
using ATSPM.Domain.Common;
using ATSPM.Domain.Extensions;
using FluentFTP;
using FluentFTP.Rules;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Utah.Gov.Udot.PipelineManager;

namespace ATSPM.Infrasturcture.Services.ControllerDownloaders
{
    public class ASCSignalControllerDownloader : ServiceObjectBase, ISignalControllerDownloader
    {
        public event EventHandler CanExecuteChanged;

        #region Fields

        private readonly ILogger _log;
        private readonly IServiceProvider _serviceProvider;
        protected readonly IOptions<SignalControllerDownloaderConfiguration> _options;

        #endregion

        public ASCSignalControllerDownloader(ILogger<ASCSignalControllerDownloader> log, IServiceProvider serviceProvider, IOptions<SignalControllerDownloaderConfiguration> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

        #region Properties

        public SignalControllerType ControllerType => SignalControllerType.ASC3 | SignalControllerType.EOS | SignalControllerType.Cobalt;

        #endregion

        #region Methods

        public override void Initialize()
        {
            //using FtpClient client = new FtpClient(s.Ipaddress);
            //client.Credentials = new NetworkCredential(s.ControllerType.UserName, s.ControllerType.Password);
            //client.ConnectTimeout = 1000;
            //client.ReadTimeout = 1000;
            //if (s.ControllerType.ActiveFtp)
            //{
            //    client.DataConnectionType = FtpDataConnectionType.AutoActive;
            //}
        }

        #region IPipelineExecute<Tin, Tout>

        public bool CanExecute(Signal value)
        {
            //if (!value.Ipaddress.IsValidIPAddress(false))
            //    _log.LogWarning(new EventId(Convert.ToInt32(value.SignalId)), $"Cant Execute: {value.Ipaddress} - {value.PrimaryName}");

            //check valid controller type
            //return ((ControllerType & (SignalControllerType)value.ControllerType.ControllerTypeId) == (SignalControllerType)value.ControllerType.ControllerTypeId)

            //check directory
            //&& !string.IsNullOrEmpty(value.ControllerType.Ftpdirectory)
            return !string.IsNullOrEmpty(value.ControllerType.Ftpdirectory)

            //check valid ipaddress
            //&& value.Ipaddress.IsValidIPAddress(_options.Value.PingControllerToVerify);
            && value.Ipaddress.IsValidIPAddress(false);
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

            if (CanExecute(parameter) && parameter.ControllerType.ControllerTypeId != 4)
            {
                using FtpClient client = new FtpClient(parameter.Ipaddress);
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
                        //await client?.ConnectAsync(cancelToken);

                        _log.LogDebug(new EventId(Convert.ToInt32(parameter.SignalId)), $"Controller Type: {parameter.ControllerType.Description} - {profile?.Host} - {profile?.DataConnection} - {profile?.SocketPollInterval} - {profile?.RetryAttempts} - {profile?.Timeout}");


                        try
                        {
                            if (client.IsConnected && await client.DirectoryExistsAsync(parameter.ControllerType.Ftpdirectory))
                            {
                                var rules = new List<FtpRule> { new FtpFileExtensionRule(true, new List<string> { "dat", "datZ" }) };
                                results = await client.DownloadDirectoryAsync(Path.Combine(_options.Value.LocalPath, parameter.SignalId), parameter.ControllerType.Ftpdirectory, FtpFolderSyncMode.Update, FtpLocalExists.Overwrite, FtpVerify.None, rules, progress: null, cancelToken);
                            }
                        }
                        catch (Exception e)
                        {
                            dir ??= new DirectoryInfo(Path.Combine(_options.Value.LocalPath, "EXECUTION ERROR", e.GetType().ToString(), parameter.SignalId));
                            _log.LogError(new EventId(Convert.ToInt32(parameter.SignalId)), e, "EXECUTION ERROR");
                        }
                        finally
                        {
                            dir ??= ProcessResults(results, parameter);
                        }
                    }
                    //catch (FtpAuthenticationException e) when (e.LogE()) { }
                    //catch (FtpAuthenticationException e)
                    //{
                    //    dir ??= new DirectoryInfo(Path.Combine(_options.Value.LocalPath, "FtpAuthenticationException", parameter.SignalId));
                    //    _log.LogError(new EventId(Convert.ToInt32(parameter.SignalId)), e, "FtpAuthenticationException");
                    //}
                    //catch (FtpCommandException e) when (e.LogE()) { }
                    //catch (FtpException e) when (e.LogE()) { }
                    //catch (SocketException e) when (e.LogE()) { }
                    //catch (IOException e) when (e.LogE()) { }
                    //catch (TimeoutException e)
                    //{
                    //    dir ??= new DirectoryInfo(Path.Combine(_options.Value.LocalPath, "TimeoutException", e.GetType().ToString(), parameter.SignalId));
                    //    _log.LogError(new EventId(Convert.ToInt32(parameter.SignalId)), e, "TimeoutException");
                    //}
                    catch (Exception e)
                    {
                        dir ??= new DirectoryInfo(Path.Combine(_options.Value.LocalPath, "CONNECTION ERROR", e.GetType().ToString(), parameter.SignalId));
                        _log.LogError(new EventId(Convert.ToInt32(parameter.SignalId)), e, "CONNECTION ERROR");
                    }
                    //finally
                    //{
                    //    if (client.IsConnected)
                    //        await client.DisconnectAsync();
                    //}

                    if (client.IsConnected)
                        await client.DisconnectAsync();
                }
            }
            else
            {
                dir ??= new DirectoryInfo(Path.Combine(_options.Value.LocalPath, "DidNotPassCanExecute", $"{parameter.SignalId} - {parameter.ControllerTypeId}"));
            }


            dir ??= new DirectoryInfo(Path.Combine(_options.Value.LocalPath, "NO IDEA", $"{parameter.SignalId} - {parameter.ControllerTypeId}"));

            if (dir != null && !dir.Exists)
                dir.Create();

            return dir;
        }

        private DirectoryInfo ProcessResults(List<FtpResult> results, Signal input)
        {
            //return directory
            DirectoryInfo dir = null;

            if (results.Count > 0)
            {
                //process results
                foreach (FtpResult r in results)
                {
                    if (r.IsSuccess && r.IsDownload && !r.IsSkippedByRule)
                    {
                        _log.LogInformation(new EventId(Convert.ToInt32(input.SignalId)), r.Exception, "Success: file:{file} downloaded:{Downloaded} failed:{Failed} skipped:{skipped} success:{success}", r.Name, r.IsDownload, r.IsFailed, r.IsSkipped, r.IsSuccess);
                    }

                    if (r.IsFailed)
                    {
                        _log.LogWarning(new EventId(Convert.ToInt32(input.SignalId)), r.Exception, "Failed: file:{file} downloaded:{Downloaded} failed:{Failed} skipped:{skipped} success:{success}", r.Name, r.IsDownload, r.IsFailed, r.IsSkipped, r.IsSuccess);
                    }

                    if (r.Exception != null)
                    {
                        if (r.Exception.InnerException != null)
                        {
                            r.Exception.InnerException.LogE(LogLevel.Error);
                        }
                        else
                        {
                            r.Exception.LogE(LogLevel.Error);
                        }
                    }
                }

                //at least on file succeeded
                if (results.Any(r => r.IsSuccess && r.IsDownload && !r.IsSkippedByRule))
                {
                    dir ??= new DirectoryInfo(Path.Combine(_options.Value.LocalPath, input.SignalId));
                }

                //at least one file had a failure
                if (results.Any(r => r.IsFailed || r.Exception != null))
                {
                    dir ??= new DirectoryInfo(Path.Combine(_options.Value.LocalPath, "ErrorOrFailedException", input.SignalId));
                }

                //all files were return successfully
                //else
                //{
                //    dir ??= new DirectoryInfo(Path.Combine(_options.Value.LocalPath, input.SignalId));
                //}
            }
            else
            {
                dir = new DirectoryInfo(Path.Combine(_options.Value.LocalPath, "NoUsableFiles", input.SignalId));
            }

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
