using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using IDEK.Tools.Logging;
using JetBrains.Annotations;
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
    /// The struct internally represents the temperature in Kelvin (<see cref="_rawTemperatureKelvin"/>).
    /// <para/>
    /// Internally using Fahrenheit would on average result in slightly more drift against the other two units.
    /// Kelvin is a standard unit which shares the same "scaling" as Celsius--another supported unit--
    /// but starts at Absolute Zero. This means we never have to worry about it being negative,
    /// making it ideal for the internal representation. 
    /// </remarks>
    [Serializable]
    [System.Diagnostics.DebuggerDisplay("Temperature: {ToString(Temperature.DEBUG_STRING_FORMAT_CODE, null)}")]
    public struct Temperature : IFormattable
    {
        public static class Constants
        {
            public static class EarthStandard
            {
                public static Temperature WaterFreeze => FromCelsius(0f);
                public static Temperature WaterBoil => FromCelsius(100f);
            }
        }
        
        public enum Unit { Fahrenheit, Celsius, Kelvin, Raw }
        
        //Custom ToString() format codes
        public const string KELVIN_STRING_FORMAT_CODE = "K";
        public const string FAHRENHEIT_STRING_FORMAT_CODE = "F";
        public const string CELSIUS_STRING_FORMAT_CODE = "C";
        public const string RAW_STRING_FORMAT_CODE = "RAW";
        public const string DEBUG_STRING_FORMAT_CODE = "ALL";
        public const string KFC_STRING_FORMAT_CODE = "KFC";

        //Symbols
        public const string DEGREE_SYMBOL = "\u00b0";
        public const string FAHRENHEIT_SYMBOL = DEGREE_SYMBOL + "F";
        public const string CELSIUS_SYMBOL = DEGREE_SYMBOL + "C";
        public const string KELVIN_SYMBOL = "K";

        //Math Constants
        public const float CELSIUS_ZERO_IN_KELVIN = 273.15f;
        public const float FAHRENHEIT_INTERVAL_IN_KELVIN = C_TO_F_RATIO;
        public const float CELSIUS_INTERVAL_IN_KELVIN = 1f;
        private const float C_TO_F_RATIO = 9f / 5f; //Made private since misleading 
        private const float F_TO_C_RATIO = 5f / 9f; //Made private since misleading
        
        /// <summary>
        /// Two of the 3 systems have the same scaling, but kelvin puts 0 at absolute zero,
        /// so we never have to worry about it being negative, making it our ideal representation.
        /// </summary>
#if ODIN_INSPECTOR
        [FormerlySerializedAs("rawTemperatureKelvin")]
        [HideInInspector]
#endif
        [SerializeField]
        private float _rawTemperatureKelvin;

        /// <inheritdoc cref="InFahrenheit"/>
        public float F
        {
            get => KelvinToFahrenheit(_rawTemperatureKelvin);
            set => _rawTemperatureKelvin = FahrenheitToKelvin(value);
        }

        /// <summary> Fahrenheit representation of the temperature </summary>
        public float InFahrenheit
        {
            get => KelvinToFahrenheit(_rawTemperatureKelvin);
            set => _rawTemperatureKelvin = FahrenheitToKelvin(value);
        }

        /// <inheritdoc cref="InCelsius"/>
        public float C
        {
            get => KelvinToCelsius(_rawTemperatureKelvin);
            set => _rawTemperatureKelvin = CelsiusToKelvin(value);
        }

        /// <summary> Celsius representation of the temperature. </summary>
        public float InCelsius
        {
            get => KelvinToCelsius(_rawTemperatureKelvin);
            set => _rawTemperatureKelvin = CelsiusToKelvin(value);
        }

        /// <inheritdoc cref="InKelvin"/>
        public float K
        {
            get => _rawTemperatureKelvin;
            set => _rawTemperatureKelvin = value;
        }

        /// <summary> Kelvin representation of the temperature. </summary>
        public float InKelvin
        {
            get => _rawTemperatureKelvin;
            set => _rawTemperatureKelvin = value;
        }

        /// <summary>
        /// Raw representation of the temperature.
        /// </summary>
        public float Raw
        {
            get => _rawTemperatureKelvin;
            set => _rawTemperatureKelvin = value;
        }


        /// <summary>
        /// Representation the temperature in terms of the given unit.
        /// </summary>
        public float In(Unit tempUnit) => tempUnit switch {
            Unit.Fahrenheit => InFahrenheit,
            Unit.Celsius => InCelsius,
            Unit.Kelvin => InKelvin,
            Unit.Raw => Raw,
            _ => throw new ArgumentOutOfRangeException(nameof(tempUnit), tempUnit, null)
        };
        
        #region Float Conversions

        public static float RawIntervalSize(Unit tempUnit) => tempUnit switch
        {
            Unit.Fahrenheit => FAHRENHEIT_INTERVAL_IN_KELVIN,
            Unit.Celsius => 1f,
            Unit.Kelvin => 1f,
            Unit.Raw => 1f,
            _ => throw new ArgumentOutOfRangeException(nameof(tempUnit), tempUnit, null)
        };
        
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

        #endregion

        #region Factories
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Temperature FromUnit(float tempInUnit, Unit inputUnit, Unit displayUnit) => new(tempInUnit, inputUnit, displayUnit);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Temperature FromUnit(float tempInUnit, Unit inputUnit) => new(tempInUnit, inputUnit);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Temperature FromFahrenheit(float fahrenheit) => new(fahrenheit, Unit.Fahrenheit);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Temperature FromCelsius(float celsius) => new(celsius, Unit.Celsius);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Temperature FromKelvin(float kelvin) => new(kelvin, Unit.Kelvin);
        
        //TODO: Rankine?

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Temperature FromF(float fahrenheit) => FromFahrenheit(fahrenheit);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Temperature FromC(float celsius) => FromCelsius(celsius);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Temperature FromK(float kelvin) => FromKelvin(kelvin);

        //TODO: Rankine?

        /// <summary>
        /// The amount of thermal energy required to raise the temperature by 1&#x00b0;F
        /// </summary>
        public static Temperature FahrenheitInterval => FromKelvin(FAHRENHEIT_INTERVAL_IN_KELVIN);
        //CelsiusInterval is just 1f - that would be silly to put here
        public static Temperature AbsoluteZero => FromKelvin(0f);
        
        #endregion

        #region Constructors
        
        public Temperature(float tempInUnit, Unit inputUnit) : this(tempInUnit, inputUnit, inputUnit) { }
        public Temperature(float tempInUnit, Unit inputUnit, Unit newDisplayUnit) : this()
        {
            _rawTemperatureKelvin = inputUnit switch
            {
                Unit.Fahrenheit => FahrenheitToKelvin(tempInUnit),
                Unit.Celsius => CelsiusToKelvin(tempInUnit),
                Unit.Kelvin => tempInUnit,
                Unit.Raw => tempInUnit,
                _ => throw new ArgumentException("Invalid temperature unit.")
            };

#if DEBUG && !SILENCE_TEMPERATURE_RANGE_WARNINGS
            //technically it should be <= 0f, since Absolute Zero is an asymptote, but it's the default value
            //for the struct, so we're excluding it. It will occur during normal operation. 
            if(_rawTemperatureKelvin < 0f) 
                Debug.LogError("Temperature struct set to impossible temperature: " + ToDebugString());
#endif
            
            defaultDisplayUnit = newDisplayUnit;
#if ODIN_INSPECTOR && UNITY_EDITOR
            editorDisplayValue = tempInUnit;
#endif
        }

        #endregion
        
        #region Operators
        
        //these ended up being misleading; Temperature.FromFahrenheit(72f) + Temperature.FromFahrenheit(1f) != Temperature.FromFahrenheit(73f) for example  
        // /// <summary>
        // /// Subtracts the two temperatures with respect to Absolute Zero.
        // /// </summary>
        // /// <remarks>
        // /// If you are trying to do relative increases (like raising the room temp by 1 degrees Fahrenheit), you'll need to specify the other temp.
        // /// Example: `roomTemp += Temperature.FromFahrenheit(1f)` or `roomTemp += Temperature.F_1
        // /// </remarks>
        // public static Temperature operator -(Temperature left, Temperature right) => left.WithRawTemp(left.Raw - right.Raw);
        //
        // /// <summary>
        // /// Adds the two temperatures with respect to Absolute Zero.
        // /// </summary>
        // public static Temperature operator +(Temperature left, Temperature right) => left.WithRawTemp(left.Raw + right.Raw);
        //
        // /// <inheritdoc cref="ScaledThermalEnergy"/>
        // public static Temperature operator *(Temperature temp, float scaleFactor) => temp.ScaledThermalEnergy(scaleFactor);
        //
        // /// <inheritdoc cref="ScaledThermalEnergy"/>
        // public static Temperature operator *(float scaleFactor, Temperature temp) => temp.ScaledThermalEnergy(scaleFactor);

        #endregion
        
        #region Operations

        /// <summary>
        /// Modifies the current temperature struct in-place.
        /// More performant than <see cref="WithChange"/>, but it mutates the given struct.
        /// </summary>
        /// <param name="deltaTemp">The change in temperature, in terms of <see cref="deltaTempUnit"/>.</param>
        /// <param name="deltaTempUnit">The unit of measurement that the temperature delta is using.</param>
        /// <remarks>
        /// It is important to note that in some temperature units, like Fahrenheit and Celsius,
        /// the zero-points are not at Absolute Zero.
        /// This means that 0&#x00b0;F and 0&#x00b0;C do not represent zero thermal energy.
        /// Since <see cref="Temperature"/> objects use a more consistent thermal representation (currently in Kelvin),
        /// directly adding, subtracting, or multiplying two <see cref="Temperature"/> objects together can give rather unintuitive results.
        /// <para/>
        /// For example:
        /// <code>
        /// //You will find that each boolean statement here holds true.
        /// Temperature.FromFahrenheit(72f).Raw + Temperature.FromFahrenheit(1f).Raw != Temperature.FromFahrenheit(73f).Raw
        /// //Breakdown:
        /// Temperature.FromFahrenheit(72f).Raw == 295.372f
        /// Temperature.FromFahrenheit(1f).Raw == 255.928f
        /// Temperature.FromFahrenheit(72f).Raw + Temperature.FromFahrenheit(1f).Raw == 551.3f
        /// //Which becomes, for those curious:
        /// 551.3f == Temperature.FromFahrenheit(532.67f).Raw
        /// //whereas
        /// Temperature.FromFahrenheit(72f).WithChange(1f, Temperature.Unit.Fahrenheit) == Temperature.FromFahrenheit(73f)
        /// //and
        /// </code>
        /// That is why the <see cref="Temperature"/> class does not overload the mathematical operators;
        /// it's incredibly misleading and confusing for developers to work with, providing false intuitions.
        /// <para/>
        /// This function instead takes in the delta and the unit,
        /// scales the INTERVAL of one unit as it would be in Kelvin, and adds that delta to the raw value.
        /// So it's not <c>72&#x00b0; + 1&#x00b0;</c>, it's <c>72&#x00b0; + the total energy required to INCREASE BY 1&#x00b0;</c>
        /// </remarks>
        public void ChangeBy(float deltaTemp, Unit deltaTempUnit)
        {
            _rawTemperatureKelvin += RawIntervalSize(deltaTempUnit) * deltaTemp;
        }

        /// <summary>
        /// Returns a new <see cref="Temperature"/> struct with the raw temp modified by the amount.
        /// Treats Temperature as immutable, but does allocate a new struct each time.
        /// </summary>
        /// <returns>A new <see cref="Temperature"/> with the updated value.</returns>
        /// <inheritdoc cref="ChangeBy"/>
        public Temperature WithChange(float deltaTemp, Unit deltaTempUnit)
        {
            float rawDeltaEnergy = RawIntervalSize(deltaTempUnit) * deltaTemp;
            return WithRawTemp(_rawTemperatureKelvin + rawDeltaEnergy);
        }

        /// <summary>
        /// Directly sums the total thermal energy of the parameters.
        /// Unless you're working with Kelvin, this will not look like regular addition.
        /// </summary>
        /// <returns>A new <see cref="Temperature"/> whose total energy equals the sum of the energies of its parameters</returns>
        public static Temperature EnergySum(Temperature a, Temperature b)
        {
            return a.WithRawTemp(a.Raw + b.Raw);
        }

        /// <inheritdoc cref="EnergySum(IDEK.PlantGame.Ecology.Temperature,IDEK.PlantGame.Ecology.Temperature)"/>
        /// <param name="temps">Parameter array.</param>
        public static Temperature EnergySum(params Temperature[] temps)
        {
            return temps.FirstOrDefault().WithRawTemp(temps.Sum(x => x.Raw));
        }

        /// <inheritdoc cref="EnergySum(IDEK.PlantGame.Ecology.Temperature,IDEK.PlantGame.Ecology.Temperature)"/>
        /// /// <param name="temps">Parameters.</param>
        public static Temperature EnergySum(IEnumerable<Temperature> temps)
        {
            var tempsArray = temps as Temperature[] ?? temps.ToArray();//cast or transform into array
            return tempsArray.FirstOrDefault().WithRawTemp(tempsArray.Sum(x => x.Raw));
        }

        public static Temperature Average(params Temperature[] temps)
        {
            return temps.FirstOrDefault().WithRawTemp(temps.Average(x => x.Raw));
        }

        public static Temperature Average(IEnumerable<Temperature> temps)
        {
            var tempsArray = temps as Temperature[] ?? temps.ToArray(); //cast or transform into array
            return tempsArray.FirstOrDefault().WithRawTemp(tempsArray.Average(x => x.Raw));
        }
        
        /// <summary>
        /// Scales the thermal energy of the temperature by the given constant, with respect to Absolute Zero.
        /// </summary>
        /// <remarks>
        /// For consistency, both internally and with scientific conventions, this is done relative to absolute zero.
        /// </remarks>
        public Temperature EnergyScaledBy(float scaleFactor) => WithRawTemp(Raw * scaleFactor);
        
        /// <summary>
        /// Makes a copy of all the struct's other values, changing only the raw temp.
        /// </summary>
        /// <returns></returns>
        public Temperature WithRawTemp(float newRaw) => FromUnit(newRaw, Unit.Raw, defaultDisplayUnit);
        
        #endregion
        
        #region Display Logic
        
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
                _rawTemperatureKelvin = defaultDisplayUnit switch
                {
                    Unit.Fahrenheit => FahrenheitToKelvin(editorDisplayValue),
                    Unit.Celsius => CelsiusToKelvin(editorDisplayValue),
                    Unit.Kelvin => editorDisplayValue,
                    Unit.Raw => editorDisplayValue,
                    _ => _rawTemperatureKelvin //if bad input, keep temp unchanged to avoid data corruption
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
                Unit.Fahrenheit => KelvinToFahrenheit(_rawTemperatureKelvin),
                Unit.Celsius => KelvinToCelsius(_rawTemperatureKelvin),
                Unit.Kelvin => _rawTemperatureKelvin,
                Unit.Raw => _rawTemperatureKelvin,
                _ => _rawTemperatureKelvin //if bad input, keep temp unchanged to avoid data corruption
            };
        }
