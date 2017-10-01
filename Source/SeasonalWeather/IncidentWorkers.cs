using UnityEngine;
using Verse;
using RimWorld;

namespace SeasonalWeather
{
    public class IncidentWorker_Earthquake : IncidentWorker
    {
        private int duration;

        protected override bool CanFireNowSub(IIncidentTarget target)
        {
            Map map = (Map)target;
            return !map.gameConditionManager.ConditionIsActive(GameConditionDefOf.Earthquake);
        }

        public override bool TryExecute(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            float points = parms.points;
            float richterMagnitude = EarthquakeHelper.GetMagnitudeWithRand(points);
            GetDuration(richterMagnitude);
            GameCondition_Earthquake gameCondition_Earthquake = (GameCondition_Earthquake)GameConditionMaker.MakeCondition(GameConditionDefOf.Earthquake, this.duration, 0);
            gameCondition_Earthquake.Magnitude = richterMagnitude;
            map.gameConditionManager.RegisterCondition(gameCondition_Earthquake);
            return true;
        }

        public void GetDuration(float magnitude)
        {
            this.duration = Mathf.CeilToInt(this.def.GetDuration() * magnitude);
        }

    }

    static class IncidentHelper
    {
        public static int GetDuration(this IncidentDef def)
        {
            return Mathf.RoundToInt(def.durationDays.RandomInRange * 60000f);
        }
    }
}
