using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace SeasonalWeather
{

    public class GameCondition_Earthquake : GameCondition
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
        public float Magnitude
        {
            set
            {
                this.magnitude = value;
            }
        }

        public override void Init()
        {
            base.Init();
            this.Foreshock();
        }

        // NOTE: provides a kind of warning before things get too bad.
        public void Foreshock()
        {
            Find.CameraDriver.shaker.DoShake(magnitude/2.0f);
        }

        public override void GameConditionTick()
        {
            //base.GameConditionTick();
            if (Find.TickManager.TicksGame > this.nextTremorTicks)
            {
                base.Map.weatherManager.eventHandler.AddEvent(new WeatherEvent_Tremor(base.Map));
                this.nextTremorTicks = Mathf.FloorToInt((Find.TickManager.TicksGame + TicksBetweenTremors.RandomInRange) / this.magnitude);
            }
        }
    }

}