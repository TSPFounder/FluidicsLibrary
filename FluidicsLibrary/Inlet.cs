using System;
using System.Collections.Generic;
using System.Text;

namespace FluidicsLibrary
{
    /// <summary>
    /// Represents a fluid inlet port where fluid enters a system or component.
    /// </summary>
    internal class Inlet
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

        public enum InletTypeEnum
        {
            Pipe = 0,
            Flange,
            Threaded,
            QuickConnect,
            WeldNeck,
            Barbed,
            Submerged,
            BellMouth,
            SharpEdged,
            Reentrant,
            Other
        }

        public enum InletGeometryEnum
        {
            Circular = 0,
            Rectangular,
            Annular,
            Other
        }

        public enum InletStatusEnum
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

        public Inlet()
        {
            Status = InletStatusEnum.Open;
            _position = 1.0;
            FluidDensity = 998.0;
            EntranceLossCoefficient = 0.5; // sharp-edged default
        }

        public Inlet(InletTypeEnum type, double diameter)
            : this()
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(diameter);

            InletType = type;
            Geometry = InletGeometryEnum.Circular;
            Diameter = diameter;

            // Set typical entrance loss coefficient based on type
            EntranceLossCoefficient = type switch
            {
                InletTypeEnum.BellMouth => 0.04,
                InletTypeEnum.Reentrant => 0.8,
                InletTypeEnum.SharpEdged => 0.5,
                _ => 0.5
            };
        }

        public Inlet(InletTypeEnum type, double width, double height)
            : this()
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

            InletType = type;
            Geometry = InletGeometryEnum.Rectangular;
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
        /// Type of inlet connection.
        /// </summary>
        public InletTypeEnum InletType { get; set; }

        /// <summary>
        /// Cross-sectional geometry of the inlet.
        /// </summary>
        public InletGeometryEnum Geometry { get; set; }

        /// <summary>
        /// Current status of the inlet.
        /// </summary>
        public InletStatusEnum Status { get; private set; }

        /// <summary>
        /// Unique identifier or tag for the inlet.
        /// </summary>
        public string Tag { get; set; } = string.Empty;

        //
        //  Geometry
        //

        /// <summary>
        /// Diameter of a circular inlet in meters.
        /// </summary>
        public double Diameter { get; set; }

        /// <summary>
        /// Width of a rectangular inlet in meters.
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Height of a rectangular inlet in meters.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Wall thickness at the inlet in meters.
        /// </summary>
        public double WallThickness { get; set; }

        /// <summary>
        /// Fillet or bell-mouth radius at the inlet edge in meters.
        /// Larger values reduce entrance losses.
        /// </summary>
        public double EdgeRadius { get; set; }

        /// <summary>
        /// Elevation of the inlet centerline relative to a datum in meters.
        /// </summary>
        public double Elevation { get; set; }

        //
        //  Operating Conditions
        //

        /// <summary>
        /// Total (stagnation) pressure at the inlet in Pascals.
        /// </summary>
        public double TotalPressure { get; set; }

        /// <summary>
        /// Static pressure at the inlet in Pascals.
        /// </summary>
        public double StaticPressure { get; set; }

        /// <summary>
        /// Fluid temperature at the inlet in °C.
        /// </summary>
        public double Temperature { get; set; }

        /// <summary>
        /// Volumetric flow rate through the inlet in m³/s.
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
                    <= 0.0 => InletStatusEnum.Closed,
                    >= 1.0 => InletStatusEnum.Open,
                    _ => InletStatusEnum.PartiallyOpen
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
        /// Dynamic viscosity of the fluid in Pa·s.
        /// </summary>
        public double FluidViscosity { get; set; } = 0.001;

        /// <summary>
        /// Entrance loss coefficient (K-factor) for the inlet geometry.
        /// Typical: 0.5 sharp-edged, 0.04 bell-mouth, 0.8 reentrant.
        /// </summary>
        public double EntranceLossCoefficient { get; set; }

        //
        //  Filter / Screen
        //

        /// <summary>
        /// Whether the inlet has a filter or strainer installed.
        /// </summary>
        public bool HasFilter { get; set; }

        /// <summary>
        /// Filter pressure drop in Pascals (when clean).
        /// </summary>
        public double FilterCleanPressureDrop { get; set; }

        /// <summary>
        /// Current filter pressure drop in Pascals (increases as filter loads).
        /// </summary>
        public double FilterCurrentPressureDrop { get; set; }

        /// <summary>
        /// Maximum allowable filter pressure drop before replacement in Pascals.
        /// </summary>
        public double FilterMaxPressureDrop { get; set; }

        //
        //  Computed Properties
        //

        /// <summary>
        /// Cross-sectional area of the inlet in m².
        /// </summary>
        public double Area => Geometry switch
        {
            InletGeometryEnum.Circular =>
                Math.PI * Math.Pow(Diameter / 2.0, 2),

            InletGeometryEnum.Rectangular =>
                Width * Height,

            InletGeometryEnum.Annular =>
                Math.PI * (Math.Pow(Diameter / 2.0, 2) - Math.Pow(Diameter / 2.0 - WallThickness, 2)),

            _ => 0.0
        };

        /// <summary>
        /// Effective flow area accounting for opening position in m².
        /// </summary>
        public double EffectiveArea => Area * _position;

        /// <summary>
        /// Hydraulic diameter of the inlet in meters.
        /// </summary>
        public double HydraulicDiameter => Geometry switch
        {
            InletGeometryEnum.Circular => Diameter,

            InletGeometryEnum.Rectangular =>
                (Width + Height) > 0 ? 2.0 * Width * Height / (Width + Height) : 0.0,

            _ => Diameter
        };

        /// <summary>
        /// Dynamic pressure at the inlet (½ρv²) in Pascals.
        /// </summary>
        public double DynamicPressure => TotalPressure - StaticPressure;

        /// <summary>
        /// Whether the inlet is fully open.
        /// </summary>
        public bool IsOpen => Status == InletStatusEnum.Open;

        /// <summary>
        /// Whether the inlet is closed or blocked.
        /// </summary>
        public bool IsClosed => Status is InletStatusEnum.Closed or InletStatusEnum.Blocked;

        /// <summary>
        /// Whether there is positive flow through the inlet.
        /// </summary>
        public bool HasFlow => FlowRate > 0 && !IsClosed;

        /// <summary>
        /// Whether the filter needs replacement (exceeds max pressure drop).
        /// </summary>
        public bool IsFilterDirty =>
            HasFilter && FilterMaxPressureDrop > 0
            && FilterCurrentPressureDrop >= FilterMaxPressureDrop;

        /// <summary>
        /// Filter loading as a fraction (0.0 = clean, 1.0 = at max ΔP).
        /// </summary>
        public double FilterLoadFraction =>
            HasFilter && FilterMaxPressureDrop > 0
                ? Math.Clamp(FilterCurrentPressureDrop / FilterMaxPressureDrop, 0.0, 1.0)
                : 0.0;

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
        /// Opens the inlet fully.
        /// </summary>
        public void Open()
        {
            if (Status == InletStatusEnum.Faulted)
                return;

            Position = 1.0;
        }

        /// <summary>
        /// Closes the inlet.
        /// </summary>
        public void Close()
        {
            if (Status == InletStatusEnum.Faulted)
                return;

            Position = 0.0;
        }

        /// <summary>
        /// Sets the inlet to a specific opening position (0.0 to 1.0).
        /// </summary>
        /// <param name="position">Target position as a fraction.</param>
        public void SetPosition(double position)
        {
            if (Status == InletStatusEnum.Faulted)
                return;

            Position = position;
        }

        /// <summary>
        /// Marks the inlet as blocked (e.g., clogged or obstructed).
        /// </summary>
        public void SetBlocked()
        {
            Status = InletStatusEnum.Blocked;
        }

        /// <summary>
        /// Clears a blocked or faulted condition and resets to open.
        /// </summary>
        public void ClearFault()
        {
            if (Status is InletStatusEnum.Blocked or InletStatusEnum.Faulted)
                Position = 1.0;
        }

        /// <summary>
        /// Sets the inlet to a faulted state.
        /// </summary>
        public void SetFault()
        {
            Status = InletStatusEnum.Faulted;
        }

        //
        //  Flow Calculations
        //

        /// <summary>
        /// Calculates the flow velocity at the inlet (v = Q / A_eff) in m/s.
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
        /// Calculates the Reynolds number at the inlet (Re = ρvD_h / μ).
        /// </summary>
        public double CalculateReynoldsNumber()
        {
            double velocity = CalculateVelocity();
            return FluidViscosity > 0
                ? FluidDensity * velocity * HydraulicDiameter / FluidViscosity
                : 0.0;
        }

        /// <summary>
        /// Calculates the velocity pressure (dynamic pressure) at the inlet in Pascals.
        /// P_v = ½ρv²
        /// </summary>
        public double CalculateVelocityPressure()
        {
            double velocity = CalculateVelocity();
            return 0.5 * FluidDensity * Math.Pow(velocity, 2);
        }

        /// <summary>
        /// Calculates the entrance pressure loss (ΔP = K × ½ρv²) in Pascals.
        /// </summary>
        public double CalculateEntrancePressureLoss()
        {
            return EntranceLossCoefficient * CalculateVelocityPressure();
        }

        /// <summary>
        /// Calculates the entrance head loss (h_L = K × v²/2g) in meters.
        /// </summary>
        public double CalculateEntranceHeadLoss()
        {
            const double gravity = 9.80665;
            double velocity = CalculateVelocity();
            return EntranceLossCoefficient * Math.Pow(velocity, 2) / (2.0 * gravity);
        }

        /// <summary>
        /// Calculates the total pressure loss at the inlet (entrance + filter) in Pascals.
        /// </summary>
        public double CalculateTotalPressureLoss()
        {
            double entranceLoss = CalculateEntrancePressureLoss();
            double filterLoss = HasFilter ? FilterCurrentPressureDrop : 0.0;
            return entranceLoss + filterLoss;
        }

        /// <summary>
        /// Estimates the entrance loss coefficient based on the edge radius ratio (r/D).
        /// Uses the empirical correlation for rounded inlets.
        /// K ≈ 0.5 × (1 − (r/D) × 4.6)  clamped to [0.04, 0.5].
        /// </summary>
        public double EstimateEntranceLossFromEdgeRadius()
        {
            if (Diameter <= 0)
                return 0.5;

            double rOverD = EdgeRadius / Diameter;
            double k = 0.5 * (1.0 - rOverD * 4.6);
            return Math.Clamp(k, 0.04, 0.5);
        }

        //
        //  Suction Calculations
        //

        /// <summary>
        /// Calculates the net positive suction head available (NPSHa) at the inlet in meters.
        /// NPSHa = (P_static / (ρg)) + (v² / 2g) − (P_vapor / (ρg)) + z
        /// </summary>
        /// <param name="vaporPressure">Fluid vapor pressure at inlet temperature in Pascals.</param>
        public double CalculateNPSHAvailable(double vaporPressure)
        {
            const double gravity = 9.80665;
            double velocity = CalculateVelocity();

            double pressureHead = StaticPressure / (FluidDensity * gravity);
            double velocityHead = Math.Pow(velocity, 2) / (2.0 * gravity);
            double vaporHead = vaporPressure / (FluidDensity * gravity);

            return pressureHead + velocityHead - vaporHead + Elevation;
        }

        /// <summary>
        /// Determines whether cavitation may occur at the inlet.
        /// </summary>
        /// <param name="vaporPressure">Fluid vapor pressure at inlet temperature in Pascals.</param>
        /// <param name="npshRequired">Required NPSH in meters.</param>
        public bool IsCavitationRisk(double vaporPressure, double npshRequired)
        {
            return CalculateNPSHAvailable(vaporPressure) < npshRequired;
        }

        //
        //  Filter Operations
        //

        /// <summary>
        /// Installs or replaces the filter, resetting the pressure drop to the clean value.
        /// </summary>
        /// <param name="cleanPressureDrop">Clean filter pressure drop in Pascals.</param>
        /// <param name="maxPressureDrop">Maximum allowable filter pressure drop in Pascals.</param>
        public void InstallFilter(double cleanPressureDrop, double maxPressureDrop)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(cleanPressureDrop);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxPressureDrop);

            HasFilter = true;
            FilterCleanPressureDrop = cleanPressureDrop;
            FilterCurrentPressureDrop = cleanPressureDrop;
            FilterMaxPressureDrop = maxPressureDrop;
        }

        /// <summary>
        /// Removes the filter from the inlet.
        /// </summary>
        public void RemoveFilter()
        {
            HasFilter = false;
            FilterCleanPressureDrop = 0.0;
            FilterCurrentPressureDrop = 0.0;
            FilterMaxPressureDrop = 0.0;
        }

        /// <summary>
        /// Replaces the filter, resetting the current pressure drop to the clean value.
        /// </summary>
        public void ReplaceFilter()
        {
            if (HasFilter)
                FilterCurrentPressureDrop = FilterCleanPressureDrop;
        }

        //
        //  Energy Calculations
        //

        /// <summary>
        /// Calculates the kinetic energy flux entering through the inlet (P_ke = ½ρQv²) in Watts.
        /// </summary>
        public double CalculateKineticEnergyFlux()
        {
            double velocity = CalculateVelocity();
            return 0.5 * FluidDensity * FlowRate * Math.Pow(velocity, 2);
        }

        /// <summary>
        /// Calculates the thermal energy flux entering through the inlet in Watts.
        /// Q_thermal = ṁ × Cp × (T − T_ref)
        /// </summary>
        /// <param name="specificHeat">Fluid specific heat in J/(kg·K).</param>
        /// <param name="referenceTemperature">Reference temperature in °C.</param>
        public double CalculateThermalEnergyFlux(double specificHeat = 4186.0, double referenceTemperature = 0.0)
        {
            return CalculateMassFlowRate() * specificHeat * (Temperature - referenceTemperature);
        }

        public override string ToString() =>
            $"Inlet [{Tag}, {InletType}, {Status}, " +
            $"Ø={Diameter * 1000:F1}mm, Q={FlowRate * 1000:F2} L/s]";

        #endregion
        //  *****************************************************************************************
    }
}
