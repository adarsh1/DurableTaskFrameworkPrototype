using DurableTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DurableTaskFrameworkPrototype
{
    public class Counter : TaskOrchestration<int, int>
    {
        TaskCompletionSource<string> waitForOperationHandle;

        public override async Task<int> RunTask(OrchestrationContext context, int currentValue)
        {
            string operation = await this.WaitForOperation();

            bool done = false;
            switch (operation?.ToLowerInvariant())
            {
                case "incr":
                    currentValue++;
                    break;
                case "decr":
                    currentValue--;
                    break;
                case "end":
                    done = true;
                    break;
            }

            Console.WriteLine($"Value:{currentValue}");

            if (!done)
            {
                context.ContinueAsNew(currentValue);
            }

            return currentValue;
        }

        async Task<string> WaitForOperation()
        {
            this.waitForOperationHandle = new TaskCompletionSource<string>();
            string operation = await this.waitForOperationHandle.Task;
            this.waitForOperationHandle = null;
            return operation;
        }

        public override void OnEvent(OrchestrationContext context, string name, string input)
        {
            if (this.waitForOperationHandle != null)
            {
                this.waitForOperationHandle.SetResult(input);
            }
        }

        public class ChildWorkflow : TaskOrchestration<string, int>
        {
            public override Task<string> RunTask(OrchestrationContext context, int input)
            {
                return Task.FromResult($"Child '{input}' completed.");
            }
        }

        public class ParentWorkflow : TaskOrchestration<string, bool>
        {
            // HACK: This is just a hack to communicate result of orchestration back to test
            public static string Result;

            public override async Task<string> RunTask(OrchestrationContext context, bool waitForCompletion)
            {
                var results = new Task<string>[5];
                for (int i = 0; i < 5; i++)
                {
                    await Task.Delay(1000);
                    Console.WriteLine("Creating Child");
                    Task<string> r = context.CreateSubOrchestrationInstance<string>(typeof(ChildWorkflow), i);
                    if (waitForCompletion)
                    {
                        await r;
                    }

                    results[i] = r;
                }

                string[] data = await Task.WhenAll(results);
                Result = string.Concat(data);
                return Result;
            }
        }
    }
}
