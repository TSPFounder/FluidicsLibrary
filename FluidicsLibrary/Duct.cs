using System;
using System.Collections.Generic;
using System.Text;

namespace FluidicsLibrary
{
    /// <summary>
    /// Represents an air/gas duct used in HVAC and ventilation systems.
    /// </summary>
    internal class Duct
    {
        //  *****************************************************************************************
        //  DECLARATIONS
        //
        //  ************************************************************
        #region

        private double _lengthValue;
        private double _widthValue;
        private double _heightValue;
        private double _diameterValue;
        private double _thicknessValue;

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  ENUMERATIONS
        //
        //  ************************************************************
        #region

        public enum DuctTypeEnum
        {
            Rectangular = 0,
            Circular,
            Oval,
            FlatOval,
            Other
        }

        public enum DuctMaterialEnum
        {
            GalvanizedSteel = 0,
            StainlessSteel,
            Aluminum,
            Fiberglass,
            FiberboardInsulated,
            FlexibleDuct,
            PVC,
            FabricDuct,
            Other
        }

        public enum DuctSealClassEnum
        {
            SealClassA = 0,
            SealClassB,
            SealClassC,
            Unsealed
        }

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  CONSTRUCTORS
        //
        //  ************************************************************
        #region

        public Duct()
        {
            AirDensity = 1.2;
            AirViscosity = 1.81e-5;
            SurfaceRoughness = 0.0003; // galvanized steel default in meters
        }

        /// <summary>
        /// Creates a circular duct with the specified dimensions in meters.
        /// </summary>
        /// <param name="length">Duct length in meters.</param>
        /// <param name="diameter">Inner diameter in meters.</param>
        public Duct(double length, double diameter)
            : this()
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(diameter);

            DuctType = DuctTypeEnum.Circular;
            _lengthValue = length;
            _diameterValue = diameter;
        }

        /// <summary>
        /// Creates a rectangular duct with the specified dimensions in meters.
        /// </summary>
        /// <param name="length">Duct length in meters.</param>
        /// <param name="width">Inner width in meters.</param>
        /// <param name="height">Inner height in meters.</param>
        public Duct(double length, double width, double height)
            : this()
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

            DuctType = DuctTypeEnum.Rectangular;
            _lengthValue = length;
            _widthValue = width;
            _heightValue = height;
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
        /// Type of duct cross-section.
        /// </summary>
        public DuctTypeEnum DuctType { get; set; }

        /// <summary>
        /// Duct material.
        /// </summary>
        public DuctMaterialEnum Material { get; set; }

        /// <summary>
        /// Duct seal class for leakage estimation.
        /// </summary>
        public DuctSealClassEnum SealClass { get; set; }

        //
        //  Dimensions
        //

        /// <summary>
        /// Duct length in meters.
        /// </summary>
        public double LengthValue
        {
            get => _lengthValue;
            set
            {
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
                _lengthValue = value;
            }
        }

        /// <summary>
        /// Inner width in meters (rectangular duct).
        /// </summary>
        public double WidthValue
        {
            get => _widthValue;
            set
            {
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
                _widthValue = value;
            }
        }

        /// <summary>
        /// Inner height in meters (rectangular duct).
        /// </summary>
        public double HeightValue
        {
            get => _heightValue;
            set
            {
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
                _heightValue = value;
            }
        }

        /// <summary>
        /// Inner diameter in meters (circular duct).
        /// </summary>
        public double DiameterValue
        {
            get => _diameterValue;
            set
            {
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
                _diameterValue = value;
            }
        }

        /// <summary>
        /// Wall thickness in meters.
        /// </summary>
        public double ThicknessValue
        {
            get => _thicknessValue;
            set
            {
                ArgumentOutOfRangeException.ThrowIfNegative(value);
                _thicknessValue = value;
            }
        }

        //
        //  Fluid Properties
        //

        /// <summary>
        /// Air density in kg/m³ (default 1.2 at standard conditions).
        /// </summary>
        public double AirDensity { get; set; }

        /// <summary>
        /// Dynamic viscosity of air in Pa·s.
        /// </summary>
        public double AirViscosity { get; set; }

        /// <summary>
        /// Internal surface roughness (absolute roughness) in meters.
        /// </summary>
        public double SurfaceRoughness { get; set; }

        /// <summary>
        /// Volumetric air flow rate in m³/s.
        /// </summary>
        public double FlowRate { get; set; }

        /// <summary>
        /// Air temperature in °C.
        /// </summary>
        public double AirTemperature { get; set; } = 20.0;

        /// <summary>
        /// Elevation change from inlet to outlet in meters (positive = upward).
        /// </summary>
        public double ElevationChange { get; set; }

        //
        //  Insulation
        //

        /// <summary>
        /// Insulation thickness in meters.
        /// </summary>
        public double InsulationThickness { get; set; }

