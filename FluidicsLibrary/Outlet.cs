using System;
using System.Collections.Generic;
using System.Text;

namespace FluidicsLibrary
{
    /// <summary>
    /// Represents a fluid outlet port where fluid exits a system or component.
    /// </summary>
    internal class Outlet
    {
        //  *****************************************************************************************
        //  DECLARATIONS
        //
        //  ************************************************************
        #region

        private double _position;

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  ENUMERATIONS
        //
        //  ************************************************************
        #region

        public enum OutletTypeEnum
        {
            Pipe = 0,
            Flange,
            Threaded,
            QuickConnect,
            WeldNeck,
            Barbed,
            Submerged,
            FreeDischarge,
            Other
        }

        public enum OutletGeometryEnum
        {
            Circular = 0,
            Rectangular,
            Annular,
            Other
        }

        public enum OutletStatusEnum
        {
            Open = 0,
            Closed,
            PartiallyOpen,
            Blocked,
            Faulted
        }

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  CONSTRUCTORS
        //
        //  ************************************************************
        #region

        public Outlet()
        {
            Status = OutletStatusEnum.Open;
            _position = 1.0;
            FluidDensity = 998.0;
            DischargeCoefficient = 0.61;
        }

        public Outlet(OutletTypeEnum type, double diameter)
            : this()
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(diameter);

            OutletType = type;
            Geometry = OutletGeometryEnum.Circular;
            Diameter = diameter;
        }

        public Outlet(OutletTypeEnum type, double width, double height)
            : this()
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

            OutletType = type;
            Geometry = OutletGeometryEnum.Rectangular;
            Width = width;
            Height = height;
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
        //

        /// <summary>
        /// Type of outlet connection.
        /// </summary>
        public OutletTypeEnum OutletType { get; set; }

        /// <summary>
        /// Cross-sectional geometry of the outlet.
        /// </summary>
        public OutletGeometryEnum Geometry { get; set; }

        /// <summary>
        /// Current status of the outlet.
        /// </summary>
        public OutletStatusEnum Status { get; private set; }

        /// <summary>
        /// Unique identifier or tag for the outlet.
        /// </summary>
        public string Tag { get; set; } = string.Empty;

        //
        //  Geometry
        //

        /// <summary>
        /// Diameter of a circular outlet in meters.
        /// </summary>
        public double Diameter { get; set; }

        /// <summary>
        /// Width of a rectangular outlet in meters.
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Height of a rectangular outlet in meters.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Wall thickness at the outlet in meters.
        /// </summary>
        public double WallThickness { get; set; }

        /// <summary>
        /// Elevation of the outlet centerline relative to a datum in meters.
        /// </summary>
        public double Elevation { get; set; }

        //
        //  Operating Conditions
        //

        /// <summary>
        /// Static pressure at the outlet in Pascals.
        /// </summary>
        public double Pressure { get; set; }

        /// <summary>
        /// Back pressure (downstream) in Pascals.
        /// </summary>
        public double BackPressure { get; set; }

        /// <summary>
        /// Fluid temperature at the outlet in °C.
        /// </summary>
        public double Temperature { get; set; }

        /// <summary>
        /// Volumetric flow rate through the outlet in m³/s.
        /// </summary>
        public double FlowRate { get; set; }

        /// <summary>
        /// Opening position as a fraction (0.0 = closed, 1.0 = fully open).
        /// </summary>
        public double Position
        {
            get => _position;
            set
            {
                _position = Math.Clamp(value, 0.0, 1.0);
                Status = _position switch
                {
                    <= 0.0 => OutletStatusEnum.Closed,
                    >= 1.0 => OutletStatusEnum.Open,
                    _ => OutletStatusEnum.PartiallyOpen
                };
            }
        }

        //
        //  Fluid Properties
        //

        /// <summary>
        /// Fluid density in kg/m³.
        /// </summary>
        public double FluidDensity { get; set; }

        /// <summary>
        /// Discharge coefficient (C_d) for flow calculations (0.0 to 1.0).
        /// </summary>
        public double DischargeCoefficient { get; set; }

        /// <summary>
        /// Loss coefficient (K-factor) for the outlet geometry.
        /// </summary>
        public double LossCoefficient { get; set; } = 1.0;

        //
        //  Computed Properties
        //

        /// <summary>
        /// Cross-sectional area of the outlet in m².
        /// </summary>
        public double Area => Geometry switch
        {
            OutletGeometryEnum.Circular =>
                Math.PI * Math.Pow(Diameter / 2.0, 2),

            OutletGeometryEnum.Rectangular =>
                Width * Height,

            OutletGeometryEnum.Annular =>
                Math.PI * (Math.Pow(Diameter / 2.0, 2) - Math.Pow(Diameter / 2.0 - WallThickness, 2)),

            _ => 0.0
        };

        /// <summary>
        /// Effective flow area accounting for opening position in m².
        /// </summary>
        public double EffectiveArea => Area * _position;

        /// <summary>
        /// Hydraulic diameter of the outlet in meters.
        /// </summary>
        public double HydraulicDiameter => Geometry switch
        {
            OutletGeometryEnum.Circular => Diameter,

            OutletGeometryEnum.Rectangular =>
                (Width + Height) > 0 ? 2.0 * Width * Height / (Width + Height) : 0.0,

            _ => Diameter
        };

        /// <summary>
        /// Pressure differential across the outlet (outlet − back pressure) in Pascals.
        /// </summary>
        public double PressureDifferential => Pressure - BackPressure;

