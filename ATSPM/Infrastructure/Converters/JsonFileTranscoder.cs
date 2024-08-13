﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Converters/JsonFileTranscoder.cs
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


namespace Utah.Udot.Atspm.Infrastructure.Converters
{
    /// <summary>
    /// Transcodes binary data to json format
    /// </summary>
    public class JsonFileTranscoder : IFileTranscoder
    {
        /// <inheritdoc/>
        public string FileExtension => ".json";

        /// <inheritdoc/>
        public T DecodeItem<T>(byte[] data) where T : new()
        {
            return data.FromEncodedJson<T>();
        }

        /// <inheritdoc/>
        public byte[] EncodeItem<T>(T item) where T : new()
        {
            return item.ToEncodedJson();
        }
    }
}
