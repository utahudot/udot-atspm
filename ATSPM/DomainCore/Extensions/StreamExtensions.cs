using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ATSPM.Domain.Extensions
{
    public static class StreamExtensions
    {
        public static MemoryStream ToMemoryStream(this FileInfo file)
        {
            var fileStream = File.Open(file.FullName, FileMode.Open, FileAccess.Read);
            var memoryStream = new MemoryStream();
            fileStream.CopyTo(memoryStream);
            fileStream.Close();

            return memoryStream;
        }
    }
}
