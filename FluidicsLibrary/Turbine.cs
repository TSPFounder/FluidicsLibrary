using System;
using System.Collections.Generic;
using System.Text;

namespace FluidicsLibrary
{
    /// <summary>
    /// Represents a turbine that extracts energy from a fluid flow.
    /// </summary>
    internal class Turbine
    {
        //  *****************************************************************************************
        //  DECLARATIONS
        //
        //  ************************************************************
        #region

        private double _currentSpeed;

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  ENUMERATIONS
        //
        //  ************************************************************
        #region

        public enum TurbineTypeEnum
        {
            GasAxial = 0,
            GasRadial,
            SteamImpulse,
            SteamReaction,
            Hydraulic,
            Wind,
            Other
        }

        public enum TurbineStatusEnum
        {
            Off = 0,
            Running,
            Overspeed,
            Faulted
        }

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  CONSTRUCTORS
        //
        //  ************************************************************
        #region

        public Turbine()
        {
            Status = TurbineStatusEnum.Off;
            IsentropicEfficiency = 0.88;
            MechanicalEfficiency = 0.97;
            GeneratorEfficiency = 0.95;
            SpecificHeatRatio = 1.4;
            GasConstant = 287.0;
        }

        public Turbine(TurbineTypeEnum type, double ratedSpeed, double ratedPower)
            : this()
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ratedSpeed);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ratedPower);

            TurbineType = type;
            RatedSpeed = ratedSpeed;
            RatedPower = ratedPower;
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
        /// Type of turbine.
        /// </summary>
        public TurbineTypeEnum TurbineType { get; set; }

        /// <summary>
        /// Current operating status.
        /// </summary>
        public TurbineStatusEnum Status { get; private set; }

        //
        //  Design (Rated) Parameters
        //

        /// <summary>
        /// Rated rotational speed in RPM.
        /// </summary>
        public double RatedSpeed { get; set; }

        /// <summary>
        /// Rated shaft power output in Watts.
        /// </summary>
        public double RatedPower { get; set; }

        /// <summary>
        /// Rated mass flow rate in kg/s.
        /// </summary>
        public double RatedMassFlowRate { get; set; }

        /// <summary>
        /// Maximum allowable speed in RPM (overspeed trip threshold).
        /// </summary>
        public double MaxSpeed { get; set; }

        /// <summary>
        /// Number of stages.
        /// </summary>
        public int StageCount { get; set; } = 1;

        //
        //  Operating Conditions
        //

        /// <summary>
        /// Current rotational speed in RPM.
        /// </summary>
        public double CurrentSpeed
        {
            get => _currentSpeed;
            set => _currentSpeed = Math.Max(value, 0.0);
        }

        /// <summary>
        /// Inlet pressure in Pascals.
        /// </summary>
        public double InletPressure { get; set; }

        /// <summary>
        /// Outlet (exhaust) pressure in Pascals.
        /// </summary>
        public double OutletPressure { get; set; }

        /// <summary>
        /// Inlet temperature in Kelvin.
        /// </summary>
        public double InletTemperature { get; set; } = 1273.15;

        /// <summary>
        /// Mass flow rate through the turbine in kg/s.
        /// </summary>
        public double MassFlowRate { get; set; }

        //
        //  Efficiencies
        //

        /// <summary>
        /// Isentropic (adiabatic) efficiency as a fraction (0.0 to 1.0).
        /// </summary>
        public double IsentropicEfficiency { get; set; }

        /// <summary>
        /// Mechanical efficiency as a fraction (0.0 to 1.0).
        /// </summary>
        public double MechanicalEfficiency { get; set; }

        /// <summary>
        /// Generator/electrical efficiency as a fraction (0.0 to 1.0).
        /// </summary>
        public double GeneratorEfficiency { get; set; }

        //
        //  Gas/Steam Properties
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
        //  Hydraulic Turbine Properties
        //

        /// <summary>
        /// Net head across a hydraulic turbine in meters.
        /// </summary>
        public double NetHead { get; set; }

        /// <summary>
        /// Fluid density in kg/m³ (for hydraulic turbines).
        /// </summary>
        public double FluidDensity { get; set; } = 998.0;

        //
        //  Computed Properties
        //

        /// <summary>
        /// Expansion ratio (P_in / P_out). Inverse of compressor pressure ratio.
        /// </summary>
        public double ExpansionRatio =>
            OutletPressure > 0 ? InletPressure / OutletPressure : 0.0;

        /// <summary>
        /// Speed ratio relative to rated speed.
        /// </summary>
        public double SpeedRatio =>
            RatedSpeed > 0 ? CurrentSpeed / RatedSpeed : 0.0;

        /// <summary>
        /// Whether the turbine is currently running.
        /// </summary>
        public bool IsRunning => Status is TurbineStatusEnum.Running or TurbineStatusEnum.Overspeed;

        /// <summary>
        /// Whether the turbine has exceeded its maximum speed.
        /// </summary>
        public bool IsOverspeed => MaxSpeed > 0 && CurrentSpeed > MaxSpeed;

        /// <summary>
        /// Current power as a fraction of rated power.
        /// </summary>
        public double LoadFraction => RatedPower > 0 ? CalculateShaftPower() / RatedPower : 0.0;

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  METHODS
        //
        //  ************************************************************
        #region

        //
        //  Lifecycle
        //

        /// <summary>
        /// Starts the turbine at rated speed.
        /// </summary>
        public void Start()
        {
            Start(RatedSpeed);
        }

        /// <summary>
        /// Starts the turbine at the specified speed.
        /// </summary>
        /// <param name="speed">Target speed in RPM.</param>
        public void Start(double speed)
        {
            if (Status == TurbineStatusEnum.Faulted)
                return;

            CurrentSpeed = speed;
            UpdateStatus();
        }

        /// <summary>
        /// Stops the turbine.
        /// </summary>
        public void Stop()
        {
            if (Status == TurbineStatusEnum.Faulted)
                return;

            CurrentSpeed = 0.0;
            Status = TurbineStatusEnum.Off;
        }

        /// <summary>
        /// Triggers an emergency trip (overspeed or fault shutdown).
        /// </summary>
        public void Trip()
        {
            _currentSpeed = 0.0;
            Status = TurbineStatusEnum.Faulted;
        }

        /// <summary>
        /// Clears a fault and resets to Off.
        /// </summary>
        public void ClearFault()
        {
            if (Status == TurbineStatusEnum.Faulted)
                Status = TurbineStatusEnum.Off;
        }

        //
        //  Gas/Steam Turbine Calculations
        //

        /// <summary>
        /// Calculates the isentropic outlet temperature in Kelvin.
        /// T2s = T1 / (P1/P2)^((γ−1)/γ)
        /// </summary>
        public double CalculateIsentropicOutletTemperature()
        {
            double er = ExpansionRatio;
            if (er <= 0 || SpecificHeatRatio <= 1.0)
                return InletTemperature;

            double exponent = (SpecificHeatRatio - 1.0) / SpecificHeatRatio;
            return InletTemperature / Math.Pow(er, exponent);
        }

        /// <summary>
        /// Calculates the actual outlet temperature accounting for efficiency in Kelvin.
        /// T2 = T1 − η_is × (T1 − T2s)
        /// </summary>
        public double CalculateActualOutletTemperature()
        {
            double t2s = CalculateIsentropicOutletTemperature();
            return InletTemperature - IsentropicEfficiency * (InletTemperature - t2s);
        }

        /// <summary>
        /// Calculates the isentropic (ideal) specific work extracted in J/kg.
        /// w_is = Cp × T1 × (1 − 1/(P1/P2)^((γ−1)/γ))
        /// </summary>
        public double CalculateIsentropicWork()
        {
            double er = ExpansionRatio;
            if (er <= 0 || SpecificHeatRatio <= 1.0)
                return 0.0;

            double exponent = (SpecificHeatRatio - 1.0) / SpecificHeatRatio;
            return Cp * InletTemperature * (1.0 - 1.0 / Math.Pow(er, exponent));
        }

        /// <summary>
        /// Calculates the actual specific work extracted in J/kg.
        /// w_actual = w_is × η_is
        /// </summary>
        public double CalculateActualWork()
        {
            return CalculateIsentropicWork() * IsentropicEfficiency;
        }

        /// <summary>
        /// Calculates the shaft power output (P = ṁ × w_actual × η_mech) in Watts.
        /// </summary>
        public double CalculateShaftPower()
        {
            return MassFlowRate * CalculateActualWork() * MechanicalEfficiency;
        }

        /// <summary>
        /// Calculates the electrical power output accounting for generator efficiency in Watts.
        /// P_elec = P_shaft × η_gen
        /// </summary>
        public double CalculateElectricalPower()
        {
            return CalculateShaftPower() * GeneratorEfficiency;
        }

        /// <summary>
        /// Calculates the overall thermal-to-electrical efficiency.
        /// η_overall = η_is × η_mech × η_gen
        /// </summary>
        public double CalculateOverallEfficiency()
        {
            return IsentropicEfficiency * MechanicalEfficiency * GeneratorEfficiency;
        }

        /// <summary>
        /// Calculates the exhaust heat rate in Watts.
        /// Q_exhaust = ṁ × Cp × (T2_actual − T_ref)
        /// </summary>
        /// <param name="referenceTemperature">Reference temperature in Kelvin (default ambient).</param>
        public double CalculateExhaustHeat(double referenceTemperature = 293.15)
        {
            double t2 = CalculateActualOutletTemperature();
            return MassFlowRate * Cp * (t2 - referenceTemperature);
        }

        /// <summary>
        /// Calculates the heat rate (energy input per unit of electrical output) in J/Wh.
        /// Lower is better.
        /// </summary>
        public double CalculateHeatRate()
        {
            double elecPower = CalculateElectricalPower();
            if (elecPower <= 0)
                return 0.0;

            double totalHeatInput = MassFlowRate * Cp * InletTemperature;
            return totalHeatInput / elecPower * 3600.0;
        }

        //
        //  Hydraulic Turbine Calculations
        //

        /// <summary>
        /// Calculates the hydraulic power available (P = ρgQH) in Watts.
        /// </summary>
        /// <param name="volumetricFlowRate">Volumetric flow rate in m³/s.</param>
        public double CalculateHydraulicPower(double volumetricFlowRate)
        {
            const double gravity = 9.80665;
            return FluidDensity * gravity * volumetricFlowRate * NetHead;
        }

        /// <summary>
        /// Calculates the hydraulic turbine shaft power output in Watts.
        /// P_shaft = η_is × η_mech × ρgQH
        /// </summary>
        /// <param name="volumetricFlowRate">Volumetric flow rate in m³/s.</param>
        public double CalculateHydraulicShaftPower(double volumetricFlowRate)
        {
            return CalculateHydraulicPower(volumetricFlowRate)
                * IsentropicEfficiency * MechanicalEfficiency;
        }

        /// <summary>
        /// Calculates the specific speed for a hydraulic turbine.
        /// Ns = N√P / (ρgH)^(5/4)
        /// </summary>
        public double CalculateSpecificSpeed()
        {
            const double gravity = 9.80665;
            double denominator = Math.Pow(FluidDensity * gravity * NetHead, 1.25);
            double shaftPower = CalculateShaftPower();

            return denominator > 0
                ? CurrentSpeed * Math.Sqrt(shaftPower) / denominator
                : 0.0;
        }

        /// <summary>
        /// Calculates the torque at the current speed (τ = P / ω) in N·m.
        /// </summary>
        public double CalculateTorque()
        {
            double omega = CurrentSpeed * 2.0 * Math.PI / 60.0; // RPM to rad/s
            return omega > 0 ? CalculateShaftPower() / omega : 0.0;
        }

        //
        //  Status
        //

        /// <summary>
        /// Updates the turbine status based on current conditions.
        /// </summary>
        private void UpdateStatus()
        {
            if (Status == TurbineStatusEnum.Faulted)
                return;

            if (CurrentSpeed <= 0)
            {
                Status = TurbineStatusEnum.Off;
            }
            else if (IsOverspeed)
            {
                Status = TurbineStatusEnum.Overspeed;
            }
            else
            {
                Status = TurbineStatusEnum.Running;
            }
        }

        public override string ToString() =>
            $"Turbine [{TurbineType}, {Status}, {CurrentSpeed:F0} RPM, " +
            $"ER={ExpansionRatio:F2}, P={CalculateShaftPower() / 1000:F1} kW]";

        #endregion
        //  *****************************************************************************************
    }
}
