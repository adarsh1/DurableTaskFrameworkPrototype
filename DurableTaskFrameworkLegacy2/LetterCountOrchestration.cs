using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DurableTaskFrameworkPrototype
{
	using DurableTask;

	public class LetterCountOrchestration : TaskOrchestration<int [], string>
	{
		public override async Task<int[]> RunTask(OrchestrationContext context, string input)
		{
			input = input.ToLower();
			var taskList = new List<Task>();
			foreach(char c in input)
			{
				taskList.Add(context.ScheduleTask<int>(typeof(AddTask), c));
			}

			await Task.WhenAll(taskList);

			await context.ScheduleTask<bool>(typeof(LogTask), AddTask.LetterCounts.Value);

			return AddTask.LetterCounts.Value;
		}
	}
}
