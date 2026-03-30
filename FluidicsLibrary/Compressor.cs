using System;
using System.Collections.Generic;
using System.Text;

namespace FluidicsLibrary
{
    /// <summary>
    /// Represents a compressor that increases the pressure of a gas.
    /// </summary>
    internal class Compressor
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

        public enum CompressorTypeEnum
        {
            Centrifugal = 0,
            Axial,
            Reciprocating,
            Screw,
            Scroll,
            Rotary,
            Diaphragm,
            Other
        }

        public enum CompressorStatusEnum
        {
            Off = 0,
            Running,
            Surge,
            Choke,
            Faulted
        }

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  CONSTRUCTORS
        //
        //  ************************************************************
        #region

        public Compressor()
        {
            Status = CompressorStatusEnum.Off;
            IsentropicEfficiency = 0.85;
            MechanicalEfficiency = 0.95;
            SpecificHeatRatio = 1.4; // air
            GasConstant = 287.0;     // air, J/(kg·K)
        }

        public Compressor(CompressorTypeEnum type, double ratedSpeed, double ratedPressureRatio)
            : this()
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ratedSpeed);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ratedPressureRatio);

            CompressorType = type;
            RatedSpeed = ratedSpeed;
            RatedPressureRatio = ratedPressureRatio;
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
        /// Type of compressor.
        /// </summary>
        public CompressorTypeEnum CompressorType { get; set; }

        /// <summary>
        /// Current operating status.
        /// </summary>
        public CompressorStatusEnum Status { get; private set; }

        //
        //  Design (Rated) Parameters
        //

        /// <summary>
        /// Rated rotational speed in RPM.
        /// </summary>
        public double RatedSpeed { get; set; }

        /// <summary>
        /// Rated pressure ratio (P_out / P_in).
        /// </summary>
        public double RatedPressureRatio { get; set; }

        /// <summary>
        /// Rated mass flow rate in kg/s.
        /// </summary>
        public double RatedMassFlowRate { get; set; }

        /// <summary>
        /// Rated shaft power in Watts.
        /// </summary>
        public double RatedPower { get; set; }

        /// <summary>
        /// Surge margin as a fraction of rated flow (below which surge occurs).
        /// </summary>
        public double SurgeMargin { get; set; } = 0.3;

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
        /// Inlet (suction) pressure in Pascals.
        /// </summary>
        public double InletPressure { get; set; }

        /// <summary>
        /// Outlet (discharge) pressure in Pascals.
        /// </summary>
        public double OutletPressure { get; set; }

        /// <summary>
        /// Inlet temperature in Kelvin.
        /// </summary>
        public double InletTemperature { get; set; } = 293.15;

        /// <summary>
        /// Mass flow rate in kg/s.
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
        /// Polytropic efficiency as a fraction (0.0 to 1.0).
        /// </summary>
        public double PolytropicEfficiency { get; set; } = 0.88;

        /// <summary>
        /// Mechanical efficiency as a fraction (0.0 to 1.0).
        /// </summary>
        public double MechanicalEfficiency { get; set; }

        /// <summary>
        /// Volumetric efficiency for positive-displacement compressors (0.0 to 1.0).
        /// </summary>
        public double VolumetricEfficiency { get; set; } = 0.9;

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
        //  Computed Properties
        //

        /// <summary>
        /// Current pressure ratio (P_out / P_in).
        /// </summary>
        public double PressureRatio =>
            InletPressure > 0 ? OutletPressure / InletPressure : 0.0;

        /// <summary>
        /// Speed ratio relative to rated speed.
        /// </summary>
        public double SpeedRatio =>
            RatedSpeed > 0 ? CurrentSpeed / RatedSpeed : 0.0;

        /// <summary>
        /// Whether the compressor is currently running.
        /// </summary>
        public bool IsRunning => Status is CompressorStatusEnum.Running
            or CompressorStatusEnum.Surge
            or CompressorStatusEnum.Choke;

        /// <summary>
        /// Whether the compressor is in surge (flow too low for the pressure ratio).
        /// </summary>
        public bool IsInSurge => Status == CompressorStatusEnum.Surge;

        /// <summary>
        /// Corrected mass flow rate (referenced to standard conditions) in kg/s.
        /// ṁ_corr = ṁ × √(T_in / T_ref) / (P_in / P_ref)
        /// </summary>
        public double CorrectedMassFlowRate
        {
            get
            {
                const double tRef = 288.15; // standard day, K
                const double pRef = 101325.0;

                if (InletPressure <= 0)
                    return 0.0;

                return MassFlowRate * Math.Sqrt(InletTemperature / tRef) / (InletPressure / pRef);
            }
        }

        /// <summary>
        /// Corrected speed (referenced to standard conditions) in RPM.
        /// N_corr = N / √(T_in / T_ref)
        /// </summary>
        public double CorrectedSpeed
        {
            get
            {
                const double tRef = 288.15;
                double ratio = InletTemperature / tRef;
                return ratio > 0 ? CurrentSpeed / Math.Sqrt(ratio) : 0.0;
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
        //  Lifecycle
        //

        /// <summary>
        /// Starts the compressor at rated speed.
        /// </summary>
        public void Start()
        {
            Start(RatedSpeed);
        }

        /// <summary>
        /// Starts the compressor at the specified speed.
        /// </summary>
        /// <param name="speed">Target speed in RPM.</param>
        public void Start(double speed)
        {
            if (Status == CompressorStatusEnum.Faulted)
                return;

            CurrentSpeed = speed;
            UpdateStatus();
        }

        /// <summary>
        /// Stops the compressor.
        /// </summary>
        public void Stop()
        {
            if (Status == CompressorStatusEnum.Faulted)
                return;

            CurrentSpeed = 0.0;
            Status = CompressorStatusEnum.Off;
        }

        /// <summary>
        /// Sets the compressor to a faulted state.
        /// </summary>
        public void SetFault()
        {
            _currentSpeed = 0.0;
            Status = CompressorStatusEnum.Faulted;
        }

        /// <summary>
        /// Clears a fault and resets to Off.
        /// </summary>
        public void ClearFault()
        {
            if (Status == CompressorStatusEnum.Faulted)
                Status = CompressorStatusEnum.Off;
        }

        //
        //  Thermodynamic Calculations
        //

        /// <summary>
        /// Calculates the isentropic outlet temperature in Kelvin.
        /// T2s = T1 × (P2/P1)^((γ−1)/γ)
        /// </summary>
        public double CalculateIsentropicOutletTemperature()
        {
            double pr = PressureRatio;
            if (pr <= 0 || SpecificHeatRatio <= 1.0)
                return InletTemperature;

            double exponent = (SpecificHeatRatio - 1.0) / SpecificHeatRatio;
            return InletTemperature * Math.Pow(pr, exponent);
        }

        /// <summary>
        /// Calculates the actual outlet temperature accounting for isentropic efficiency in Kelvin.
        /// T2 = T1 + (T2s − T1) / η_is
        /// </summary>
        public double CalculateActualOutletTemperature()
        {
            double t2s = CalculateIsentropicOutletTemperature();
            return IsentropicEfficiency > 0
                ? InletTemperature + (t2s - InletTemperature) / IsentropicEfficiency
                : InletTemperature;
        }

        /// <summary>
        /// Calculates the isentropic (ideal) work per unit mass in J/kg.
        /// w_is = Cp × T1 × ((P2/P1)^((γ−1)/γ) − 1)
        /// </summary>
        public double CalculateIsentropicWork()
        {
            double pr = PressureRatio;
            if (pr <= 0 || SpecificHeatRatio <= 1.0)
                return 0.0;

            double exponent = (SpecificHeatRatio - 1.0) / SpecificHeatRatio;
            return Cp * InletTemperature * (Math.Pow(pr, exponent) - 1.0);
        }

        /// <summary>
        /// Calculates the actual shaft work per unit mass in J/kg.
        /// w_actual = w_is / η_is
        /// </summary>
        public double CalculateActualWork()
        {
            return IsentropicEfficiency > 0
                ? CalculateIsentropicWork() / IsentropicEfficiency
                : 0.0;
        }

        /// <summary>
        /// Calculates the shaft power required (P = ṁ × w_actual / η_mech) in Watts.
        /// </summary>
        public double CalculateShaftPower()
        {
            double actualWork = CalculateActualWork();
            return MechanicalEfficiency > 0
                ? MassFlowRate * actualWork / MechanicalEfficiency
                : 0.0;
        }

        /// <summary>
        /// Calculates the polytropic head in J/kg.
        /// H_poly = (n/(n−1)) × R × T1 × ((P2/P1)^((n−1)/n) − 1)
        /// where n is the polytropic exponent.
        /// </summary>
        public double CalculatePolytropicHead()
        {
            double pr = PressureRatio;
            if (pr <= 0 || PolytropicEfficiency <= 0 || SpecificHeatRatio <= 1.0)
                return 0.0;

            // Polytropic exponent: (n-1)/n = (γ-1)/(γ × η_poly)
            double nMinusOneOverN = (SpecificHeatRatio - 1.0)
                / (SpecificHeatRatio * PolytropicEfficiency);

            double nOverNMinusOne = nMinusOneOverN > 0 ? 1.0 / nMinusOneOverN : 0.0;

            return nOverNMinusOne * GasConstant * InletTemperature
                * (Math.Pow(pr, nMinusOneOverN) - 1.0);
        }

        /// <summary>
        /// Calculates the number of stages required for a given max stage pressure ratio.
        /// </summary>
        /// <param name="maxStagePressureRatio">Maximum pressure ratio per stage.</param>
        public int CalculateRequiredStages(double maxStagePressureRatio)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxStagePressureRatio);

            if (maxStagePressureRatio <= 1.0)
                return int.MaxValue;

            double pr = PressureRatio;
            if (pr <= 1.0)
                return 0;

            return (int)Math.Ceiling(Math.Log(pr) / Math.Log(maxStagePressureRatio));
        }

        /// <summary>
        /// Calculates the inlet volumetric flow rate (Q = ṁ × R × T / P) in m³/s.
        /// </summary>
        public double CalculateInletVolumetricFlowRate()
        {
            return InletPressure > 0
                ? MassFlowRate * GasConstant * InletTemperature / InletPressure
                : 0.0;
        }

        /// <summary>
        /// Calculates the heat of compression rejected to cooling in Watts.
        /// Q = ṁ × Cp × (T2_actual − T1)
        /// </summary>
        public double CalculateHeatOfCompression()
        {
            double t2 = CalculateActualOutletTemperature();
            return MassFlowRate * Cp * (t2 - InletTemperature);
        }

        //
        //  Status
        //

        /// <summary>
        /// Updates the compressor status based on current operating conditions.
        /// </summary>
        private void UpdateStatus()
        {
            if (Status == CompressorStatusEnum.Faulted)
                return;

            if (CurrentSpeed <= 0)
            {
                Status = CompressorStatusEnum.Off;
                return;
            }

            // Simple surge detection: flow below surge margin
            if (RatedMassFlowRate > 0 && MassFlowRate < RatedMassFlowRate * SurgeMargin)
            {
                Status = CompressorStatusEnum.Surge;
            }
            else
            {
                Status = CompressorStatusEnum.Running;
            }
        }

        public override string ToString() =>
            $"Compressor [{CompressorType}, {Status}, {CurrentSpeed:F0} RPM, " +
            $"PR={PressureRatio:F2}, P={CalculateShaftPower() / 1000:F1} kW]";

        #endregion
        //  *****************************************************************************************
    }
}
