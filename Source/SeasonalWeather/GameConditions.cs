using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace SeasonalWeather
{
    public class NaturalDisaster : GameCondition
    {
        public override float AnimalDensityFactor(Map m) => 0.1f;
        public override float PlantDensityFactor(Map m) => 0.4f;
        public override bool AllowEnjoyableOutsideNow(Map m) => false;
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
                base.SingleMap.weatherManager.eventHandler.AddEvent(new WeatherEvent_Tremor(base.SingleMap));
                this.nextTremorTicks = Mathf.FloorToInt((Find.TickManager.TicksGame + TicksBetweenTremors.RandomInRange) / this.magnitude);
            }
        }
    }

    public class GameCondition_Wildfire : NaturalDisaster
    {
        private static readonly IntRange TicksBetweenFires = new IntRange(320, 800);
        private static readonly bool noFirewatcher;

        private readonly List<SkyOverlay> overlays;
        private Rot4 direction;
        private int nextFireTicks = 0;
        private int fires = 0;
        private bool seedingFires = true;
        private SkyColorSet AshCloudColors;

        static GameCondition_Wildfire()
        {
            noFirewatcher = ModLister.AllInstalledMods.FirstOrDefault(m => m.Name == "No Firewatcher")?.Active == true;
        }

        public GameCondition_Wildfire() : base()
        {
            this.AshCloudColors = new SkyColorSet(new ColorInt(216, 255, 150).ToColor, new ColorInt(234, 200, 255).ToColor, new Color(0.7f, 0.85f, 0.65f), 0.85f);
            this.overlays = new List<SkyOverlay>{ new WeatherOverlay_DustCloud() };
        }

        public override List<SkyOverlay> SkyOverlays(Map map) => this.overlays;

        public override float SkyTargetLerpFactor(Map map) => GameConditionUtility.LerpInOutValue(this, 5000f, 0.5f);

        public override SkyTarget? SkyTarget(Map map)
        {
            return new SkyTarget?(new SkyTarget(0.85f, this.AshCloudColors, 1f, 1f));
        }

        public override void Init()
        {
            base.Init();
            IntRange range = new IntRange((int)(base.SingleMap.Size.x * 0.23f), (int)(base.SingleMap.Size.x * 0.4f));
            this.fires = range.RandomInRange;
            Log.Message($"{this.fires}");
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
            if (this.seedingFires)
            {
                if (Find.TickManager.TicksGame > this.nextFireTicks)
                {
                    this.SpawnFire(base.SingleMap);
                    this.nextFireTicks = Find.TickManager.TicksGame + GameCondition_Wildfire.TicksBetweenFires.RandomInRange;
                    this.fires--;
                    if (this.fires <= 0)
                        this.seedingFires = false;
                }
            } else
            {
                if (noFirewatcher)
                    base.SingleMap.fireWatcher.FireWatcherTick();
                if (!base.SingleMap.fireWatcher.LargeFireDangerPresent)
                    this.Duration = 0; // Expired => true
            }

            for (int j = 0; j < this.overlays.Count; j++)
                this.overlays[j].TickOverlay(base.SingleMap);
        }

        public override void GameConditionDraw(Map map)
        {
            for (int i = 0; i < this.overlays.Count; i++)
                this.overlays[i].DrawOverlay(map);
        }

        private void SpawnFire(Map map)
        {
            IntVec3 cell = CellFinder.RandomEdgeCell(this.direction, map);
            Fire fire = (Fire)ThingMaker.MakeThing(ThingDefOf.Fire, null);
            fire.fireSize = 1.0f;
            GenSpawn.Spawn(fire, cell, map, Rot4.North);
        }

    }

}