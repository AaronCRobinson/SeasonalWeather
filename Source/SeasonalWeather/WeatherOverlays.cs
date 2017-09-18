using UnityEngine;
using Verse;

namespace SeasonalWeather
{
    [StaticConstructorOnStartup]
    public class WeatherOverlay_DustCloud : SkyOverlay
    {
        private static readonly Material DustCloudOverlay = MatLoader.LoadMat("Weather/FogOverlayWorld", -1);

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
        private static readonly Material DustParticlesOverlay = MatLoader.LoadMat("Weather/SnowOverlayWorld", -1);

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
