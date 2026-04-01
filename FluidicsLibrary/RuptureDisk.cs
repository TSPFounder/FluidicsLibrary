using System;
using System.Collections.Generic;
using System.Text;

namespace FluidicsLibrary
{
    /// <summary>
    /// Represents a rupture disk (burst disk) — a non-reclosing pressure relief device
    /// that bursts at a predetermined differential pressure to protect equipment from overpressure.
    /// </summary>
    internal class RuptureDisk
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

        public enum DiskTypeEnum
        {
            ForwardActingTensionLoaded = 0,
            ForwardActingPreBulged,
            ReverseActingKnifeBlades,
            ReverseActingScored,
            GraphiteMonolithic,
            CompositeAssembly,
            Other
        }

        public enum DiskMaterialEnum
        {
            StainlessSteel316 = 0,
            StainlessSteel304,
            Inconel,
            Monel,
            Nickel,
            Hastelloy,
            Tantalum,
            Graphite,
            PTFE,
            Aluminum,
            Copper,
            Other
        }

        public enum DiskStatusEnum
        {
            Intact = 0,
            Ruptured,
            Leaking,
            Expired,
            Faulted
        }

        public enum HolderTypeEnum
        {
            Insert = 0,
            Union,
            ScrewType,
            BoltedFlange,
            Sanitary,
            Other
        }

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  CONSTRUCTORS
        //
        //  ************************************************************
        #region

        public RuptureDisk()
        {
            Status = DiskStatusEnum.Intact;
            ManufacturingRange = 0.05;
            TemperatureDerateFactor = 1.0;
            BackPressureDerateFactor = 1.0;
        }

        /// <summary>
        /// Creates a rupture disk with the specified burst pressure and nominal size.
        /// </summary>
        /// <param name="burstPressure">Marked (stamped) burst pressure in Pascals.</param>
        /// <param name="nominalDiameter">Nominal disk diameter in meters.</param>
        public RuptureDisk(double burstPressure, double nominalDiameter)
            : this()
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(burstPressure);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(nominalDiameter);