#endif

        #endregion Display Logic

        #region ToString
        
        /// <summary>
        /// Renders temp to string using the given <see cref="Unit"/>. 
        /// </summary>
        /// <param name="unit">The temperature unit to display in. Must be a valid member of the <see cref="Unit"/> enum.</param>
        /// <exception cref="ArgumentException">Thrown if the given <see cref="Unit"/> is not a valid enum value.</exception>
        public string ToString(Unit unit, [CanBeNull] string numberFormatCode=null, IFormatProvider formatProvider=null)
        {
            formatProvider ??= CultureInfo.CurrentCulture;
            numberFormatCode = string.IsNullOrEmpty(numberFormatCode) ? null : numberFormatCode;
            return unit switch
            {
                Unit.Fahrenheit => $"{InFahrenheit.ToString(numberFormatCode ?? "F1", formatProvider)}{FAHRENHEIT_SYMBOL}",
                Unit.Celsius => $"{InCelsius.ToString(numberFormatCode ?? "F1", formatProvider)}{CELSIUS_SYMBOL}",
                Unit.Kelvin => $"{InKelvin.ToString(numberFormatCode ?? "F2", formatProvider)}{KELVIN_SYMBOL}",
                Unit.Raw => $"{Raw.ToString(numberFormatCode ?? "F4", formatProvider)}",
                _ => throw new ArgumentException("Invalid temperature unit.")
            };
        }
        
        #endregion ToString

        #region Overrides of ValueType

        /// <summary>
        /// Renders temp to string using the <see cref="defaultDisplayUnit"/> member field.
        /// </summary>
        /// <remarks>
        /// Would instead recommend sticking with the <see cref="ToString(IDEK.PlantGame.Ecology.Temperature.Unit)"/>
        /// overload when possible for consistent results.
        /// </remarks>
        public override string ToString() => ToString(defaultDisplayUnit);
        
        public string ToDebugString() => ToString(DEBUG_STRING_FORMAT_CODE, null);

        #endregion Overrides of ValueType
        
        #region IFormattable Implementation
        
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format)) format = defaultDisplayUnit switch
            {
                Unit.Kelvin => KELVIN_STRING_FORMAT_CODE,
                Unit.Fahrenheit => FAHRENHEIT_STRING_FORMAT_CODE,
                Unit.Celsius => CELSIUS_STRING_FORMAT_CODE,
                Unit.Raw => RAW_STRING_FORMAT_CODE,
                _ => throw new InvalidOperationException(
                    $"{nameof(defaultDisplayUnit)} has an invalid value of {defaultDisplayUnit}. " +
                    $"This is likely either a serialization error, " +
                    $"or someone forgot to update this switch case after adding a new temperature Unit.")
            }; 

            string formattedFormat = format.ToUpperInvariant();

            if (formattedFormat == KFC_STRING_FORMAT_CODE)
            {
                ConsoleLog.LogError("KFC is not a temperature format! I think you're hungry. " +
                    "If you instead meant Kelvin-Fahrenheit-Celsius, you'll need to do " +
                    "them as separate function calls.");
            }
            return formattedFormat switch
            {
                KELVIN_STRING_FORMAT_CODE => ToString(Unit.Kelvin, null, formatProvider),
                FAHRENHEIT_STRING_FORMAT_CODE => ToString(Unit.Fahrenheit, null, formatProvider),
                CELSIUS_STRING_FORMAT_CODE => ToString(Unit.Celsius, null, formatProvider),
                RAW_STRING_FORMAT_CODE => ToString(Unit.Raw, null, formatProvider),
                DEBUG_STRING_FORMAT_CODE => $"{ToString(Unit.Kelvin, null, formatProvider)} | {ToString(Unit.Fahrenheit, null, formatProvider)} | {ToString(Unit.Celsius, null, formatProvider)} | Raw: {ToString(Unit.Raw, null, formatProvider)}",
                KFC_STRING_FORMAT_CODE => ToString(Unit.Raw, null, formatProvider) + " Kentucky Fried Chickens",
                _ => ToString(Unit.Raw, formattedFormat, formatProvider)
            };
        }
        
        #endregion IFormattable Implementation
    }
}