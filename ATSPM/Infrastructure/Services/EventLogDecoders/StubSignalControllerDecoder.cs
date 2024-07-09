#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - %Namespace%/StubSignalControllerDecoder.cs
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
using ATSPM.Application.Configuration;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ATSPM.Infrastructure.Services.ControllerDecoders
{
    public class StubLocationControllerDecoder : ControllerDecoderBase
    {
        public StubLocationControllerDecoder(ILogger<StubLocationControllerDecoder> log, IOptionsSnapshot<SignalControllerDecoderConfiguration> options) : base(log, options) { }

        #region Properties

        #endregion

        #region Methods

        public override bool CanExecute(FileInfo parameter)
        {
            return true;
        }

        public override IAsyncEnumerable<ControllerEventLog> DecodeAsync(string locationId, Stream stream, CancellationToken cancelToken = default)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}