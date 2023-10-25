using ATSPM.Application.Repositories;
using ATSPM.Application.Specifications;
using ATSPM.Application.ValueObjects;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ATSPM.Application.Extensions
{
    public static class SignalRepositoryExtensions
    {
        /// <summary>
        /// Copies <see cref="Signal"/> and associated <see cref="Approach"/> to new version
        /// and archives old version
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="id">Signal version to copy</param>
        /// <returns>New version of copied <see cref="Signal"/></returns>
        public static async Task<Signal> CopySignalToNewVersion(this ISignalRepository repo, int id)
        {
            Signal signal = await repo.LookupAsync(id);

            if (signal != null)
            {
                var newVersion = (Signal)signal.Clone();

                newVersion.VersionActionId = SignalVersionActions.NewVersion;
                newVersion.Start = DateTime.Today;
                newVersion.Note = $"Copy of {signal.Note}";

                newVersion.Id = 0;

                newVersion.ControllerType = null;
                newVersion.Jurisdiction = null;
                newVersion.Region = null;
                newVersion.VersionAction = null;

                await repo.AddAsync(newVersion);

                return newVersion;
            }
            else
            {
                throw new ArgumentException($"{id} is not a valid signal");
            }
        }

        /// <summary>
        /// Marks <see cref="Signal"/> to deleted
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="id">Id of <see cref="Signal"/> to mark as deleted</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">id is not a valid signal</exception>
        public static async Task SetSignalToDeleted(this ISignalRepository repo, int id)
        {
            Signal signal = await repo.LookupAsync(id);

            if (signal != null)
            {
                signal.VersionActionId = SignalVersionActions.Delete;
                await repo.UpdateAsync(signal);
            }
            else
            {
                throw new ArgumentException($"{id} is not a valid signal");
            }
        }

        #region Obsolete

        [Obsolete("This method isn't currently being used")]
        public static void AddList(this ISignalRepository repo, List<Signal> signals)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use the add in respository base class")]
        public static void AddOrUpdate(this ISignalRepository repo, Signal signal)
        {
            throw new NotImplementedException();
        }

        [Obsolete("This method isn't currently being used")]
        public static int CheckVersionWithFirstDate(this ISignalRepository repo, string signalId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Redundant to GetAllSignals")]
        public static IReadOnlyList<Signal> EagerLoadAllSignals(this ISignalRepository repo)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Not Required anymore")]
        public static bool Exists(this ISignalRepository repo, string signalId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Redundant to GetLatestVersionOfAllSignals")]
        public static IReadOnlyList<Signal> GetAllEnabledSignals(this ISignalRepository repo)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Redundant to GetLatestVersionOfAllSignals")]
        public static IList<Signal> GetAllSignals(this ISignalRepository repo)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use overload of GetLatestVersionOfAllSignals")]
        public static IReadOnlyList<Signal> GetAllVersionsOfSignalBySignalId(this ISignalRepository repo, string signalId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use overload of GetLatestVersionOfAllSignals")]
        public static IReadOnlyList<Signal> GetLatestVerionOfAllSignalsByControllerType(this ISignalRepository repo, int controllerTypeId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("This method isn't currently being used")]
        public static string GetSignalDescription(this ISignalRepository repo, string signalId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use GetLatestVersionOfSignal")]
        public static Signal GetLatestVersionOfSignalBySignalId(this ISignalRepository repo, string signalId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("This should not be in respository")]
        public static IReadOnlyList<Pin> GetPinInfo(this ISignalRepository repo)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use GetLatestVersionOfSignal")]
        public static string GetSignalLocation(this ISignalRepository repo, string signalId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use Lookup instead")]
        public static Signal GetSignalVersionByVersionId(this ISignalRepository repo, int versionId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use GetLatestVersionOfSignal")]
        public static Signal GetVersionOfSignalByDate(this ISignalRepository repo, string signalId, DateTime startDate)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use GetLatestVersionOfSignal")]
        public static Signal GetVersionOfSignalByDateWithDetectionTypes(this ISignalRepository repo, string signalId, DateTime startDate)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use SetSignalToDeleted")]
        public static void SetAllVersionsOfASignalToDeleted(this ISignalRepository repo, string id)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use SetSignalToDeleted")]
        public static void SetVersionToDeleted(this ISignalRepository repo, int versionId)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
