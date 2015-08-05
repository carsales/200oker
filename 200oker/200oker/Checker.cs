using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using CsQuery;

using HtmlParserSharp.Core;


namespace _200oker
{
    public class Checker
    {
        public ConcurrentDictionary<string, HttpStatusCode?> Results { get; set; }

        public Checker()
        {
            Results = new ConcurrentDictionary<string, HttpStatusCode?>();
        }

        public void PerformCheck(string url, string childSelector)
        {
            // check that it hasn't been done already
            if (Results.ContainsKey(url))
                return;

            // put in placeholder value
            Results.TryAdd(url, null);

            
           

            var req = (HttpWebRequest) WebRequest.Create(url);
            req.UserAgent = "200OKer (https://github.com/carsales/200oker)";
            req.Timeout = 60000; // 5 * 60 * 1000; // 5 minutes

            try
            {
                using (var resp = (HttpWebResponse)req.GetResponse())
                {
                    Results.TryUpdate(url, resp.StatusCode, null);
                    //out put
                    Console.WriteLine("URL: " + url);
                    Console.WriteLine("Status Code: " + (int)resp.StatusCode);
                    Console.WriteLine("Status Description: " + resp.StatusDescription + "\n");

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
                Console.WriteLine("URL: " + url);
                Console.WriteLine(ex.Status);
                Console.WriteLine("Status Code: " + HttpStatusCode.NotFound);
                Results.TryUpdate(url, HttpStatusCode.NotFound, null);

//                if (ex.Response != null)
//                {
//                    // can use ex.Response.Status, .StatusDescription
//                    if (ex.Response.ContentLength != 0)
//                    {
//                        using (var stream = ex.Response.GetResponseStream())
//                        {
//                            using (var reader = new StreamReader(stream))
//                            {
//                                Console.WriteLine(reader.ReadToEnd());
//                            }
//                        }
//                    }
//                }
            }
            
        }

        public void CheckChildren(string parentUrl, string childSelector, string html)
        {
            var dom = new CQ(html);
            var links = dom.Select(childSelector);

            Parallel.ForEach(links, new ParallelOptions() {MaxDegreeOfParallelism = 3}, o =>
            {
                var url = o.Attributes["href"];
                if (String.IsNullOrWhiteSpace(url))
                    return;
                if (url.StartsWith("http://crtl"))
                {
                    //ignore media motive ads
                    return;
                }
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

                PerformCheck(url, null);
            });
        }
    }
}