using System;
using System.Collections.Generic;
using System.Text;

namespace FluidicsLibrary
{
    /// <summary>
    /// Represents a condenser heat exchanger that rejects heat to condense a vapor to liquid.
    /// </summary>
    internal class Condenser
    {
        //  *****************************************************************************************
        //  DECLARATIONS
        //
        //  ************************************************************
        #region

        private double _heatRejection;
        private double _subcooling;

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  ENUMERATIONS
        //
        //  ************************************************************
        #region

        public enum CondenserTypeEnum
        {
            ShellAndTube = 0,
            PlateAndFrame,
            AirCooled,
            EvaporativeCooled,
            WaterCooled,
            Other
        }

        public enum CondenserStatusEnum
        {
            Off = 0,
            Running,
            Fouled,
            Faulted
        }

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  CONSTRUCTORS
        //
        //  ************************************************************
        #region

        public Condenser()
        {
            Status = CondenserStatusEnum.Off;
            CoolantSpecificHeat = 4186.0;
        }

        public Condenser(CondenserTypeEnum condenserType, double ratedCapacity, double condensingTemperature)
            : this()
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ratedCapacity);

            CondenserType = condenserType;
            RatedCapacity = ratedCapacity;
            CondensingTemperature = condensingTemperature;
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
        /// Type of condenser.
        /// </summary>
        public CondenserTypeEnum CondenserType { get; set; }

        /// <summary>
        /// Current operating status.
        /// </summary>
        public CondenserStatusEnum Status { get; private set; }

        //
        //  Rated (Design) Values
        //

        /// <summary>
        /// Rated heat rejection capacity in Watts.
        /// </summary>
        public double RatedCapacity { get; set; }

        /// <summary>
        /// Heat transfer surface area in square meters.
        /// </summary>
        public double SurfaceArea { get; set; }

        /// <summary>
        /// Overall heat transfer coefficient (U-value) in W/(m²·K).
        /// </summary>
        public double OverallHeatTransferCoefficient { get; set; }

        /// <summary>
        /// Fouling factor in (m²·K)/W. Increases as the condenser fouls.
        /// </summary>
        public double FoulingFactor { get; set; }

        //
        //  Operating Temperatures
        //

        /// <summary>
        /// Condensing (saturation) temperature of the refrigerant in °C.
        /// </summary>
        public double CondensingTemperature { get; set; }

        /// <summary>
        /// Inlet temperature of the cooling medium in °C.
        /// </summary>
        public double CoolantInletTemperature { get; set; }

        /// <summary>
        /// Outlet temperature of the cooling medium in °C.
        /// </summary>
        public double CoolantOutletTemperature { get; set; }

        /// <summary>
        /// Ambient air temperature in °C (for air-cooled condensers).
        /// </summary>
        public double AmbientTemperature { get; set; }

        /// <summary>
        /// Subcooling at the condenser outlet in °C (liquid temperature below saturation).
        /// </summary>
        public double Subcooling
        {
            get => _subcooling;
            set => _subcooling = Math.Max(value, 0.0);
        }

        //
        //  Operating Data
        //

        /// <summary>
        /// Current heat rejection rate in Watts.
        /// </summary>
        public double HeatRejection
        {
            get => _heatRejection;
            set => _heatRejection = Math.Max(value, 0.0);
        }

        /// <summary>
        /// Mass flow rate of the cooling medium in kg/s.
        /// </summary>
        public double CoolantMassFlowRate { get; set; }

        /// <summary>
        /// Specific heat capacity of the cooling medium in J/(kg·K). Default is water.
        /// </summary>
        public double CoolantSpecificHeat { get; set; }

        /// <summary>
        /// Refrigerant mass flow rate in kg/s.
        /// </summary>
        public double RefrigerantMassFlowRate { get; set; }

        /// <summary>
        /// Condensing pressure in Pascals.
        /// </summary>
        public double CondensingPressure { get; set; }

        /// <summary>
        /// Fan speed as a fraction (0.0 to 1.0) for air-cooled condensers.
        /// </summary>
        public double FanSpeed { get; set; }

        //
        //  Computed Properties
        //

        /// <summary>
        /// Temperature rise of the cooling medium in °C.
        /// </summary>
        public double CoolantTemperatureRise => CoolantOutletTemperature - CoolantInletTemperature;

        /// <summary>
        /// Refrigerant outlet temperature (condensing temp − subcooling) in °C.
        /// </summary>
        public double RefrigerantOutletTemperature => CondensingTemperature - Subcooling;

        /// <summary>
        /// Whether the condenser is currently running.
        /// </summary>
        public bool IsRunning => Status == CondenserStatusEnum.Running || Status == CondenserStatusEnum.Fouled;

        /// <summary>
        /// Whether the condenser has significant fouling.
        /// </summary>
        public bool IsFouled => FoulingFactor > 0.0;

        /// <summary>
        /// Current capacity as a fraction of rated capacity (0.0 to 1.0+).
        /// </summary>
        public double LoadFraction => RatedCapacity > 0 ? HeatRejection / RatedCapacity : 0.0;

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  METHODS
        //
        //  ************************************************************
        #region

