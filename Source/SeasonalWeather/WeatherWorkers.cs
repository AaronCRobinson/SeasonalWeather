using RimWorld;
using Verse;

using RimworldWeatherWorker = Verse.WeatherWorker;

namespace SeasonalWeather
{
    class WeatherWorker : RimworldWeatherWorker
    {
        protected WeatherWorker(WeatherDef def) : base(def) { }

        public virtual void Init()
        {
            Log.Error("Unimplemented!");
        }

        // redirecting
        public new virtual void WeatherTick(Map map, float lerpFactor)
        {
            Log.Error("Unimplemented!");
        }

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

    // forces weather to stay the same...
    // NOTE: considerWeatherDecider.DisableRainFor()

    class WeatherWorker_Wildfire : WeatherWorker
    {
        private static string defName = "wildfire";
        private static int minWeatherAge = 20000;
        private int fires;
        private Rot4 direction;
        private int cnt;
        //private WildfireParams parms;

        // NOTE: consider parms for move complexity and builders.
        /*private class WildfireParms
        {
            private int fires;
            private Rot4 direction;
            private int cnt;

            WildfireParams()
            {
                this.fires = Rand.Range(10, 25);
                this.cnt = Rand.Range(10, 25);
                // how to find out if this side is a mountain face?
                this.direction = Rot4.Random; 
            }
        }*/

        public WeatherWorker_Wildfire(WeatherDef def) : base(def) { }

        public override void Init()
        {
            //this.parms = new WildfireParms()
            this.fires = Rand.Range(8, 18);
            this.cnt = Rand.Range(90, 360);
            // how to find out if this side is a mountain face?
            this.direction = Rot4.Random;
        }

        public override void WeatherTick(Map map, float lerpFactor)
        {
            base._WeatherTick(map, lerpFactor);

            if (this.fires != 0) // spawning fires still
            {
                if (this.cnt == 0)
                {
                    IntVec3 cell = CellFinder.RandomEdgeCell(this.direction, map);
                    Fire fire = (Fire)ThingMaker.MakeThing(ThingDefOf.Fire, null);
                    fire.fireSize = 1.0f;
                    GenSpawn.Spawn(fire, cell, map, Rot4.North, false);
                    this.fires--;
                    this.cnt = Rand.Range(10, 25);
                }
                else
                    this.cnt--;
            }
            else // checking to see if wildfire is over
            {
                if (!map.fireWatcher.LargeFireDangerPresent && map.weatherManager.curWeatherAge > minWeatherAge && map.weatherManager.curWeather.defName == defName)
                {
                    //Log.Message("WeatherWorker_Wildfire StartNextWeather");
                    map.weatherDecider.StartNextWeather(); // end the wild fire
                }
            }
        }
    }

    /*class WeatherWorker_Earthquake : WeatherWorker
    {
        private Rot4 direction, curDirection;
        private List<IntVec3> path;
        private Map map;

        public WeatherWorker_Earthquake(WeatherDef def) : base(def) { }

        public override void Init(Map m)
        {
            this.path = this.CreatePath();
            this.map = m;
        }

        private List<IntVec3> CreatePath()
        {
            IntVec3 start, curCell, offset;
            List<IntVec3> path = new List<IntVec3>();
            bool breaking = false;

            this.curDirection = this.direction = Rot4.Random;
            curCell = start = CellFinder.RandomCell(this.map);
            path.Add(start);

            int edges = Rand.Range(3, 7);
            for (int i = 0; i < edges; i++) // edges
            {
                offset = this.Offset(this.curDirection);
                for (int j = 0; j < Rand.Range(11, 19); j++) // edge length
                {
                    curCell += offset;
                    if (GenGrid.InBounds(curCell, this.map))
                        path.Add(curCell);
                    else
                    {
                        breaking = true;
                        break;
                    }
                    if (Rand.Range(0f, 1f) > 0.85f) // radical
                    {
                        IntVec3 radicalCell = curCell + this.GetRadicalOffset();
                        if (GenGrid.InBounds(radicalCell, this.map))
                            path.Add(radicalCell);
                    }
                }
                if (breaking || i == edges - 1) break; // no need for jagged part on end.
                this.curDirection.Rotate(this.GetRandomRotationDirection());
                offset = this.Offset(this.curDirection);
                for (int j = 0; j < Rand.Range(1, 3); j++) // jagged part (consider weights?)
                {
                    curCell += offset;
                    if (GenGrid.InBounds(curCell, this.map))
                        path.Add(curCell);
                    else
                    {
                        breaking = true;
                        break;
                    }
                }
                if (breaking) break;

                this.curDirection = this.direction;
            }
            return path;
        }

        private IntVec3 Offset(Rot4 direction)
        {
            IntVec3 offset = new IntVec3();
            if (direction == Rot4.North)
                offset.z = 1;
            else if (direction == Rot4.South)
                offset.z = -1;
            else if (direction == Rot4.East)
                offset.x = 1;
            else if (direction == Rot4.West)
                offset.x = -1;
            return offset;
        }

        private IntVec3 GetRadicalOffset()
        {
            IntVec3 cell = new IntVec3();
            if (this.direction.IsHorizontal)
            {
                if (Rand.Range(0, 2) == 1) cell.z = 1;
                else cell.z = -1;
            }
            else
            {
                if (Rand.Range(0, 2) == 1) cell.x = 1;
                else cell.x = -1;
            }
            return cell;
        }

        private RotationDirection GetRandomRotationDirection(bool allowNone = true)
        {
            return (RotationDirection)Enum.GetValues(typeof(RotationDirection)).GetValue(Rand.Range(1, 3));
        }

        public override void WeatherTick(Map map, float lerpFactor)
        {
            for (int i = 0; i < Rand.Range(0, 4); i++)
            {
                this.CreateFaultCell(this.path[0]);
                this.path.RemoveAt(0);
                Find.CameraDriver.shaker.DoShake(Rand.Range(2f, 5f));
                //SoundDefOf.Thunder_OffMap.PlayOneShotOnCamera(this.map);
                if (this.path.Count == 0) break;
            }
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
                    thing.Destroy(DestroyMode.Vanish);
                }
                else if (thing is Pawn) // TODO: consider doing pawns last to do variable types of damage based on what's in the cell...
                {
                    Pawn p = (Pawn)thing;

                    if (roofCollapsed) HediffGiveUtility.TryApply(p, HediffDefOf.Shredded, null, true, 3, null);
                    else HediffGiveUtility.TryApply(p, HediffDefOf.Shredded, null, true, 1, null);
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
                    catch (NullReferenceException e)
                    {
                        Log.Warning($"NullReferenceException trying to drop roof: {newCell.x},0,{newCell.z}");
                    }
                }
            }
        }
    }*/

}
