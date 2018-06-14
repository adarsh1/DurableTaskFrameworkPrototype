
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    using DurableTask;
    using DurableTask.Tracking;
    using DurableTaskFrameworkPrototype;

    public class Program
    {
        const int MessageCompressionThresholdInBytes = 4096;
        const int MaxTaskHubDeliveryCount = 100;
        const int ConcurrentOrchstrations = 40;
        const int ConcurrentTrackingSessions = 40;
        const int ConcurrentActivities = 40;
        const int TaskHubDispatcherCount = 4;
        const int SessionMaxSizeInBytes = 20 * 1024 * 1024;
        static OrchestrationInstance instance = null;//[InstanceId: 0c13e0789be44aff9d7e114872c87730, ExecutionId: 6424a5cafbaa4c92aafad4317b632a2a]

        static void Main(string[] args)
        {

            var settings = new TaskHubClientSettings
            {
                MessageCompressionSettings = new CompressionSettings
                {
                    Style = CompressionStyle.Always,
                    ThresholdInBytes = MessageCompressionThresholdInBytes
                }
            };
            TaskHubClient client = new TaskHubClient(ServiceSettings.TaskHubName,ServiceSettings.ServiceBusConnectionString, ServiceSettings.StorageConnectionString, settings);
            Console.WriteLine("Enter -1 to exit anything else will be fed to the orchestration");
            do
            {
                var s = Console.ReadLine();

                if (s == "-1")
                {
                    break;
                }

                DoCron(client, s);
            } while (1 == 1);
        }

        private static void DoLetterCount(TaskHubClient client, string s)
        {
            var instance = client.CreateOrchestrationInstanceAsync(typeof(LetterCountOrchestration), s).Result;
            Console.WriteLine("Workflow Instance Started: " + instance);
        }

        private static void DoCron(TaskHubClient client, string s)
        {
            var instance = client.CreateOrchestrationInstanceAsync(typeof(CronOrchestration), string.Empty).Result;
            Console.WriteLine("Workflow Instance Started: " + instance);
        }

        private static void DoCounter(TaskHubClient client, string s)
        {
            if (s == "begin")
            {
                instance = client.CreateOrchestrationInstanceAsync(typeof(Counter), 0).Result;
                Console.WriteLine("Workflow Instance Started: " + instance);
                instance.ExecutionId = string.Empty;
            }
            else
            {
                client.RaiseEventAsync(instance, "operation", s).Wait();
            }
        }

        private static void DoParentChild(TaskHubClient client, string s)
        {
            var instance = client.CreateOrchestrationInstanceAsync(typeof(Counter.ParentWorkflow), false).Result;
            Console.WriteLine("Workflow Instance Started: " + instance);
        }
    }
}
