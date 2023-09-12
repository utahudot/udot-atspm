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
