using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DurableTaskFrameworkPrototype
{
	using DurableTask.Core;

	public class LetterCountOrchestration : TaskOrchestration<int [], string>
	{
		public override Task<int[]> RunTask(OrchestrationContext context, string input)
		{
			input = input.ToLower();
			foreach(char c in input)
			{
				context.ScheduleTask<int>(typeof(AddTask), c);
			}
		}
	}
}
