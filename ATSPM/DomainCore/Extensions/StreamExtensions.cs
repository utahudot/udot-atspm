using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace ATSPM.Domain.Extensions
{
    public static class StreamExtensions
    {
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
