using System.Collections.Generic;

namespace ExcelMerge.GUI.Settings
{
    public static class EMEnvironmentValue
    {
        private static readonly Dictionary<string, string> ValueTable = new Dictionary<string, string>();

        public static string Get(string key)
        {
            if (ValueTable.ContainsKey(key))
                return ValueTable[key];

            return string.Empty;
        }

        public static void Set(string key, string value)
        {
            if (ValueTable.ContainsKey(key))
            {
                ValueTable[key] = value;
            }
            else
            {
                ValueTable.Add(key, value);
            }
        }
    }
}
