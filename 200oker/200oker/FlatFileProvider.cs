using System;
using System.Collections.Generic;
using System.IO;

namespace _200oker
{
    public class FlatFileProvider
    {
        public List<Check> GetChecks(string filename)
        {
            var checks = new List<Check>();

            if (!File.Exists(filename))
                throw new ArgumentException("Input file {0} not found", filename);

            using (var fr = File.OpenRead(filename))
            using (var sr = new StreamReader(fr))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (!String.IsNullOrWhiteSpace(line))
                    {
                        var check = ParseCheckLine(line);
                        if (check != null)
                            checks.Add(check);
                    }
                }
            }

            return checks;
        }

        private Check ParseCheckLine(string line)
        {
            var lineParts = line.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            var url = lineParts[0].Trim();
            string childSelector = null;
            if (lineParts.Length > 1)
                childSelector = lineParts[1].Trim();

            return new Check()
            {
                Url = url,
                ChildSelector = childSelector,
            };
        }

    }
}
