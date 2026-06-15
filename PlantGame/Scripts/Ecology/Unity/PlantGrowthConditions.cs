using UnityEngine;

namespace IDEK.PlantGame.Ecology
{
    /// <summary>
    /// Describes the ideal conditions for plant growth, along with how that growth is impacted by non-ideal conditions.
    /// </summary>
    public class PlantGrowthConditions
    {
        public float baseNutritionNeed;
        public Humidity idealSoilHumidity;
        public Temperature idealSoilTemperature;

        // [Tooltip("How the base growth rate is impacted by temperature, relative to the ideal. " +
        //     "\nEvaluated with the current temperature offset in Fahrenheit (because that's what I know)." +
        //     "\nMultiplied by the base rate.")]
        // [InspectorName("Growth Impact of Relative Non-Ideal Temperatures (F)")]
        // public AnimationCurve growthNonIdealNutritionFactor;

        [Tooltip("How the base growth rate is impacted by temperature, relative to the ideal. " +
            "\nEvaluated with the current temperature offset in Fahrenheit (because that's what I know)." +
            "\nMultiplied by the base rate.")]
        [InspectorName("Growth Impact of Relative Non-Ideal Temperatures " + Temperature.FAHRENHEIT_SYMBOL)]
        public AnimationCurve growthNonIdealTempFactor_f = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);

        [Tooltip("How the base growth rate is impacted by SOIL humidity (not air humidity), relative to the ideal. " +
            "\nEvaluated with current humidity percentage offset on range (0,1]. " +
            "\nMultiplied by the base rate.")]
        [InspectorName("Growth Impact of Relative Non-Ideal Humidity")]
        public AnimationCurve growthNonIdealHumidityFactor = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);
        
        //TODO: set up proper logic here
        //Keeping it simple for now, but there may be thresholds for this
        //perhaps it becomes non-viable if the growth rate is calculated to be less than 0?
        public virtual bool IsViableSoil(SoilComponent soil) => true; 
        public virtual bool IsViableSoil(SoilState soilState) => true;

        /// <summary>
        /// Based on the given soil's properties and the conditions desired by the plant, answers how to scale the plant's grow. 
        /// </summary>
        /// <param name="soil"></param>
        /// <returns></returns>
        public virtual float CalcSoilGrowthFactor(SoilComponent soil)
        {
            if (soil == null)
                throw new System.ArgumentNullException(nameof(soil));
            
            //both values should be bound [0,1), so max error is 1
            float humidityError = idealSoilHumidity - soil.Humidity;
            
            //not abs since being hot or cold may change things
            float tempError_f = idealSoilTemperature.InFahrenheit - soil.Temperature.InFahrenheit;
            
            //simple for now, will prob set up some curve later that starts to plateaus as nutrition content exceeds needs.
            //for gameplay reasons, we may be using different kinds of nutrients,
            //like some freaky plants doing better when feeding on corpses vs regular fertilizer or acid, etc. 
            float nutritionFactor = baseNutritionNeed > 0.0f ? (soil.state.nutrition / baseNutritionNeed) : 1f;
            float humidityFactor = growthNonIdealHumidityFactor.Evaluate(humidityError);
            float temperatureFactor = growthNonIdealTempFactor_f.Evaluate(tempError_f);
            
            return humidityFactor * temperatureFactor * nutritionFactor;
        }
    }
}