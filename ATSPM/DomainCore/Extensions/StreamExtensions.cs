#region license
// Copyright 2024 Utah Departement of Transportation
// for DomainCore - ATSPM.Domain.Extensions/StreamExtensions.cs
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace ATSPM.Domain.Extensions
{
    /// <summary>
    /// Stream extensions
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Converts a <see cref="FileInfo"/> object to <see cref="MemoryStream"/> object
        /// </summary>
        /// <param name="file">File to convert to <see cref="MemoryStream"/></param>
        /// <returns><see cref="MemoryStream"/></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static MemoryStream ToMemoryStream(this FileInfo file)
        {
            if (file.Exists)
            {
                var fileStream = File.Open(file.FullName, FileMode.Open, FileAccess.Read);
                var memoryStream = new MemoryStream();
                fileStream.CopyTo(memoryStream);
                fileStream.Close();

                return memoryStream;
            }

            throw new FileNotFoundException("File not found", file.FullName);
        }

        //public static byte[] SerializeObjectToData<T>(this T obj)
        //{
        //    BinaryFormatter bf = new BinaryFormatter();
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        bf.Serialize(ms, obj);
        //        return ms.ToArray();
        //    }
        //}

        //public static T DeserializeObjectFromStream<T> (this Stream stream)
        //{
        //    BinaryFormatter bf = new BinaryFormatter();
        //    return (T)bf.Deserialize(stream);
        //}
    }
}
