using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using Verse;
using RimWorld;
using Harmony;

namespace SeasonalWeather
{
    class FollowsRainExtension : DefModExtension
    {
        public bool followsRain = false;
    }

    class FollowsRainHelper
    {
        //check that all predicates are true...
        static public bool FollowsRain(Map map, WeatherDef weather)
        {
            FollowsRainExtension ext = weather.GetModExtension<FollowsRainExtension>();
            if (ext != null && ext.followsRain && map.weatherManager.lastWeather.rainRate <= 0.1f) return false; // CurrentWeatherCommonality => defines `rain` vs `sprinkle`
            return true;
        }
    }

    [StaticConstructorOnStartup]
    class WeatherProceedsExtensionPatches
    {
        static WeatherProceedsExtensionPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.whyisthat.seasonalweather.weatherproceedsextension");

            harmony.Patch(AccessTools.Method(typeof(WeatherDecider), "CurrentWeatherCommonality"), null, null, new HarmonyMethod(typeof(WeatherProceedsExtensionPatches), nameof(Transpiler)));
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            int i;
            for (i = 0; i < instructionList.Count; i++)
            {
                yield return instructionList[i];
                if (instructionList[i].opcode == OpCodes.Stloc_0)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, typeof(WeatherManager).GetField("map"));
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, typeof(FollowsRainHelper).GetMethod(nameof(FollowsRainHelper.FollowsRain)));
                    Label @continue = il.DefineLabel();
                    yield return new CodeInstruction(OpCodes.Brtrue, @continue);
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 0.0f);
                    yield return new CodeInstruction(OpCodes.Ret);
                    instructionList[++i].labels.Add(@continue);
                    yield return instructionList[i];
                    // NOTE: could break here for avoiding future check...
                }
            }
        }
    }

}