        /// <summary>
        /// Whether the outlet is fully open.
        /// </summary>
        public bool IsOpen => Status == OutletStatusEnum.Open;

        /// <summary>
        /// Whether the outlet is closed or blocked.
        /// </summary>
        public bool IsClosed => Status is OutletStatusEnum.Closed or OutletStatusEnum.Blocked;

        /// <summary>
        /// Whether there is positive flow through the outlet.
        /// </summary>
        public bool HasFlow => FlowRate > 0 && !IsClosed;

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  METHODS
        //
        //  ************************************************************
        #region

        //
        //  Control
        //

        /// <summary>
        /// Opens the outlet fully.
        /// </summary>
        public void Open()
        {
            if (Status == OutletStatusEnum.Faulted)
                return;

            Position = 1.0;
        }

        /// <summary>
        /// Closes the outlet.
        /// </summary>
        public void Close()
        {
            if (Status == OutletStatusEnum.Faulted)
                return;

            Position = 0.0;
        }

        /// <summary>
        /// Sets the outlet to a specific opening position (0.0 to 1.0).
        /// </summary>
        /// <param name="position">Target position as a fraction.</param>
        public void SetPosition(double position)
        {
            if (Status == OutletStatusEnum.Faulted)
                return;

            Position = position;
        }

        /// <summary>
        /// Marks the outlet as blocked (e.g., clogged or obstructed).
        /// </summary>
        public void SetBlocked()
        {
            Status = OutletStatusEnum.Blocked;
        }

        /// <summary>
        /// Clears a blocked or faulted condition and resets to open.
        /// </summary>
        public void ClearFault()
        {
            if (Status is OutletStatusEnum.Blocked or OutletStatusEnum.Faulted)
                Position = 1.0;
        }

        /// <summary>
        /// Sets the outlet to a faulted state.
        /// </summary>
        public void SetFault()
        {
            Status = OutletStatusEnum.Faulted;
        }

        //
        //  Flow Calculations
        //

        /// <summary>
        /// Calculates the flow velocity at the outlet (v = Q / A_eff) in m/s.
        /// </summary>
        public double CalculateVelocity()
        {
            return EffectiveArea > 0 ? FlowRate / EffectiveArea : 0.0;
        }

        /// <summary>
        /// Calculates the mass flow rate (ṁ = ρQ) in kg/s.
        /// </summary>
        public double CalculateMassFlowRate()
        {
            return FluidDensity * FlowRate;
        }

        /// <summary>
        /// Calculates the discharge flow rate using the orifice equation in m³/s.
        /// Q = C_d × A_eff × √(2ΔP / ρ)
        /// </summary>
        public double CalculateDischargeFlowRate()
        {
            if (IsClosed || PressureDifferential <= 0 || FluidDensity <= 0)
                return 0.0;

            return DischargeCoefficient * EffectiveArea
                * Math.Sqrt(2.0 * PressureDifferential / FluidDensity);
        }

        /// <summary>
        /// Calculates the discharge velocity from the orifice equation in m/s.
        /// V = C_d × √(2ΔP / ρ)
        /// </summary>
        public double CalculateDischargeVelocity()
        {
            if (PressureDifferential <= 0 || FluidDensity <= 0)
                return 0.0;

            return DischargeCoefficient * Math.Sqrt(2.0 * PressureDifferential / FluidDensity);
        }

        /// <summary>
        /// Calculates the velocity pressure (dynamic pressure) at the outlet in Pascals.
        /// P_v = ½ρv²
        /// </summary>
        public double CalculateVelocityPressure()
        {
            double velocity = CalculateVelocity();
            return 0.5 * FluidDensity * Math.Pow(velocity, 2);
        }

        /// <summary>
        /// Calculates the pressure loss through the outlet (ΔP = K × ½ρv²) in Pascals.
        /// </summary>
        public double CalculatePressureLoss()
        {
            return LossCoefficient * CalculateVelocityPressure();
        }

        /// <summary>
        /// Calculates the head loss through the outlet (h_L = K × v²/2g) in meters.
        /// </summary>
        public double CalculateHeadLoss()
        {
            const double gravity = 9.80665;
            double velocity = CalculateVelocity();
            return LossCoefficient * Math.Pow(velocity, 2) / (2.0 * gravity);
        }

        //
        //  Energy Calculations
        //

        /// <summary>
        /// Calculates the kinetic energy flux at the outlet (P_ke = ½ρQv²) in Watts.
        /// </summary>
        public double CalculateKineticEnergyFlux()
        {
            double velocity = CalculateVelocity();
            return 0.5 * FluidDensity * FlowRate * Math.Pow(velocity, 2);
        }

        /// <summary>
        /// Calculates the thermal energy flux at the outlet in Watts.
        /// Q_thermal = ṁ × Cp × T
        /// </summary>
        /// <param name="specificHeat">Fluid specific heat in J/(kg·K).</param>
        /// <param name="referenceTemperature">Reference temperature in °C.</param>
        public double CalculateThermalEnergyFlux(double specificHeat = 4186.0, double referenceTemperature = 0.0)
        {
            return CalculateMassFlowRate() * specificHeat * (Temperature - referenceTemperature);
        }

        public override string ToString() =>
            $"Outlet [{Tag}, {OutletType}, {Status}, " +
            $"Ø={Diameter * 1000:F1}mm, Q={FlowRate * 1000:F2} L/s]";

        #endregion
        //  *****************************************************************************************
    }
}
