using ATSPM.Domain.Common;
using ATSPM.Domain.Extensions;
using Parquet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ATSPM.Infrastructure.Converters
{
    public class ParquetFileTranscoder : IFileTranscoder
    {
        public string FileExtension => ".parquet";

        public T DecodeItem<T>(byte[] data) where T : new()
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                return ParquetConvert.Deserialize<T>(stream).FirstOrDefault();
            }
        }

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
