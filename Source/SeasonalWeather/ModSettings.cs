using Verse;
using UnityEngine;
using SettingsHelper;

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

        public override string SettingsCategory() => "SeasonalWeatherSettingsCategoryLabel".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            listing_Standard.AddLabeledCheckbox("EnableEarthquakesLabel".Translate() + ": ", ref settings.enableEarthquakes);
            listing_Standard.AddLabeledCheckbox("EnableWildfiresLabel".Translate() + ": ", ref settings.enableWildfires);
            listing_Standard.AddLabelLine("DynamicDefNote".Translate());
            listing_Standard.End();
            settings.Write();
        }
    }
}
