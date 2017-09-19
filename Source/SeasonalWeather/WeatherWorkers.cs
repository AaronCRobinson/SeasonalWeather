using RimWorld;
using Verse;

using RimworldWeatherWorker = Verse.WeatherWorker;

namespace SeasonalWeather
{
    class WeatherWorker : RimworldWeatherWorker
    {
        protected WeatherWorker(WeatherDef def) : base(def) { }

        public virtual void Init() { }

        // redirecting
        public new virtual void WeatherTick(Map map, float lerpFactor) { }

        protected void _WeatherTick(Map map, float lerpFactor)
        {
            base.WeatherTick(map, lerpFactor);
        }
    }

    class WeatherWorker_Default : WeatherWorker
    {
        public WeatherWorker_Default(WeatherDef def) : base(def) { }

        public override void WeatherTick(Map map, float lerpFactor)
        {
            base._WeatherTick(map, lerpFactor);
        }

        public override void Init() { }
    }

    class WeatherWorker_Wildfire : WeatherWorker
    {
        private static int minWeatherAge = 30000;
        private int fires;
        private Rot4 direction;
        private int hashInterval;

        public WeatherWorker_Wildfire(WeatherDef def) : base(def)
        {
            hashInterval = 15;
        }

        public override void Init()
        {
            this.fires = Rand.Range(8, 18);
            // how to find out if this side is a mountain face?
            this.direction = Rot4.Random;
        }

        public bool Spawning
        {
            get
            {
                return this.fires != 0;
            }
        }

        private void SpawnFire(Map map)
        {
            IntVec3 cell = CellFinder.RandomEdgeCell(this.direction, map);
            Fire fire = (Fire)ThingMaker.MakeThing(ThingDefOf.Fire, null);
            fire.fireSize = 1.0f;
            GenSpawn.Spawn(fire, cell, map, Rot4.North, false);
            this.fires--;
        }

        public override void WeatherTick(Map map, float lerpFactor)
        {
            base._WeatherTick(map, lerpFactor);

            if ( GenHelper.IsHashIntervalTick(this, hashInterval))
            {
                if (this.Spawning)
                {
                    if (Rand.Value > 0.8) SpawnFire(map);
                }
                else // checking to see if wildfire is over
                {
                    if (!map.fireWatcher.LargeFireDangerPresent && map.weatherManager.curWeatherAge > minWeatherAge && map.weatherManager.curWeather == WeatherDefOf.Wildfire)
                    {
                        map.weatherDecider.StartNextWeather(); // end the wild fire
                    }
                }
            }

        }
    }

}
