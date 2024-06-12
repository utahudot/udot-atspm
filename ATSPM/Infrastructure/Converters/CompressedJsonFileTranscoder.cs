#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Converters/CompressedJsonFileTranscoder.cs
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
using ATSPM.Domain.Common;
using ATSPM.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATSPM.Infrastructure.Converters
{
    public class CompressedJsonFileTranscoder : IFileTranscoder
    {
        public string FileExtension => ".gz";

        public T DecodeItem<T>(byte[] data) where T : new()
        {
            return data.GZipDecompressToString().FromJson<T>();
        }

        public byte[] EncodeItem<T>(T item) where T : new()
        {
            return item.ToJson().GZipCompressToByte();
        }
    }
}
