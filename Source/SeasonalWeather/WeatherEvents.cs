using System;
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

            this.TryToDropRoof(cell); // TODO: consider type of roof in the drop.a
            for (int i = 0; i < things.Length; i++)
            {
                Thing thing = things[i];
                if (thing is Building && thing.def.destroyable && !thing.def.building.isNaturalRock && !thing.Destroyed)
                {
                    Building b = thing as Building;

                    CompRefuelable compRefuel = b.TryGetComp<CompRefuelable>();
                    if (compRefuel != null && compRefuel.HasFuel)
                    {
                        FireUtility.TryStartFireIn(cell, this.map, 3.0f * compRefuel.Fuel);
                    }       
                    else
                    {
                        CompPower compPower = b.TryGetComp<CompPower>();
                        if (compPower != null) // consider more specific fires for different types. (explosions for power plants)
                            FireUtility.TryStartFireIn(cell, this.map, 2.0f);
                    }
                    b.Destroy(DestroyMode.Vanish);
                }
                else if (thing is Pawn) // TODO: consider doing pawns last to do variable types of damage based on what's in the cell...
                {
                    if (roofCollapsed) HediffGiveUtility.TryApply((Pawn)thing, HediffDefOf.Shredded, null, true, 3, null);
                    else HediffGiveUtility.TryApply((Pawn)thing, HediffDefOf.Shredded, null, true, 1, null);
                }
            }
            this.map.terrainGrid.RemoveTopLayer(cell, false);
            FilthMaker.MakeFilth(cell, this.map, ThingDefOf.RockRubble, 1);
        }

        private void TryToDropRoof(IntVec3 cell)
        {
            //ValidateCell
            IntVec3 newCell;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    newCell = cell + new IntVec3(i, 0, j);
                    try
                    {
                        if (GenGrid.InBounds(newCell, this.map) && GridsUtility.Roofed(newCell, this.map))
                            RoofCollapserImmediate.DropRoofInCells(newCell, this.map);
                    }
                    catch (NullReferenceException)
                    {
                        Log.Warning($"NullReferenceException trying to drop roof: {newCell.x},0,{newCell.z}");
                    }
                }
            }
        }

    }

}