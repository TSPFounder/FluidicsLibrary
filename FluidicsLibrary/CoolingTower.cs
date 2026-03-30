using System;
using System.Collections.Generic;
using System.Text;

namespace FluidicsLibrary
{
    /// <summary>
    /// Represents a cooling tower that rejects heat from water to the atmosphere
    /// through evaporative cooling.
    /// </summary>
    internal class CoolingTower
    {
        //  *****************************************************************************************
        //  DECLARATIONS
        //
        //  ************************************************************
        #region

        private double _fanSpeed;

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  ENUMERATIONS
        //
        //  ************************************************************
        #region

        public enum CoolingTowerTypeEnum
        {
            InducedDraftCounterFlow = 0,
            InducedDraftCrossFlow,
            ForcedDraftCounterFlow,
            ForcedDraftCrossFlow,
            NaturalDraft,
            HybridDraft,
            Other
        }

        public enum CoolingTowerStatusEnum
        {
            Off = 0,
            Running,
            LowFlow,
            Faulted
        }

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  CONSTRUCTORS
        //
        //  ************************************************************
        #region

        public CoolingTower()
        {
            Status = CoolingTowerStatusEnum.Off;
            WaterSpecificHeat = 4186.0;
            WaterDensity = 998.0;
            CyclesOfConcentration = 5.0;
        }

        public CoolingTower(CoolingTowerTypeEnum towerType, double ratedCapacity, double designWetBulb)
            : this()
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ratedCapacity);

            TowerType = towerType;
            RatedCapacity = ratedCapacity;
            DesignWetBulbTemperature = designWetBulb;
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
        /// Type of cooling tower.
        /// </summary>
        public CoolingTowerTypeEnum TowerType { get; set; }

        /// <summary>
        /// Current operating status.
        /// </summary>
        public CoolingTowerStatusEnum Status { get; private set; }

        //
        //  Design (Rated) Parameters
        //

        /// <summary>
        /// Rated heat rejection capacity in Watts.
        /// </summary>
        public double RatedCapacity { get; set; }

        /// <summary>
        /// Design wet-bulb temperature in °C.
        /// </summary>
        public double DesignWetBulbTemperature { get; set; }

        /// <summary>
        /// Design approach temperature (cold water temp − wet-bulb temp) in °C.
        /// </summary>
        public double DesignApproach { get; set; }

        /// <summary>
        /// Design range (hot water temp − cold water temp) in °C.
        /// </summary>
        public double DesignRange { get; set; }

        /// <summary>
        /// Design water flow rate in kg/s.
        /// </summary>
        public double DesignWaterFlowRate { get; set; }

        /// <summary>
        /// Number of fan cells.
        /// </summary>
        public int FanCellCount { get; set; } = 1;

        /// <summary>
        /// Rated fan power per cell in Watts.
        /// </summary>
        public double RatedFanPower { get; set; }

        //
        //  Ambient Conditions
        //

        /// <summary>
        /// Current ambient dry-bulb temperature in °C.
        /// </summary>
        public double AmbientDryBulbTemperature { get; set; }

        /// <summary>
        /// Current ambient wet-bulb temperature in °C.
        /// </summary>
        public double AmbientWetBulbTemperature { get; set; }

        /// <summary>
        /// Relative humidity as a fraction (0.0 to 1.0).
        /// </summary>
        public double RelativeHumidity { get; set; }

        /// <summary>
        /// Barometric pressure in Pascals (default sea level).
        /// </summary>
        public double BarometricPressure { get; set; } = 101325.0;

        //
        //  Water-Side Operating Data
        //

        /// <summary>
        /// Hot water inlet temperature (from the process) in °C.
        /// </summary>
        public double HotWaterTemperature { get; set; }

        /// <summary>
        /// Cold water outlet temperature (returned to the process) in °C.
        /// </summary>
        public double ColdWaterTemperature { get; set; }

        /// <summary>
        /// Water mass flow rate through the tower in kg/s.
        /// </summary>
        public double WaterMassFlowRate { get; set; }

        /// <summary>
        /// Water density in kg/m³.
        /// </summary>
        public double WaterDensity { get; set; }

