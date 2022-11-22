using ATSPM.Domain.Common;
using ATSPM.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATSPM.Infrastructure.Converters
{
    public class JsonFileTranscoder : IFileTranscoder
    {
        public string FileExtension => ".json";

        public T DecodeItem<T>(byte[] data) where T : new()
        {
            return data.FromEncodedJson<T>();
        }

        public byte[] EncodeItem<T>(T item) where T : new()
        {
            return item.ToEncodedJson();
        }
    }
}
