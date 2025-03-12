#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Converters/ParquetFileTranscoder.cs
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

using Parquet;

namespace Utah.Udot.Atspm.Infrastructure.Converters
{
    /// <summary>
    /// Transcodes binary data to parquet format
    /// </summary>
    public class ParquetFileTranscoder : IFileTranscoder
    {
        /// <inheritdoc/>
        public string FileExtension => ".parquet";

        /// <inheritdoc/>
        public T DecodeItem<T>(byte[] data) where T : new()
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                return ParquetConvert.Deserialize<T>(stream).FirstOrDefault();
            }
        }

        /// <inheritdoc/>
        public byte[] EncodeItem<T>(T item) where T : new()
        {
            List<T> items = new List<T>();
            items.Add(item);

            using (MemoryStream stream = new MemoryStream())
            {
                ParquetConvert.Serialize(items, stream);
                return stream.ToArray();
            }
        }
    }
}