        /// <summary>
        /// Specific heat of water in J/(kg·K).
        /// </summary>
        public double WaterSpecificHeat { get; set; }

        //
        //  Air-Side Operating Data
        //

        /// <summary>
        /// Air mass flow rate through the tower in kg/s.
        /// </summary>
        public double AirMassFlowRate { get; set; }

        /// <summary>
        /// Fan speed as a fraction (0.0 = off, 1.0 = full speed).
        /// </summary>
        public double FanSpeed
        {
            get => _fanSpeed;
            set => _fanSpeed = Math.Clamp(value, 0.0, 1.0);
        }

        //
        //  Water Treatment & Make-Up
        //

        /// <summary>
        /// Cycles of concentration (ratio of dissolved solids in circulating water to make-up water).
        /// </summary>
        public double CyclesOfConcentration { get; set; }

        /// <summary>
        /// Drift loss as a fraction of circulating water flow (typically 0.001–0.005).
        /// </summary>
        public double DriftLossFraction { get; set; } = 0.002;

        //
        //  Computed Properties
        //

        /// <summary>
        /// Cooling range: temperature difference between hot and cold water in °C.
        /// </summary>
        public double Range => HotWaterTemperature - ColdWaterTemperature;

        /// <summary>
        /// Approach: temperature difference between cold water and ambient wet-bulb in °C.
        /// Lower values indicate better tower performance.
        /// </summary>
        public double Approach => ColdWaterTemperature - AmbientWetBulbTemperature;

        /// <summary>
        /// Whether the cooling tower is currently running.
        /// </summary>
        public bool IsRunning => Status is CoolingTowerStatusEnum.Running or CoolingTowerStatusEnum.LowFlow;

        /// <summary>
        /// Liquid-to-gas ratio (L/G).
        /// </summary>
        public double LiquidToGasRatio => AirMassFlowRate > 0 ? WaterMassFlowRate / AirMassFlowRate : 0.0;

        /// <summary>
        /// Tower effectiveness: Range / (Range + Approach).
        /// </summary>
        public double Effectiveness
        {
            get
            {
                double total = Range + Approach;
                return total > 0 ? Range / total : 0.0;
            }
        }

        /// <summary>
        /// Current heat rejection as a fraction of rated capacity.
        /// </summary>
        public double LoadFraction => RatedCapacity > 0 ? CalculateHeatRejection() / RatedCapacity : 0.0;

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
        /// Starts the cooling tower at full fan speed.
        /// </summary>
        public void Start()
        {
            Start(1.0);
        }

        /// <summary>
        /// Starts the cooling tower at the specified fan speed.
        /// </summary>
        /// <param name="fanSpeed">Fan speed fraction (0.0 to 1.0).</param>
        public void Start(double fanSpeed)
        {
            if (Status == CoolingTowerStatusEnum.Faulted)
                return;

            FanSpeed = fanSpeed;
            UpdateStatus();
        }

        /// <summary>
        /// Stops the cooling tower.
        /// </summary>
        public void Stop()
        {
            if (Status == CoolingTowerStatusEnum.Faulted)
                return;

            FanSpeed = 0.0;
            Status = CoolingTowerStatusEnum.Off;
        }

        /// <summary>
        /// Sets the cooling tower to a faulted state and stops the fan.
        /// </summary>
        public void SetFault()
        {
            _fanSpeed = 0.0;
            Status = CoolingTowerStatusEnum.Faulted;
        }

        /// <summary>
        /// Clears a fault and resets to Off.
        /// </summary>
        public void ClearFault()
        {
            if (Status == CoolingTowerStatusEnum.Faulted)
                Status = CoolingTowerStatusEnum.Off;
        }

        //
        //  Heat Transfer Calculations
        //

        /// <summary>
        /// Calculates the heat rejected by the tower (Q = ṁ × Cp × Range) in Watts.
        /// </summary>
        public double CalculateHeatRejection()
        {
            return WaterMassFlowRate * WaterSpecificHeat * Range;
        }

        /// <summary>
        /// Calculates the required water flow rate for a target heat rejection in kg/s.
        /// </summary>
        /// <param name="targetHeatRejection">Target heat rejection in Watts.</param>
        public double CalculateRequiredWaterFlowRate(double targetHeatRejection)
        {
            double denominator = WaterSpecificHeat * Range;
            return denominator > 0 ? targetHeatRejection / denominator : 0.0;
        }

