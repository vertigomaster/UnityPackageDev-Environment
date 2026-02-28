using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace IDEK.PlantGame.Ecology
{
    /// <summary>
    /// Struct wrapper that avoids unit mixups and assumptions.
    /// <br/>
    /// Can explicitly retrieve the temperature as a float in the unit desired with <see cref="InFahrenheit"/>, <see cref="InCelsius"/>, and <see cref="InKelvin"/>.
    /// <br/>
    /// This class also provides multiple static temperature conversion functions and some related constants.
    /// </summary>
    /// <remarks>
    /// Odin Inspector supported! Note that in editor builds the struct will appear much larger due to editor-only visualization data
    /// <para/>
    /// The struct internally represents the temperature in Kelvin (<see cref="rawTemperatureKelvin"/>).
    /// <para/>
    /// Internally using Fahrenheit would on average result in slightly more drift against the other two units.
    /// Kelvin is a standard unit which shares the same "scaling" as Celsius--another supported unit--
    /// but starts at Absolute Zero. This means we never have to worry about it being negative,
    /// making it ideal for the internal representation. 
    /// </remarks>
    [Serializable]
    public struct Temperature : IFormattable
    {
        public enum Unit { Fahrenheit, Celsius, Kelvin }
        public const string DEGREE_SYMBOL = "\u00b0";
        public const string FAHRENHEIT_SYMBOL = DEGREE_SYMBOL + "F";
        public const string CELSIUS_SYMBOL = DEGREE_SYMBOL + "C";
        public const string KELVIN_SYMBOL = "K";
        
        public const float CELSIUS_ZERO_IN_KELVIN = 273.15f;
        //made these private because seeing it in intellisense is misleading;
        //you need more than just these ratio to correctly do the conversion.
        private const float C_TO_F_RATIO = 5f/9f; 
        private const float F_TO_C_RATIO = 9f/5f;
        
        // public static Temperature FromFahrenheit(float fahrenheit) => new
        // public static Temperature FromUnit(float fahrenheit) => new

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float KelvinToFahrenheit(float kelvin) => CelsiusToFahrenheit(KelvinToCelsius(kelvin));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float FahrenheitToKelvin(float fahrenheit) => CelsiusToKelvin(FahrenheitToCelsius(fahrenheit));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float FahrenheitToCelsius(float fahrenheit) => (fahrenheit - 32f) * F_TO_C_RATIO;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CelsiusToFahrenheit(float celsius) => (celsius / C_TO_F_RATIO) + 32f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float KelvinToCelsius(float kelvin) => kelvin - CELSIUS_ZERO_IN_KELVIN;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CelsiusToKelvin(float celsius) => CELSIUS_ZERO_IN_KELVIN + celsius;
        
        public static Temperature FromUnit(float tempInUnit, Unit inputUnit) => new(tempInUnit, inputUnit);
        public static Temperature FromFahrenheit(float fahrenheit) => new(fahrenheit, Unit.Fahrenheit);
        public static Temperature FromCelsius(float celsius) => new(celsius, Unit.Celsius);
        public static Temperature FromKelvin(float kelvin) => new(kelvin, Unit.Kelvin);
        
        public Temperature(float tempInUnit, Unit inputUnit) : this()
        {
            rawTemperatureKelvin = inputUnit switch
            {
                Unit.Fahrenheit => FahrenheitToKelvin(tempInUnit),
                Unit.Celsius => CelsiusToKelvin(tempInUnit),
                Unit.Kelvin => tempInUnit,
                _ => throw new ArgumentException("Invalid temperature unit.")
            };
            
            defaultDisplayUnit = inputUnit;
#if ODIN_INSPECTOR && UNITY_EDITOR
            editorDisplayValue = tempInUnit;
#endif
        }
        
        /// <summary>
        /// Unit used to display the temperature, both in the inspector and with the default <see cref="ToString()"/>
        /// </summary>
        [SerializeField]
#if ODIN_INSPECTOR
        [HideLabel]
        [HorizontalGroup("Temp")]
        [ValueDropdown("_unitsDropdown")]
        [OnValueChanged("INSPECTOR_OnDisplayUnitChanged")]
#endif
        public Unit defaultDisplayUnit; 
                                        
        
#if ODIN_INSPECTOR && UNITY_EDITOR
        //We only compile this stuff in editor to avoid bloating the struct to like 5x its regular size!
        
        //serializing this stabilizes the value, unless opening the inspector triggers both OnValueChanged callbacks
        [SerializeField, HideLabel]
        [HorizontalGroup("Temp", Width = 0.7f)]
        [OnValueChanged("INSPECTOR_OnDisplayValueChanged")]
        private float editorDisplayValue;

        private static IEnumerable _unitsDropdown = new ValueDropdownList<Unit>()
        {
            { FAHRENHEIT_SYMBOL, Unit.Fahrenheit },
            { CELSIUS_SYMBOL, Unit.Celsius },
            { KELVIN_SYMBOL, Unit.Kelvin }
        };
        
        //helps avoid drift by not churning the underlying value on every unit change
        private bool _unitChangedDebouncer;

        ///update the raw value from the display
        private void INSPECTOR_OnDisplayValueChanged()
        {
            //avoids churn, as updating the Unit will change this float but not in a way that should
            //change the underlying value.
            //Without this, swapping the unit display back and forth would cause the temp to drift a
            //tiny bit every time, which is annoying to look at.
            if(!_unitChangedDebouncer)
            {
                rawTemperatureKelvin = defaultDisplayUnit switch
                {
                    Unit.Fahrenheit => FahrenheitToKelvin(editorDisplayValue),
                    Unit.Celsius => CelsiusToKelvin(editorDisplayValue),
                    Unit.Kelvin => editorDisplayValue,
                    _ => rawTemperatureKelvin //if bad input, keep temp unchanged to avoid data corruption
                };
            }

            //turn off the flag every time
            _unitChangedDebouncer = false;
        }

        //update the display value (based on Unit)
        private void INSPECTOR_OnDisplayUnitChanged()
        {
            _unitChangedDebouncer = true;
            editorDisplayValue = defaultDisplayUnit switch
            {
                Unit.Fahrenheit => KelvinToFahrenheit(rawTemperatureKelvin),
                Unit.Celsius => KelvinToCelsius(rawTemperatureKelvin),
                Unit.Kelvin => rawTemperatureKelvin,
                _ => rawTemperatureKelvin //if bad input, keep temp unchanged to avoid data corruption
            };
        }
#endif
        
        /// <summary>
        /// Two of the 3 systems have the same scaling, but kelvin puts 0 at absolute zero,
        /// so we never have to worry about it being negative, making it our ideal representation.
        /// </summary>
#if ODIN_INSPECTOR
        [FormerlySerializedAs("_rawTemperatureKelvin")]
        [HideInInspector]
#endif
        [SerializeField]
        private float rawTemperatureKelvin;

        public float InFahrenheit
        {
            get => KelvinToFahrenheit(rawTemperatureKelvin);
            set => rawTemperatureKelvin = FahrenheitToKelvin(value);
        }

        public float InCelsius
        {
            get => KelvinToCelsius(rawTemperatureKelvin);
            set => rawTemperatureKelvin = CelsiusToKelvin(value);
        }

        public float InKelvin
        {
            get => rawTemperatureKelvin;
            set => rawTemperatureKelvin = value;
        }
        
        /// <summary>
        /// Renders temp to string using the given <see cref="Unit"/>. 
        /// </summary>
        /// <param name="unit">The temperature unit to display in. Must be a valid member of the <see cref="Unit"/> enum.</param>
        /// <exception cref="ArgumentException">Thrown if the given <see cref="Unit"/> is not a valid enum value.</exception>
        public string ToString(Unit unit)
        {
            return unit switch
            {
                Unit.Fahrenheit => $"{InFahrenheit} {FAHRENHEIT_SYMBOL}",
                Unit.Celsius => $"{InCelsius} {CELSIUS_SYMBOL}",
                Unit.Kelvin => $"{InKelvin} {KELVIN_SYMBOL}",
                _ => throw new ArgumentException("Invalid temperature unit.")
            };
        }

        #region Overrides of ValueType

        /// <summary>
        /// Renders temp to string using the <see cref="defaultDisplayUnit"/> member field.
        /// </summary>
        /// <remarks>
        /// Would instead recommend sticking with the <see cref="ToString(IDEK.PlantGame.Ecology.Temperature.Unit)"/>
        /// overload when possible for consistent results.
        /// </remarks>
        public override string ToString() => ToString(defaultDisplayUnit);

        #endregion
    }
}