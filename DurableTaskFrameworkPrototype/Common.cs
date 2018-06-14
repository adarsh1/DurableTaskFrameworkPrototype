using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DurableTaskFrameworkPrototype
{
    internal static class Common
    {
        internal static async Task<int> RandGen()
        {
            await Task.Run(() =>
            {
                var time = DateTime.UtcNow + TimeSpan.FromSeconds(1);
                while (DateTime.UtcNow < time)
                {
                    Console.WriteLine("Waiting");
                }
            });

            Random r = new Random();
            return r.Next(1, 999999);
        }
    }
}
