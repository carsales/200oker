using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace _200oker
{
    internal class Program
    {
        private static readonly FlatFileProvider _provider = new FlatFileProvider();
        private static readonly Checker _checker = new Checker();

        private static int Main(string[] args)
        {
            PrintApplicationInfo();

            var inputfile = "checks.txt";
            if (args.Length > 0)
                inputfile = args[0];

            List<UrlToCheck> checks;
            try
            {
                checks = _provider.GetChecks(inputfile);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }

            Console.WriteLine("Checking {0} initial urls...", checks.Count);

            var sw = Stopwatch.StartNew();
            Parallel.ForEach(checks, new ParallelOptions() { MaxDegreeOfParallelism = Config.MaxParentThreads },
                c => _checker.PerformCheck(c.Url, c.ChildSelector));
            sw.Stop();

            Console.WriteLine();
            Console.WriteLine("Performed {0} checks in {1:0.00} seconds.",
                _checker.Results.Count,
                sw.Elapsed.TotalSeconds);

            var counts = from r in _checker.Results
                         group r by r.Value
                into status
                         select new { status = status.Key, count = status.Count() };

            foreach (var count in counts)
            {
                Console.WriteLine("{0} urls had a status of {1}", count.count, count.status);
            }

            var problems = _checker.Results.Where(x => x.Value != HttpStatusCode.OK).ToList();
            if (problems.Any())
            {
                Console.WriteLine("Problem URLs:");
                foreach (var problem in problems)
                {
                    Console.WriteLine("URL: {0} - Status: {1}", problem.Key, problem.Value);
                }
            }
            else
            {
                Console.WriteLine("No problem URLs, no worries mate.");
            }

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }

            return _checker.Results.Values.All(x => x == HttpStatusCode.OK) ? 0 : -1;
        }

        private static void PrintApplicationInfo()
        {
            var version = Assembly.GetCallingAssembly().GetName().Version.ToString();
            Console.WriteLine("200OKer v{0} - http://github.com/carsales/200oker", version);
        }
    }
}