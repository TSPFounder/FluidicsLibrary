using System;
using System.Collections.Generic;
using System.Text;

namespace FluidicsLibrary
{
    /// <summary>
    /// Represents a nozzle that accelerates a fluid by converting pressure energy to kinetic energy.
    /// </summary>
    internal class Nozzle
    {
        //  *****************************************************************************************
        //  DECLARATIONS
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

        public enum NozzleTypeEnum
        {
            Convergent = 0,
            Divergent,
            ConvergentDivergent, // de Laval
            Orifice,
            Venturi,
            Other
        }

        public enum NozzleStatusEnum
        {
            Off = 0,
            Subsonic,
            Choked,
            Supersonic,
            Faulted
        }

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  CONSTRUCTORS
        //
        //  ************************************************************
        #region

        public Nozzle()
        {
            Status = NozzleStatusEnum.Off;
            DischargeCoefficient = 0.98;
            SpecificHeatRatio = 1.4;
            GasConstant = 287.0;
        }

        public Nozzle(NozzleTypeEnum type, double throatArea, double exitArea)
            : this()
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(throatArea);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(exitArea);

            NozzleType = type;
            ThroatArea = throatArea;
            ExitArea = exitArea;
        }

        public Nozzle(NozzleTypeEnum type, double throatArea, double exitArea, double inletArea)
            : this(type, throatArea, exitArea)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(inletArea);
            InletArea = inletArea;
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
        /// Type of nozzle.
        /// </summary>
        public NozzleTypeEnum NozzleType { get; set; }

        /// <summary>
        /// Current flow status.
        /// </summary>
        public NozzleStatusEnum Status { get; private set; }

        //
        //  Geometry
        //

        /// <summary>
        /// Inlet cross-sectional area in m².
        /// </summary>
        public double InletArea { get; set; }

        /// <summary>
        /// Throat (minimum) cross-sectional area in m².
        /// </summary>
        public double ThroatArea { get; set; }

        /// <summary>
        /// Exit cross-sectional area in m².
        /// </summary>
        public double ExitArea { get; set; }

        /// <summary>
        /// Half-angle of the convergent section in degrees.
        /// </summary>
        public double ConvergentHalfAngle { get; set; } = 15.0;

        /// <summary>
        /// Half-angle of the divergent section in degrees.
        /// </summary>
        public double DivergentHalfAngle { get; set; } = 7.0;

        //
        //  Operating Conditions
        //

        /// <summary>
        /// Stagnation (total) pressure upstream in Pascals.
        /// </summary>
        public double StagnationPressure { get; set; }

        /// <summary>
        /// Stagnation (total) temperature upstream in Kelvin.
        /// </summary>
        public double StagnationTemperature { get; set; } = 293.15;

        /// <summary>
        /// Back pressure (ambient/downstream) in Pascals.
        /// </summary>
        public double BackPressure { get; set; } = 101325.0;

        //
        //  Performance
        //

        /// <summary>
        /// Discharge coefficient (C_d), accounts for non-ideal flow (0.0 to 1.0).
        /// </summary>
        public double DischargeCoefficient { get; set; }

        /// <summary>
        /// Velocity coefficient (C_v), ratio of actual to ideal exit velocity (0.0 to 1.0).
        /// </summary>
        public double VelocityCoefficient { get; set; } = 0.97;

        //
        //  Gas Properties
        //

        /// <summary>
        /// Specific heat ratio (γ = Cp / Cv).
        /// </summary>
        public double SpecificHeatRatio { get; set; }

        /// <summary>
        /// Specific gas constant in J/(kg·K).
        /// </summary>
        public double GasConstant { get; set; }

        /// <summary>
        /// Specific heat at constant pressure in J/(kg·K).
        /// </summary>
        public double Cp => GasConstant * SpecificHeatRatio / (SpecificHeatRatio - 1.0);

        //
        //  Liquid Nozzle Properties
        //

        /// <summary>
        /// Fluid density for incompressible (liquid) flow in kg/m³.
        /// </summary>
        public double FluidDensity { get; set; } = 998.0;

