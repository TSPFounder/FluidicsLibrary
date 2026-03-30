using System;
using System.Collections.Generic;
using System.Text;

namespace FluidicsLibrary
{
    /// <summary>
    /// Represents a piston rod that transmits force from the piston to the external load.
    /// </summary>
    internal class PistonRod
    {
        /// <summary>
        /// Diameter of the rod in meters.
        /// </summary>
        public double Diameter { get; }

        /// <summary>
        /// Total length of the rod in meters.
        /// </summary>
        public double Length { get; }

        /// <summary>
        /// Material of the rod (e.g., stainless steel, chrome-plated steel).
        /// </summary>
        public string Material { get; }

        /// <summary>
        /// Young's modulus (modulus of elasticity) of the rod material in Pascals.
        /// </summary>
        public double ElasticModulus { get; }

        /// <summary>
        /// Yield strength of the rod material in Pascals.
        /// </summary>
        public double YieldStrength { get; }

        /// <summary>
        /// Cross-sectional area of the rod in square meters.
        /// </summary>
        public double CrossSectionalArea => Math.PI * Math.Pow(Diameter / 2.0, 2);

        /// <summary>
        /// Second moment of area (moment of inertia) for the circular cross-section in m⁴.
        /// </summary>
        public double MomentOfInertia => Math.PI * Math.Pow(Diameter, 4) / 64.0;

        /// <summary>
        /// Current axial load applied to the rod in Newtons. Positive = tension, negative = compression.
        /// </summary>
        public double AxialLoad { get; set; }

        /// <summary>
        /// Current extension distance beyond the cylinder in meters.
        /// </summary>
        public double Extension { get; private set; }

        /// <summary>
        /// Whether the rod is currently under compression.
        /// </summary>
        public bool IsInCompression => AxialLoad < 0;

        /// <summary>
        /// Whether the rod is currently under tension.
        /// </summary>
        public bool IsInTension => AxialLoad > 0;

        /// <summary>
        /// Whether the rod is fully retracted (extension is zero).
        /// </summary>
        public bool IsFullyRetracted => Extension <= 0.0;

        /// <summary>
        /// Initializes a new piston rod with the specified geometry and material properties.
        /// </summary>
        /// <param name="diameter">Rod diameter in meters.</param>
        /// <param name="length">Total rod length in meters.</param>
        /// <param name="elasticModulus">Young's modulus of the material in Pascals.</param>
        /// <param name="yieldStrength">Yield strength of the material in Pascals.</param>
        /// <param name="material">Material name/description.</param>
        public PistonRod(double diameter, double length, double elasticModulus, double yieldStrength, string material = "Steel")
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(diameter);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(elasticModulus);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(yieldStrength);
            ArgumentException.ThrowIfNullOrWhiteSpace(material);

            Diameter = diameter;
            Length = length;
            ElasticModulus = elasticModulus;
            YieldStrength = yieldStrength;
            Material = material;
        }

        /// <summary>
        /// Sets the current extension of the rod, clamped to [0, Length].
        /// </summary>
        public void SetExtension(double extension)
        {
            Extension = Math.Clamp(extension, 0.0, Length);
        }

        /// <summary>
        /// Calculates the axial stress in the rod (σ = F / A) in Pascals.
        /// </summary>
        public double CalculateAxialStress() =>
            CrossSectionalArea > 0 ? Math.Abs(AxialLoad) / CrossSectionalArea : 0.0;

        /// <summary>
        /// Calculates the axial deflection/deformation under the current load (δ = FL / AE) in meters.
        /// </summary>
        public double CalculateAxialDeflection()
        {
            if (CrossSectionalArea <= 0 || ElasticModulus <= 0)
                return 0.0;

            return Math.Abs(AxialLoad) * Length / (CrossSectionalArea * ElasticModulus);
        }

        /// <summary>
        /// Calculates the critical buckling load using Euler's formula (Pcr = π²EI / L²) in Newtons.
        /// </summary>
        /// <param name="effectiveLengthFactor">
        /// Column effective length factor (K). 
        /// Common values: 1.0 (pinned-pinned), 0.5 (fixed-fixed), 2.0 (fixed-free).
        /// </param>
        public double CalculateCriticalBucklingLoad(double effectiveLengthFactor = 1.0)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(effectiveLengthFactor);

            double effectiveLength = effectiveLengthFactor * Length;
            if (effectiveLength <= 0)
                return 0.0;

            return Math.PI * Math.PI * ElasticModulus * MomentOfInertia / Math.Pow(effectiveLength, 2);
        }

        /// <summary>
        /// Determines whether the rod is at risk of buckling under the current compressive load.
        /// </summary>
        /// <param name="effectiveLengthFactor">Column effective length factor (K).</param>
        /// <param name="safetyFactor">Required safety factor (must be ≥ 1.0).</param>
        public bool IsBucklingRisk(double effectiveLengthFactor = 1.0, double safetyFactor = 2.0)
        {
            if (!IsInCompression)
                return false;

            double criticalLoad = CalculateCriticalBucklingLoad(effectiveLengthFactor);
            return Math.Abs(AxialLoad) * safetyFactor >= criticalLoad;
        }

        /// <summary>
        /// Calculates the safety factor against yielding at the current load.
        /// Returns <see cref="double.PositiveInfinity"/> if no load is applied.
        /// </summary>
        public double CalculateYieldSafetyFactor()
        {
            double stress = CalculateAxialStress();
            return stress > 0 ? YieldStrength / stress : double.PositiveInfinity;
        }

        /// <summary>
        /// Determines whether the rod is yielding under the current load.
        /// </summary>
        public bool IsYielding() => CalculateAxialStress() >= YieldStrength;

        /// <summary>
        /// Calculates the slenderness ratio of the rod (L / r), a key factor in buckling analysis.
        /// </summary>
        public double CalculateSlendernessRatio()
        {
            // Radius of gyration: r = sqrt(I / A)
            if (CrossSectionalArea <= 0)
                return 0.0;

            double radiusOfGyration = Math.Sqrt(MomentOfInertia / CrossSectionalArea);
            return radiusOfGyration > 0 ? Length / radiusOfGyration : 0.0;
        }

        /// <summary>
        /// Calculates the weight of the rod in Newtons given the material density.
        /// </summary>
        /// <param name="density">Material density in kg/m³ (default 7850 for steel).</param>
        public double CalculateWeight(double density = 7850.0)
        {
            const double gravity = 9.80665;
            return CrossSectionalArea * Length * density * gravity;
        }

        /// <summary>
        /// Calculates the volume of the rod in cubic meters.
        /// </summary>
        public double CalculateVolume() => CrossSectionalArea * Length;

        /// <summary>
        /// Resets the rod to its unloaded, fully retracted state.
        /// </summary>
        public void Reset()
        {
            AxialLoad = 0.0;
            Extension = 0.0;
        }

        public override string ToString() =>
            $"PistonRod [Ø{Diameter * 1000:F1}mm, L={Length * 1000:F1}mm, {Material}, Stress={CalculateAxialStress() / 1e6:F2} MPa]";
    }
}
