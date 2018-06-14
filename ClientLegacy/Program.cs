
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    using DurableTask;
    using DurableTask.Common;
    using DurableTask.Settings;
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
        static OrchestrationInstance instance = null;

        static void Main(string[] args)
        {
            IOrchestrationServiceInstanceStore instanceStore = new AzureTableInstanceStore(ServiceSettings.TaskHubName, ServiceSettings.StorageConnectionString);
            IOrchestrationServiceBlobStore blobStore = new AzureStorageBlobStore(ServiceSettings.TaskHubName, ServiceSettings.StorageConnectionString);
            var settings = new ServiceBusOrchestrationServiceSettings
            {
                MessageCompressionSettings = new CompressionSettings
                {
                    Style = CompressionStyle.Always,
                    ThresholdInBytes = MessageCompressionThresholdInBytes
                },
                JumpStartSettings =
                {
                    JumpStartEnabled = false
                }
            };

            ServiceBusOrchestrationService orchestrationService = new ServiceBusOrchestrationService(ServiceSettings.ServiceBusConnectionString, ServiceSettings.TaskHubName, instanceStore, blobStore, settings);

            TaskHubClient client = new TaskHubClient(orchestrationService);
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

            var result = client.WaitForOrchestrationAsync(instance, TimeSpan.MaxValue).Result;

            Console.WriteLine($"Task done: {result?.OrchestrationStatus}");
        }

        private static void DoCron(TaskHubClient client, string s)
        {
            var instance = client.CreateOrchestrationInstanceAsync(typeof(CronOrchestration), string.Empty).Result;
            Console.WriteLine("Workflow Instance Started: " + instance);

            var result = client.WaitForOrchestrationAsync(instance, TimeSpan.MaxValue).Result;

            Console.WriteLine($"Task done: {result?.OrchestrationStatus}");
        }
        private static void DoCounter(TaskHubClient client, string s)
        {
            if (s == "begin")
            {
                instance = client.CreateOrchestrationInstanceAsync(typeof(Counter), 0).Result;
                Console.WriteLine("Workflow Instance Started: " + instance);
            }
            else
            {
                client.RaiseEventAsync(instance,"operation", s).Wait();
            }

            var result = client.WaitForOrchestrationAsync(instance, TimeSpan.MaxValue).Result;

            Console.WriteLine($"Task done: {result?.OrchestrationStatus}");
        }
    }
}
