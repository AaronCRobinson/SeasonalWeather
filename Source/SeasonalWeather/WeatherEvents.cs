using System;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace SeasonalWeather
{
    public class WeatherEvent_Tremor : WeatherEvent
    {
        static float noise;

        private float time; // avoiding int to save from casting
        private float magnitude;
        private float xScale = 2.0f;
        private IntVec3 curPos;
        private Rot4 direction;
        private bool expired = false;

        public WeatherEvent_Tremor(Map map) : this(map, 1.0f) { }

        public WeatherEvent_Tremor(Map map, float magnitude) : base(map)
        {
            this.map = map;
            this.magnitude = magnitude;
            this.xScale *= this.magnitude;
            this.curPos = CellFinder.RandomCell(this.map);
            this.direction = Rot4.Random;
            this.time = Find.TickManager.TicksGame;
        }

        public override bool Expired
        {
            get { return this.expired; }
        }

        public float MinLoopNoiseThreshold
        {
            get { return this.magnitude * 0.25f; }
        }

        public float ExpireNoiseThreshold
        {
            get { return this.magnitude * 0.1f; }
        }

        // NOTE: sure what should go here yet.
        public override void FireEvent()
        {
            Find.CameraDriver.shaker.DoShake(this.magnitude);
            SoundDefOf.Thunder_OffMap.PlayOneShotOnCamera(this.map);
        }

        public override void WeatherEventTick()
        {
            do
            {
                this.CreateFaultCell(this.curPos);
                this.TremorWalk();
                if (!this.curPos.InBounds(this.map))
                {
                    expired = true;
                    return;
                }
                GetNoise(out noise);
            } while (noise > this.MinLoopNoiseThreshold);

            if (noise < this.ExpireNoiseThreshold) this.expired = true;
        }

        private void GetNoise(out float noise)
        {
            noise = Mathf.PerlinNoise(this.time, 0.0f);
            this.time += this.xScale;
        }

        public void TremorWalk()
        {
            GetNoise(out float noise);
            if (noise < 0.25f)
                this.direction.Rotate(RotationDirection.Clockwise);
            else if (noise < 0.5f)
                this.direction.Rotate(RotationDirection.Clockwise);
            this.curPos += this.direction.FacingCell;
        }

        private void CreateFaultCell(IntVec3 cell)
        {
            bool roofCollapsed = GridsUtility.Roofed(cell, this.map);

            Thing[] things = GridsUtility.GetThingList(cell, this.map).ToArray();
            foreach (Building building in things.OfType<Building>())
            {
                if (!building.def.destroyable || !building.def.building.isNaturalRock || building.Destroyed)
                    continue;
                if (building.def.holdsRoof)
                {
                    roofCollapsed = false;
                    continue;
                }
                CompRefuelable compRefuel = building.TryGetComp<CompRefuelable>();
                if (compRefuel != null && compRefuel.HasFuel)
                    FireUtility.TryStartFireIn(cell, this.map, 3.0f * compRefuel.Fuel);
                else
                {
                    CompPower compPower = building.TryGetComp<CompPower>();
                    if (compPower != null) // consider more specific fires for different types. (explosions for power plants)
                        FireUtility.TryStartFireIn(cell, this.map, 2.0f);
                }
                building.Destroy(DestroyMode.Vanish);
            }

            foreach (Pawn pawn in things.OfType<Pawn>())
            {
                if (roofCollapsed) HediffGiveUtility.TryApply(pawn, HediffDefOf.Shredded, null, true, 3, null);
                else HediffGiveUtility.TryApply(pawn, HediffDefOf.Shredded, null, true, 1, null);
            }

            // TODO: consider type of roof in the drop.
            if (roofCollapsed)
            {
                this.map.roofCollapseBuffer.MarkToCollapse(cell);
                this.map.roofCollapseBufferResolver.CollapseRoofsMarkedToCollapse();
            }

            this.map.terrainGrid.RemoveTopLayer(cell, false);
            FilthMaker.MakeFilth(cell, this.map, ThingDefOf.RockRubble, 1);
        }

    }

}