        /// <summary>
        /// Starts the condenser.
        /// </summary>
        public void Start()
        {
            if (Status == CondenserStatusEnum.Faulted)
                return;

            Status = IsFouled ? CondenserStatusEnum.Fouled : CondenserStatusEnum.Running;
        }

        /// <summary>
        /// Stops the condenser.
        /// </summary>
        public void Stop()
        {
            if (Status == CondenserStatusEnum.Faulted)
                return;

            Status = CondenserStatusEnum.Off;
        }

        /// <summary>
        /// Sets the condenser to a faulted state.
        /// </summary>
        public void SetFault()
        {
            Status = CondenserStatusEnum.Faulted;
        }

        /// <summary>
        /// Clears a fault and resets to Off.
        /// </summary>
        public void ClearFault()
        {
            if (Status == CondenserStatusEnum.Faulted)
                Status = CondenserStatusEnum.Off;
        }

        /// <summary>
        /// Calculates the Log Mean Temperature Difference (LMTD) for a counter-flow arrangement in °C.
        /// </summary>
        public double CalculateLMTD()
        {
            double deltaT1 = CondensingTemperature - CoolantOutletTemperature;
            double deltaT2 = CondensingTemperature - CoolantInletTemperature;

            if (deltaT1 <= 0 || deltaT2 <= 0)
                return 0.0;

            if (Math.Abs(deltaT1 - deltaT2) < 1e-9)
                return deltaT1;

            return (deltaT1 - deltaT2) / Math.Log(deltaT1 / deltaT2);
        }

        /// <summary>
        /// Calculates the heat transfer rate using Q = U × A × LMTD in Watts.
        /// </summary>
        public double CalculateHeatTransferRate()
        {
            return OverallHeatTransferCoefficient * SurfaceArea * CalculateLMTD();
        }

        /// <summary>
        /// Calculates the effective U-value accounting for fouling in W/(m²·K).
        /// U_eff = 1 / (1/U + R_f)
        /// </summary>
        public double CalculateEffectiveHeatTransferCoefficient()
        {
            if (OverallHeatTransferCoefficient <= 0)
                return 0.0;

            double resistance = (1.0 / OverallHeatTransferCoefficient) + FoulingFactor;
            return resistance > 0 ? 1.0 / resistance : 0.0;
        }

        /// <summary>
        /// Calculates the heat rejected to the cooling medium using Q = ṁ × Cp × ΔT in Watts.
        /// </summary>
        public double CalculateCoolantHeatAbsorption()
        {
            return CoolantMassFlowRate * CoolantSpecificHeat * CoolantTemperatureRise;
        }

        /// <summary>
        /// Calculates the required coolant outlet temperature for a given heat rejection in °C.
        /// </summary>
        /// <param name="targetHeatRejection">Desired heat rejection in Watts.</param>
        public double CalculateRequiredCoolantOutletTemperature(double targetHeatRejection)
        {
            double denominator = CoolantMassFlowRate * CoolantSpecificHeat;
            return denominator > 0
                ? CoolantInletTemperature + (targetHeatRejection / denominator)
                : CoolantInletTemperature;
        }

        /// <summary>
        /// Calculates the required refrigerant mass flow rate for the current heat rejection
        /// given the latent heat of condensation (Q = ṁ × h_fg) in kg/s.
        /// </summary>
        /// <param name="latentHeat">Latent heat of condensation in J/kg.</param>
        public double CalculateRequiredRefrigerantFlow(double latentHeat)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(latentHeat);
            return HeatRejection / latentHeat;
        }

        /// <summary>
        /// Calculates the approach temperature (condensing temp minus coolant outlet temp) in °C.
        /// </summary>
        public double CalculateApproachTemperature()
        {
            return CondensingTemperature - CoolantOutletTemperature;
        }

        /// <summary>
        /// Estimates the effectiveness of the condenser (ε = Q_actual / Q_max).
        /// </summary>
        public double CalculateEffectiveness()
        {
            double maxHeatTransfer = CoolantMassFlowRate * CoolantSpecificHeat
                * (CondensingTemperature - CoolantInletTemperature);

            return maxHeatTransfer > 0 ? HeatRejection / maxHeatTransfer : 0.0;
        }

        /// <summary>
        /// Cleans the condenser by resetting the fouling factor.
        /// </summary>
        public void Clean()
        {
            FoulingFactor = 0.0;

            if (Status == CondenserStatusEnum.Fouled)
                Status = CondenserStatusEnum.Running;
        }

        /// <summary>
        /// Updates fouling status based on the current fouling factor.
        /// </summary>
        public void UpdateFoulingStatus()
        {
            if (!IsRunning)
                return;

            Status = IsFouled ? CondenserStatusEnum.Fouled : CondenserStatusEnum.Running;
        }

        public override string ToString() =>
            $"Condenser [{CondenserType}, {Status}, Q={HeatRejection / 1000:F2} kW, Tcond={CondensingTemperature:F1}°C]";

        #endregion
        //  *****************************************************************************************
    }
}
