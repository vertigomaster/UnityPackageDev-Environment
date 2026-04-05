using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;

namespace IDEK.PlantGame.Ecology
{
    /// <summary>
    /// Defines a type of soil, declares its inherent properties, and provides functions to calculate derived or externally-relative properties.
    /// </summary>
    [Serializable]
    public class SoilDef
    {
        /// <summary>
        /// How many ml of water can it hold (per cubic meter) before becoming saturated?
        /// </summary>
        public float saturationPoint = 1f;

        [Tooltip("Under \"standard\" conditions, how many ml of water would leave a cubic meter of this soil every second?")]
        [InspectorName("Base Water Loss Rate (ml/sm^3)")]
        public float baseWaterLossRate = 1f;
        
        [FormerlySerializedAs("waterLossTempFactor")]
        [Tooltip("How the base loss rate is impacted by temperature. " +
            "\nEvaluated with the current temperature in Fahrenheit (because that's what I know)." +
            "\nMultiplied by the base rate.")]
        [InspectorName("Water Loss Temperature Factor " + Temperature.FAHRENHEIT_SYMBOL)]
        public AnimationCurve waterLossTempFactor_f;

        [FormerlySerializedAs("waterLossSaturationFactor")]
        [Tooltip("How the base loss rate is impacted by SOIL humidity (not air humidity). " +
            "\nEvaluated with current humidity percentage on range (0,1]. " +
            "\nMultiplied by the base rate.")]
        [InspectorName("Water Loss Saturation Factor")]
        public AnimationCurve waterLossHumidityFactor;

        [Tooltip("Abstracted rate at which air temp affects this soil's temp, and vice versa." +
            "\n Currently used as a lerp factor" +
            "\nStill working on the units. Still based on " + Temperature.FAHRENHEIT_SYMBOL + ".")]
        public float airHeatTransferRate = 0.5f;

        /// <summary>
        /// Humidity is the ratio of the water present in a substance (<see cref="waterPerCubicMeter"/>) relative to
        /// that substance's water capacity (its <see cref="saturationPoint"/>).
        /// A saturated substance cannot hold more water.
        /// </summary>
        /// <param name="waterPerCubicMeter"></param>
        //TODO: if we start dealing with multiple/other liquids, like blood or acid, we'll need to rework this.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Humidity CalcHumidity(float waterPerCubicMeter) => Humidity.FromRatio(waterPerCubicMeter, saturationPoint);
        
        /// <summary>
        /// How many ml of water to lose per second, per cubic meter of soil, based on input conditions (ml/sm^3)
        /// </summary>
        /// <param name="temp">Current temperature, which impacts the <see cref="waterLossTempFactor_f"/>.</param>
        /// <param name="currentHumidity">Current HUMIDITY</param>
        /// <returns></returns>
        public float CalcWaterLossRate(Temperature temp, Humidity currentHumidity)
        {
            float humidityWaterLoss = CalcHumidity(currentHumidity);
            float tempWaterLoss = waterLossTempFactor_f.Evaluate(temp.F);
            return tempWaterLoss * humidityWaterLoss;
        }
    }
}