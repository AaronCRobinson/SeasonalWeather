using System.Collections.Generic;
using System.Reflection;
using Verse;
using RimWorld;
using Harmony;

using SeasonalWeather.Utils;

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
                case Season.PermanentSummer:
                    map.Biome.baseWeatherCommonalities = this.summer;
                    break;
                case Season.Fall:
                    map.Biome.baseWeatherCommonalities = this.fall;
                    break;
                case Season.Winter:
                case Season.PermanentWinter:
                    map.Biome.baseWeatherCommonalities = this.winter;
                    break;
            }
        }
    }

    [StaticConstructorOnStartup]
    static class SeasonalWeatherExtensionPatches
    {
        private const int TickerTypeLong = 2000;

        private static MethodInfo MI_FindPlayerHomeWithMinTimezone = AccessTools.Method(typeof(DateNotifier), "FindPlayerHomeWithMinTimezone");
        private static FieldInfo FI_lastSeason = AccessTools.Field(typeof(DateNotifier), "lastSeason");

        static SeasonalWeatherExtensionPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.whyisthat.seasonalweather.seasonalweatherextension");

            harmony.Patch(AccessTools.Method(typeof(GameComponentUtility), nameof(GameComponentUtility.FinalizeInit)), null, new HarmonyMethod(typeof(SeasonalWeatherExtensionPatches), nameof(FinalizeInit)));
            harmony.Patch(AccessTools.Method(typeof(DateNotifier), nameof(DateNotifier.DateNotifierTick)), new HarmonyMethod(typeof(SeasonalWeatherExtensionPatches), nameof(DateNotifierTickPrefix)), null);
        }

        // NOTE: should this be somewhere else?
        public static void FinalizeInit() => CheckBaseWeatherCommonalities(Find.DateNotifier);

        public static void DateNotifierTickPrefix(DateNotifier __instance)
        {
            if (__instance.IsHashIntervalTick(TickerTypeLong))
                CheckBaseWeatherCommonalities(__instance);
        }

        public static void CheckBaseWeatherCommonalities(DateNotifier __instance)
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

        private static Season GetSeason(this Map map)
        {
            float latitude = Find.WorldGrid.LongLatOf(map.Tile).y;
            float longitude = Find.WorldGrid.LongLatOf(map.Tile).x;
            return GenDate.Season((long)Find.TickManager.TicksAbs, latitude, longitude);
        }
    }

}