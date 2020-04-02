using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace SeasonalWeather
{
    // NOTE: These broke in A18... but lol this works instead.

    [StaticConstructorOnStartup]
    public class WeatherOverlay_DustCloud : SkyOverlay
    {
        //private static readonly Material DustCloudOverlay = MatLoader.LoadMat("Weather/FogOverlayWorld", -1);
        private static readonly Material DustCloudOverlay;

        static WeatherOverlay_DustCloud()
        {
            DustCloudOverlay = (Material)AccessTools.Field(typeof(WeatherOverlay_Fog), "FogOverlayWorld").GetValue(new WeatherOverlay_Fog());
        }

        public WeatherOverlay_DustCloud()
        {
            this.worldOverlayMat = DustCloudOverlay;
            this.worldOverlayPanSpeed1 = 0.008f;
            this.worldPanDir1 = new Vector2(-1f, -0.26f); //new Vector2(1f, 1f);
            this.worldPanDir1.Normalize();
            this.worldOverlayPanSpeed2 = 0.012f;
            this.worldPanDir2 = new Vector2(-1f, -0.24f); //new Vector2(1f, -1f);
            this.worldPanDir2.Normalize();
        }
    }

    [StaticConstructorOnStartup]
    public class WeatherOverlay_DustParticles : SkyOverlay
    {
        //private static readonly Material DustParticlesOverlay = MatLoader.LoadMat("Weather/SnowOverlayWorld", -1); 
        private static readonly Material DustParticlesOverlay;

        static WeatherOverlay_DustParticles()
        {
            DustParticlesOverlay = (Material)AccessTools.Field(typeof(WeatherOverlay_SnowHard), "SnowOverlayWorld").GetValue(new WeatherOverlay_SnowHard());
        }

        public WeatherOverlay_DustParticles()
        {
            this.worldOverlayMat = DustParticlesOverlay;
            this.worldOverlayPanSpeed1 = 0.018f;
            this.worldPanDir1 = new Vector2(-1f, -0.26f); //new Vector2(1f, 1f);
            this.worldPanDir1.Normalize();
            this.worldOverlayPanSpeed2 = 0.022f;
            this.worldPanDir2 = new Vector2(-1f, -0.24f); //new Vector2(1f, -1f);
            this.worldPanDir2.Normalize();
        }
    }
}
