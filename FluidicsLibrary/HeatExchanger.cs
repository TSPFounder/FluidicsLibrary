#nullable enable

using System;
using SE_Library;
using CAD;
using Mathematics;

namespace ThermalManagement
{
    public class HeatExchanger : SE_System
    {
        //  *****************************************************************************************
        //  DECLARATIONS
        //
        //  ************************************************************
        #region

        private double _hotInletTemp;
        private double _hotOutletTemp;
        private double _coldInletTemp;
        private double _coldOutletTemp;

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  ENUMERATIONS
        //
        //  ************************************************************
        #region

        public enum HeatExchangerTypeEnum
        {
            ShellAndTube = 0,
            PlateAndFrame,
            FinAndTube,
            DoublePipe,
            SpiralPlate,
            CrossFlow,
            Other
        }

        public enum FlowArrangementEnum
        {
            CounterFlow = 0,
            ParallelFlow,
            CrossFlow,
            MultiPass
        }

        public enum HeatExchangerStatusEnum
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

        public HeatExchanger()
        {
            Status = HeatExchangerStatusEnum.Off;
            FlowArrangement = FlowArrangementEnum.CounterFlow;
        }

        public HeatExchanger(
            HeatExchangerTypeEnum type,
            double surfaceArea,
            double overallHeatTransferCoefficient)
            : this()
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(surfaceArea);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(overallHeatTransferCoefficient);

            ExchangerType = type;
            SurfaceArea = surfaceArea;
            OverallHeatTransferCoefficient = overallHeatTransferCoefficient;
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
        /// Type of heat exchanger.
        /// </summary>
        public HeatExchangerTypeEnum ExchangerType { get; set; }

        /// <summary>
        /// Flow arrangement (counter-flow, parallel, cross-flow, etc.).
        /// </summary>
        public FlowArrangementEnum FlowArrangement { get; set; }

        /// <summary>
        /// Current operating status.
        /// </summary>
        public HeatExchangerStatusEnum Status { get; private set; }

        //
        //  Design Parameters
        //

        /// <summary>
        /// Heat transfer surface area in m².
        /// </summary>
        public double SurfaceArea { get; set; }

        /// <summary>
        /// Overall heat transfer coefficient (U-value) in W/(m²·K).
        /// </summary>
        public double OverallHeatTransferCoefficient { get; set; }

        /// <summary>
        /// Fouling factor on the hot side in (m²·K)/W.
        /// </summary>
        public double HotSideFoulingFactor { get; set; }

        /// <summary>
        /// Fouling factor on the cold side in (m²·K)/W.
        /// </summary>
        public double ColdSideFoulingFactor { get; set; }

        /// <summary>
        /// Maximum allowable pressure drop across either side in Pascals.
        /// </summary>
        public double MaxPressureDrop { get; set; }

        //
        //  Hot Side
        //

        /// <summary>
        /// Hot-side inlet temperature in °C.
        /// </summary>
        public double HotInletTemperature
        {
            get => _hotInletTemp;
            set => _hotInletTemp = value;
        }

        /// <summary>
        /// Hot-side outlet temperature in °C.
        /// </summary>
        public double HotOutletTemperature
        {
            get => _hotOutletTemp;
            set => _hotOutletTemp = value;
        }

        /// <summary>
        /// Hot-side mass flow rate in kg/s.
        /// </summary>
        public double HotMassFlowRate { get; set; }

        /// <summary>
        /// Hot-side specific heat capacity in J/(kg·K).
        /// </summary>
        public double HotSpecificHeat { get; set; } = 4186.0;

        /// <summary>
        /// Hot-side pressure drop in Pascals.
        /// </summary>
        public double HotPressureDrop { get; set; }

        //
        //  Cold Side
        //

        /// <summary>
        /// Cold-side inlet temperature in °C.
        /// </summary>
        public double ColdInletTemperature
        {
            get => _coldInletTemp;
            set => _coldInletTemp = value;
        }

        /// <summary>
        /// Cold-side outlet temperature in °C.
        /// </summary>
        public double ColdOutletTemperature
        {
            get => _coldOutletTemp;
            set => _coldOutletTemp = value;
        }

        /// <summary>
        /// Cold-side mass flow rate in kg/s.
        /// </summary>
        public double ColdMassFlowRate { get; set; }

        /// <summary>
        /// Cold-side specific heat capacity in J/(kg·K).
        /// </summary>
        public double ColdSpecificHeat { get; set; } = 4186.0;

        /// <summary>
        /// Cold-side pressure drop in Pascals.
        /// </summary>
        public double ColdPressureDrop { get; set; }

        //
        //  Computed Properties
        //

        /// <summary>
        /// Hot-side heat capacity rate (C_h = ṁ_h × Cp_h) in W/K.
        /// </summary>
        public double HotCapacityRate => HotMassFlowRate * HotSpecificHeat;