        //
        //  Computed Properties
        //

        /// <summary>
        /// Area ratio (exit area / throat area). Determines the design Mach number for C-D nozzles.
        /// </summary>
        public double AreaRatio => ThroatArea > 0 ? ExitArea / ThroatArea : 1.0;

        /// <summary>
        /// Contraction ratio (inlet area / throat area).
        /// </summary>
        public double ContractionRatio => ThroatArea > 0 && InletArea > 0 ? InletArea / ThroatArea : 1.0;

        /// <summary>
        /// Nozzle pressure ratio (P_0 / P_back).
        /// </summary>
        public double NozzlePressureRatio =>
            BackPressure > 0 ? StagnationPressure / BackPressure : 0.0;

        /// <summary>
        /// Critical pressure ratio at which the throat becomes choked.
        /// (P*/P_0) = (2/(γ+1))^(γ/(γ−1))
        /// </summary>
        public double CriticalPressureRatio
        {
            get
            {
                if (SpecificHeatRatio <= 1.0)
                    return 0.0;

                double exponent = SpecificHeatRatio / (SpecificHeatRatio - 1.0);
                return Math.Pow(2.0 / (SpecificHeatRatio + 1.0), exponent);
            }
        }

        /// <summary>
        /// Whether the nozzle is choked (throat at Mach 1).
        /// </summary>
        public bool IsChoked
        {
            get
            {
                if (StagnationPressure <= 0 || CriticalPressureRatio <= 0)
                    return false;

                // Choked when P_back / P_0 ≤ critical ratio
                return BackPressure / StagnationPressure <= CriticalPressureRatio;
            }
        }

        /// <summary>
        /// Throat diameter in meters (assuming circular throat).
        /// </summary>
        public double ThroatDiameter => ThroatArea > 0
            ? 2.0 * Math.Sqrt(ThroatArea / Math.PI)
            : 0.0;

        /// <summary>
        /// Exit diameter in meters (assuming circular exit).
        /// </summary>
        public double ExitDiameter => ExitArea > 0
            ? 2.0 * Math.Sqrt(ExitArea / Math.PI)
            : 0.0;

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  METHODS
        //
        //  ************************************************************
        #region

        //
        //  Compressible Flow — Isentropic Relations
        //

        /// <summary>
        /// Calculates the exit Mach number for isentropic expansion to the given pressure.
        /// M = √((2/(γ−1)) × ((P_0/P)^((γ−1)/γ) − 1))
        /// </summary>
        /// <param name="staticPressure">Exit static pressure in Pascals.</param>
        public double CalculateMachNumber(double staticPressure)
        {
            if (staticPressure <= 0 || StagnationPressure <= 0 || SpecificHeatRatio <= 1.0)
                return 0.0;

            double pr = StagnationPressure / staticPressure;
            double exponent = (SpecificHeatRatio - 1.0) / SpecificHeatRatio;
            double term = (2.0 / (SpecificHeatRatio - 1.0)) * (Math.Pow(pr, exponent) - 1.0);

            return term >= 0 ? Math.Sqrt(term) : 0.0;
        }

        /// <summary>
        /// Calculates the exit Mach number based on back pressure.
        /// For a choked convergent nozzle, returns 1.0 at the exit.
        /// </summary>
        public double CalculateExitMachNumber()
        {
            if (NozzleType == NozzleTypeEnum.Convergent && IsChoked)
                return 1.0;

            return CalculateMachNumber(BackPressure);
        }

        /// <summary>
        /// Calculates the speed of sound at the given temperature (a = √(γRT)) in m/s.
        /// </summary>
        /// <param name="temperature">Static temperature in Kelvin.</param>
        public double CalculateSpeedOfSound(double temperature)
        {
            return temperature > 0 && SpecificHeatRatio > 0
                ? Math.Sqrt(SpecificHeatRatio * GasConstant * temperature)
                : 0.0;
        }

