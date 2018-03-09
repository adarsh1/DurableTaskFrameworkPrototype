using DurableTask.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DurableTaskFrameworkPrototype
{
	public class AddTask : TaskActivity<char, int>
	{
		public static Lazy<int[]> LetterCounts = new Lazy<int[]>(() => new int[26]);
		protected override int Execute(TaskContext context, char input)
		{
			if (input >= 'a' && input <= 'z')
			{
				Interlocked.Increment(ref LetterCounts.Value[input - 'a']);
				return 1;
			}
			return 0;
		}
	}
}
