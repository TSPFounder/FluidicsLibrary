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
    public class Pump : SE_System
    {
        //  *****************************************************************************************
        //  DECLARATIONS
        //
        //  ************************************************************
        #region
        //  
        //  Identification

        //
        //  Data
        private double _currentSpeed;
        //
        //  Owned & Owning Objects
        /// <summary>
        /// Head Table
        /// </summary>
        public SE_Table HeadTable { get; set; }
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
        public enum PumpTypeEnum
        {
            Centrifugal = 0,
            Diaphragm,
            Gear,
            Peristaltic,
            Piston,
            Vane,
            Other
        }

        public enum NetworkTypeEnum
        {
            Liquid = 0,
            Gas,
            TwoPhase,
            Other
        }

        public enum PumpStatusEnum
        {
            Off = 0,
            Running,
            Cavitating,
            Faulted
        }
        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  PUMP CONSTRUCTOR
        //
        //  ************************************************************
        #region
        public Pump()
        {
            Status = PumpStatusEnum.Off;
            Efficiency = 1.0;
        }

        public Pump(PumpTypeEnum pumpType, double ratedFlowRate, double ratedHead, double ratedSpeed)
            : this()
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ratedFlowRate);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ratedHead);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ratedSpeed);

            PumpType = pumpType;
            RatedFlowRate = ratedFlowRate;
            RatedHead = ratedHead;
            RatedSpeed = ratedSpeed;
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

        /// <summary>
        /// Type of pump (centrifugal, gear, piston, etc.).
        /// </summary>
        public PumpTypeEnum PumpType { get; set; }

        /// <summary>
        /// Type of fluid network the pump operates in.
        /// </summary>
        public NetworkTypeEnum NetworkType { get; set; }

        /// <summary>
        /// Current operating status of the pump.
        /// </summary>
        public PumpStatusEnum Status { get; private set; }

        //  
        //  Data — Rated (Nameplate) Values
        //

        /// <summary>
        /// Rated (design) flow rate in cubic meters per second.
        /// </summary>
        public double RatedFlowRate { get; set; }

        /// <summary>
        /// Rated (design) head in meters.
        /// </summary>
        public double RatedHead { get; set; }

        /// <summary>
        /// Rated (design) speed in revolutions per minute.
        /// </summary>
        public double RatedSpeed { get; set; }

        /// <summary>
        /// Rated (design) power in Watts.
        /// </summary>
        public double RatedPower { get; set; }

        //
        //  Data — Operating Values
        //

        /// <summary>
        /// Current operating speed in revolutions per minute.
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
        /// Pump efficiency as a fraction (0.0 to 1.0).
        /// </summary>
        public double Efficiency { get; set; }

        /// <summary>
        /// Net Positive Suction Head available in meters.
        /// </summary>
        public double NPSHAvailable { get; set; }

        /// <summary>
        /// Net Positive Suction Head required in meters.
        /// </summary>
        public double NPSHRequired { get; set; }

        /// <summary>
        /// Fluid density in kg/m³ (default water at ~20°C).
        /// </summary>
        public double FluidDensity { get; set; } = 998.0;

        //
        //  Data — Computed Values
        //

        /// <summary>
        /// Differential pressure across the pump in Pascals.
        /// </summary>
        public double DifferentialPressure => OutletPressure - InletPressure;

        /// <summary>
        /// Whether the pump is currently running.
        /// </summary>
        public bool IsRunning => Status == PumpStatusEnum.Running || Status == PumpStatusEnum.Cavitating;

        /// <summary>
        /// Whether cavitation conditions exist (NPSHa &lt; NPSHr).
        /// </summary>
        public bool IsCavitating => NPSHAvailable < NPSHRequired;

        /// <summary>
        /// Speed ratio relative to rated speed (0.0 to 1.0+).
        /// </summary>
        public double SpeedRatio => RatedSpeed > 0 ? CurrentSpeed / RatedSpeed : 0.0;

        #endregion
        //  *****************************************************************************************


        //  *****************************************************************************************
        //  METHODS
        //
        //  ************************************************************
        #region

        /// <summary>
        /// Starts the pump at the rated speed.
        /// </summary>
        public void Start()
        {
            Start(RatedSpeed);
        }

        /// <summary>
        /// Starts the pump at the specified speed.
        /// </summary>
        /// <param name="speed">Target speed in RPM.</param>
        public void Start(double speed)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(speed);
            CurrentSpeed = speed;
            Status = IsCavitating ? PumpStatusEnum.Cavitating : PumpStatusEnum.Running;
        }

        /// <summary>
        /// Stops the pump.
        /// </summary>
        public void Stop()
        {
            CurrentSpeed = 0.0;
            Status = PumpStatusEnum.Off;
        }

        /// <summary>
        /// Sets the pump to a faulted state and stops rotation.
        /// </summary>
        public void SetFault()
        {
            CurrentSpeed = 0.0;
            Status = PumpStatusEnum.Faulted;
        }

        /// <summary>
        /// Clears a fault condition and resets the pump to Off.
        /// </summary>
        public void ClearFault()
        {
            if (Status == PumpStatusEnum.Faulted)
                Status = PumpStatusEnum.Off;
        }

        /// <summary>
        /// Calculates the current flow rate using affinity laws (Q ∝ N).
        /// </summary>
        /// <returns>Estimated flow rate in cubic meters per second.</returns>
        public double CalculateFlowRate()
        {
            return RatedFlowRate * SpeedRatio;
        }

        /// <summary>
        /// Calculates the current head using affinity laws (H ∝ N²).
        /// </summary>
        /// <returns>Estimated head in meters.</returns>
        public double CalculateHead()
        {
            return RatedHead * Math.Pow(SpeedRatio, 2);
        }

        /// <summary>
        /// Calculates the current hydraulic power output (P_h = ρgQH) in Watts.
        /// </summary>
        public double CalculateHydraulicPower()
        {
            const double gravity = 9.80665;
            return FluidDensity * gravity * CalculateFlowRate() * CalculateHead();
        }

        /// <summary>
        /// Calculates the shaft (input) power required based on efficiency in Watts.
        /// </summary>
        public double CalculateShaftPower()
        {
            return Efficiency > 0 ? CalculateHydraulicPower() / Efficiency : 0.0;
        }

        /// <summary>
        /// Calculates the power consumed using affinity laws (P ∝ N³) in Watts.
        /// </summary>
        public double CalculatePowerByAffinityLaw()
        {
            return RatedPower * Math.Pow(SpeedRatio, 3);
        }

        /// <summary>
        /// Converts head in meters to pressure in Pascals using the current fluid density.
        /// </summary>
        /// <param name="head">Head in meters.</param>
        public double HeadToPressure(double head)
        {
            const double gravity = 9.80665;
            return FluidDensity * gravity * head;
        }

        /// <summary>
        /// Converts pressure in Pascals to head in meters using the current fluid density.
        /// </summary>
        /// <param name="pressure">Pressure in Pascals.</param>
        public double PressureToHead(double pressure)
        {
            const double gravity = 9.80665;
            double denominator = FluidDensity * gravity;
            return denominator > 0 ? pressure / denominator : 0.0;
        }

        /// <summary>
        /// Estimates the operating point at a new speed using the affinity laws.
        /// </summary>
        /// <param name="newSpeed">New speed in RPM.</param>
        /// <returns>Tuple of (FlowRate, Head, Power) at the new speed.</returns>
        public (double FlowRate, double Head, double Power) EstimateAtSpeed(double newSpeed)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(newSpeed);

            double ratio = RatedSpeed > 0 ? newSpeed / RatedSpeed : 0.0;
            double flow = RatedFlowRate * ratio;
            double head = RatedHead * Math.Pow(ratio, 2);
            double power = RatedPower * Math.Pow(ratio, 3);

            return (flow, head, power);
        }

        /// <summary>
        /// Calculates the specific speed of the pump (a dimensionless type number).
        /// Ns = N√Q / H^(3/4)
        /// </summary>
        public double CalculateSpecificSpeed()
        {
            if (RatedHead <= 0 || RatedFlowRate <= 0)
                return 0.0;

            return RatedSpeed * Math.Sqrt(RatedFlowRate) / Math.Pow(RatedHead, 0.75);
        }

        /// <summary>
        /// Updates the pump status based on current NPSH conditions.
        /// </summary>
        public void UpdateCavitationStatus()
        {
            if (!IsRunning)
                return;

            Status = IsCavitating ? PumpStatusEnum.Cavitating : PumpStatusEnum.Running;
        }

        public override string ToString() =>
            $"Pump [{PumpType}, {Status}, {CurrentSpeed:F0} RPM, Q={CalculateFlowRate():E2} m³/s, H={CalculateHead():F2} m]";

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
