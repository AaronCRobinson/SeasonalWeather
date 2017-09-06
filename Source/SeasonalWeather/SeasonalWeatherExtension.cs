using RimWorld;
using Harmony;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Reflection;

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
    }

    [StaticConstructorOnStartup]
    class SeasonalWeatherExtensionPatches
    {
        static SeasonalWeatherExtensionPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.whyisthat.seasonalweather.seasonalweatherextension");

            harmony.Patch(AccessTools.Method(typeof(DateNotifier), nameof(DateNotifier.DateNotifierTick)), new HarmonyMethod(typeof(SeasonalWeatherExtensionPatches), nameof(DateNotifierTickPrefix)), null);
        }

        static private MethodInfo MI_FindPlayerHomeWithMinTimezone = AccessTools.Method(typeof(DateNotifier), "FindPlayerHomeWithMinTimezone");
        static private FieldInfo FI_lastSeason = AccessTools.Field(typeof(DateNotifier), "lastSeason");

        // TODO: convert this to a transpiler
        public static void DateNotifierTickPrefix(DateNotifier __instance)
        {
            // NOTE: can map be null?
            Map map = (Map)MI_FindPlayerHomeWithMinTimezone.Invoke(__instance, new object[] { });
            SeasonalWeatherExtension ext = map.Biome.GetModExtension<SeasonalWeatherExtension>();

            if (ext != null)
            {
                // copy pasta (RimWorld.DateNotifier)
                float latitude = Find.WorldGrid.LongLatOf(map.Tile).y;
                float longitude = Find.WorldGrid.LongLatOf(map.Tile).x;
                Season season = GenDate.Season((long)Find.TickManager.TicksAbs, latitude, longitude);

                Season lastSeason = (Season)FI_lastSeason.GetValue(__instance);
                if (season != lastSeason && (lastSeason == Season.Undefined || season != lastSeason.GetPreviousSeason()))
                {
                    Log.Message("SeasonalWeather: season change");
                    switch (season)
                    {
                        case Season.Spring:
                            map.Biome.baseWeatherCommonalities = ext.spring;
                            break;
                        case Season.Summer:
                            map.Biome.baseWeatherCommonalities = ext.summer;
                            break;
                        case Season.Fall:
                            map.Biome.baseWeatherCommonalities = ext.fall;
                            break;
                        case Season.Winter:
                            map.Biome.baseWeatherCommonalities = ext.winter;
                            break;
                    }
                }

            }
            else
            {
                // NOTE: see how expensive this ends up being.
                LogUtility.MessageOnce("Custom biome does not have Seasonal Weather data.",725491);
            }

        }
    }

    [StaticConstructorOnStartup]
    public class WeatherOverlay_DustCloud : SkyOverlay
    {
        private static readonly Material DustCloudOverlay = MatLoader.LoadMat("Weather/FogOverlayWorld", -1);

        public WeatherOverlay_DustCloud()
        {
            this.worldOverlayMat = DustCloudOverlay;
            this.worldOverlayPanSpeed1 = 0.008f;
            this.worldPanDir1 = new Vector2(-1f, -0.26f); //new Vector2(1f, 1f);
            this.worldPanDir1.Normalize();
            this.worldOverlayPanSpeed2 = 0.012f;
            this.worldPanDir2 = new Vector2(-1f, -0.24f); //new Vector2(1f, -1f);
            this.worldPanDir2.Normalize();
        }
    }

    [StaticConstructorOnStartup]
    public class WeatherOverlay_DustParticles : SkyOverlay
    {
        private static readonly Material DustParticlesOverlay = MatLoader.LoadMat("Weather/SnowOverlayWorld", -1);

        public WeatherOverlay_DustParticles()
        {
            this.worldOverlayMat = DustParticlesOverlay;
            this.worldOverlayPanSpeed1 = 0.018f;
            this.worldPanDir1 = new Vector2(-1f, -0.26f); //new Vector2(1f, 1f);
            this.worldPanDir1.Normalize();
            this.worldOverlayPanSpeed2 = 0.022f;
            this.worldPanDir2 = new Vector2(-1f, -0.24f); //new Vector2(1f, -1f);
            this.worldPanDir2.Normalize();
        }
    }

}