        /// <summary>
        /// Calculates the static temperature at a given Mach number in Kelvin.
        /// T = T_0 / (1 + ((γ−1)/2) × M²)
        /// </summary>
        /// <param name="machNumber">Mach number.</param>
        public double CalculateStaticTemperature(double machNumber)
        {
            return StagnationTemperature
                / (1.0 + (SpecificHeatRatio - 1.0) / 2.0 * Math.Pow(machNumber, 2));
        }

        /// <summary>
        /// Calculates the static pressure at a given Mach number in Pascals.
        /// P = P_0 / (1 + ((γ−1)/2) × M²)^(γ/(γ−1))
        /// </summary>
        /// <param name="machNumber">Mach number.</param>
        public double CalculateStaticPressure(double machNumber)
        {
            double exponent = SpecificHeatRatio / (SpecificHeatRatio - 1.0);
            double denominator = Math.Pow(
                1.0 + (SpecificHeatRatio - 1.0) / 2.0 * Math.Pow(machNumber, 2), exponent);

            return denominator > 0 ? StagnationPressure / denominator : 0.0;
        }

        /// <summary>
        /// Calculates the ideal exit velocity for isentropic expansion in m/s.
        /// V = √(2 × Cp × T_0 × (1 − (P_e/P_0)^((γ−1)/γ)))
        /// </summary>
        public double CalculateIdealExitVelocity()
        {
            if (StagnationPressure <= 0 || BackPressure <= 0 || SpecificHeatRatio <= 1.0)
                return 0.0;

            double exponent = (SpecificHeatRatio - 1.0) / SpecificHeatRatio;
            double pressureRatioTerm = Math.Pow(BackPressure / StagnationPressure, exponent);
            double term = 2.0 * Cp * StagnationTemperature * (1.0 - pressureRatioTerm);

            return term >= 0 ? Math.Sqrt(term) : 0.0;
        }

        /// <summary>
        /// Calculates the actual exit velocity (accounting for velocity coefficient) in m/s.
        /// </summary>
        public double CalculateActualExitVelocity()
        {
            return CalculateIdealExitVelocity() * VelocityCoefficient;
        }

        //
        //  Mass Flow Rate
        //

        /// <summary>
        /// Calculates the ideal mass flow rate for compressible flow through the nozzle in kg/s.
        /// For choked flow: ṁ = A* × P_0 × √(γ / (R × T_0)) × (2/(γ+1))^((γ+1)/(2(γ−1)))
        /// For unchoked flow: uses isentropic relations.
        /// </summary>
        public double CalculateIdealMassFlowRate()
        {
            if (StagnationPressure <= 0 || StagnationTemperature <= 0 || ThroatArea <= 0)
                return 0.0;

            if (IsChoked)
            {
                // Choked flow at the throat
                double gamma = SpecificHeatRatio;
                double exponent = (gamma + 1.0) / (2.0 * (gamma - 1.0));
                double factor = Math.Pow(2.0 / (gamma + 1.0), exponent);

                return ThroatArea * StagnationPressure
                    * Math.Sqrt(gamma / (GasConstant * StagnationTemperature))
                    * factor;
            }
            else
            {
                // Unchoked: flow area is throat, exit pressure equals back pressure
                double mach = CalculateMachNumber(BackPressure);
                double staticTemp = CalculateStaticTemperature(mach);
                double staticPressure = CalculateStaticPressure(mach);
                double density = GasConstant > 0 && staticTemp > 0
                    ? staticPressure / (GasConstant * staticTemp)
                    : 0.0;
                double velocity = mach * CalculateSpeedOfSound(staticTemp);

                return density * velocity * ThroatArea;
            }
        }

        /// <summary>
        /// Calculates the actual mass flow rate accounting for the discharge coefficient in kg/s.
        /// </summary>
        public double CalculateActualMassFlowRate()
        {
            return CalculateIdealMassFlowRate() * DischargeCoefficient;
        }

        //
        //  Incompressible (Liquid) Flow
        //

