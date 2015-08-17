using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;

namespace _200oker
{
    public class FlatFileProvider
    {
        public List<UrlToCheck> GetChecks(string filename)
        {
            var checks = new List<UrlToCheck>();

            if (!File.Exists(filename))
                throw new ArgumentException(String.Format("Input file {0} not found", filename));

            using (var fr = File.OpenRead(filename))
            using (var sr = new StreamReader(fr))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();

                    // ignore empty and comment lines
                    if (!String.IsNullOrWhiteSpace(line) &&
                        !line.StartsWith("#"))
                    {
                        var check = ParseCheckLine(line);
                        if (check != null)
                            checks.Add(check);
                    }
                }
            }

            return checks;
        }

        private UrlToCheck ParseCheckLine(string line)
        {
            var lineParts = line.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            var url = lineParts[0].Trim();
            string childSelector = null;
            if (lineParts.Length > 1)
                childSelector = lineParts[1].Trim();

            return new UrlToCheck()
            {
                Url = url,
                ChildSelector = childSelector,
            };
        }

    }
}