        /// <summary>
        /// Estimates the cold water temperature using a simple effectiveness model in °C.
        /// T_cold = T_wb + (1 − ε) × (T_hot − T_wb)
        /// </summary>
        /// <param name="effectiveness">Tower effectiveness (0.0 to 1.0).</param>
        public double EstimateColdWaterTemperature(double effectiveness)
        {
            return AmbientWetBulbTemperature
                + (1.0 - Math.Clamp(effectiveness, 0.0, 1.0))
                * (HotWaterTemperature - AmbientWetBulbTemperature);
        }

        //
        //  Water Loss Calculations
        //

        /// <summary>
        /// Calculates the evaporation rate in kg/s.
        /// Approximation: evaporation ≈ Q / (h_fg), simplified as ≈ 0.00153 × Q(kW).
        /// More precisely: ṁ_evap = Q / h_fg where h_fg ≈ 2,260,000 J/kg.
        /// </summary>
        public double CalculateEvaporationRate()
        {
            const double latentHeatOfVaporization = 2_260_000.0; // J/kg at ~100°C, approximate
            double heatRejection = CalculateHeatRejection();
            return heatRejection > 0 ? heatRejection / latentHeatOfVaporization : 0.0;
        }

        /// <summary>
        /// Calculates the drift loss rate in kg/s.
        /// </summary>
        public double CalculateDriftLoss()
        {
            return WaterMassFlowRate * DriftLossFraction;
        }

        /// <summary>
        /// Calculates the blowdown rate based on cycles of concentration in kg/s.
        /// Blowdown = Evaporation / (CoC − 1) − Drift
        /// </summary>
        public double CalculateBlowdownRate()
        {
            if (CyclesOfConcentration <= 1.0)
                return 0.0;

            double evaporation = CalculateEvaporationRate();
            double drift = CalculateDriftLoss();
            double blowdown = (evaporation / (CyclesOfConcentration - 1.0)) - drift;
            return Math.Max(blowdown, 0.0);
        }

        /// <summary>
        /// Calculates the total make-up water requirement in kg/s.
        /// Make-up = Evaporation + Drift + Blowdown
        /// </summary>
        public double CalculateMakeUpRate()
        {
            return CalculateEvaporationRate() + CalculateDriftLoss() + CalculateBlowdownRate();
        }

        //
        //  Performance Metrics
        //

        /// <summary>
        /// Calculates the Merkel number (KaV/L), a dimensionless measure of tower capability.
        /// Uses the Chebyshev four-point numerical integration method.
        /// </summary>
        public double CalculateMerkelNumber()
        {
            if (Range <= 0)
                return 0.0;

            // Chebyshev four-point integration: ∫ dT/(h_s − h_a)
            // Sample at 0.1, 0.4, 0.6, 0.9 of the range
            double t1 = ColdWaterTemperature + 0.1 * Range;
            double t2 = ColdWaterTemperature + 0.4 * Range;
            double t3 = ColdWaterTemperature + 0.6 * Range;
            double t4 = ColdWaterTemperature + 0.9 * Range;

            double h1 = EstimateSaturationEnthalpy(t1);
            double h2 = EstimateSaturationEnthalpy(t2);
            double h3 = EstimateSaturationEnthalpy(t3);
            double h4 = EstimateSaturationEnthalpy(t4);

            // Air enthalpy varies linearly from inlet to outlet
            double airEnthalpyIn = EstimateAirEnthalpy();
            double airEnthalpyOut = airEnthalpyIn + (WaterMassFlowRate * WaterSpecificHeat * Range)
                / (AirMassFlowRate > 0 ? AirMassFlowRate : 1.0);

            double ha1 = airEnthalpyIn + 0.1 * (airEnthalpyOut - airEnthalpyIn);
            double ha2 = airEnthalpyIn + 0.4 * (airEnthalpyOut - airEnthalpyIn);
            double ha3 = airEnthalpyIn + 0.6 * (airEnthalpyOut - airEnthalpyIn);
            double ha4 = airEnthalpyIn + 0.9 * (airEnthalpyOut - airEnthalpyIn);

            double d1 = h1 - ha1;
            double d2 = h2 - ha2;
            double d3 = h3 - ha3;
            double d4 = h4 - ha4;

            // Avoid division by zero
            if (d1 <= 0 || d2 <= 0 || d3 <= 0 || d4 <= 0)
                return 0.0;

            return Range / 4.0 * (1.0 / d1 + 1.0 / d2 + 1.0 / d3 + 1.0 / d4);
        }

