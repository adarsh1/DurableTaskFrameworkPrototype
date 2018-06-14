using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLegacy2
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Tracing;

    [EventSource(
        Guid = "E8F59A1B-3688-4B50-8240-07995F147E40",
        Name = "TaskHubEventSource")]
    public class TaskHubEventSource : EventSource
    {
        public static class Keywords
        {
            public const EventKeywords Diagnostics = (EventKeywords)1L;
        }

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Should be a constant")]
        public static readonly TaskHubEventSource Log = new TaskHubEventSource();

        [NonEvent]
        unsafe private void WriteEventInternal(int eventId, string eventType, string instanceId, string executionId, string message)
        {
            eventType = eventType ?? String.Empty;
            instanceId = instanceId ?? String.Empty;
            executionId = executionId ?? String.Empty;
            message = message ?? String.Empty;

            const int EventDataCount = 4;
            fixed (char* eventTypeChPtr = eventType)
            fixed (char* instanceIdChPtr = instanceId)
            fixed (char* executionIdChPtr = executionId)
            fixed (char* messageChPtr = message)
            {
                EventSource.EventData* data = stackalloc EventSource.EventData[EventDataCount];
                data->DataPointer = (IntPtr)eventTypeChPtr;
                data->Size = (eventType.Length + 1) * 2;
                data[1].DataPointer = (IntPtr)instanceIdChPtr;
                data[1].Size = (instanceId.Length + 1) * 2;
                data[2].DataPointer = (IntPtr)executionIdChPtr;
                data[2].Size = (executionId.Length + 1) * 2;
                data[3].DataPointer = (IntPtr)messageChPtr;
                data[3].Size = (message.Length + 1) * 2;


                WriteEventCore(eventId, EventDataCount, data);
            }
        }

        [Event(9321, Keywords = Keywords.Diagnostics, Level = EventLevel.Informational)]
        public void Informational(string eventType, string instanceId, string executionId, string message)
        {
            if (this.IsEnabled(EventLevel.Informational, Keywords.Diagnostics))
            {
                this.WriteEventInternal(9321, eventType, instanceId, executionId, message);
            }
        }

        [Event(9322, Keywords = Keywords.Diagnostics, Level = EventLevel.Warning)]
        public void Warning(string eventType, string instanceId, string executionId, string message)
        {
            if (this.IsEnabled(EventLevel.Warning, Keywords.Diagnostics))
            {
                this.WriteEventInternal(9322, eventType, instanceId, executionId, message);
            }
        }

        [Event(9323, Keywords = Keywords.Diagnostics, Level = EventLevel.Error)]
        public void Error(string eventType, string instanceId, string executionId, string message)
        {
            if (this.IsEnabled(EventLevel.Error, Keywords.Diagnostics))
            {
                this.WriteEventInternal(9323, eventType, instanceId, executionId, message);
            }
        }

        [Event(9324, Keywords = Keywords.Diagnostics, Level = EventLevel.Critical)]
        public void Critical(string eventType, string instanceId, string executionId, string message)
        {
            if (this.IsEnabled(EventLevel.Critical, Keywords.Diagnostics))
            {
                this.WriteEventInternal(9324, eventType, instanceId, executionId, message);
            }
        }
    }
}
