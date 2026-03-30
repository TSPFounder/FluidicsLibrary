using System;
using System.Collections.Generic;
using System.Text;

namespace FluidicsLibrary
{
    /// <summary>
    /// Represents an evaporator heat exchanger that absorbs heat to vaporize a fluid.
    /// </summary>
    internal class Evaporator
    {
        //  *****************************************************************************************
        //  DECLARATIONS
        //
        //  ************************************************************
        #region

        private double _heatLoad;
        private double _superheat;

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  ENUMERATIONS
        //
        //  ************************************************************
        #region

        public enum EvaporatorTypeEnum
        {
            ShellAndTube = 0,
            PlateAndFrame,
            FinAndTube,
            FallingFilm,
            Flooded,
            DirectExpansion,
            Other
        }

        public enum EvaporatorStatusEnum
        {
            Off = 0,
            Running,
            Frosted,
            Faulted
        }

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  CONSTRUCTORS
        //
        //  ************************************************************
        #region

        public Evaporator()
        {
            Status = EvaporatorStatusEnum.Off;
            FluidSpecificHeat = 4186.0;
        }

        public Evaporator(EvaporatorTypeEnum evaporatorType, double ratedCapacity, double evaporatingTemperature)
            : this()
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ratedCapacity);

            EvaporatorType = evaporatorType;
            RatedCapacity = ratedCapacity;
            EvaporatingTemperature = evaporatingTemperature;
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
        /// Type of evaporator.
        /// </summary>
        public EvaporatorTypeEnum EvaporatorType { get; set; }

        /// <summary>
        /// Current operating status.
        /// </summary>
        public EvaporatorStatusEnum Status { get; private set; }

        //
        //  Rated (Design) Values
        //

        /// <summary>
        /// Rated cooling capacity in Watts.
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

        //
        //  Operating Temperatures
        //

        /// <summary>
        /// Evaporating (saturation) temperature of the refrigerant in °C.
        /// </summary>
        public double EvaporatingTemperature { get; set; }

        /// <summary>
        /// Inlet temperature of the fluid being cooled in °C.
        /// </summary>
        public double FluidInletTemperature { get; set; }

        /// <summary>
        /// Outlet temperature of the fluid being cooled in °C.
        /// </summary>
        public double FluidOutletTemperature { get; set; }

        /// <summary>
        /// Superheat at the evaporator outlet in °C (vapor temperature above saturation).
        /// </summary>
        public double Superheat
        {
            get => _superheat;
            set => _superheat = Math.Max(value, 0.0);
        }

        //
        //  Operating Data
        //

        /// <summary>
        /// Current heat load (cooling duty) in Watts.
        /// </summary>
        public double HeatLoad
        {
            get => _heatLoad;
            set => _heatLoad = Math.Max(value, 0.0);
        }

        /// <summary>
        /// Mass flow rate of the fluid being cooled in kg/s.
        /// </summary>
        public double FluidMassFlowRate { get; set; }

        /// <summary>
        /// Specific heat capacity of the fluid being cooled in J/(kg·K). Default is water.
        /// </summary>
        public double FluidSpecificHeat { get; set; }

        /// <summary>
        /// Refrigerant mass flow rate in kg/s.
        /// </summary>
        public double RefrigerantMassFlowRate { get; set; }

        /// <summary>
        /// Evaporating pressure in Pascals.
        /// </summary>
        public double EvaporatingPressure { get; set; }

        /// <summary>
        /// Frost thickness on the coil surface in meters.
        /// </summary>
        public double FrostThickness { get; set; }

        //
        //  Computed Properties
        //

        /// <summary>
        /// Temperature difference between fluid inlet and outlet in °C.
        /// </summary>
        public double FluidTemperatureDrop => FluidInletTemperature - FluidOutletTemperature;

        /// <summary>
        /// Refrigerant outlet temperature (evaporating temp + superheat) in °C.
        /// </summary>
        public double RefrigerantOutletTemperature => EvaporatingTemperature + Superheat;

        /// <summary>
        /// Whether the evaporator is currently running.
        /// </summary>
        public bool IsRunning => Status == EvaporatorStatusEnum.Running || Status == EvaporatorStatusEnum.Frosted;

        /// <summary>
        /// Whether frost has accumulated on the evaporator surface.
        /// </summary>
        public bool IsFrosted => FrostThickness > 0.0;

