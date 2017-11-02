using Harmony;
using RimWorld;
using Verse;

namespace SeasonalWeather
{
    [StaticConstructorOnStartup]
    public class GrowthRatePatches
    {
        static GrowthRatePatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.whyisthat.seasonalweather.growthrate");
            harmony.Patch(AccessTools.Property(typeof(Plant), nameof(Plant.GrowthRate)).GetGetMethod(), null, new HarmonyMethod(typeof(GrowthRatePatches), nameof(GrowRateFactor_Rain)));
        }

        // NOTE: consider other ways of doing this math...
        public static void GrowRateFactor_Rain(Plant __instance, ref float __result)
        {
            if (__instance.Map.roofGrid.Roofed(__instance.Position)) return;
            __result *= (1f + __instance.Map.weatherManager.curWeather.rainRate * 0.2f);
        }
    }

}
