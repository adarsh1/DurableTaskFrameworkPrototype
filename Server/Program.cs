﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Server
{
    using DurableTask.ServiceBus;
    using DurableTask.ServiceBus.Tracking;
    using DurableTask.ServiceBus.Settings;
    using DurableTask.Core;
    using DurableTaskFrameworkPrototype;
    using DurableTask.Core.Tracking;
    using DurableTask.Core.Settings;
    using DurableTask.Core.Common;
    using DurableTask.AzureStorage;

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
            AzureStorageOrchestrationService orchestrationService = new AzureStorageOrchestrationService(new AzureStorageOrchestrationServiceSettings()
            {
                StorageConnectionString = ServiceSettings.StorageConnectionString,
                TaskHubName = ServiceSettings.TaskHubName
            });

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