        /// <summary>
        /// Insulation thermal conductivity in W/(m·K).
        /// </summary>
        public double InsulationConductivity { get; set; } = 0.04;

        //
        //  Computed Geometry Properties
        //

        /// <summary>
        /// Cross-sectional flow area in m².
        /// </summary>
        public double FlowArea => DuctType switch
        {
            DuctTypeEnum.Circular =>
                Math.PI * Math.Pow(_diameterValue / 2.0, 2),

            DuctTypeEnum.Rectangular =>
                _widthValue * _heightValue,

            _ => 0.0
        };

        /// <summary>
        /// Wetted perimeter in meters.
        /// </summary>
        public double WettedPerimeter => DuctType switch
        {
            DuctTypeEnum.Circular =>
                Math.PI * _diameterValue,

            DuctTypeEnum.Rectangular =>
                2.0 * (_widthValue + _heightValue),

            _ => 0.0
        };

        /// <summary>
        /// Hydraulic diameter (D_h = 4A / P) in meters.
        /// </summary>
        public double HydraulicDiameter =>
            WettedPerimeter > 0 ? 4.0 * FlowArea / WettedPerimeter : 0.0;

        /// <summary>
        /// Equivalent circular diameter for a rectangular duct in meters.
        /// D_eq = 1.3 × (a×b)^0.625 / (a+b)^0.25
        /// </summary>
        public double EquivalentDiameter
        {
            get
            {
                if (DuctType != DuctTypeEnum.Rectangular || _widthValue <= 0 || _heightValue <= 0)
                    return _diameterValue;

                return 1.3 * Math.Pow(_widthValue * _heightValue, 0.625)
                    / Math.Pow(_widthValue + _heightValue, 0.25);
            }
        }

        /// <summary>
        /// Internal volume of the duct in m³.
        /// </summary>
        public double InternalVolume => FlowArea * _lengthValue;

        /// <summary>
        /// Aspect ratio for rectangular ducts (width / height). 
        /// Values above 4:1 are generally discouraged.
        /// </summary>
        public double AspectRatio =>
            _heightValue > 0 && DuctType == DuctTypeEnum.Rectangular
                ? _widthValue / _heightValue
                : 1.0;

        /// <summary>
        /// Relative roughness (ε / D_h).
        /// </summary>
        public double RelativeRoughness =>
            HydraulicDiameter > 0 ? SurfaceRoughness / HydraulicDiameter : 0.0;

        /// <summary>
        /// Inner surface area in m².
        /// </summary>
        public double InnerSurfaceArea => WettedPerimeter * _lengthValue;

        /// <summary>
        /// Outer surface area in m² (including wall thickness).
        /// </summary>
        public double OuterSurfaceArea => DuctType switch
        {
            DuctTypeEnum.Circular =>
                Math.PI * (_diameterValue + 2.0 * _thicknessValue) * _lengthValue,

            DuctTypeEnum.Rectangular =>
                2.0 * ((_widthValue + 2.0 * _thicknessValue) + (_heightValue + 2.0 * _thicknessValue))
                    * _lengthValue,

            _ => 0.0
        };

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  METHODS
        //
        //  ************************************************************
        #region

        //
        //  Flow Calculations
        //

        /// <summary>
        /// Calculates the average air velocity (v = Q / A) in m/s.
        /// </summary>
        public double CalculateVelocity()
        {
            return FlowArea > 0 ? FlowRate / FlowArea : 0.0;
        }

        /// <summary>
        /// Calculates the mass flow rate (ṁ = ρQ) in kg/s.
        /// </summary>
        public double CalculateMassFlowRate()
        {
            return AirDensity * FlowRate;
        }

        /// <summary>
        /// Calculates the Reynolds number (Re = ρvD_h / μ).
        /// </summary>
        public double CalculateReynoldsNumber()
        {
            double velocity = CalculateVelocity();
            return AirViscosity > 0
                ? AirDensity * velocity * HydraulicDiameter / AirViscosity
                : 0.0;
        }

        /// <summary>
        /// Calculates the velocity pressure (P_v = ½ρv²) in Pascals.
        /// </summary>
        public double CalculateVelocityPressure()
        {
            double velocity = CalculateVelocity();
            return 0.5 * AirDensity * Math.Pow(velocity, 2);
        }

        //
        //  Friction & Pressure Drop
        //

        /// <summary>
        /// Calculates the Darcy friction factor.
        /// Uses f = 64/Re for laminar flow and iterative Colebrook-White for turbulent flow.
        /// </summary>
        public double CalculateFrictionFactor()
        {
            double re = CalculateReynoldsNumber();

            if (re <= 0)
                return 0.0;

            // Laminar flow
            if (re < 2300)
                return 64.0 / re;

            // Turbulent flow: Colebrook-White
            double relRoughness = RelativeRoughness;
            double f = 0.02;

            for (int i = 0; i < 50; i++)
            {
                double rhs = -2.0 * Math.Log10(relRoughness / 3.7 + 2.51 / (re * Math.Sqrt(f)));
                double fNew = 1.0 / Math.Pow(rhs, 2);

                if (Math.Abs(fNew - f) < 1e-10)
                    break;

                f = fNew;
            }

            return f;
        }

