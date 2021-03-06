﻿using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CsQuery;

namespace _200oker
{
    public class Checker
    {
        public ConcurrentDictionary<string, HttpStatusCode?> Results { get; set; }

        public Checker()
        {
            Results = new ConcurrentDictionary<string, HttpStatusCode?>();
        }

        public void PerformCheck(string url, string childSelector, bool dupeCheck = false)
        {
            // check that it hasn't been done already
            if (dupeCheck && Results.ContainsKey(url))
                return;

            // put in placeholder value
            Results.TryAdd(url, null);

            var req = (HttpWebRequest)WebRequest.Create(url);
            req.UserAgent = "200OKer (https://github.com/carsales/200oker)";
            req.Timeout = Config.TimeoutInSeconds * 1000;

            try
            {
                using (var resp = (HttpWebResponse)req.GetResponse())
                {
                    SetResultFromResponse(url, resp);

                    if (resp.StatusCode == HttpStatusCode.OK && !String.IsNullOrWhiteSpace(childSelector))
                    {
                        using (var rs = resp.GetResponseStream())
                        {
                            if (rs != null)
                            {
                                using (var sr = new StreamReader(rs))
                                {
                                    var html = sr.ReadToEnd();
                                    CheckChildren(url, childSelector, html);
                                }
                            }
                        }

                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    var resp = (HttpWebResponse)ex.Response;
                    SetResultFromResponse(url, resp);
                }
                else
                {
                    Console.WriteLine("URL: " + url);
                    Console.WriteLine("Other error:" + ex.Status + " - " + ex.Message);
                }
            }
        }

        private void SetResultFromResponse(string url, HttpWebResponse resp)
        {
            Console.WriteLine("URL: {0} - Status: {1} - Description: {2}", url, resp.StatusCode, resp.StatusDescription);
            Results.TryUpdate(url, resp.StatusCode, null);
        }

        public void CheckChildren(string parentUrl, string childSelector, string html)
        {
            var dom = new CQ(html);
            var links = dom.Select(childSelector);
            var childrenChecked = 0;

            Parallel.ForEach(links, new ParallelOptions() { MaxDegreeOfParallelism = Config.MaxChildThreads }, o =>
            {
                childrenChecked++;
                var url = o.Attributes["href"];
                // ignore empty
                if (String.IsNullOrWhiteSpace(url))
                    return;
                // ignore javascript
                if (url.ToLower().StartsWith("javascript:"))
                    return;

                // jump out if url should be ignored
                if (Config.IgnoreUrlsStartingWith.Any(ignore => url.StartsWith(ignore)))
                    return;

                // check for relative url
                if (!url.StartsWith("http://") &&
                    !url.StartsWith("https://") &&
                    !url.StartsWith("//"))
                {
                    if (url.StartsWith("/"))
                        url = new Uri(parentUrl).GetLeftPart(UriPartial.Authority) + url;

                    else
                        url = parentUrl + url;
                }

                // check for protocol relative url
                if (url.StartsWith("//"))
                {
                    url = new Uri(parentUrl).GetLeftPart(UriPartial.Scheme) + url.Substring(2);
                }

                PerformCheck(url, null, true);
            });

            Console.WriteLine("Checked {0} children of {1}", childrenChecked, parentUrl);
        }
    }
}