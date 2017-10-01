using System.Collections.Generic;
using System.Reflection;
using Verse;
using RimWorld;
using Harmony;

// if (weather.favorability == Favorability.VeryGood && this.map.weatherManager.lastWeather.rainRate > 0.1f) => "Double Rainbow"
// map.gameConditionManager.RegisterCondition(gameCondition_PsychicEmanation); => like Psychic Soothe

namespace SeasonalWeather
{
    public class SeasonalWeatherExtension : DefModExtension
    {
        public List<WeatherCommonalityRecord> spring = new List<WeatherCommonalityRecord>();
        public List<WeatherCommonalityRecord> summer = new List<WeatherCommonalityRecord>();
        public List<WeatherCommonalityRecord> fall = new List<WeatherCommonalityRecord>();
        public List<WeatherCommonalityRecord> winter = new List<WeatherCommonalityRecord>();

        public void AdjustBaseWeatherCommonalities(Map map, Season season)
        {
            Log.Message("SeasonalWeather: adjusting baseWeatherCommonalities");
            switch (season)
            {
                case Season.Spring:
                    map.Biome.baseWeatherCommonalities = this.spring;
                    break;
                case Season.Summer:
                    map.Biome.baseWeatherCommonalities = this.summer;
                    break;
                case Season.Fall:
                    map.Biome.baseWeatherCommonalities = this.fall;
                    break;
                case Season.Winter:
                    map.Biome.baseWeatherCommonalities = this.winter;
                    break;
            }
        }
    }

    [StaticConstructorOnStartup]
    class SeasonalWeatherExtensionPatches
    {
        private const int TickerTypeLong = 2000;

        private static MethodInfo MI_FindPlayerHomeWithMinTimezone = AccessTools.Method(typeof(DateNotifier), "FindPlayerHomeWithMinTimezone");
        private static FieldInfo FI_lastSeason = AccessTools.Field(typeof(DateNotifier), "lastSeason");

        static SeasonalWeatherExtensionPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.whyisthat.seasonalweather.seasonalweatherextension");

            harmony.Patch(AccessTools.Method(typeof(DateNotifier), nameof(DateNotifier.DateNotifierTick)), new HarmonyMethod(typeof(SeasonalWeatherExtensionPatches), nameof(DateNotifierTickPrefix)), null);
        }

        // NOTE: should this be somewhere else?
        public static void FinalizeInit()
        {
            Map map = (Map)MI_FindPlayerHomeWithMinTimezone.Invoke(Find.DateNotifier, new object[] { });
            SeasonalWeatherExtension ext = map.Biome.GetModExtension<SeasonalWeatherExtension>();
            
            if (ext != null)
            {
                Season season = map.GetSeason();
                ext.AdjustBaseWeatherCommonalities(map, season);
            }
            else
            {
                LogUtility.MessageOnce("Custom biome does not have Seasonal Weather data.", 725491);
            }

        }

        public static void DateNotifierTickPrefix(DateNotifier __instance)
        {

            if (__instance.IsHashIntervalTick(TickerTypeLong))
            {
                Map map = (Map)MI_FindPlayerHomeWithMinTimezone.Invoke(__instance, new object[] { });
                SeasonalWeatherExtension ext = map.Biome.GetModExtension<SeasonalWeatherExtension>();

                if (ext != null)
                {
                    Season season = map.GetSeason();
                    Season lastSeason = (Season)FI_lastSeason.GetValue(__instance);
                    if (season != lastSeason && (lastSeason == Season.Undefined || season != lastSeason.GetPreviousSeason()))
                    {
                        Log.Message("SeasonalWeather: season changed");
                        ext.AdjustBaseWeatherCommonalities(map, season);
                    }
                }
                else
                {
                    LogUtility.MessageOnce("Custom biome does not have Seasonal Weather data.", 725491);
                }
            }

        }
    }

    public static class SeasonHelper
    {
        public static Season GetSeason(this Map map)
        {
            float latitude = Find.WorldGrid.LongLatOf(map.Tile).y;
            float longitude = Find.WorldGrid.LongLatOf(map.Tile).x;
            return GenDate.Season((long)Find.TickManager.TicksAbs, latitude, longitude);
        }
    }

}