using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

// NOTE: consider using factorials with magnitude?
// NOTE: Mercalli intensity scale
// NOTE: consider animal reaction
// NOTE: consider earthquake swarms

namespace SeasonalWeather
{
    // REFERNECE: https://en.wikipedia.org/wiki/Richter_magnitude_scale
    public enum RichterMagnitude : byte
    {
        Undefined,
        Micro,
        Minor,
        Light,
        Moderate,
        Strong,
        Major,
        Great
    }

    static class EarthquakeHelper
    {
        // REFERNECE: https://stackoverflow.com/questions/20147879/switch-case-can-i-use-a-range-instead-of-a-one-number
        private static readonly List<KeyValuePair<Func<float, bool>, RichterMagnitude>> richterSwitch =
            new List<KeyValuePair<Func<float, bool>, RichterMagnitude>>()
            {
                { new KeyValuePair<Func<float, bool>, RichterMagnitude>(x => x < 1.0f, RichterMagnitude.Undefined) },
                { new KeyValuePair<Func<float, bool>, RichterMagnitude>(x => x < 2.0f, RichterMagnitude.Micro) },
                { new KeyValuePair<Func<float, bool>, RichterMagnitude>(x => x < 4.0f, RichterMagnitude.Minor) },
                { new KeyValuePair<Func<float, bool>, RichterMagnitude>(x => x < 5.0f, RichterMagnitude.Light) },
                { new KeyValuePair<Func<float, bool>, RichterMagnitude>(x => x < 6.0f, RichterMagnitude.Moderate) },
                { new KeyValuePair<Func<float, bool>, RichterMagnitude>(x => x < 7.0f, RichterMagnitude.Strong) },
                { new KeyValuePair<Func<float, bool>, RichterMagnitude>(x => x < 8.0f, RichterMagnitude.Major) },
                { new KeyValuePair<Func<float, bool>, RichterMagnitude>(x => true, RichterMagnitude.Great) },
            };

        public static RichterMagnitude GetMagnitudeType(float magnitude)
        {
            return richterSwitch.First(sw => sw.Key(magnitude)).Value;
        }

        private static readonly SimpleCurve Curve_MagnitudeScale = new SimpleCurve
        {
            { new CurvePoint(0f, 1.0f), true },
            { new CurvePoint(10f, 2.0f), true },
            { new CurvePoint(100f, 3.0f), true },
            { new CurvePoint(500f, 4.0f), true },
            { new CurvePoint(1000f, 5.0f), true },
            { new CurvePoint(1500f, 6.0f), true },
            { new CurvePoint(2000f, 7.0f), true }
        };

        public static float GetMagnitude(float points)
        {
            return Curve_MagnitudeScale.Evaluate(points) + Rand.Range(0f, 1f);
        }

        public static float GetMagnitudeWithRand(float points)
        {
            return GetMagnitude(points) + Rand.Range(0f, 1f);
        }

    }
}
