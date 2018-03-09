
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    using DurableTask.AzureStorage;
    using DurableTask.Core;
    using DurableTaskFrameworkPrototype;

    public class Program
    {
        public static void Main(string[] args)
        {
            AzureStorageOrchestrationService orchestrationService = new AzureStorageOrchestrationService(new AzureStorageOrchestrationServiceSettings()
            {
                StorageConnectionString = ServiceSettings.StorageConnectionString,
                TaskHubName = ServiceSettings.TaskHubName
            });

            TaskHubClient client = new TaskHubClient(orchestrationService);
            Console.WriteLine("Enter -1 to exit anything else will be fed to the orchestration");
            do
            {
                var s = Console.ReadLine();

                if (s == "-1")
                {
                    break;
                }

                var instance = client.CreateOrchestrationInstanceAsync(typeof(LetterCountOrchestration), s).Result;
                Console.WriteLine("Workflow Instance Started: " + instance);

                var result = client.WaitForOrchestrationAsync(instance, TimeSpan.MaxValue).Result;

                Console.WriteLine($"Task done: {result?.OrchestrationStatus}");
            } while (1 == 1);
        }
    }
}