        /// <summary>
        /// Cold-side heat capacity rate (C_c = ṁ_c × Cp_c) in W/K.
        /// </summary>
        public double ColdCapacityRate => ColdMassFlowRate * ColdSpecificHeat;

        /// <summary>
        /// Minimum heat capacity rate (C_min) in W/K.
        /// </summary>
        public double MinCapacityRate => Math.Min(HotCapacityRate, ColdCapacityRate);

        /// <summary>
        /// Maximum heat capacity rate (C_max) in W/K.
        /// </summary>
        public double MaxCapacityRate => Math.Max(HotCapacityRate, ColdCapacityRate);

        /// <summary>
        /// Heat capacity ratio (C_r = C_min / C_max).
        /// </summary>
        public double CapacityRatio => MaxCapacityRate > 0 ? MinCapacityRate / MaxCapacityRate : 0.0;

        /// <summary>
        /// Temperature drop on the hot side in °C.
        /// </summary>
        public double HotTemperatureDrop => HotInletTemperature - HotOutletTemperature;

        /// <summary>
        /// Temperature rise on the cold side in °C.
        /// </summary>
        public double ColdTemperatureRise => ColdOutletTemperature - ColdInletTemperature;

        /// <summary>
        /// Whether the heat exchanger is currently running.
        /// </summary>
        public bool IsRunning => Status is HeatExchangerStatusEnum.Running or HeatExchangerStatusEnum.Fouled;

        /// <summary>
        /// Whether fouling is present on either side.
        /// </summary>
        public bool IsFouled => HotSideFoulingFactor > 0.0 || ColdSideFoulingFactor > 0.0;

        /// <summary>
        /// Number of Transfer Units (NTU = UA / C_min).
        /// </summary>
        public double NTU => MinCapacityRate > 0
            ? OverallHeatTransferCoefficient * SurfaceArea / MinCapacityRate
            : 0.0;

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  METHODS
        //
        //  ************************************************************
        #region

        /// <summary>
        /// Starts the heat exchanger.
        /// </summary>
        public void Start()
        {
            if (Status == HeatExchangerStatusEnum.Faulted)
                return;

            Status = IsFouled ? HeatExchangerStatusEnum.Fouled : HeatExchangerStatusEnum.Running;
        }

        /// <summary>
        /// Stops the heat exchanger.
        /// </summary>
        public void Stop()
        {
            if (Status == HeatExchangerStatusEnum.Faulted)
                return;

            Status = HeatExchangerStatusEnum.Off;
        }

        /// <summary>
        /// Sets the heat exchanger to a faulted state.
        /// </summary>
        public void SetFault()
        {
            Status = HeatExchangerStatusEnum.Faulted;
        }

        /// <summary>
        /// Clears a fault and resets to Off.
        /// </summary>
        public void ClearFault()
        {
            if (Status == HeatExchangerStatusEnum.Faulted)
                Status = HeatExchangerStatusEnum.Off;
        }

        /// <summary>
        /// Calculates the heat duty on the hot side (Q = ṁ_h × Cp_h × ΔT_h) in Watts.
        /// </summary>
        public double CalculateHotSideDuty()
        {
            return HotCapacityRate * HotTemperatureDrop;
        }

        /// <summary>
        /// Calculates the heat duty on the cold side (Q = ṁ_c × Cp_c × ΔT_c) in Watts.
        /// </summary>
        public double CalculateColdSideDuty()
        {
            return ColdCapacityRate * ColdTemperatureRise;
        }