        /// <summary>
        /// Calculates the exit velocity for incompressible flow using Bernoulli's equation in m/s.
        /// V = C_v × √(2ΔP / ρ)
        /// </summary>
        public double CalculateIncompressibleExitVelocity()
        {
            double dp = StagnationPressure - BackPressure;
            if (dp <= 0 || FluidDensity <= 0)
                return 0.0;

            return VelocityCoefficient * Math.Sqrt(2.0 * dp / FluidDensity);
        }

        /// <summary>
        /// Calculates the volumetric flow rate for incompressible flow in m³/s.
        /// Q = C_d × A_throat × √(2ΔP / ρ)
        /// </summary>
        public double CalculateIncompressibleFlowRate()
        {
            double dp = StagnationPressure - BackPressure;
            if (dp <= 0 || FluidDensity <= 0 || ThroatArea <= 0)
                return 0.0;

            return DischargeCoefficient * ThroatArea * Math.Sqrt(2.0 * dp / FluidDensity);
        }

        //
        //  Thrust (Rocket / Propulsion)
        //

        /// <summary>
        /// Calculates the thrust produced by the nozzle in Newtons.
        /// F = ṁ × V_e + (P_e − P_amb) × A_e
        /// </summary>
        /// <param name="ambientPressure">Ambient pressure in Pascals.</param>
        public double CalculateThrust(double ambientPressure)
        {
            double massFlow = CalculateActualMassFlowRate();
            double exitVelocity = CalculateActualExitVelocity();
            double exitPressure = CalculateStaticPressure(CalculateExitMachNumber());

            // Momentum thrust + pressure thrust
            return massFlow * exitVelocity + (exitPressure - ambientPressure) * ExitArea;
        }

        /// <summary>
        /// Calculates the specific impulse (Isp = F / (ṁ × g)) in seconds.
        /// </summary>
        /// <param name="ambientPressure">Ambient pressure in Pascals.</param>
        public double CalculateSpecificImpulse(double ambientPressure)
        {
            const double gravity = 9.80665;
            double massFlow = CalculateActualMassFlowRate();

            if (massFlow <= 0)
                return 0.0;

            return CalculateThrust(ambientPressure) / (massFlow * gravity);
        }

        /// <summary>
        /// Calculates the thrust coefficient (C_F = F / (P_0 × A*)).
        /// </summary>
        /// <param name="ambientPressure">Ambient pressure in Pascals.</param>
        public double CalculateThrustCoefficient(double ambientPressure)
        {
            double denominator = StagnationPressure * ThroatArea;
            return denominator > 0
                ? CalculateThrust(ambientPressure) / denominator
                : 0.0;
        }

        //
        //  Status
        //

        /// <summary>
        /// Updates the nozzle flow status based on current conditions.
        /// </summary>
        public void UpdateStatus()
        {
            if (Status == NozzleStatusEnum.Faulted)
                return;

            if (StagnationPressure <= BackPressure)
            {
                Status = NozzleStatusEnum.Off;
            }
            else if (!IsChoked)
            {
                Status = NozzleStatusEnum.Subsonic;
            }
            else if (NozzleType == NozzleTypeEnum.ConvergentDivergent && AreaRatio > 1.0)
            {
                Status = NozzleStatusEnum.Supersonic;
            }
            else
            {
                Status = NozzleStatusEnum.Choked;
            }
        }

        /// <summary>
        /// Sets the nozzle to a faulted state.
        /// </summary>
        public void SetFault()
        {
            Status = NozzleStatusEnum.Faulted;
        }

        /// <summary>
        /// Clears a fault and resets to Off.
        /// </summary>
        public void ClearFault()
        {
            if (Status == NozzleStatusEnum.Faulted)
                Status = NozzleStatusEnum.Off;
        }

        public override string ToString() =>
            $"Nozzle [{NozzleType}, {Status}, A*={ThroatArea * 1e6:F2} mm², " +
            $"A_e/A*={AreaRatio:F2}, NPR={NozzlePressureRatio:F2}]";

        #endregion
        //  *****************************************************************************************
    }
}
