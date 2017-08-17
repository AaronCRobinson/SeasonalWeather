using System;
using System.Collections.Generic;
using Verse;
using Harmony;

namespace SeasonalWeather
{
    public static class DynamicWeatherDefs
    {
        public class DynamicWeatherDefHelper : DynamicDefHelper<WeatherDef> { }

        public static DynamicWeatherDefHelper dynamicWeatherDefHelper = new DynamicWeatherDefHelper();
    }

    public abstract class DynamicDefHelper<T> where T : Def, new()
    {
        private Dictionary<string, T> defs = new Dictionary<string, T>();

        public void SetDynamicDet(string defName, bool add = false)
        {
            if (add)
            {
                if (DefDatabase<T>.GetNamed(defName, false) == null)
                {   // Add def and references
                    this.defs.TryGetValue(defName, out T def);
                    DefDatabase<T>.Add(def);
                }
            }
            else
            {
                if (DefDatabase<WeatherDef>.GetNamed(defName, false) != null)
                {   // Remove def and references
                    T def = DefDatabase<T>.GetNamed(defName);
                    this.defs.Add(defName, def);
                    AccessTools.Method(typeof(DefDatabase<T>), "Remove").Invoke(null, new[] { def });
                }
            }
        }
    }
}