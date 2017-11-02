using System.Collections.Generic;
using Verse;
using RimWorld;

namespace SeasonalWeather.Utils
{
    /// <summary>
    /// provides IsHashIntervalTick implementations with caching
    /// </summary>
    static class HashCache
    {
        private static Dictionary<string, int> hashCache = new Dictionary<string, int>();

        #region DateNotifier
        public static bool IsHashIntervalTick(this DateNotifier dn, int interval)
        {
            return dn.HashOffsetTicks() % interval == 0;
        }

        public static int HashOffsetTicks(this DateNotifier dn)
        {
            return Find.TickManager.TicksGame + dn.GetHashOffset();
        }

        public static int GetHashOffset(this DateNotifier dn)
        {
            if(!hashCache.TryGetValue(dn.ToString(), out int val))
            {
                val = dn.GetHashCode().HashOffset();
                hashCache.Add(dn.ToString(), val);
            }
            return val;
        }
        #endregion DateNotifie

        #region WeatherWorker
        public static int HashOffsetTicks(this WeatherWorker w)
        {
            return Find.TickManager.TicksGame + w.GetHashOffset();
        }

        public static bool IsHashIntervalTick(this WeatherWorker w, int interval)
        {
            return w.HashOffsetTicks() % interval == 0;
        }

        public static int GetHashOffset(this WeatherWorker ww)
        {
            if (!hashCache.TryGetValue(ww.ToString(), out int val))
            {
                val = ww.GetHashCode().HashOffset();
                hashCache.Add(ww.ToString(), val);
            }
            return val;
        }
        #endregion WeatherWorker

    }

}
