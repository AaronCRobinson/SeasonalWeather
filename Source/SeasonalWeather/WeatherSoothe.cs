using RimWorld;
using Harmony;
using Verse;
using System;

namespace SeasonalWeather
{
    public class GameCondition_WeatherEmanation : GameCondition
    {
        public WeatherDroneLevel weatherDroneLevel;
    }

    public class WeatherConditionDef : GameConditionDef
    {
        public WeatherDroneLevel weatherDroneLevel;
    }

    [StaticConstructorOnStartup]
    class WeatherSoothe
    {
        static WeatherSoothe()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.whyisthat.seasonalweather.weathersoothe");

            harmony.Patch(AccessTools.Method(typeof(WeatherDecider), nameof(WeatherDecider.StartNextWeather)), null, new HarmonyMethod(typeof(WeatherSoothe), nameof(StartNextWeatherPostfix)));

            // WeatherDebugActionFix
            harmony.Patch(AccessTools.Method(typeof(WeatherDebugActionFix), nameof(WeatherDebugActionFix.Postfix)), null, new HarmonyMethod(typeof(WeatherSoothe), nameof(StartNextWeatherPostfix)));
        }

        // NOTE: avoiding use of instance here to add debug compatibility.
        public static void StartNextWeatherPostfix()
        {
            // NOTE: there should be a better home for this code...
            Map map = Find.VisibleMap;
            WeatherDef curWeather = map.weatherManager.curWeather;
            if (curWeather.favorability == Favorability.VeryGood)
            {
                // NOTE: look up weather condition def based on weather name.
                // NOTE: if more of these are created, consider a better location
                WeatherConditionDef def = DefDatabase<WeatherConditionDef>.GetNamed(curWeather.defName);
                GameCondition_WeatherEmanation gameCondition_WeatherEmanation = (GameCondition_WeatherEmanation)GameConditionMaker.MakeCondition(def, Traverse.Create(map.weatherDecider).Field("curWeatherDuration").GetValue<int>(), 0);
                gameCondition_WeatherEmanation.weatherDroneLevel = def.weatherDroneLevel;
                Find.VisibleMap.gameConditionManager.RegisterCondition(gameCondition_WeatherEmanation);
                Find.LetterStack.ReceiveLetter(def.label, def.description, LetterDefOf.Good, null);
            }
        }
    }

    public class ThoughtWorker_WeatherDrone : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            WeatherDroneLevel weatherDroneLevel = WeatherDroneLevel.None;
            GameCondition_WeatherEmanation activeCondition = p.Map.gameConditionManager.GetActiveCondition<GameCondition_WeatherEmanation>();
            if (activeCondition != null && activeCondition.weatherDroneLevel > weatherDroneLevel)
            {
                weatherDroneLevel = activeCondition.weatherDroneLevel;
            }
            switch (weatherDroneLevel)
            {
                case WeatherDroneLevel.None:
                    return false;
                case WeatherDroneLevel.GoodLow:
                    return ThoughtState.ActiveAtStage(0);
                default:
                    throw new NotImplementedException();
            }
        }

    }

    public enum WeatherDroneLevel : byte
    {
        None,
        GoodLow,
    }

}