        /// <summary>
        /// Current capacity as a fraction of rated capacity (0.0 to 1.0+).
        /// </summary>
        public double LoadFraction => RatedCapacity > 0 ? HeatLoad / RatedCapacity : 0.0;

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  METHODS
        //
        //  ************************************************************
        #region

        /// <summary>
        /// Starts the evaporator.
        /// </summary>
        public void Start()
        {
            if (Status == EvaporatorStatusEnum.Faulted)
                return;

            Status = IsFrosted ? EvaporatorStatusEnum.Frosted : EvaporatorStatusEnum.Running;
        }

        /// <summary>
        /// Stops the evaporator.
        /// </summary>
        public void Stop()
        {
            if (Status == EvaporatorStatusEnum.Faulted)
                return;

            Status = EvaporatorStatusEnum.Off;
        }

        /// <summary>
        /// Sets the evaporator to a faulted state.
        /// </summary>
        public void SetFault()
        {
            Status = EvaporatorStatusEnum.Faulted;
        }

        /// <summary>
        /// Clears a fault and resets to Off.
        /// </summary>
        public void ClearFault()
        {
            if (Status == EvaporatorStatusEnum.Faulted)
                Status = EvaporatorStatusEnum.Off;
        }

        /// <summary>
        /// Calculates the Log Mean Temperature Difference (LMTD) for a counter-flow arrangement in °C.
        /// </summary>
        public double CalculateLMTD()
        {
            double deltaT1 = FluidInletTemperature - EvaporatingTemperature;
            double deltaT2 = FluidOutletTemperature - EvaporatingTemperature;

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
        /// Calculates the heat absorbed from the fluid using Q = ṁ × Cp × ΔT in Watts.
        /// </summary>
        public double CalculateFluidHeatLoad()
        {
            return FluidMassFlowRate * FluidSpecificHeat * FluidTemperatureDrop;
        }

        /// <summary>
        /// Calculates the required fluid outlet temperature for a given heat load in °C.
        /// </summary>
        /// <param name="targetHeatLoad">Desired heat load in Watts.</param>
        public double CalculateRequiredOutletTemperature(double targetHeatLoad)
        {
            double denominator = FluidMassFlowRate * FluidSpecificHeat;
            return denominator > 0
                ? FluidInletTemperature - (targetHeatLoad / denominator)
                : FluidInletTemperature;
        }

        /// <summary>
        /// Calculates the required refrigerant mass flow rate for the current heat load
        /// given the latent heat of vaporization (Q = ṁ × h_fg) in kg/s.
        /// </summary>
        /// <param name="latentHeat">Latent heat of vaporization in J/kg.</param>
        public double CalculateRequiredRefrigerantFlow(double latentHeat)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(latentHeat);
            return HeatLoad / latentHeat;
        }

        /// <summary>
        /// Calculates the approach temperature (fluid outlet temp minus evaporating temp) in °C.
        /// </summary>
        public double CalculateApproachTemperature()
        {
            return FluidOutletTemperature - EvaporatingTemperature;
        }

        /// <summary>
        /// Estimates the effectiveness of the evaporator (ε = Q_actual / Q_max).
        /// </summary>
        public double CalculateEffectiveness()
        {
            double maxHeatTransfer = FluidMassFlowRate * FluidSpecificHeat
                * (FluidInletTemperature - EvaporatingTemperature);

            return maxHeatTransfer > 0 ? HeatLoad / maxHeatTransfer : 0.0;
        }

        /// <summary>
        /// Initiates a defrost cycle by clearing frost and stopping the evaporator temporarily.
        /// </summary>
        public void Defrost()
        {
            FrostThickness = 0.0;

            if (Status == EvaporatorStatusEnum.Frosted)
                Status = EvaporatorStatusEnum.Running;
        }

        /// <summary>
        /// Updates frost status based on current frost thickness.
        /// </summary>
        public void UpdateFrostStatus()
        {
            if (!IsRunning)
                return;

            Status = IsFrosted ? EvaporatorStatusEnum.Frosted : EvaporatorStatusEnum.Running;
        }

        public override string ToString() =>
            $"Evaporator [{EvaporatorType}, {Status}, Q={HeatLoad / 1000:F2} kW, Tevap={EvaporatingTemperature:F1}°C]";

        #endregion
        //  *****************************************************************************************
    }
}
