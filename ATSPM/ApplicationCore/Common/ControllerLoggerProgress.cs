using ATSPM.Application.Models;
using ATSPM.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ATSPM.Application.Common
{
    public abstract class ControllerLoggerProgress<T>
    {
        protected T _item;

        public ControllerLoggerProgress(T item, Exception exception, int current = 0, int total = 0)
        {
            _item = item;
            Exception = exception;
            Current = current;
            Total = total;
        }
        
        public Exception Exception { get; protected set; }

        public bool HasException => Exception != null;

        public bool IsSuccessful => _item != null;

        public int Total { get; protected set; }

        public int Current { get; protected set; }

        public double Progress => Total.ToPercent(Current);

        public override string ToString()
        {
            if (HasException)
                return $"{this.GetType().Name}: {Exception?.GetType().Name} {Current}/{Total} Successful {IsSuccessful}";

            return $"{this.GetType().Name}: {_item?.GetType().Name} {Current}/{Total} Successful {IsSuccessful}";
        }
    }

    public class ControllerDownloadProgress : ControllerLoggerProgress<FileInfo>
    {
        public ControllerDownloadProgress(FileInfo file) : this(file, 0, 0) { }

        public ControllerDownloadProgress(FileInfo file, int current = 0, int total = 0) : base(file, null, current, total) { }

        public ControllerDownloadProgress(Exception exception, int current = 0, int total = 0) : base(null, exception, current, total) { }

        public FileInfo File => _item;
    }

    public class ControllerDecodeProgress : ControllerLoggerProgress<ControllerEventLog>
    {
        public ControllerDecodeProgress(ControllerEventLog log) : this(log, 0, 0) { }

        public ControllerDecodeProgress(ControllerEventLog log, int current = 0, int total = 0) : base(log, null, current, total) { }

        public ControllerDecodeProgress(Exception exception, int current = 0, int total = 0) : base(null, exception, current, total) { }

        public ControllerEventLog Log => _item;
    }
}
