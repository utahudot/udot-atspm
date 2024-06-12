#region license
// Copyright 2024 Utah Departement of Transportation
// for DomainCore - ATSPM.Domain.Common/IFileTranscoder.cs
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
using ATSPM.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATSPM.Domain.Common
{
    /// <summary>
    /// Provides an abrstraction when working with different file types
    /// </summary>
    public interface IFileTranscoder
    {
        /// <summary>
        /// File extension type to work with
        /// <example>.txt</example>
        /// </summary>
        string FileExtension { get; }

        /// <summary>
        /// Defines how the object should be encoded
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <param name="item">data to encode</param>
        /// <returns></returns>
        byte[] EncodeItem<T>(T item) where T : new();

        /// <summary>
        /// Defines how the file should be decoded
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <param name="data">data to decode</param>
        /// <returns></returns>
        T DecodeItem<T>(byte[] data) where T : new();
    }
}
