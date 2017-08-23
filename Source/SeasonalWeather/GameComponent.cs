using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SeasonalWeather
{
    class ApplyDynamicsFromSettings : GameComponent
    {
        public ApplyDynamicsFromSettings() { }

        public ApplyDynamicsFromSettings(Game g) { }

        public override void FinalizeInit()
        {
            Log.Message("SeasonalWeather: Applying dynamic settings!");
            DynamicWeatherDefs.dynamicWeatherDefHelper.SetDynamicDet("Earthquake", SeasonalWeatherMod.settings.enableEarthquakes);
            DynamicWeatherDefs.dynamicWeatherDefHelper.SetDynamicDet("Wildfire", SeasonalWeatherMod.settings.enableWildfires);
        }
    }
}
