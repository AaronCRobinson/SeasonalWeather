using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using Verse;
using RimWorld;
using Harmony;

using RimworldWeatherWorker = Verse.WeatherWorker;
    
namespace SeasonalWeather
{
    static class WeatherWorkerHelper
    {
        static public WeatherWorker Worker(this WeatherDef def)
        {
            WeatherWorkerExtension workerExt = def.GetModExtension<WeatherWorkerExtension>();

            if (workerExt?.workerInt != null) return workerExt.workerInt;

            if (workerExt == null)
            {
                workerExt = new WeatherWorkerExtension();
                if (def.modExtensions == null) def.modExtensions = new List<DefModExtension>();
                def.modExtensions.Add(workerExt);
            }
            workerExt.workerInt = (WeatherWorker)Activator.CreateInstance(workerExt.workerClass, new object[] { def });
            return workerExt.workerInt;
        }
    }

    class WeatherWorkerExtension : DefModExtension
    {
        public WeatherWorker workerInt;
        public Type workerClass = typeof(WeatherWorker_Default);
        // NOTE: consider putting singleton here?
    }

    [StaticConstructorOnStartup]
    class CustomWeatherWorkerPatches
    {
        static CustomWeatherWorkerPatches()
        {
            Type t = typeof(CustomWeatherWorkerPatches);

            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.whyisthat.seasonalweather.weatherdef");

            harmony.Patch(AccessTools.Method(typeof(WeatherManager), nameof(WeatherManager.WeatherManagerTick)), null, null, new HarmonyMethod(t, nameof(WeatherManagerTickTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(WeatherManager), nameof(WeatherManager.DrawAllWeather)), null, null, new HarmonyMethod(t, nameof(DrawAllWeatherTranspiler)));
            // NOTE: patch on TransitionTo versus StartNextWeather as TransitionTo is called also by debugger window
            harmony.Patch(AccessTools.Method(typeof(WeatherManager), nameof(WeatherManager.TransitionTo)), null, new HarmonyMethod(t, nameof(AddWorkerInitPostfix)));

            harmony.Patch(AccessTools.Method(typeof(SkyManager), "CurrentSkyTarget"), null, null, new HarmonyMethod(t, nameof(CurrentSkyTargetTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(SkyManager), "UpdateOverlays"), null, null, new HarmonyMethod(t, nameof(UpdateOverlaysTranspiler)));

            //NOTE: this would clean up some unncessary init.
            //harmony.Patch(AccessTools.Method(typeof(WeatherDef), nameof(WeatherDef.PostLoad)), null, null), new HarmonyMethod(t, nameof(RemoveWorkerIntTranspiler)));
        }

        public static IEnumerable<CodeInstruction> WeatherManagerTickTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return GetWeatherWorkerTranspiler(instructions, OpCodes.Callvirt, typeof(RimworldWeatherWorker).GetMethod(nameof(RimworldWeatherWorker.WeatherTick)), typeof(WeatherWorker).GetMethod(nameof(WeatherWorker.WeatherTick)));
        }

        public static IEnumerable<CodeInstruction> DrawAllWeatherTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return GetWeatherWorkerTranspiler(instructions, OpCodes.Callvirt, typeof(RimworldWeatherWorker).GetMethod(nameof(RimworldWeatherWorker.DrawWeather)), typeof(WeatherWorker).GetMethod(nameof(WeatherWorker.DrawWeather)));
        }

        public static IEnumerable<CodeInstruction> CurrentSkyTargetTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return GetWeatherWorkerTranspiler(instructions, OpCodes.Callvirt, typeof(RimworldWeatherWorker).GetMethod(nameof(RimworldWeatherWorker.CurSkyTarget)), typeof(WeatherWorker).GetMethod(nameof(WeatherWorker.CurSkyTarget)));
        }

        public static IEnumerable<CodeInstruction> UpdateOverlaysTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return GetWeatherWorkerTranspiler(instructions, OpCodes.Ldfld, typeof(RimworldWeatherWorker).GetField(nameof(RimworldWeatherWorker.overlays)), typeof(WeatherWorker).GetField(nameof(WeatherWorker.overlays)));
        }

        private static MethodInfo getWeatherWorkerMethodInfo = AccessTools.Property(typeof(WeatherDef), nameof(WeatherDef.Worker)).GetGetMethod();

        public static IEnumerable<CodeInstruction> GetWeatherWorkerTranspiler(IEnumerable<CodeInstruction> instructions, OpCode oc, object @checked, object refed)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            CodeInstruction instruction;
            for (int i = 0; i < instructionList.Count; i++)
            {
                // NOTE: currently ignoring labels...
                instruction = instructionList[i];
                if (instruction.opcode == OpCodes.Callvirt && instruction.operand == getWeatherWorkerMethodInfo)
                {
                    yield return new CodeInstruction(OpCodes.Call, typeof(WeatherWorkerHelper).GetMethod(nameof(WeatherWorkerHelper.Worker)));
                }
                else if (instruction.opcode == oc && instruction.operand == @checked)
                {
                    yield return new CodeInstruction(oc, refed);
                }
                else
                    yield return instruction;
            }
        }

        // NOTE: should be a better way of doing this regarding parms objects...
        public static void AddWorkerInitPostfix(WeatherManager __instance)
        {
            __instance.curWeather.Worker().Init(); // __instance.map);
        }

        /*public static IEnumerable<CodeInstruction> RemoveWorkerIntTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Getting some error here => Fuck it, let's do it live (full detour)
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Editable), nameof(Editable.PostLoad)));
            yield return new CodeInstruction(OpCodes.Ret);
        }*/
    }
}
