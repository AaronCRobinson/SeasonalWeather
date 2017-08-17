using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;

namespace SeasonalWeather
{
    public class SeasonalWeatherSettings : ModSettings
    {
        public bool enableEarthquakes = true;
        public bool enableWildfires = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.enableEarthquakes, "enableEarthquakes", true);
            Scribe_Values.Look(ref this.enableWildfires, "enableWildfires", true);
        }
    }

    class SeasonalWeatherMod : Mod
    {
        public static SeasonalWeatherSettings settings;

        public SeasonalWeatherMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<SeasonalWeatherSettings>();
        }

        public override string SettingsCategory() => "SettingsCategoryLabel".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            ModWindowHelper.Reset();
            ModWindowHelper.MakeLabeledCheckbox(inRect, "EnableEarthquakesLabel".Translate() + ": ", ref settings.enableEarthquakes);
            ModWindowHelper.MakeLabeledCheckbox(inRect, "EnableWildfiresLabel".Translate() + ": ", ref settings.enableWildfires);
            ModWindowHelper.MakeLabel(inRect, "DynamicDefNote".Translate());
            settings.Write();
        }
    }
}
