using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CsQuery;

namespace _200oker
{
    public class Checker
    {
        public ConcurrentBag<string> Visited { get; set; }

        public Checker()
        {
            Visited = new ConcurrentBag<string>();
        }

        public void PerformCheck(Check check)
        {
            if (Visited.Contains(check.Url))
                return;
            Visited.Add(check.Url);

            //Console.WriteLine("Checking {0}...", check.Url);
            Console.Write(".");
            var req = (HttpWebRequest)WebRequest.Create(check.Url);
            req.Timeout = 5 * 60 * 1000; // 5 minutes

            using (var resp = (HttpWebResponse)req.GetResponse())
            {
                check.Result = resp.StatusCode;

                using (var rs = resp.GetResponseStream())
                using (var sr = new StreamReader(rs))
                {
                    var html = sr.ReadToEnd();
                    CheckChildren(check, html);
                }
            }
        }

        public void CheckChildren(Check check, string html)
        {
            if (String.IsNullOrWhiteSpace(check.ChildSelector))
                return;

            var dom = new CQ(html);

            var links = dom.Select(check.ChildSelector);

            Parallel.ForEach(links, new ParallelOptions() { MaxDegreeOfParallelism = 50 }, o =>
              {
                  var url = o.Attributes["href"];
                  if (String.IsNullOrWhiteSpace(url))
                      return;

                  // check for relative url
                  if (!url.StartsWith("http:") &&
                        !url.StartsWith("https:"))
                  {
                      if (url.StartsWith("/"))
                          url = new Uri(check.Url).GetLeftPart(UriPartial.Authority) + url;
                      else
                          url = check.Url + url;
                  }

                  var childCheck = new Check()
                  {
                      Url = url,
                  };
                  check.Children.Add(childCheck);
                  PerformCheck(childCheck);
              });
        }
    }
}
