using Parquet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Infrastructure.Services.EventLogDecoders
{
    /// <summary>
    /// Reads parquet file, converts each row into IndianaEvent object for database storage
    /// </summary>

    /// <inheritdoc/>
    public class ParquetToIndianaDecoder : EventLogDecoderBase<IndianaEvent>
    {
        #region Properties
        #endregion

        #region Methods
        /// <inheritdoc/>
        public override Stream Decompress(Stream stream)
        {
            return stream;
        }

        /// <inheritdoc/>
        public override IEnumerable<IndianaEvent> Decode(Device device, Stream stream, CancellationToken cancelToken = default)
        {
            cancelToken.ThrowIfCancellationRequested();

            if (device == null)
                throw new ArgumentNullException(nameof(device), "Device cannot be null");

            if (stream?.Length == 0)
                throw new InvalidDataException("Stream is empty");

            var locationIdentifier = device.Location.LocationIdentifier;

            HashSet<IndianaEvent> decodedLogs = new();

            try
            {
                using var parquetReader = new ParquetReader(stream);
                var dataFields = parquetReader.Schema.GetDataFields();

                // Looping through list of records
                for (int rowGroupIndex = 0; rowGroupIndex < parquetReader.RowGroupCount; rowGroupIndex++)
                {
                    using var rowGroupReader = parquetReader.OpenRowGroupReader(rowGroupIndex);

                    var dates = (string[])rowGroupReader.ReadColumn(dataFields[1]).Data;
                    var timestampsMs = (double[])rowGroupReader.ReadColumn(dataFields[2]).Data;
                    var eventCodes = (int[])rowGroupReader.ReadColumn(dataFields[3]).Data;
                    var eventParams = (int[])rowGroupReader.ReadColumn(dataFields[4]).Data;

                    for (int i = 0; i < timestampsMs.Length; i++)
                    {
                        cancelToken.ThrowIfCancellationRequested();

                        try
                        {
                            var baseDate = DateTime.Parse(dates[i]);
                            var timestamp = baseDate.AddMilliseconds(timestampsMs[i]);

                            var log = new IndianaEvent
                            {
                                LocationIdentifier = locationIdentifier,
                                EventCode = (short)eventCodes[i],
                                EventParam = (byte)eventParams[i],
                                Timestamp = timestamp
                            };

                            decodedLogs.Add(log);
                        }
                        catch (Exception e)
                        {
                            throw new EventLogDecoderException(e);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new EventLogDecoderException(e);
            }

            return decodedLogs;
        }

        #endregion
    }
}
