using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using DynaFlux.Build;


namespace DynaFlux.Result
{
    /// <summary>
    /// Represents the ETTV calculation result for a specific orientation.
    /// Inherits from FluxOrientation and adds result computation properties.
    /// Based on Singapore BCA ETTV standard (ref: retv.pdf)
    /// </summary>
    public class FluxOrientationEttvResult : FluxOrientation
    {
        /// <summary>
        /// Opaque conduction heat gain in W/m²
        /// Formula: 12 × Aw × Uw
        /// Where: Aw = area of opaque wall, Uw = U-value of wall
        /// </summary>
        public double OpaqueConductionHeatGain { get; set; }

        /// <summary>
        /// Fenestration conduction heat gain in W/m²
        /// Formula: 3.4 × Af × Uf
        /// Where: Af = area of fenestration (windows), Uf = U-value of fenestration
        /// </summary>
        public double FenestrationConductionHeatGain { get; set; }

        /// <summary>
        /// Fenestration radiation heat gain in W/m²
        /// Formula: 211 × (Af × SCf) × CorrectionFactor
        /// Where: Af = fenestration area, SCf = Shading Coefficient of fenestration
        ///        CorrectionFactor = orientation-specific correction factor from FluxOrientation
        /// </summary>
        public double FenestrationRadiationHeatGain { get; set; }

        /// <summary>
        /// Total gross heat gain for this orientation in W/m²
        /// OrientationGrossHeatGain = OpaqueConductionHeatGain + FenestrationConductionHeatGain + FenestrationRadiationHeatGain
        /// </summary>
        public double OrientationGrossHeatGain => OpaqueConductionHeatGain + FenestrationConductionHeatGain + FenestrationRadiationHeatGain;

        /// <summary>
        /// Total gross envelope area (opaque + fenestration) in m²
        /// </summary>
        public double GrossArea { get; set; }

        /// <summary>
        /// Total opaque surface area in m²
        /// </summary>
        public double OpaqueArea { get; set; }

        /// <summary>
        /// Total fenestration surface area in m²
        /// </summary>
        public double FenestrationArea { get; set; }

        /// <summary>
        /// Window-to-wall ratio (WWR)
        /// WWR = FenestrationArea / GrossArea
        /// </summary>
        public double Wwr => GrossArea > 0 ? FenestrationArea / GrossArea : 0.0;

        /// <summary>
        /// List of unique FluxConstruction assemblies present in this orientation,
        /// sorted alphabetically by FluxConstruction.Id
        /// </summary>
        public List<FluxConstruction> UniqueConstructions { get; set; }

        /// <summary>
        /// Creates a new FluxOrientationResult from a FluxOrientation
        /// </summary>
        /// <param name="orientation">Source FluxOrientation</param>
        public FluxOrientationEttvResult(FluxOrientation orientation)
            : base(orientation.Name, orientation.Normal, orientation.Angle)
        {
            OpaqueConductionHeatGain = 0.0;
            FenestrationConductionHeatGain = 0.0;
            FenestrationRadiationHeatGain = 0.0;
            GrossArea = 0.0;
            OpaqueArea = 0.0;
            FenestrationArea = 0.0;
            UniqueConstructions = new List<FluxConstruction>();
        }

        /// <summary>
        /// Calculates total ETTV for this orientation
        /// ETTV = OpaqueConductionHeatGain + FenestrationConductionHeatGain + FenestrationRadiationHeatGain
        /// </summary>
        public double CalculateTotalETTV()
        {
            return OpaqueConductionHeatGain + FenestrationConductionHeatGain + FenestrationRadiationHeatGain;
        }
    }
}
