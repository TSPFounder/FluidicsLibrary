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
    public class Pipe 
    {
        //  *****************************************************************************************
        //  DECLARATIONS
        //
        //  ************************************************************
        #region
        //
        //  Identification
        private CrossSectionalGeometryTypeEnum _CrossSectionType;
        //
        //  Data
        //
        //  Dimensions
        private CAD_Dimension _Length;
        private CAD_Dimension _Thickness;
        //
        //  Round Tube
        private CAD_Dimension _OuterRadius;
        private CAD_Dimension _InnerRadius;
        //
        //  Rectangular Tube
        private CAD_Dimension _FilletRadius;
        private CAD_Dimension _Width;
        private CAD_Dimension _Height;
        //
        //  Elliptical Tube
        private CAD_Dimension _MajorRadius;
        private CAD_Dimension _MinorRadius;
        //
        //  Owned & Owning Objects
        //
        //  Thread
        private CAD_Feature _Thread;
        //
        //  Numeric backing values for calculations
        private double _lengthValue;
        private double _thicknessValue;
        private double _outerRadiusValue;
        private double _innerRadiusValue;
        private double _widthValue;
        private double _heightValue;
        private double _majorRadiusValue;
        private double _minorRadiusValue;
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
        public enum CrossSectionalGeometryTypeEnum
        {
            Circular = 0,
            Square,
            Rectangular,
            Elliptical,
            Other
        }

        public enum PipeMaterialEnum
        {
            CarbonSteel = 0,
            StainlessSteel,
            Copper,
            PVC,
            CPVC,
            HDPE,
            CastIron,
            DuctileIron,
            Aluminum,
            Brass,
            Other
        }

        public enum PipeScheduleEnum
        {
            Schedule5 = 0,
            Schedule10,
            Schedule20,
            Schedule40,
            Schedule80,
            Schedule160,
            DoubleExtraStrong,
            Other
        }
        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  PIPE CONSTRUCTOR
        //
        //  ************************************************************
        #region
        public Pipe()
        {
            FluidDensity = 998.0;
            FluidViscosity = 0.001;
            SurfaceRoughness = 0.000045; // smooth steel default in meters
        }

        /// <summary>
        /// Creates a circular pipe with the specified dimensions in meters.
        /// </summary>
        /// <param name="length">Pipe length in meters.</param>
        /// <param name="outerRadius">Outer radius in meters.</param>
        /// <param name="innerRadius">Inner radius in meters.</param>
        public Pipe(double length, double outerRadius, double innerRadius)
            : this()
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(outerRadius);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(innerRadius);

            if (innerRadius >= outerRadius)
                throw new ArgumentOutOfRangeException(nameof(innerRadius),
                    "Inner radius must be less than outer radius.");

            _CrossSectionType = CrossSectionalGeometryTypeEnum.Circular;
            _lengthValue = length;
            _outerRadiusValue = outerRadius;
            _innerRadiusValue = innerRadius;
            _thicknessValue = outerRadius - innerRadius;
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
        //  Cross-Section Type
        public CrossSectionalGeometryTypeEnum CrossSectionType
        {
            set => _CrossSectionType = value;
            get
            {
                return _CrossSectionType;
            }
        }

        /// <summary>
        /// Pipe material.
        /// </summary>
        public PipeMaterialEnum Material { get; set; }

        /// <summary>
        /// Pipe schedule / wall thickness class.
        /// </summary>
        public PipeScheduleEnum Schedule { get; set; }

        /// <summary>
        /// Nominal pipe size designation (e.g., NPS in inches or DN in mm).
        /// </summary>
        public string NominalSize { get; set; } = string.Empty;

        //
        //  Data
        //
        //  Dimensions
        //
        //  Length
        public CAD_Dimension Length
        {
            set => _Length = value;
            get
            {
                return _Length;
            }
        }
        //
        //  Thickness
        public CAD_Dimension Thickness
        {
            set => _Thickness = value;
            get
            {
                return _Thickness;
            }
        }
        //
        //  Round Tube
        //
        //  Outer Radius
        public CAD_Dimension OuterRadius
        {
            set => _OuterRadius = value;
            get
            {
                return _OuterRadius;
            }
        }
        //
        //  Inner Radius
        public CAD_Dimension InnerRadius
        {
            set => _InnerRadius = value;
            get
            {
                return _InnerRadius;
            }
        }
        //
        //  Rectangular Tube
        //
        //  Fillet Radius
        public CAD_Dimension FilletRadius
        {
            set => _FilletRadius = value;
            get
            {
                return _FilletRadius;
            }
        }
        //
        //  Width
        public CAD_Dimension Width
        {
            set => _Width = value;
            get
            {
                return _Width;
            }
        }
        //
        //  Height
        public CAD_Dimension Height
        {
            set => _Height = value;
            get
            {
                return _Height;
            }
        }
        //
        //  Elliptical Tube
        //
        // Major Radius
        public CAD_Dimension MajorRadius
        {
            set => _MajorRadius = value;
            get
            {
                return _MajorRadius;
            }
        }
        //
        //  Minor Radius
        public CAD_Dimension MinorRadius
        {
            set => _MinorRadius = value;
            get
            {
                return _MinorRadius;
            }
        }

        //
        //  Numeric Dimension Values (for calculations)
        //

        /// <summary>
        /// Pipe length as a numeric value in meters.
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
        /// Wall thickness as a numeric value in meters.
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

        /// <summary>
        /// Outer radius as a numeric value in meters.
        /// </summary>
        public double OuterRadiusValue
        {
            get => _outerRadiusValue;
            set
            {
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
                _outerRadiusValue = value;
            }
        }

        /// <summary>
        /// Inner radius as a numeric value in meters.
        /// </summary>
        public double InnerRadiusValue
        {
            get => _innerRadiusValue;
            set
            {
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
                _innerRadiusValue = value;
            }
        }

        /// <summary>
        /// Width as a numeric value in meters (rectangular/square cross-section).
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
        /// Height as a numeric value in meters (rectangular cross-section).
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
        /// Major radius as a numeric value in meters (elliptical cross-section).
        /// </summary>
        public double MajorRadiusValue
        {
            get => _majorRadiusValue;
            set
            {
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
                _majorRadiusValue = value;
            }
        }

        /// <summary>
        /// Minor radius as a numeric value in meters (elliptical cross-section).
        /// </summary>
        public double MinorRadiusValue
        {
            get => _minorRadiusValue;
            set
            {
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
                _minorRadiusValue = value;
            }
        }

        //
        //  Fluid Properties
        //

        /// <summary>
        /// Fluid density in kg/m³ (default water at ~20°C).
        /// </summary>
        public double FluidDensity { get; set; }

        /// <summary>
        /// Dynamic viscosity of the fluid in Pa·s (default water at ~20°C).
        /// </summary>
        public double FluidViscosity { get; set; }

        /// <summary>
        /// Volumetric flow rate through the pipe in m³/s.
        /// </summary>
        public double FlowRate { get; set; }

        /// <summary>
        /// Internal surface roughness (absolute roughness) in meters.
        /// </summary>
        public double SurfaceRoughness { get; set; }

        /// <summary>
        /// Elevation change from inlet to outlet in meters (positive = upward).
        /// </summary>
        public double ElevationChange { get; set; }

        //
        //  Computed Geometry Properties
        //

        /// <summary>
        /// Inner diameter in meters (circular pipe).
        /// </summary>
        public double InnerDiameter => _innerRadiusValue * 2.0;

        /// <summary>
        /// Outer diameter in meters (circular pipe).
        /// </summary>
        public double OuterDiameter => _outerRadiusValue * 2.0;

        /// <summary>
        /// Cross-sectional flow area in m² based on the cross-section type.
        /// </summary>
        public double FlowArea => _CrossSectionType switch
        {
            CrossSectionalGeometryTypeEnum.Circular =>
                Math.PI * Math.Pow(_innerRadiusValue, 2),

            CrossSectionalGeometryTypeEnum.Square =>
                Math.Pow(_widthValue - 2.0 * _thicknessValue, 2),

            CrossSectionalGeometryTypeEnum.Rectangular =>
                (_widthValue - 2.0 * _thicknessValue) * (_heightValue - 2.0 * _thicknessValue),

            CrossSectionalGeometryTypeEnum.Elliptical =>
                Math.PI * (_majorRadiusValue - _thicknessValue) * (_minorRadiusValue - _thicknessValue),

            _ => 0.0
        };

        /// <summary>
        /// Wetted perimeter in meters based on the cross-section type.
        /// </summary>
        public double WettedPerimeter => _CrossSectionType switch
        {
            CrossSectionalGeometryTypeEnum.Circular =>
                Math.PI * InnerDiameter,

            CrossSectionalGeometryTypeEnum.Square =>
                4.0 * (_widthValue - 2.0 * _thicknessValue),

            CrossSectionalGeometryTypeEnum.Rectangular =>
                2.0 * ((_widthValue - 2.0 * _thicknessValue) + (_heightValue - 2.0 * _thicknessValue)),

            CrossSectionalGeometryTypeEnum.Elliptical =>
                EllipsePerimeterApprox(_majorRadiusValue - _thicknessValue, _minorRadiusValue - _thicknessValue),

            _ => 0.0
        };

        /// <summary>
        /// Hydraulic diameter (D_h = 4A / P) in meters.
        /// For circular pipes this equals the inner diameter.
        /// </summary>
        public double HydraulicDiameter =>
            WettedPerimeter > 0 ? 4.0 * FlowArea / WettedPerimeter : 0.0;

        /// <summary>
        /// Internal volume of the pipe in m³.
        /// </summary>
        public double InternalVolume => FlowArea * _lengthValue;

        /// <summary>
        /// Relative roughness (ε / D_h).
        /// </summary>
        public double RelativeRoughness =>
            HydraulicDiameter > 0 ? SurfaceRoughness / HydraulicDiameter : 0.0;

        //
        //  Owned & Owning Objects
        //
        //  Thread
        public CAD_Feature Thread
        {
            set => _Thread = value;
            get
            {
                return _Thread;
            }
        }
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
        /// Calculates the average fluid velocity (v = Q / A) in m/s.
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
            return FluidDensity * FlowRate;
        }

        /// <summary>
        /// Calculates the Reynolds number (Re = ρvD_h / μ).
        /// </summary>
        public double CalculateReynoldsNumber()
        {
            double velocity = CalculateVelocity();
            return FluidViscosity > 0
                ? FluidDensity * velocity * HydraulicDiameter / FluidViscosity
                : 0.0;
        }

        /// <summary>
        /// Determines whether the flow is laminar (Re &lt; 2300).
        /// </summary>
        public bool IsLaminarFlow() => CalculateReynoldsNumber() < 2300;

        /// <summary>
        /// Determines whether the flow is turbulent (Re &gt; 4000).
        /// </summary>
        public bool IsTurbulentFlow() => CalculateReynoldsNumber() > 4000;

        /// <summary>
        /// Determines whether the flow is in the transitional regime (2300 ≤ Re ≤ 4000).
        /// </summary>
        public bool IsTransitionalFlow()
        {
            double re = CalculateReynoldsNumber();
            return re >= 2300 && re <= 4000;
        }

        //
        //  Friction & Pressure Drop
        //

        /// <summary>
        /// Calculates the Darcy friction factor.
        /// Uses the Hagen-Poiseuille formula for laminar flow (f = 64/Re)
        /// and the Colebrook-White equation (iterative) for turbulent flow.
        /// </summary>
        public double CalculateFrictionFactor()
        {
            double re = CalculateReynoldsNumber();

            if (re <= 0)
                return 0.0;

            // Laminar flow: f = 64 / Re
            if (re < 2300)
                return 64.0 / re;

            // Turbulent flow: Colebrook-White (iterative solution)
            double relRoughness = RelativeRoughness;
            double f = 0.02; // initial guess

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
        /// Calculates the major (friction) head loss using the Darcy-Weisbach equation
        /// (h_f = f × L/D_h × v²/2g) in meters.
        /// </summary>
        public double CalculateMajorHeadLoss()
        {
            const double gravity = 9.80665;
            double velocity = CalculateVelocity();
            double dh = HydraulicDiameter;

            if (dh <= 0)
                return 0.0;

            double f = CalculateFrictionFactor();
            return f * (_lengthValue / dh) * Math.Pow(velocity, 2) / (2.0 * gravity);
        }

        /// <summary>
        /// Calculates the friction pressure drop (ΔP = ρ × g × h_f) in Pascals.
        /// </summary>
        public double CalculateFrictionPressureDrop()
        {
            const double gravity = 9.80665;
            return FluidDensity * gravity * CalculateMajorHeadLoss();
        }

        /// <summary>
        /// Calculates the hydrostatic pressure change due to elevation (ΔP = ρgΔz) in Pascals.
        /// Positive value when pipe goes upward (pressure decreases in flow direction).
        /// </summary>
        public double CalculateElevationPressureChange()
        {
            const double gravity = 9.80665;
            return FluidDensity * gravity * ElevationChange;
        }

        /// <summary>
        /// Calculates the total pressure drop (friction + elevation) in Pascals.
        /// </summary>
        public double CalculateTotalPressureDrop()
        {
            return CalculateFrictionPressureDrop() + CalculateElevationPressureChange();
        }

        //
        //  Structural Calculations
        //

        /// <summary>
        /// Calculates the hoop stress in the pipe wall for a given internal pressure
        /// using the thin-wall approximation (σ_h = P × r / t) in Pascals.
        /// </summary>
        /// <param name="internalPressure">Internal gauge pressure in Pascals.</param>
        public double CalculateHoopStress(double internalPressure)
        {
            return _thicknessValue > 0
                ? internalPressure * _innerRadiusValue / _thicknessValue
                : 0.0;
        }

        /// <summary>
        /// Calculates the axial (longitudinal) stress in a capped pipe
        /// (σ_a = P × r / 2t) in Pascals.
        /// </summary>
        /// <param name="internalPressure">Internal gauge pressure in Pascals.</param>
        public double CalculateAxialStress(double internalPressure)
        {
            return _thicknessValue > 0
                ? internalPressure * _innerRadiusValue / (2.0 * _thicknessValue)
                : 0.0;
        }

        /// <summary>
        /// Calculates the maximum allowable internal pressure using thin-wall theory
        /// (P_max = σ_allow × t / r) in Pascals.
        /// </summary>
        /// <param name="allowableStress">Allowable material stress in Pascals.</param>
        public double CalculateMaxAllowablePressure(double allowableStress)
        {
            return _innerRadiusValue > 0
                ? allowableStress * _thicknessValue / _innerRadiusValue
                : 0.0;
        }

        //
        //  Physical Properties
        //

        /// <summary>
        /// Calculates the mass of fluid inside the pipe in kg.
        /// </summary>
        public double CalculateFluidMass()
        {
            return InternalVolume * FluidDensity;
        }

        /// <summary>
        /// Calculates the pipe wall cross-sectional area in m² (circular pipe).
        /// </summary>
        public double CalculateWallArea()
        {
            return Math.PI * (Math.Pow(_outerRadiusValue, 2) - Math.Pow(_innerRadiusValue, 2));
        }

        /// <summary>
        /// Calculates the pipe wall volume in m³ (circular pipe).
        /// </summary>
        public double CalculateWallVolume()
        {
            return CalculateWallArea() * _lengthValue;
        }

        /// <summary>
        /// Calculates the pipe wall mass in kg.
        /// </summary>
        /// <param name="pipeDensity">Pipe material density in kg/m³ (default 7850 for steel).</param>
        public double CalculatePipeMass(double pipeDensity = 7850.0)
        {
            return CalculateWallVolume() * pipeDensity;
        }

        /// <summary>
        /// Calculates the outer surface area of the pipe in m² (circular pipe).
        /// </summary>
        public double CalculateOuterSurfaceArea()
        {
            return 2.0 * Math.PI * _outerRadiusValue * _lengthValue;
        }

        /// <summary>
        /// Calculates the inner surface area of the pipe in m² (circular pipe).
        /// </summary>
        public double CalculateInnerSurfaceArea()
        {
            return 2.0 * Math.PI * _innerRadiusValue * _lengthValue;
        }

        //
        //  Thermal
        //

        /// <summary>
        /// Calculates the conductive heat loss through the pipe wall
        /// using the radial conduction formula for a cylinder in Watts.
        /// Q = 2πkL(T_in − T_out) / ln(r_o / r_i)
        /// </summary>
        /// <param name="thermalConductivity">Pipe material thermal conductivity in W/(m·K).</param>
        /// <param name="innerSurfaceTemp">Inner wall temperature in °C.</param>
        /// <param name="outerSurfaceTemp">Outer wall temperature in °C.</param>
        public double CalculateHeatLoss(double thermalConductivity, double innerSurfaceTemp, double outerSurfaceTemp)
        {
            if (_innerRadiusValue <= 0 || _outerRadiusValue <= _innerRadiusValue)
                return 0.0;

            double logRatio = Math.Log(_outerRadiusValue / _innerRadiusValue);
            return logRatio > 0
                ? 2.0 * Math.PI * thermalConductivity * _lengthValue
                    * (innerSurfaceTemp - outerSurfaceTemp) / logRatio
                : 0.0;
        }

        //
        //  Helpers
        //

        /// <summary>
        /// Ramanujan's approximation for the perimeter of an ellipse.
        /// P ≈ π(a + b)(1 + 3h / (10 + √(4 − 3h))) where h = ((a−b)/(a+b))²
        /// </summary>
        /// <param name="a">Semi-major axis in meters.</param>
        /// <param name="b">Semi-minor axis in meters.</param>
        private static double EllipsePerimeterApprox(double a, double b)
        {
            if (a <= 0 || b <= 0)
                return 0.0;

            double h = Math.Pow((a - b) / (a + b), 2);
            return Math.PI * (a + b) * (1.0 + 3.0 * h / (10.0 + Math.Sqrt(4.0 - 3.0 * h)));
        }

        public override string ToString() =>
            $"Pipe [{_CrossSectionType}, L={_lengthValue * 1000:F1}mm, " +
            $"D_h={HydraulicDiameter * 1000:F2}mm, " +
            $"Re={CalculateReynoldsNumber():F0}, v={CalculateVelocity():F2} m/s]";

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
