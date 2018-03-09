using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DurableTaskFrameworkPrototype
{
	using DurableTask.AzureStorage;
	using DurableTask.Core;

	class Program
	{
		static void Main(string[] args)
		{
			AzureStorageOrchestrationService orchestrationService = new AzureStorageOrchestrationService(new AzureStorageOrchestrationServiceSettings()
			{
				StorageConnectionString = "connection",
				TaskHubName = "Prototype"
			});

			orchestrationService.CreateIfNotExistsAsync().Wait();
			TaskHubWorker taskHub = new TaskHubWorker(orchestrationService);
			try
			{
				taskHub.AddTaskOrchestrations(
					typeof(LetterCountOrchestration)
					);

				taskHub.AddTaskActivities(
					new AddTask(),
					new LogTask()
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
