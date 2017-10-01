using System.Collections.Generic;
using Verse;
using System.Reflection;
using Harmony;

namespace SeasonalWeather
{
    class LogUtility
    {
        private static FieldInfo FI_usedKeys = AccessTools.Field(typeof(Log), "usedKeys");
        private static HashSet<int> usedKeys = (HashSet<int>)FI_usedKeys.GetValue(null);
        
        // Verse.Log
        public static void MessageOnce(string text, int key)
        {
            if (LogUtility.usedKeys.Contains(key)) return;
            LogUtility.usedKeys.Add(key);
            Log.Message(text);
        }
    }

}
