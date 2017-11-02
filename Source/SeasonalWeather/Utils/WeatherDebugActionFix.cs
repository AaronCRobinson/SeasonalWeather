using Harmony;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Verse;

namespace SeasonalWeather.Utils
{
#if DEBUG
    [StaticConstructorOnStartup]
    static class WeatherDebugActionFix
    {
        static BindingFlags bf = BindingFlags.NonPublic | BindingFlags.Instance;
        static string varName = "localWeather";
        static Type anonType = typeof(Dialog_DebugActionsMenu).GetNestedTypes(BindingFlags.NonPublic).First(t => t.HasAttribute<CompilerGeneratedAttribute>() && t.GetField(varName, bf) != null);
        static MethodInfo anonMethod = anonType.GetMethods(bf).First(); // assuming first for now...

        static WeatherDebugActionFix()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.whyisthat.weatherdebug");
            Log.Message("anonMethod: " + anonMethod.Name);
            harmony.Patch(anonMethod, null, new HarmonyMethod(typeof(WeatherDebugActionFix).GetMethod(nameof(Postfix))));
        }

        public static void Postfix()
        {
            Log.Message("Setting curWeatherDuration");
            Traverse.Create(Find.VisibleMap.weatherDecider).Field("curWeatherDuration").SetValue(Find.VisibleMap.weatherManager.curWeather.durationRange.RandomInRange);
        }

    }
#endif
}
