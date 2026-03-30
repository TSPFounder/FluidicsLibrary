using System;
using System.Collections.Generic;
using System.Text;
using SE_Library;
using Mathematics;
using CAD;

namespace FluidicsLibrary
{
    /// <summary>
    /// Represents a hydraulic/pneumatic piston in a fluidics system.
    /// </summary>
    internal class Piston
    {
        private double _currentPosition;

        /// <summary>
        /// Diameter of the piston bore in meters.
        /// </summary>
        public double Diameter { get; }

        /// <summary>
        /// Maximum stroke length in meters.
        /// </summary>
        public double StrokeLength { get; }

        /// <summary>
        /// Rod diameter in meters (for differential area calculations).
        /// </summary>
        public double RodDiameter { get; }

        /// <summary>
        /// Current position along the stroke in meters (0 = fully retracted).
        /// </summary>
        public double CurrentPosition
        {
            get => _currentPosition;
            private set => _currentPosition = Math.Clamp(value, 0.0, StrokeLength);
        }

        /// <summary>
        /// Current operating pressure in Pascals.
        /// </summary>
        public double Pressure { get; set; }       

        /// <summary>
        /// Cross-sectional area of the piston bore in square meters.
        /// </summary>
        public double BoreArea => Math.PI * Math.Pow(Diameter / 2.0, 2);

        /// <summary>
        /// Annular area on the rod side in square meters.
        /// </summary>
        public double RodSideArea => BoreArea - Math.PI * Math.Pow(RodDiameter / 2.0, 2);

        /// <summary>
        /// Volume displaced at the current position in cubic meters.
        /// </summary>
        public double DisplacedVolume => BoreArea * CurrentPosition;

        /// <summary>
        /// Maximum volume the piston can displace in cubic meters.
        /// </summary>
        public double MaxVolume => BoreArea * StrokeLength;

        /// <summary>
        /// Whether the piston is fully retracted (at bottom dead center).
        /// </summary>
        public bool IsFullyRetracted => CurrentPosition <= 0.0;

        /// <summary>
        /// Whether the piston is fully extended (at top dead center).
        /// </summary>
        public bool IsFullyExtended => CurrentPosition >= StrokeLength;

        /// <summary>
        /// Current stroke percentage (0–100).
        /// </summary>
        public double StrokePercentage => StrokeLength > 0 ? (CurrentPosition / StrokeLength) * 100.0 : 0.0;

       

        //  *****************************************************************************************
        //  ENUMERATIONS
        //
        //  ************************************************************
        #region        
        public enum NetworkTypeEnum
        {
            Liquid = 0,
            Gas,
            TwoPhase,
            Other
        }
        #endregion

        /// <summary>
        /// Initializes a new piston with the specified geometry.
        /// </summary>
        /// <param name="diameter">Bore diameter in meters.</param>
        /// <param name="strokeLength">Maximum stroke length in meters.</param>
        /// <param name="rodDiameter">Rod diameter in meters.</param>
        public Piston(double diameter, double strokeLength, double rodDiameter = 0.0)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(diameter);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(strokeLength);
            ArgumentOutOfRangeException.ThrowIfNegative(rodDiameter);

            if (rodDiameter >= diameter)
                throw new ArgumentOutOfRangeException(nameof(rodDiameter), "Rod diameter must be less than bore diameter.");

            Diameter = diameter;
            StrokeLength = strokeLength;
            RodDiameter = rodDiameter;
        }

        /// <summary>
        /// Calculates the force produced on the bore side at the current pressure (F = P × A).
        /// </summary>
        public double CalculateExtendForce() => Pressure * BoreArea;

        /// <summary>
        /// Calculates the force produced on the rod side at the current pressure.
        /// </summary>
        public double CalculateRetractForce() => Pressure * RodSideArea;

        /// <summary>
        /// Moves the piston by the specified distance in meters. Positive values extend; negative values retract.
        /// </summary>
        /// <returns>The actual distance moved (clamped to stroke limits).</returns>
        public double Move(double distance)
        {
            double previousPosition = CurrentPosition;
            CurrentPosition += distance;
            return CurrentPosition - previousPosition;
        }

        /// <summary>
        /// Extends the piston by the specified distance in meters.
        /// </summary>
        public double Extend(double distance)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(distance);
            return Move(distance);
        }

        /// <summary>
        /// Retracts the piston by the specified distance in meters.
        /// </summary>
        public double Retract(double distance)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(distance);
            return Move(-distance);
        }

        /// <summary>
        /// Moves the piston to a specific position along the stroke.
        /// </summary>
        public void MoveTo(double position)
        {
            CurrentPosition = position;
        }

        /// <summary>
        /// Fully extends the piston to the end of its stroke.
        /// </summary>
        public void FullyExtend() => CurrentPosition = StrokeLength;

        /// <summary>
        /// Fully retracts the piston to the start of its stroke.
        /// </summary>
        public void FullyRetract() => CurrentPosition = 0.0;

        /// <summary>
        /// Resets the piston to its initial state (fully retracted, zero pressure).
        /// </summary>
        public void Reset()
        {
            CurrentPosition = 0.0;
            Pressure = 0.0;
        }

        /// <summary>
        /// Calculates the flow rate needed to achieve the target velocity (Q = A × v).
        /// </summary>
        /// <param name="velocity">Target velocity in meters per second.</param>
        /// <returns>Required flow rate in cubic meters per second.</returns>
        public double CalculateRequiredFlowRate(double velocity) => BoreArea * Math.Abs(velocity);

        /// <summary>
        /// Calculates the velocity given a flow rate (v = Q / A).
        /// </summary>
        /// <param name="flowRate">Flow rate in cubic meters per second.</param>
        /// <returns>Piston velocity in meters per second.</returns>
        public double CalculateVelocity(double flowRate) => BoreArea > 0 ? flowRate / BoreArea : 0.0;

        public override string ToString() =>
            $"Piston [Ø{Diameter * 1000:F1}mm, Stroke={StrokeLength * 1000:F1}mm, Position={StrokePercentage:F1}%]";
    }
}
