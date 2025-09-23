using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Newtonsoft.Json.Linq;
using Utah.Udot.Atspm.Data.Models.EventLogModels;        // for SpeedEvent : EventLogModelBase
using Utah.Udot.Atspm.Infrastructure.Messaging;            // for EventBatchEnvelope
using Utah.Udot.NetStandardToolkit.Workflows;             // for TransformManyProcessStepBaseAsync
using Utah.Udot.Atspm.Data.Interfaces;                    // for ILocationLayer, ITimestamp, etc.

/// <summary>
/// Takes each EventBatchEnvelope (which contains Items:JArray plus DeviceId/LocationIdentifier)
/// and compresses them into hourly, per-device & per-type CompressedEventLogs&lt;T&gt; chunks.
/// </summary>
public class ArchiveEnvelopeDataEvents
    : TransformManyProcessStepBaseAsync<EventBatchEnvelope, CompressedEventLogBase>
{
    public ArchiveEnvelopeDataEvents(
        ExecutionDataflowBlockOptions opts = null
    ) : base(opts ?? new ExecutionDataflowBlockOptions()) { }

    protected override async IAsyncEnumerable<CompressedEventLogBase> Process(
        EventBatchEnvelope envelope,
        [EnumeratorCancellation] CancellationToken ct = default
    )
    {
        // 1) Materialize raw JSON into your EF model (here SpeedEvent)
        var rawEvents = ((JArray)envelope.Items)
            .ToObject<List<SpeedEvent>>()!;
        // restore the location from the envelope
        rawEvents.ForEach(e => e.LocationIdentifier = envelope.LocationIdentifier);

        // 2) Group by (location, year, month, day, hour, deviceId, eventType)
        var groups = rawEvents.GroupBy(e => (
            e.LocationIdentifier,
            e.Timestamp.Year,
            e.Timestamp.Month,
            e.Timestamp.Day,
            e.Timestamp.Hour,
            DeviceId: envelope.DeviceId,
            Type: e.GetType()
        ));

        foreach (var g in groups)
        {
            // build a List<T> where T == g.Key.Type
            dynamic list = Activator.CreateInstance(
                typeof(List<>).MakeGenericType(g.Key.Type)
            );
            foreach (var evt in g)
                ((IList)list).Add(evt);

            // compute the 1-hour time window
            var tl = new Timeline<StartEndRange>(list, TimeSpan.FromHours(1));

            // create a CompressedEventLogs<T> wrapper
            dynamic comp = Activator.CreateInstance(
                typeof(CompressedEventLogs<>).MakeGenericType(g.Key.Type)
            );
            comp.LocationIdentifier = g.Key.LocationIdentifier;
            comp.Start = tl.Start;
            comp.End = tl.End;
            comp.DataType = g.Key.Type;
            comp.DeviceId = g.Key.DeviceId;
            comp.Data = list;

            yield return comp;
        }
    }
}
