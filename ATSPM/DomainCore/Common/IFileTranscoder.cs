using ATSPM.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATSPM.Domain.Common
{
    public interface IFileTranscoder
    {
        string FileExtension { get; }

        byte[] EncodeItem<T>(T item) where T : class;

        T DecodeItem<T>(byte[] data) where T : class;
    }
}
