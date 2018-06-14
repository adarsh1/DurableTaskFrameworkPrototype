using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ServerLegacy2
{
    using DurableTask;
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
            var settings = new TaskHubWorkerSettings
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
                },
                TrackingDispatcherSettings =
                {
                    MaxConcurrentTrackingSessions = ConcurrentTrackingSessions,
                },
                TaskActivityDispatcherSettings =
                {
                    MaxConcurrentActivities = ConcurrentActivities,
                },
            };

            TaskHubWorker taskHub = new TaskHubWorker(ServiceSettings.TaskHubName, ServiceSettings.ServiceBusConnectionString, ServiceSettings.StorageConnectionString, settings);
            taskHub.CreateHubIfNotExists();
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


                taskHub.Start();

                Console.WriteLine("Press any key to quit.");
                Console.ReadLine();

                taskHub.Stop(true);
            }
            catch (Exception e)
            {
                // silently eat any unhadled exceptions.
                Console.WriteLine($"worker exception: {e}");
            }
        }
    }
}
