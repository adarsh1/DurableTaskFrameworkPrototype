using DurableTask.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DurableTaskFrameworkPrototype
{
	public class LogTask : TaskActivity<int[], bool>
	{
		protected override bool Execute(TaskContext context, int[] a)
		{
			int i;
			for(i =0 ; i<a.Length ; i++){
				Console.Write(a[i] + "\t");
			}
			Console.WriteLine();

			for (i = 0; i < a.Length; i++)
			{
				Console.Write((char)('a'+i) + "\t");
			}

			Console.WriteLine();
			Console.WriteLine();

			return true;
		}
	}
}
