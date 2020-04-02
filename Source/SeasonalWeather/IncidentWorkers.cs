using UnityEngine;
using Verse;
using RimWorld;

namespace SeasonalWeather
{
    public class IncidentWorker_Earthquake : IncidentWorker
    {
        private int duration;

        protected override bool CanFireNowSub(IncidentParms parms) => 
            SeasonalWeatherMod.settings.enableEarthquakes 
            && !((Map)parms.target).gameConditionManager.ConditionIsActive(GameConditionDefOf.Earthquake);

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            float points = parms.points;
            float richterMagnitude = EarthquakeHelper.GetMagnitudeWithRand(points);
            GetDuration(richterMagnitude);
            GameCondition_Earthquake gameCondition_Earthquake = (GameCondition_Earthquake)GameConditionMaker.MakeCondition(GameConditionDefOf.Earthquake, this.duration);
            gameCondition_Earthquake.Magnitude = richterMagnitude;
            map.gameConditionManager.RegisterCondition(gameCondition_Earthquake);
            return true;
        }

        // multiply duration by magnitude
        public void GetDuration(float magnitude) => this.duration = Mathf.CeilToInt(this.def.GetDuration() * magnitude);

    }

    public class IncidentWorker_Wildfire : IncidentWorker
    {
        private int duration;

        protected override bool CanFireNowSub(IncidentParms parms) => 
            SeasonalWeatherMod.settings.enableWildfires 
            && !((Map)parms.target).gameConditionManager.ConditionIsActive(GameConditionDefOf.Wildfire);

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            //float points = parms.points;
            this.duration = this.def.GetDuration();
            GameCondition_Wildfire gameCondition_Wildfire = (GameCondition_Wildfire)GameConditionMaker.MakeCondition(GameConditionDefOf.Wildfire, this.duration);
            map.gameConditionManager.RegisterCondition(gameCondition_Wildfire);
            return true;
        }

    }

    internal static class IncidentDef_Helper
    {
        public static int GetDuration(this IncidentDef def) => Mathf.RoundToInt(def.durationDays.RandomInRange * 60000f);
    }

}
