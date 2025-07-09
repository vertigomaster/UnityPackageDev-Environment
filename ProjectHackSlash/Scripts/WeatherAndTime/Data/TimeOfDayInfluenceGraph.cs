using System;
using UnityEngine;

namespace IDEK.Tools.GameplayEssentials.Samples.HackSlash
{
    /// <summary>
    /// Denotes how the influence of some given data should react based on time of day
    /// (within a weather system or day/night cycle system).
    /// Utilize the <see cref="influenceOverTime"/> AnimationCurve,
    /// where time is relative to the <see cref="peakTimeOfDay"/>.
    /// </summary>
    /// <remarks>
    /// This asset does NOT denote WHAT data the influence is a graph of.
    /// </remarks>
    [CreateAssetMenu(
        menuName = "IDEK/Weather and Time/Time-Of-Day Influence Graph", 
        fileName = "TimeOfDayInfluenceGraph", order = 0)]
    [Serializable]
    public class TimeOfDayInfluenceGraph : ScriptableObject
    {
        [Tooltip("Measured in % of day.\nDenotes the point in the day")]
        public TimeOfDay peakTimeOfDay;

        // #if ODIN_INSPECTOR
        //         [Sirenix.OdinInspector.ShowIf("@influenceInputMode == InfluenceInputMode.Curve")]
        // #endif
        [Tooltip("Measured in percentages.\n" +
            "X (Time) = % of day, Y = weight %.\n" +
            "Time 0 is Peak Time, negative is before, positive is after.")]

        public AnimationCurve influenceOverTime;

        #region Other Influence Modes (Commented out for Now)
        //thought this was going to be easier...
        //TODO: finish eventually if the need arises
        
        //         public enum InfluenceInputMode { Curve, Linear, SimpleCrossfade, AsymmetricLinear, AsymmetricCrossFade }
        //         public InfluenceInputMode influenceInputMode;
        //
        //         
        // #if ODIN_INSPECTOR
        //         [FormerlySerializedAs("maxWeight")]
        //         [Sirenix.OdinInspector.ShowIf("@influenceInputMode != InfluenceInputMode.Curve")]
        // #endif
        //         [InspectorName("Max Influence")]
        //         [Tooltip("Measured in %")]
        //         [Range(0f, 1f)]
        //         public float nonCurve_maxInfluenceWeight;
        //
        // #if ODIN_INSPECTOR
        //         [Sirenix.OdinInspector.ShowIf("@influenceInputMode == InfluenceInputMode.Linear")]
        //         [Sirenix.OdinInspector.ShowIf("@influenceInputMode == InfluenceInputMode.SimpleCrossfade")]
        // #endif
        //         [InspectorName("Falloff Time")]
        //         [Tooltip("Measured in % of day")]
        //         [Range(0f, 1f)]
        //         public float symmetric_FalloffTime;
        //
        // #if ODIN_INSPECTOR
        //         [Sirenix.OdinInspector.ShowIf("@influenceInputMode == InfluenceInputMode.AsymmetricCrossFade")]
        //         [Sirenix.OdinInspector.ShowIf("@influenceInputMode == InfluenceInputMode.AsymmetricLinear")]
        // #endif
        //         [InspectorName("Ramp Up Time")]
        //         [Tooltip("Measured in % of day")]
        //         [Range(0f, 1f)]
        //         public float asym_rampUpTime;
        //
        // #if ODIN_INSPECTOR
        //         [Sirenix.OdinInspector.ShowIf("@influenceInputMode == InfluenceInputMode.AsymmetricCrossFade")]
        //         [Sirenix.OdinInspector.ShowIf("@influenceInputMode == InfluenceInputMode.AsymmetricLinear")]
        // #endif
        //         [InspectorName("Ramp Down Time")]
        //         [Tooltip("Measured in % of day")]
        //         [Range(0f, 1f)]
        //         public float asym_rampDownTime;
        #endregion
        

        public float GetWeightAtTime(float currentPercentageOfDay)
        {
            float i = currentPercentageOfDay - peakTimeOfDay.PercentageOfDay;

            //wide falloff curves will effectively "wrap around" the day cycle
            //if we need to qccount for multiple days in a later iteration, this will need to be adjusted
            return Mathf.Max(influenceOverTime.Evaluate(i), influenceOverTime.Evaluate(i % 1f));

            #region Other Influence Modes (Commented out for Now)

            //thought this was going to be easier...
            //TODO: finish eventually if the need arises
            // switch (influenceInputMode)
            // {
            //     case InfluenceInputMode.Curve:
            //         return Mathf.Max(influenceOverTime.Evaluate(i), influenceOverTime.Evaluate(i % 1f));
            //         break;
            //     case InfluenceInputMode.SimpleCrossfade:
            //         return Mathf.Max(
            //             IDEKMath.FastSmoothLerp(
            //                 0, nonCurve_maxInfluenceWeight, Mathf.Abs(
            //                     Mathf.Abs(i) - symmetric_FalloffTime)
            //             ));
            //         break;
            //     case InfluenceInputMode.AsymmetricCrossFade:
            //         break;
            //     default:
            //         throw new ArgumentOutOfRangeException();
            // }
            #endregion
        }
    }
}