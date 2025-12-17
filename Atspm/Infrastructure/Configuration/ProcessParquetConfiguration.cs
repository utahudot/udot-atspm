using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    /// <summary>
    /// Holds the file path parameter passed from the command line.
    /// Specifies where parquet files are located.
    /// </summary>
    public class ProcessParquetConfiguration
    {
        public string Path { get; set; } = System.IO.Path.GetTempPath();
    }
}
