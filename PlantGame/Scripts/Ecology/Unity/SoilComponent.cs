using IDEK.PlantGame.Ecology.Unity;
using IDEK.Tools.GameplayEssentials.Updating;
using IDEK.Tools.ShocktroopUtils;
using IDEK.Tools.ShocktroopUtils.Services;

namespace IDEK.PlantGame.Ecology
{
    public class SoilComponent : TickBehaviour
    {
        public readonly Temperature defaultTemp = new(70f, Temperature.Unit.Fahrenheit);
        
        public SoilState state;
        public SoilDefAsset defAsset;
        public SoilDef Def => defAsset.data; 
        
        //TODO: calculate area of the soil represented by this component
        //TODO: soil volume/mass calculator (question - how does soil density impact these things? packed soil vs loose soil will probably act differently)
        //need that to interpret water content
        
        //calc from soil def
        public float Humidity => Def.CalcHumidity(state.WaterMlPerCubicMeter);
        public Temperature Temperature => state.soilTemp;

        #region Overrides of TickBehaviour

        /// <inheritdoc />
        protected override void Tick(float deltaTickTime)
        {
            _AdvanceState(deltaTickTime);
        }

        private void _AdvanceState(float deltaTime)
        {
            Temperature currentAirTemp = _CalcAirTemperature();
            
            //temp changes 
            //TODO: Maybe make it a better approximation of the heat equation at some point. Overkill to do right now.
            state.soilTemp.F = state.soilTemp.F.Damp(currentAirTemp.F, Def.airHeatTransferRate, deltaTime);
            
            //water level change rate dependent on temp, so it is calced after
            state.waterLevel_ml -= Def.CalcWaterLossRate(state.soilTemp, state.WaterMlPerCubicMeter) * deltaTime;
            
            //TODO: anything else?
        }

        #endregion

        private Temperature _CalcAirTemperature()
        {
            if (!ServiceLocator.TryResolve(out IUnityClimateDataService climate)) return defaultTemp;
            
            WeatherState localWeatherState = climate.GetWeatherAt(transform.position);
            return localWeatherState.AirTemperature;
        }

    }
}