        /// <summary>
        /// Calculates the fan power at the current speed using the fan affinity law (P ∝ N³) in Watts.
        /// </summary>
        public double CalculateFanPower()
        {
            return RatedFanPower * FanCellCount * Math.Pow(FanSpeed, 3);
        }

        /// <summary>
        /// Calculates the coefficient of performance (COP = Q_rejected / P_fan).
        /// </summary>
        public double CalculateCOP()
        {
            double fanPower = CalculateFanPower();
            return fanPower > 0 ? CalculateHeatRejection() / fanPower : 0.0;
        }

        /// <summary>
        /// Calculates the water-side volumetric flow rate in m³/s.
        /// </summary>
        public double CalculateVolumetricFlowRate()
        {
            return WaterDensity > 0 ? WaterMassFlowRate / WaterDensity : 0.0;
        }

        //
        //  Psychrometric Helpers
        //

        /// <summary>
        /// Estimates the enthalpy of saturated air at the given water temperature in J/kg.
        /// Uses a simplified curve fit: h_s ≈ 1006T + W_s × 2,501,000
        /// where W_s ≈ 0.622 × P_s / (P_atm − P_s).
        /// </summary>
        /// <param name="waterTemperature">Water temperature in °C.</param>
        private double EstimateSaturationEnthalpy(double waterTemperature)
        {
            double satPressure = EstimateSaturationPressure(waterTemperature);
            double denominator = BarometricPressure - satPressure;

            if (denominator <= 0)
                return 0.0;

            double humidityRatio = 0.622 * satPressure / denominator;
            return 1006.0 * waterTemperature + humidityRatio * 2_501_000.0;
        }

        /// <summary>
        /// Estimates the enthalpy of the inlet air at ambient conditions in J/kg.
        /// </summary>
        private double EstimateAirEnthalpy()
        {
            double satPressure = EstimateSaturationPressure(AmbientWetBulbTemperature);
            double denominator = BarometricPressure - satPressure;

            if (denominator <= 0)
                return 0.0;

            // At wet-bulb, air is saturated
            double humidityRatio = 0.622 * satPressure / denominator;
            return 1006.0 * AmbientWetBulbTemperature + humidityRatio * 2_501_000.0;
        }

        /// <summary>
        /// Estimates saturation vapor pressure using the Antoine equation approximation in Pascals.
        /// </summary>
        /// <param name="temperature">Temperature in °C.</param>
        private static double EstimateSaturationPressure(double temperature)
        {
            // Buck equation: P_s = 611.21 × exp((18.678 − T/234.5) × (T/(257.14 + T)))
            return 611.21 * Math.Exp((18.678 - temperature / 234.5)
                * (temperature / (257.14 + temperature)));
        }

        //
        //  Status
        //

        /// <summary>
        /// Updates the tower status based on current operating conditions.
        /// </summary>
        private void UpdateStatus()
        {
            if (Status == CoolingTowerStatusEnum.Faulted)
                return;

            if (FanSpeed <= 0)
            {
                Status = CoolingTowerStatusEnum.Off;
            }
            else if (WaterMassFlowRate <= 0 || DesignWaterFlowRate > 0
                && WaterMassFlowRate < DesignWaterFlowRate * 0.5)
            {
                Status = CoolingTowerStatusEnum.LowFlow;
            }
            else
            {
                Status = CoolingTowerStatusEnum.Running;
            }
        }

        public override string ToString() =>
            $"CoolingTower [{TowerType}, {Status}, Fan={FanSpeed * 100:F0}%, " +
            $"Range={Range:F1}°C, Approach={Approach:F1}°C, " +
            $"Q={CalculateHeatRejection() / 1000:F1} kW]";

        #endregion
        //  *****************************************************************************************
    }
}
