using System.Collections.Generic;

namespace ATSPM.Application.Reports.ViewModels.TimingAndActuation;

public class EventType
{
    public EventType(string description, ICollection<Event> events)
    {
        Description = description;
        Events = events;
    }

    public string Description { get; internal set; }

    public System.Collections.Generic.ICollection<Event> Events { get; internal set; }
}
