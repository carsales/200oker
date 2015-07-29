using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace _200oker
{
    internal class Program
    {
        static readonly FlatFileProvider _provider = new FlatFileProvider();
        static readonly Checker _checker = new Checker();

        private static void Main(string[] args)
        {
            var checks = _provider.GetChecks("checks.txt");

            Console.Write("Checking");

            var sw = Stopwatch.StartNew();
            Parallel.ForEach(checks, new ParallelOptions() { MaxDegreeOfParallelism = 50 }, _checker.PerformCheck);
            sw.Stop();

            Console.WriteLine();
            Console.WriteLine("Performed {0} checks in {1:0.00} seconds.",
                _checker.Visited.Count,
                sw.Elapsed.TotalSeconds);

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }
    }
}