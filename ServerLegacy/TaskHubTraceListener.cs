using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLegacy
{
    public class TaskHubTraceListener : TraceListener
    {
        const string InstanceIdToken = "iid";
        const string ExecutionIdToken = "eid";
        const string MessageToken = "msg";

        static readonly Tuple<string, TraceEventType>[] ReducedSeverityMessageMappings = new[]
                    {
                        new Tuple<string, TraceEventType>("Exception: DurableTask.TaskFailureException", TraceEventType.Warning),
                        new Tuple<string, TraceEventType>("Failed to write history entity", TraceEventType.Warning),
                        new Tuple<string, TraceEventType>("Microsoft.ServiceBus.Messaging.MessagingException", TraceEventType.Warning),
                    };

        static readonly string[] KnownMessageTokens = new[] { InstanceIdToken, ExecutionIdToken, MessageToken }.Select(t => t + "=").ToArray();
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            try
            {
                string message = format;
                if (args.Length > 0)
                {
                    message = string.Format(CultureInfo.InvariantCulture, format, args);
                }

                this.TraceEvent(eventCache, source, eventType, id, message);
            }
            catch (Exception ex)
            {
                string message = string.Format(CultureInfo.InvariantCulture, "format:{0} args: {1} error: {2}", format, string.Join(",", args), ex.ToString());
                TaskHubEventSource.Log.Error("OrchestrationTraceListenerFormattingFailed", String.Empty, String.Empty, message);
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            ParseAndLogMessage(eventType, message);
        }

        public override void Write(string message)
        {
            // skip as we trace only in TraceEvent methods
        }

        public override void WriteLine(string message)
        {
            // skip as we trace only in TraceEvent methods
        }

        static void ParseAndLogMessage(TraceEventType eventType, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            try
            {
                IEnumerable<string> messageParts = SplitMessage(message);
                Dictionary<string, string> dict = messageParts
                    .Select(part => part.Split(new[] { '=' }, count: 2)) // Only care about splitting on the first '=' so no more than 2 items
                    .Where(split => split.Length == 2) // Check we have 2 items to not fall with indexoutofrange
                    .ToDictionary(split => split[0], split => split[1]);

                TraceEventType normalizedEventType = eventType;
                String msg = dict.GetValueOrDefault(MessageToken);

                if (msg != null && (eventType == TraceEventType.Critical || eventType == TraceEventType.Error))
                {
                    IEnumerable<Tuple<string, TraceEventType>> matchingTraces = ReducedSeverityMessageMappings.Where(
                        m => msg.IndexOf(m.Item1, StringComparison.OrdinalIgnoreCase) >= 0);

                    if (matchingTraces.Any())
                    {
                        normalizedEventType = matchingTraces.First().Item2;
                    }
                }

                WriteEventOrchestrationTraceListenerTraceEvent(
                    normalizedEventType,
                    dict.GetValueOrDefault(InstanceIdToken),
                    dict.GetValueOrDefault(ExecutionIdToken),
                    msg);
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.InvariantCulture, "message:{0} error: {1}", message, ex.ToString());
                TaskHubEventSource.Log.Error("OrchestrationTraceListenerParsingFailed", String.Empty, String.Empty, msg);
            }
        }

        static IEnumerable<string> SplitMessage(string message)
        {
            List<string> messageParts = message.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var processedMessageParts = new List<string>();

            string partToAdd = messageParts[0];
            for (var i = 1; i < messageParts.Count; i++)
            {
                string current = messageParts[i];
                if (KnownMessageTokens.Any(t => current.StartsWith(t, StringComparison.Ordinal)))
                {
                    processedMessageParts.Add(partToAdd);
                    partToAdd = current;
                }
                else
                {
                    // If a part doesn't start with a known token assume that it was the continuation of the previous part that was split erroneously
                    partToAdd = partToAdd + ";" + current;
                }
            }

            processedMessageParts.Add(partToAdd);

            return processedMessageParts;
        }

        static void WriteEventOrchestrationTraceListenerTraceEvent(TraceEventType eventType, string instanceId, string executionId, string message)
        {
            switch (eventType)
            {
                case TraceEventType.Critical:
                    TaskHubEventSource.Log.Critical("OrchestrationTraceListenerTraceEvent", instanceId, executionId, message);
                    break;
                case TraceEventType.Error:
                    TaskHubEventSource.Log.Error("OrchestrationTraceListenerTraceEvent", instanceId, executionId, message);
                    break;
                case TraceEventType.Warning:
                    TaskHubEventSource.Log.Warning("OrchestrationTraceListenerTraceEvent", instanceId, executionId, message);
                    break;
                default:
                    TaskHubEventSource.Log.Informational("OrchestrationTraceListenerTraceEvent", instanceId, executionId, message);
                    break;
            }
        }
    }
}