        /// <summary>
        /// Calculates the friction pressure drop using the Darcy-Weisbach equation in Pascals.
        /// ΔP = f × (L / D_h) × ½ρv²
        /// </summary>
        public double CalculateFrictionPressureDrop()
        {
            double dh = HydraulicDiameter;
            if (dh <= 0)
                return 0.0;

            double f = CalculateFrictionFactor();
            return f * (_lengthValue / dh) * CalculateVelocityPressure();
        }

        /// <summary>
        /// Calculates the friction pressure drop per unit length in Pa/m.
        /// </summary>
        public double CalculateFrictionRate()
        {
            return _lengthValue > 0 ? CalculateFrictionPressureDrop() / _lengthValue : 0.0;
        }

        /// <summary>
        /// Calculates the pressure drop for a fitting with a given loss coefficient (ΔP = K × P_v) in Pascals.
        /// </summary>
        /// <param name="lossCoefficient">Loss coefficient (K) for the fitting.</param>
        public double CalculateFittingLoss(double lossCoefficient)
        {
            return lossCoefficient * CalculateVelocityPressure();
        }

        /// <summary>
        /// Calculates the stack effect pressure due to elevation change in Pascals.
        /// </summary>
        public double CalculateStackEffect()
        {
            const double gravity = 9.80665;
            return AirDensity * gravity * ElevationChange;
        }

        //
        //  Noise
        //

        /// <summary>
        /// Estimates the duct-generated noise level based on velocity in dB.
        /// Approximate: L_w ≈ 10 + 50 × log10(v) + 10 × log10(A)
        /// </summary>
        public double EstimateNoiseLevel()
        {
            double velocity = CalculateVelocity();
            double area = FlowArea;

            if (velocity <= 0 || area <= 0)
                return 0.0;

            return 10.0 + 50.0 * Math.Log10(velocity) + 10.0 * Math.Log10(area);
        }

        //
        //  Thermal
        //

        /// <summary>
        /// Calculates the heat gain or loss through the duct wall and insulation in Watts.
        /// Q = U × A × (T_outside − T_air)
        /// </summary>
        /// <param name="outsideTemperature">Temperature outside the duct in °C.</param>
        /// <param name="ductWallConductivity">Duct wall thermal conductivity in W/(m·K) (default steel).</param>
        public double CalculateHeatTransfer(double outsideTemperature, double ductWallConductivity = 50.0)
        {
            double uValue = CalculateOverallUValue(ductWallConductivity);
            return uValue * InnerSurfaceArea * (outsideTemperature - AirTemperature);
        }

        /// <summary>
        /// Calculates the overall U-value for the duct wall + insulation in W/(m²·K).
        /// Simplified: U = 1 / (t_wall/k_wall + t_insul/k_insul + R_surface)
        /// </summary>
        /// <param name="ductWallConductivity">Duct wall thermal conductivity in W/(m·K).</param>
        private double CalculateOverallUValue(double ductWallConductivity)
        {
            const double surfaceResistance = 0.12; // typical combined interior + exterior in (m²·K)/W

            double wallResistance = ductWallConductivity > 0 ? _thicknessValue / ductWallConductivity : 0.0;
            double insulResistance = InsulationConductivity > 0 ? InsulationThickness / InsulationConductivity : 0.0;
            double totalResistance = wallResistance + insulResistance + surfaceResistance;

            return totalResistance > 0 ? 1.0 / totalResistance : 0.0;
        }

        //
        //  Leakage
        //

        /// <summary>
        /// Estimates the leakage flow rate based on seal class and static pressure in m³/s.
        /// Q_leak = C_L × P^0.65 × A_surface  (SMACNA method, simplified)
        /// </summary>
        /// <param name="staticPressure">Static pressure in the duct in Pascals.</param>
        public double EstimateLeakageRate(double staticPressure)
        {
            // Leakage class factor (L/s per m² at 1 Pa), SMACNA-based
            double leakageClassFactor = SealClass switch
            {
                DuctSealClassEnum.SealClassA => 0.027,
                DuctSealClassEnum.SealClassB => 0.009,
                DuctSealClassEnum.SealClassC => 0.003,
                DuctSealClassEnum.Unsealed => 0.068,
                _ => 0.027
            };

            double area = InnerSurfaceArea;
            return leakageClassFactor * Math.Pow(Math.Abs(staticPressure), 0.65) * area / 1000.0;
        }

        public override string ToString() =>
            $"Duct [{DuctType}, {Material}, L={_lengthValue:F1}m, " +
            $"D_h={HydraulicDiameter * 1000:F1}mm, v={CalculateVelocity():F2} m/s]";

        #endregion
        //  *****************************************************************************************
    }
}
