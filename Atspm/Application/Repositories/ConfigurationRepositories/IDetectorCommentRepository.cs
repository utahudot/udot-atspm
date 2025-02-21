#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Repositories.ConfigurationRepositories/IDetectorCommentRepository.cs
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

using Utah.Udot.NetStandardToolkit.Services;

namespace Utah.Udot.Atspm.Repositories.ConfigurationRepositories
{
    /// <summary>
    /// Detector Comment Repository
    /// </summary>
    public interface IDetectorCommentRepository : IAsyncRepository<DetectorComment>
    {
        #region ExtensionMethods

        #endregion

        #region Obsolete

        //[Obsolete("Use GetList in the BaseClass", true)]
        //List<DetectorComment> GetAllDetectorComments();

        //[Obsolete("Use Lookup in the BaseClass", true)]
        //DetectorComment GetDetectorCommentByDetectorCommentID(int detectorCommentID);

        //[Obsolete("Use Add in the BaseClass")]
        //void AddOrUpdate(DetectorComment detectorComment);

        //[Obsolete("Use Add in the BaseClass", true)]
        //void Add(DetectorComment detectorComment);

        //[Obsolete("Use Remove in the BaseClass", true)]
        //void Remove(DetectorComment detectorComment);

        #endregion
    }
}
