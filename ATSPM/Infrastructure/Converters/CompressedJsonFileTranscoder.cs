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
