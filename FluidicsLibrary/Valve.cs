using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using SE_Library;
using CAD;
using Mathematics;

namespace Fluidics
{
    public class Valve : SE_System
    {
        //  *****************************************************************************************
        //  DECLARATIONS
        //
        //  ************************************************************
        #region
        //  
        //  Identification

        //
        //  Data
        private ValveTypeEnum _MyValveType;
        private CAD_Parameter _position = new CAD_Parameter();
        private double _positionValue;

        //
        //  Owned & Owning Objects

        #endregion
        //  *****************************************************************************************


        //  ****************************************************************************************
        //  INITIALIZATIONS
        //
        //  ************************************************************
        #region

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  ENUMERATIONS
        //
        //  ************************************************************
        #region
        public enum ValveTypeEnum
        {
            Ball = 0,
            Needle,
            Butterfly,
            Koltek,
            Gate,
            Globe,
            Check,
            Relief,
            Solenoid,
            Other
        }

        public enum ValveStatusEnum
        {
            Closed = 0,
            Open,
            Throttling,
            Faulted
        }

        public enum FailModeEnum
        {
            FailClosed = 0,
            FailOpen,
            FailInPlace
        }

        public enum FlowCharacteristicEnum
        {
            Linear = 0,
            EqualPercentage,
            QuickOpening
        }
        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  VALVE CONSTRUCTOR
        //
        //  ************************************************************
        #region
        public Valve()
        {
            Status = ValveStatusEnum.Closed;
            FailMode = FailModeEnum.FailClosed;
            FlowCharacteristic = FlowCharacteristicEnum.Linear;
            Cv = 1.0;
        }

        public Valve(ValveTypeEnum valveType, double nominalSize, double cv)
            : this()
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(nominalSize);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cv);