            MarkedBurstPressure = burstPressure;
            NominalDiameter = nominalDiameter;
        }

        /// <summary>
        /// Creates a rupture disk with full design parameters.
        /// </summary>
        /// <param name="burstPressure">Marked (stamped) burst pressure in Pascals.</param>
        /// <param name="nominalDiameter">Nominal disk diameter in meters.</param>
        /// <param name="diskType">Type of rupture disk.</param>
        /// <param name="material">Disk material.</param>
        public RuptureDisk(double burstPressure, double nominalDiameter,
            DiskTypeEnum diskType, DiskMaterialEnum material)
            : this(burstPressure, nominalDiameter)
        {
            DiskType = diskType;
            Material = material;
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
        /// Type of rupture disk.
        /// </summary>
        public DiskTypeEnum DiskType { get; set; }

        /// <summary>
        /// Disk material.
        /// </summary>
        public DiskMaterialEnum Material { get; set; }

        /// <summary>
        /// Type of disk holder.
        /// </summary>
        public HolderTypeEnum HolderType { get; set; }

        /// <summary>
        /// Current status of the rupture disk.
        /// </summary>
        public DiskStatusEnum Status { get; private set; }

        /// <summary>
        /// Unique tag or identifier for the disk.
        /// </summary>
        public string Tag { get; set; } = string.Empty;

        /// <summary>
        /// Manufacturer lot/batch number.
        /// </summary>
        public string LotNumber { get; set; } = string.Empty;

        //
        //  Geometry
        //

        /// <summary>
        /// Nominal disk diameter in meters.
        /// </summary>
        public double NominalDiameter { get; set; }

        /// <summary>
        /// Minimum net flow area after burst in m².
        /// Typically less than the full bore area due to petals/fragments.
        /// </summary>
        public double MinNetFlowArea { get; set; }

        /// <summary>
        /// Full bore cross-sectional area in m².
        /// </summary>
        public double BoreArea => Math.PI * Math.Pow(NominalDiameter / 2.0, 2);

        /// <summary>
        /// Ratio of net flow area to bore area. Typically 0.6–0.9.
        /// </summary>
        public double AreaRatio => BoreArea > 0 && MinNetFlowArea > 0
            ? MinNetFlowArea / BoreArea
            : 0.0;

        //
        //  Burst Pressure
        //

        /// <summary>
        /// Marked (stamped) burst pressure at the specified disk temperature in Pascals.
        /// This is the manufacturer's certified burst pressure.
        /// </summary>
        public double MarkedBurstPressure { get; set; }

        /// <summary>
        /// Manufacturing range as a ± fraction of the marked burst pressure (e.g., 0.05 = ±5%).
        /// Per ASME, typically ±2% for >2" and ±5% for ≤2".
        /// </summary>
        public double ManufacturingRange { get; set; }

        /// <summary>
        /// Specified disk temperature at which the burst pressure is rated in °C.
        /// </summary>
        public double SpecifiedDiskTemperature { get; set; }

        /// <summary>
        /// Temperature derate factor (ratio of burst pressure at operating temp to burst at rated temp).
        /// 1.0 at rated temperature, decreases at higher temperatures.
        /// </summary>
        public double TemperatureDerateFactor { get; set; }

        /// <summary>
        /// Back pressure derate factor for reverse-acting disks.
        /// </summary>
        public double BackPressureDerateFactor { get; set; }

        //
        //  Operating Conditions
        //

        /// <summary>
        /// Current operating pressure upstream of the disk in Pascals.
        /// </summary>
        public double OperatingPressure { get; set; }

        /// <summary>
        /// Current back pressure (downstream) in Pascals.
        /// </summary>
        public double BackPressure { get; set; }

        /// <summary>
        /// Current disk temperature in °C.
        /// </summary>
        public double DiskTemperature { get; set; }

        /// <summary>
        /// Maximum Allowable Working Pressure of the protected equipment in Pascals.
        /// </summary>
        public double MAWP { get; set; }

        //
        //  Service Life
        //

        /// <summary>
        /// Installation date of the rupture disk.
        /// </summary>
        public DateTime? InstallDate { get; set; }

        /// <summary>
        /// Recommended replacement interval.
        /// </summary>
        public TimeSpan? RecommendedServiceLife { get; set; }

        /// <summary>
        /// Number of pressure cycles experienced (fatigue tracking).
        /// </summary>
        public long CycleCount { get; set; }

        /// <summary>
        /// Maximum recommended pressure cycles before replacement.
        /// </summary>
        public long MaxCycleCount { get; set; }

        //
        //  Flow Properties (Post-Burst)
        //

        /// <summary>
        /// Certified KR resistance coefficient for the disk (ASME).
        /// Used to calculate flow capacity after burst.
        /// </summary>
        public double ResistanceCoefficient { get; set; } = 2.4;

        /// <summary>
        /// ASME capacity discharge coefficient (Kd) when used in combination with a relief valve.
        /// </summary>
        public double DischargeCoefficientCombination { get; set; } = 0.9;

        /// <summary>
        /// Fluid density for flow calculations in kg/m³.
        /// </summary>
        public double FluidDensity { get; set; } = 998.0;

        /// <summary>
        /// Specific heat ratio (γ) for gas service.
        /// </summary>
        public double SpecificHeatRatio { get; set; } = 1.4;

        /// <summary>
        /// Molecular weight of the gas in kg/kmol.
        /// </summary>
        public double MolecularWeight { get; set; } = 29.0;

        //
        //  Computed Properties
        //

        /// <summary>
        /// Differential pressure across the disk (upstream − downstream) in Pascals.
        /// </summary>
        public double DifferentialPressure => OperatingPressure - BackPressure;

        /// <summary>
        /// Maximum burst pressure (marked + manufacturing range) in Pascals.
        /// </summary>
        public double MaxBurstPressure => MarkedBurstPressure * (1.0 + ManufacturingRange);

        /// <summary>
        /// Minimum burst pressure (marked − manufacturing range) in Pascals.
        /// </summary>
        public double MinBurstPressure => MarkedBurstPressure * (1.0 - ManufacturingRange);

        /// <summary>
        /// Derated burst pressure at the current operating temperature in Pascals.
        /// </summary>
        public double DeratedBurstPressure =>
            MarkedBurstPressure * TemperatureDerateFactor * BackPressureDerateFactor;

        /// <summary>
        /// Operating ratio (operating pressure / marked burst pressure).
        /// Should typically be ≤ 0.90 for long service life.
        /// </summary>
        public double OperatingRatio =>
            MarkedBurstPressure > 0 ? OperatingPressure / MarkedBurstPressure : 0.0;

        /// <summary>
        /// Whether the disk is intact and functional.
        /// </summary>
        public bool IsIntact => Status == DiskStatusEnum.Intact;

        /// <summary>
        /// Whether the disk has ruptured.
        /// </summary>
        public bool IsRuptured => Status == DiskStatusEnum.Ruptured;

        /// <summary>
        /// Whether the operating ratio exceeds the recommended maximum (typically 0.90).
        /// </summary>
        public bool IsHighOperatingRatio => OperatingRatio > 0.90;

        /// <summary>
        /// Whether the disk has exceeded its recommended service life.
        /// </summary>
        public bool IsExpired
        {
            get
            {
                if (InstallDate.HasValue && RecommendedServiceLife.HasValue)
                    return DateTime.UtcNow - InstallDate.Value >= RecommendedServiceLife.Value;

                if (MaxCycleCount > 0 && CycleCount >= MaxCycleCount)
                    return true;

                return false;
            }
        }

        /// <summary>
        /// Remaining service life, or null if not tracked.
        /// </summary>
        public TimeSpan? RemainingServiceLife
        {
            get
            {
                if (!InstallDate.HasValue || !RecommendedServiceLife.HasValue)
                    return null;

                var remaining = (InstallDate.Value + RecommendedServiceLife.Value) - DateTime.UtcNow;
                return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
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
        //  Burst & Status
        //

        /// <summary>
        /// Evaluates whether the current differential pressure causes the disk to rupture.
        /// If the differential exceeds the derated burst pressure, the disk status changes to Ruptured.
        /// </summary>
        /// <returns>True if the disk ruptured during this evaluation.</returns>
        public bool EvaluateBurst()
        {
            if (Status != DiskStatusEnum.Intact)
                return false;

            if (DifferentialPressure >= DeratedBurstPressure)
            {
                Status = DiskStatusEnum.Ruptured;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Records a pressure cycle for fatigue tracking and checks expiration.
        /// </summary>
        public void RecordCycle()
        {
            if (Status != DiskStatusEnum.Intact)
                return;

            CycleCount++;

            if (IsExpired)
                Status = DiskStatusEnum.Expired;
        }

        /// <summary>
        /// Marks the disk as leaking (premature failure, pinhole, etc.).
        /// </summary>
        public void SetLeaking()
        {
            if (Status == DiskStatusEnum.Intact)
                Status = DiskStatusEnum.Leaking;
        }

        /// <summary>
        /// Sets the disk to a faulted state (e.g., holder damage, improper installation detected).
        /// </summary>
        public void SetFault()
        {
            Status = DiskStatusEnum.Faulted;
        }

        /// <summary>
        /// Replaces the rupture disk, resetting to intact with a new install date.
        /// </summary>
        public void Replace()
        {
            Status = DiskStatusEnum.Intact;
            CycleCount = 0;
            InstallDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the expiration status based on current service life and cycle count.
        /// </summary>
        public void UpdateExpirationStatus()
        {
            if (Status == DiskStatusEnum.Intact && IsExpired)
                Status = DiskStatusEnum.Expired;
        }

        //
        //  Burst Pressure Calculations
        //

        /// <summary>
        /// Calculates the burst pressure at a given temperature using a linear temperature derate.
        /// P_burst(T) = P_marked × (T_derate_factor)
        /// </summary>
        /// <param name="temperatureDerateFactor">Temperature derate factor at the target temperature.</param>
        public double CalculateBurstPressureAtTemperature(double temperatureDerateFactor)
        {
            return MarkedBurstPressure * Math.Clamp(temperatureDerateFactor, 0.0, 1.5);
        }

        /// <summary>
        /// Calculates the maximum allowable operating pressure for a given operating ratio.
        /// P_operating_max = P_burst × operating_ratio
        /// </summary>
        /// <param name="targetOperatingRatio">Target operating ratio (typically 0.80–0.90).</param>
        public double CalculateMaxOperatingPressure(double targetOperatingRatio = 0.90)
        {
            return DeratedBurstPressure * Math.Clamp(targetOperatingRatio, 0.0, 1.0);
        }

        /// <summary>
        /// Validates that the burst pressure is properly set relative to the MAWP.
        /// Per ASME: burst pressure ≤ MAWP, and max burst (with mfg range) should not exceed 
        /// the relieving pressure of the system.
        /// </summary>
        public bool ValidateBurstPressureVsMAWP()
        {
            if (MAWP <= 0)
                return false;

            // Marked burst pressure should not exceed MAWP
            return MarkedBurstPressure <= MAWP;
        }

        /// <summary>
        /// Validates the manufacturing range per ASME requirements.
        /// Max burst must not exceed MAWP for sole relief devices.
        /// </summary>
        public bool ValidateManufacturingRange()
        {
            if (MAWP <= 0)
                return false;

            return MaxBurstPressure <= MAWP;
        }

        //
        //  Flow Capacity (Post-Burst)
        //

        /// <summary>
        /// Calculates the liquid volumetric flow capacity after burst in m³/s.
        /// Uses the resistance coefficient method: Q = A × √(2ΔP / (KR × ρ))
        /// </summary>
        public double CalculateLiquidFlowCapacity()
        {
            if (!IsRuptured || ResistanceCoefficient <= 0 || FluidDensity <= 0)
                return 0.0;

            double dp = DifferentialPressure;
            if (dp <= 0)
                return 0.0;

            double area = MinNetFlowArea > 0 ? MinNetFlowArea : BoreArea;
            return area * Math.Sqrt(2.0 * dp / (ResistanceCoefficient * FluidDensity));
        }

        /// <summary>
        /// Calculates the liquid mass flow rate after burst in kg/s.
        /// </summary>
        public double CalculateLiquidMassFlowRate()
        {
            return CalculateLiquidFlowCapacity() * FluidDensity;
        }

        /// <summary>
        /// Calculates the gas mass flow rate after burst (subsonic flow) in kg/s.
        /// Uses the resistance coefficient method with compressibility.
        /// </summary>
        /// <param name="upstreamTemperature">Upstream gas temperature in Kelvin.</param>
        public double CalculateGasMassFlowRate(double upstreamTemperature)
        {
            if (!IsRuptured || ResistanceCoefficient <= 0
                || OperatingPressure <= 0 || upstreamTemperature <= 0)
                return 0.0;

            double area = MinNetFlowArea > 0 ? MinNetFlowArea : BoreArea;
            double gamma = SpecificHeatRatio;
            double mw = MolecularWeight;

            // Universal gas constant
            const double Ru = 8314.0; // J/(kmol·K)
            double R = Ru / mw;

            double pressureRatio = BackPressure / OperatingPressure;
            if (pressureRatio >= 1.0)
                return 0.0;

            // Check for critical (choked) flow
            double criticalRatio = Math.Pow(2.0 / (gamma + 1.0), gamma / (gamma - 1.0));
            double effectiveRatio = Math.Max(pressureRatio, criticalRatio);

            double term1 = 2.0 * gamma / (gamma - 1.0);
            double term2 = Math.Pow(effectiveRatio, 2.0 / gamma)
                - Math.Pow(effectiveRatio, (gamma + 1.0) / gamma);

            double density = OperatingPressure / (R * upstreamTemperature);
            return area * Math.Sqrt(density * OperatingPressure * term1 * term2 / ResistanceCoefficient);
        }

        /// <summary>
        /// Calculates the velocity through the disk opening after burst in m/s.
        /// </summary>
        public double CalculatePostBurstVelocity()
        {
            double flowRate = CalculateLiquidFlowCapacity();
            double area = MinNetFlowArea > 0 ? MinNetFlowArea : BoreArea;
            return area > 0 ? flowRate / area : 0.0;
        }

        //
        //  Sizing Helpers
        //

        /// <summary>
        /// Calculates the required minimum disk diameter for a given liquid relief flow rate in meters.
        /// </summary>
        /// <param name="requiredFlowRate">Required relief flow rate in m³/s.</param>
        /// <param name="availableDifferentialPressure">Available ΔP across the disk in Pascals.</param>
        public double CalculateRequiredDiameter(double requiredFlowRate, double availableDifferentialPressure)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(requiredFlowRate);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(availableDifferentialPressure);

            if (ResistanceCoefficient <= 0 || FluidDensity <= 0)
                return 0.0;

            // A = Q / √(2ΔP / (KR × ρ))
            double flowTerm = Math.Sqrt(2.0 * availableDifferentialPressure
                / (ResistanceCoefficient * FluidDensity));

            if (flowTerm <= 0)
                return 0.0;

            double requiredArea = requiredFlowRate / flowTerm;

            // D = 2 × √(A / π)
            return 2.0 * Math.Sqrt(requiredArea / Math.PI);
        }

        /// <summary>
        /// Calculates the equivalent Kv (flow coefficient) of the ruptured disk in m³/h.
        /// Kv = Q × √(SG / ΔP_bar)  where Q in m³/h and ΔP in bar.
        /// </summary>
        public double CalculateEquivalentKv()
        {
            double flowRate = CalculateLiquidFlowCapacity();
            double dpBar = DifferentialPressure / 100000.0;
            double sg = FluidDensity / 998.0;

            if (dpBar <= 0)
                return 0.0;

            // Convert m³/s to m³/h
            return flowRate * 3600.0 * Math.Sqrt(sg / dpBar);
        }

        public override string ToString() =>
            $"RuptureDisk [{Tag}, {DiskType}, {Material}, {Status}, " +
            $"Ø={NominalDiameter * 1000:F1}mm, P_burst={MarkedBurstPressure / 1000:F1} kPa, " +
            $"OR={OperatingRatio * 100:F1}%]";

        #endregion
        //  *****************************************************************************************
    }
}
