using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace SeasonalWeather
{
    public class NaturalDisaster : GameCondition
    {
        public override float AnimalDensityFactor() => 0.1f;
        public override float PlantDensityFactor() => 0.4f;
        public override bool AllowEnjoyableOutsideNow() => false;
    }

    public class GameCondition_Earthquake : NaturalDisaster
    {
        private static readonly IntRange TicksBetweenTremors = new IntRange(800, 1800);
        private int nextTremorTicks;
        private float magnitude;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.nextTremorTicks, "nextTremorTicks", 0, false);
        }

        // sets magnitude based on points
        public float Magnitude { set => this.magnitude = value; } // get?

        public override void Init()
        {
            base.Init();
            this.Foreshock();
        }

        // NOTE: provides a kind of warning before things get too bad.
        public void Foreshock()
        {
            Find.CameraDriver.shaker.DoShake(magnitude / 2.0f);
        }

        public override void GameConditionTick()
        {
            base.GameConditionTick();
            if (Find.TickManager.TicksGame > this.nextTremorTicks)
            {
                base.Map.weatherManager.eventHandler.AddEvent(new WeatherEvent_Tremor(base.Map));
                this.nextTremorTicks = Mathf.FloorToInt((Find.TickManager.TicksGame + TicksBetweenTremors.RandomInRange) / this.magnitude);
            }
        }
    }

    public class GameCondition_Wildfire : NaturalDisaster
    {
        private static readonly IntRange TicksBetweenFires = new IntRange(320, 800);

        private List<SkyOverlay> overlays;
        private Rot4 direction;
        private int nextFireTicks = 0;
        private int fires = 0;
        private bool seedingFires = true;

        public GameCondition_Wildfire() : base()
        {
            this.overlays = new List<SkyOverlay>{ new WeatherOverlay_DustCloud() };
        }

        public override List<SkyOverlay> SkyOverlays() => this.overlays;

        public override void Init()
        {
            base.Init();
            IntRange range = new IntRange((int)(base.Map.Size.x * 0.23f), (int)(base.Map.Size.x * 0.4f));
            this.fires = range.RandomInRange;
            // how to find out if this side is a mountain face?
            this.direction = Rot4.Random;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<Rot4>(ref this.direction, "direction");
            Scribe_Values.Look<int>(ref this.nextFireTicks, "nextFireTicks", 0);
        }

        public override void GameConditionTick()
        {
            if (seedingFires && Find.TickManager.TicksGame > this.nextFireTicks)
            {
                SpawnFire(base.Map);
                this.nextFireTicks = Find.TickManager.TicksGame + GameCondition_Wildfire.TicksBetweenFires.RandomInRange;
                fires--;
                if (fires <= 0)
                    seedingFires = false;
            }
            for (int j = 0; j < this.overlays.Count; j++)
                this.overlays[j].TickOverlay(base.Map);
        }

        public override void GameConditionDraw()
        {
            Map map = base.Map;
            for (int i = 0; i < this.overlays.Count; i++)
                this.overlays[i].DrawOverlay(map);
        }

        private void SpawnFire(Map map)
        {
            IntVec3 cell = CellFinder.RandomEdgeCell(this.direction, map);
            Fire fire = (Fire)ThingMaker.MakeThing(ThingDefOf.Fire, null);
            fire.fireSize = 1.0f;
            GenSpawn.Spawn(fire, cell, map, Rot4.North, false);
        }

    }

}