            _MyValveType = valveType;
            NominalSize = nominalSize;
            Cv = cv;
        }
        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  PROPERTIES
        //
        //  ************************************************************
        #region
        //
        //  Identification

        /// <summary>
        /// Type of valve (ball, needle, butterfly, etc.).
        /// </summary>
        public ValveTypeEnum ValveType
        {
            get => _MyValveType;
            set => _MyValveType = value;
        }

        /// <summary>
        /// Current operating status of the valve.
        /// </summary>
        public ValveStatusEnum Status { get; private set; }

        /// <summary>
        /// Failure mode behavior of the valve.
        /// </summary>
        public FailModeEnum FailMode { get; set; }

        /// <summary>
        /// Flow characteristic curve type.
        /// </summary>
        public FlowCharacteristicEnum FlowCharacteristic { get; set; }

        //  
        //  Data

        /// <summary>
        /// Valve position as a CAD_Parameter.
        /// </summary>
        public CAD_Parameter Position
        {
            get => _position;
            private set => _position = value;
        }

        /// <summary>
        /// Valve position as a numeric fraction (0.0 = fully closed, 1.0 = fully open).
        /// Setting this value also updates <see cref="Position"/> and <see cref="Status"/>.
        /// </summary>
        public double PositionValue
        {
            get => _positionValue;
            private set
            {
                _positionValue = Math.Clamp(value, 0.0, 1.0);
                Status = _positionValue switch
                {
                    <= 0.0 => ValveStatusEnum.Closed,
                    >= 1.0 => ValveStatusEnum.Open,
                    _ => ValveStatusEnum.Throttling
                };
            }
        }

        /// <summary>
        /// Nominal pipe size in meters.
        /// </summary>
        public double NominalSize { get; set; }

        /// <summary>
        /// Flow coefficient (Cv) at fully open position.
        /// </summary>
        public double Cv { get; set; }

        /// <summary>
        /// Inlet (upstream) pressure in Pascals.
        /// </summary>
        public double InletPressure { get; set; }

        /// <summary>
        /// Outlet (downstream) pressure in Pascals.
        /// </summary>
        public double OutletPressure { get; set; }

        /// <summary>
        /// Fluid density in kg/m³ (default water at ~20°C).
        /// </summary>
        public double FluidDensity { get; set; } = 998.0;

        /// <summary>
        /// Cracking pressure for check/relief valves in Pascals.
        /// </summary>
        public double CrackingPressure { get; set; }

        /// <summary>
        /// Set pressure for relief valves in Pascals.
        /// </summary>
        public double SetPressure { get; set; }

        /// <summary>
        /// Maximum allowable operating pressure in Pascals.
        /// </summary>
        public double MaxPressure { get; set; }

        //
        //  Computed Properties

        /// <summary>
        /// Pressure drop across the valve in Pascals.
        /// </summary>
        public double PressureDrop => InletPressure - OutletPressure;

        /// <summary>
        /// Whether the valve is fully closed.
        /// </summary>
        public bool IsClosed => PositionValue <= 0.0;

        /// <summary>
        /// Whether the valve is fully open.
        /// </summary>
        public bool IsFullyOpen => PositionValue >= 1.0;

        /// <summary>
        /// Whether the valve is in a throttling (partially open) state.
        /// </summary>
        public bool IsThrottling => PositionValue > 0.0 && PositionValue < 1.0;

        /// <summary>
        /// Effective Cv at the current position based on the flow characteristic.
        /// </summary>
        public double EffectiveCv => Cv * CalculateCharacteristicFactor(PositionValue);

        //
        //  Owned & Owning Objects

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  METHODS
        //
        //  ************************************************************
        #region

        /// <summary>
        /// Opens the valve fully.
        /// </summary>
        public void Open()
        {
            if (Status == ValveStatusEnum.Faulted)
                return;

            PositionValue = 1.0;
        }

        /// <summary>
        /// Closes the valve fully.
        /// </summary>
        public void Close()
        {
            if (Status == ValveStatusEnum.Faulted)
                return;

            PositionValue = 0.0;
        }

        /// <summary>
        /// Sets the valve to a specific position (0.0 to 1.0).
        /// </summary>
        /// <param name="position">Target position as a fraction.</param>
        public void SetPosition(double position)
        {
            if (Status == ValveStatusEnum.Faulted)
                return;

            PositionValue = position;
        }

        /// <summary>
        /// Incrementally opens the valve by the specified amount.
        /// </summary>
        /// <param name="amount">Fraction to open (0.0 to 1.0).</param>
        public void IncrementOpen(double amount)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(amount);
            SetPosition(PositionValue + amount);
        }

        /// <summary>
        /// Incrementally closes the valve by the specified amount.
        /// </summary>
        /// <param name="amount">Fraction to close (0.0 to 1.0).</param>
        public void IncrementClose(double amount)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(amount);
            SetPosition(PositionValue - amount);
        }

        /// <summary>
        /// Triggers a fault condition; the valve moves to its fail-safe position.
        /// </summary>
        public void TriggerFault()
        {
            switch (FailMode)
            {
                case FailModeEnum.FailClosed:
                    _positionValue = 0.0;
                    break;
                case FailModeEnum.FailOpen:
                    _positionValue = 1.0;
                    break;
                case FailModeEnum.FailInPlace:
                    // Position stays where it is
                    break;
            }

            Status = ValveStatusEnum.Faulted;
        }

        /// <summary>
        /// Clears a fault condition and resets the valve to closed.
        /// </summary>
        public void ClearFault()
        {
            if (Status == ValveStatusEnum.Faulted)
            {
                _positionValue = 0.0;
                Status = ValveStatusEnum.Closed;
            }
        }

        /// <summary>
        /// Calculates the volumetric flow rate through the valve using the Cv equation.
        /// Q = Cv_eff × √(ΔP / SG)
        /// </summary>
        /// <returns>Flow rate in gallons per minute (GPM).</returns>
        public double CalculateFlowRate()
        {
            if (IsClosed || PressureDrop <= 0)
                return 0.0;

            // Specific gravity relative to water
            const double waterDensity = 998.0;
            double specificGravity = FluidDensity / waterDensity;

            if (specificGravity <= 0)
                return 0.0;

            // Convert pressure drop from Pascals to PSI (1 PSI = 6894.76 Pa)
            double pressureDropPsi = PressureDrop / 6894.76;

            return EffectiveCv * Math.Sqrt(pressureDropPsi / specificGravity);
        }

        /// <summary>
        /// Calculates the flow characteristic factor for the given position.
        /// </summary>
        /// <param name="position">Valve position (0.0 to 1.0).</param>
        private double CalculateCharacteristicFactor(double position)
        {
            return FlowCharacteristic switch
            {
                // f(x) = x
                FlowCharacteristicEnum.Linear => position,

                // f(x) = R^(x-1), using rangeability R = 50
                FlowCharacteristicEnum.EqualPercentage => Math.Pow(50.0, position - 1.0),

                // f(x) = √x
                FlowCharacteristicEnum.QuickOpening => Math.Sqrt(position),

                _ => position
            };
        }

        /// <summary>
        /// Calculates the resistance coefficient (K) from the current effective Cv and nominal size.
        /// K = 891 × d⁴ / Cv²  (d in inches)
        /// </summary>
        public double CalculateResistanceCoefficient()
        {
            if (EffectiveCv <= 0 || NominalSize <= 0)
                return double.PositiveInfinity;

            // Convert nominal size from meters to inches
            double diameterInches = NominalSize / 0.0254;
            return 891.0 * Math.Pow(diameterInches, 4) / Math.Pow(EffectiveCv, 2);
        }

        /// <summary>
        /// Checks whether the inlet pressure exceeds the maximum allowable pressure.
        /// </summary>
        public bool IsOverPressure() => MaxPressure > 0 && InletPressure > MaxPressure;

        /// <summary>
        /// For check/relief valves, determines whether the differential pressure
        /// exceeds the cracking pressure threshold.
        /// </summary>
        public bool IsCrackingPressureExceeded() => PressureDrop >= CrackingPressure && CrackingPressure > 0;

        public override string ToString() =>
            $"Valve [{_MyValveType}, {Status}, Pos={PositionValue * 100:F1}%, ΔP={PressureDrop / 1000:F2} kPa]";

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  EVENTS
        //
        //  ************************************************************
        #region

        #endregion
        //  *****************************************************************************************
    }
}
