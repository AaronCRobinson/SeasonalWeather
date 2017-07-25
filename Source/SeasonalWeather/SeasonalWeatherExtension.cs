using RimWorld;
using Harmony;
using UnityEngine;
using Verse;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

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

        static BindingFlags bf = BindingFlags.NonPublic | BindingFlags.Instance;
        static string varName = "localWeather";
        static Type anonType = typeof(Dialog_DebugActionsMenu).GetNestedTypes(BindingFlags.NonPublic).First(t => t.HasAttribute<CompilerGeneratedAttribute>() && t.GetField(varName, bf) != null);
        static MethodInfo anonMethod = anonType.GetMethods(bf).First(); // assuming first for now...

        // TODO: convert this to a transpiler
        public static void DateNotifierTickPrefix(DateNotifier __instance)
        {
            Traverse t = Traverse.Create(__instance); // NOTE: rewrite this with manual reflection.

            // copy pasta (RimWorld.DateNotifier)
            Map map = t.Method("FindPlayerHomeWithMinTimezone").GetValue<Map>();
            float latitude = (map == null) ? 0f : Find.WorldGrid.LongLatOf(map.Tile).y;
            float longitude = (map == null) ? 0f : Find.WorldGrid.LongLatOf(map.Tile).x;
            Season season = GenDate.Season((long)Find.TickManager.TicksAbs, latitude, longitude);

            Season lastSeason = t.Field("lastSeason").GetValue<Season>();
            if (season != lastSeason && (lastSeason == Season.Undefined || season != lastSeason.GetPreviousSeason()))
            {
                //Log.Message("SeasonalWeather: season change");
                switch (season)
                {
                    case Season.Spring:
                        map.Biome.baseWeatherCommonalities = map.Biome.GetModExtension<SeasonalWeatherExtension>().spring;
                        break;
                    case Season.Summer:
                        map.Biome.baseWeatherCommonalities = map.Biome.GetModExtension<SeasonalWeatherExtension>().summer;
                        break;
                    case Season.Fall:
                        map.Biome.baseWeatherCommonalities = map.Biome.GetModExtension<SeasonalWeatherExtension>().fall;
                        break;
                    case Season.Winter:
                        map.Biome.baseWeatherCommonalities = map.Biome.GetModExtension<SeasonalWeatherExtension>().winter;
                        break;
                }
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