        /// <summary>
        /// Calculates the Log Mean Temperature Difference (LMTD) in °C
        /// based on the current flow arrangement.
        /// </summary>
        public double CalculateLMTD()
        {
            double deltaT1, deltaT2;

            if (FlowArrangement == FlowArrangementEnum.ParallelFlow)
            {
                // Parallel flow: both fluids enter at same end
                deltaT1 = HotInletTemperature - ColdInletTemperature;
                deltaT2 = HotOutletTemperature - ColdOutletTemperature;
            }
            else
            {
                // Counter flow (and default for cross/multi-pass before correction)
                deltaT1 = HotInletTemperature - ColdOutletTemperature;
                deltaT2 = HotOutletTemperature - ColdInletTemperature;
            }

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
        /// Calculates the effective U-value accounting for fouling on both sides in W/(m²·K).
        /// U_eff = 1 / (1/U + R_f_hot + R_f_cold)
        /// </summary>
        public double CalculateEffectiveHeatTransferCoefficient()
        {
            if (OverallHeatTransferCoefficient <= 0)
                return 0.0;

            double totalResistance = (1.0 / OverallHeatTransferCoefficient)
                + HotSideFoulingFactor
                + ColdSideFoulingFactor;

            return totalResistance > 0 ? 1.0 / totalResistance : 0.0;
        }

        /// <summary>
        /// Calculates the effectiveness using the ε-NTU method.
        /// Applies the appropriate formula based on the flow arrangement.
        /// </summary>
        public double CalculateEffectiveness()
        {
            double ntu = NTU;
            double cr = CapacityRatio;

            if (ntu <= 0)
                return 0.0;

            // Special case: one fluid undergoes phase change (C_max → ∞, C_r → 0)
            if (cr < 1e-9)
                return 1.0 - Math.Exp(-ntu);

            return FlowArrangement switch
            {
                // ε = (1 - exp(-NTU(1 - Cr))) / (1 - Cr·exp(-NTU(1 - Cr)))
                FlowArrangementEnum.CounterFlow =>
                    Math.Abs(cr - 1.0) < 1e-9
                        ? ntu / (1.0 + ntu)
                        : (1.0 - Math.Exp(-ntu * (1.0 - cr)))
                            / (1.0 - cr * Math.Exp(-ntu * (1.0 - cr))),

                // ε = (1 - exp(-NTU(1 + Cr))) / (1 + Cr)
                FlowArrangementEnum.ParallelFlow =>
                    (1.0 - Math.Exp(-ntu * (1.0 + cr))) / (1.0 + cr),

                // Default: use counter-flow as a conservative estimate
                _ => Math.Abs(cr - 1.0) < 1e-9
                    ? ntu / (1.0 + ntu)
                    : (1.0 - Math.Exp(-ntu * (1.0 - cr)))
                        / (1.0 - cr * Math.Exp(-ntu * (1.0 - cr)))
            };
        }

        /// <summary>
        /// Calculates the maximum possible heat transfer rate (Q_max = C_min × (T_h,in − T_c,in)) in Watts.
        /// </summary>
        public double CalculateMaxHeatTransferRate()
        {
            return MinCapacityRate * (HotInletTemperature - ColdInletTemperature);
        }

        /// <summary>
        /// Calculates the actual heat transfer using the ε-NTU method (Q = ε × Q_max) in Watts.
        /// </summary>
        public double CalculateActualHeatTransfer()
        {
            return CalculateEffectiveness() * CalculateMaxHeatTransferRate();
        }

        /// <summary>
        /// Calculates the hot-side outlet temperature for a given duty in °C.
        /// </summary>
        /// <param name="duty">Heat duty in Watts.</param>
        public double CalculateHotOutletTemperature(double duty)
        {
            return HotCapacityRate > 0
                ? HotInletTemperature - (duty / HotCapacityRate)
                : HotInletTemperature;
        }

        /// <summary>
        /// Calculates the cold-side outlet temperature for a given duty in °C.
        /// </summary>
        /// <param name="duty">Heat duty in Watts.</param>
        public double CalculateColdOutletTemperature(double duty)
        {
            return ColdCapacityRate > 0
                ? ColdInletTemperature + (duty / ColdCapacityRate)
                : ColdInletTemperature;
        }

        /// <summary>
        /// Calculates the required surface area to achieve the target duty using LMTD in m².
        /// A = Q / (U × LMTD)
        /// </summary>
        /// <param name="targetDuty">Target heat duty in Watts.</param>
        public double CalculateRequiredSurfaceArea(double targetDuty)
        {
            double lmtd = CalculateLMTD();
            double product = OverallHeatTransferCoefficient * lmtd;
            return product > 0 ? targetDuty / product : 0.0;
        }

        /// <summary>
        /// Checks the energy balance between hot and cold sides.
        /// Returns the imbalance as a fraction of the average duty.
        /// A value near zero indicates a valid energy balance.
        /// </summary>
        public double CheckEnergyBalance()
        {
            double hotDuty = CalculateHotSideDuty();
            double coldDuty = CalculateColdSideDuty();
            double average = (Math.Abs(hotDuty) + Math.Abs(coldDuty)) / 2.0;

            return average > 0 ? Math.Abs(hotDuty - coldDuty) / average : 0.0;
        }

        /// <summary>
        /// Cleans the heat exchanger by resetting fouling factors on both sides.
        /// </summary>
        public void Clean()
        {
            HotSideFoulingFactor = 0.0;
            ColdSideFoulingFactor = 0.0;

            if (Status == HeatExchangerStatusEnum.Fouled)
                Status = HeatExchangerStatusEnum.Running;
        }

        /// <summary>
        /// Updates the fouling status based on current fouling factors.
        /// </summary>
        public void UpdateFoulingStatus()
        {
            if (!IsRunning)
                return;

            Status = IsFouled ? HeatExchangerStatusEnum.Fouled : HeatExchangerStatusEnum.Running;
        }

        public override string ToString() =>
            $"HeatExchanger [{ExchangerType}, {FlowArrangement}, {Status}, " +
            $"Q={CalculateHeatTransferRate() / 1000:F2} kW, LMTD={CalculateLMTD():F1}°C]";

        #endregion
        //  *****************************************************************************************
    }
}
