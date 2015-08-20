using System;
using System.Collections.Generic;
using System.Configuration;

namespace _200oker
{
    public static class Config
    {
        public static List<string> IgnoreUrlsStartingWith { get; set; }
        public static int MaxParentThreads { get; set; }
        public static int MaxChildThreads { get; set; }
        public static int TimeoutInSeconds { get; set; }

        static Config()
        {
            IgnoreUrlsStartingWith = new List<string>();
            MaxParentThreads = 5;
            MaxChildThreads = 5;
            TimeoutInSeconds = 60;

            // try to read settings from app.config

            var urls = GetSettingValueAsStringList("IgnoreUrlsStartingWith");
            if (urls != null)
                IgnoreUrlsStartingWith = urls;

            var mpt = GetSettingValueAsInt("maxParentThreads");
            if (mpt != null)
                MaxParentThreads = mpt.Value;

            var mct = GetSettingValueAsInt("maxChildThreads");
            if (mct != null)
                MaxChildThreads = mct.Value;

            var timeout = GetSettingValueAsInt("timeoutInSeconds");
            if (timeout != null)
                TimeoutInSeconds = timeout.Value;
        }

        private static int? GetSettingValueAsInt(string key)
        {
            var value = ConfigurationManager.AppSettings[key];
            if (String.IsNullOrWhiteSpace(value))
                return null;

            var intValue = 0;
            if (!int.TryParse(value, out intValue))
                return null;

            return intValue;
        }

        private static List<string> GetSettingValueAsStringList(string key)
        {
            var value = ConfigurationManager.AppSettings[key];
            if (String.IsNullOrWhiteSpace(value))
                return null;

            var list = new List<string>();
            foreach (var s in value.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
            {
                list.Add(s);
            }
            return list;
        }
    }
}