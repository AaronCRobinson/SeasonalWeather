using Harmony;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using System.Reflection.Emit;

namespace SeasonalWeather
{
    [StaticConstructorOnStartup]
    public class GrowthRatePatches
    {
        static GrowthRatePatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.whyisthat.seasonalweather.growthrate");

            harmony.Patch(AccessTools.Property(typeof(Plant), nameof(Plant.GrowthRate)).GetGetMethod(), null, null, new HarmonyMethod(typeof(GrowthRatePatches), nameof(GrowthRateTranspiler)));
        }

        public static IEnumerable<CodeInstruction> GrowthRateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo growthRateFactorRainMethodInfo = AccessTools.Method(typeof(PlantHelper), nameof(PlantHelper.GrowthRateFactor_Rain));

            List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
            int i;
            for (i = 0; i < instructionList.Count - 1; i++) yield return instructionList[i];
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, growthRateFactorRainMethodInfo);
            yield return new CodeInstruction(OpCodes.Mul);
            yield return instructionList[i];
        }
    }

    public static class PlantHelper
    {
        public static float GrowthRateFactor_Rain(this Plant p)
        {
            return p.Map.roofGrid.Roofed(p.Position) ? 1f : (p.Map.weatherManager.curWeather.rainRate * 0.2f + 1f);
        }
    }
}
