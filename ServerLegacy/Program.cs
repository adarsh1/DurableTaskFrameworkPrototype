using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ServerLegacy
{
    using DurableTask;
    using DurableTask.Common;
    using DurableTask.Settings;
    using DurableTask.Tracking;
    using DurableTaskFrameworkPrototype;

    class Program
    {
        const int MessageCompressionThresholdInBytes = 4096;
        const int MaxTaskHubDeliveryCount = 100;
        const int ConcurrentOrchstrations = 2;
        const int ConcurrentTrackingSessions = 2;
        const int ConcurrentActivities = 40;
        const int TaskHubDispatcherCount = 2;
        const int SessionMaxSizeInBytes = 20 * 1024 * 1024;

        static void Main(string[] args)
        {
            IOrchestrationServiceInstanceStore instanceStore = new AzureTableInstanceStore(ServiceSettings.TaskHubName, ServiceSettings.StorageConnectionString);
            IOrchestrationServiceBlobStore blobStore = new AzureStorageBlobStore(ServiceSettings.TaskHubName, ServiceSettings.StorageConnectionString);
            var settings = new ServiceBusOrchestrationServiceSettings
            {
                MessageCompressionSettings = new CompressionSettings
                {
                    Style = CompressionStyle.Threshold,
                    ThresholdInBytes = MessageCompressionThresholdInBytes
                },
                TaskOrchestrationDispatcherSettings =
                {
                    MaxConcurrentOrchestrations = ConcurrentOrchstrations,
                    CompressOrchestrationState = true,
                    DispatcherCount = TaskHubDispatcherCount
                },
                TrackingDispatcherSettings =
                {
                    MaxConcurrentTrackingSessions = ConcurrentTrackingSessions,
                    DispatcherCount = TaskHubDispatcherCount,
                    TrackHistoryEvents = false
                },
                TaskActivityDispatcherSettings =
                {
                    MaxConcurrentActivities = ConcurrentActivities,
                    DispatcherCount = TaskHubDispatcherCount
                },
                SessionSettings =
                {
                    SessionMaxSizeInBytes = SessionMaxSizeInBytes
                },
                MaxTaskActivityDeliveryCount = MaxTaskHubDeliveryCount,
                MaxTaskOrchestrationDeliveryCount = MaxTaskHubDeliveryCount,
                MaxTrackingDeliveryCount = MaxTaskHubDeliveryCount
            };

            ServiceBusOrchestrationService orchestrationService = new ServiceBusOrchestrationService(ServiceSettings.ServiceBusConnectionString, ServiceSettings.TaskHubName, instanceStore, blobStore, settings);

            orchestrationService.CreateIfNotExistsAsync().Wait();
            TaskHubWorker taskHub = new TaskHubWorker(orchestrationService);
            try
            {
                taskHub.AddTaskOrchestrations(
                    typeof(LetterCountOrchestration),
                    typeof(CronOrchestration),
                    typeof(Counter),
                    typeof(Counter.ParentWorkflow),
                    typeof(Counter.ChildWorkflow)
                    );

                taskHub.AddTaskActivities(
                    new AddTask(),
                    new LogTask(),
                    new CronTask()
                    );


                taskHub.StartAsync().Wait();

                Console.WriteLine("Press any key to quit.");
                Console.ReadLine();

                taskHub.StopAsync(true).Wait();
            }
            catch (Exception e)
            {
                // silently eat any unhadled exceptions.
                Console.WriteLine($"worker exception: {e}");
            }
        }
    }
}
