using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;

namespace DynaFlux.Build
{
    /// <summary>
    /// Represents a building surface (wall, roof, floor) with thermal properties.
    /// Used for Singapore BCA ETTV (Envelope Thermal Transfer Value) calculations.
    /// </summary>
    public class FluxSurface
    {
        /// <summary>
        /// The geometric surface representing this building surface
        /// </summary>
        public Surface SurfaceGeometry { get; set; }

        /// <summary>
        /// Construction assembly of the surface
        /// </summary>
        public FluxConstruction Construction { get; set; }

        /// <summary>
        /// Surface area in square meters
        /// </summary>
        public double Area { get; private set; }

        /// <summary>
        /// Orientation of the surface (for solar heat gain calculations)
        /// </summary>
        public FluxOrientation Orientation { get; set; }

        /// <summary>
        /// Correction factor for solar heat gain calculations.
        /// double.NaN for Opaque surfaces — set automatically in the constructor based on Construction.Type.
        /// </summary>
        public double CorrectionFactor => Orientation?.CorrectionFactor ?? double.NaN;

        /// <summary>
        /// Creates a new FluxSurface with automatic area and orientation assignment
        /// </summary>
        /// <param name="surface">Geometric surface</param>
        /// <param name="construction">Construction assembly</param>
        /// <param name="orientation">Surface orientation (if null, will be automatically derived from surface normal)</param>
        public FluxSurface(Surface surface, FluxConstruction construction, FluxOrientation orientation = null)
        {
            SurfaceGeometry = surface ?? throw new ArgumentNullException(nameof(surface));
            Construction = construction ?? throw new ArgumentNullException(nameof(construction));
            
            // Auto-assign orientation from surface normal if not provided
            if (orientation == null)
            {
                var normal = surface.NormalAtParameter(0.5, 0.5);
                Orientation = FluxOrientation.FromNormal(normal);
            }
            else
            {
                Orientation = orientation;
            }

            // NaN-out CorrectionFactor for opaque surfaces — it only applies to fenestration
            if (string.Equals(Construction.Type, "Opaque", StringComparison.OrdinalIgnoreCase))
            {
                Orientation.CorrectionFactor = double.NaN;
            }

            // Auto-assign area from the surface
            Area = CalculateArea();
        }

        /// <summary>
        /// Creates a FluxSurface and automatically determines orientation from surface normal
        /// </summary>
        /// <param name="surface">Geometric surface</param>
        /// <param name="construction">Construction assembly</param>
        /// <returns>FluxSurface with calculated orientation</returns>
        public static FluxSurface Create(Surface surface, FluxConstruction construction)
        {
            if (surface == null)
            {
                throw new ArgumentNullException(nameof(surface));
            }

            // Create orientation from normal
            var normal = surface.NormalAtParameter(0.5, 0.5);
            var orientation = FluxOrientation.FromNormal(normal);

            return new FluxSurface(surface, construction, orientation);
        }

        /// <summary>
        /// Creates FluxSurface objects from a list of surfaces.
        /// </summary>
        /// <param name="surfaces">Geometric surfaces</param>
        /// <param name="construction">Construction assembly</param>
        /// <returns>List of FluxSurface objects with calculated orientation</returns>
        public static List<FluxSurface> Create(List<Surface> surfaces, FluxConstruction construction)
        {
            if (surfaces == null)
            {
                throw new ArgumentNullException(nameof(surfaces));
            }

            if (construction == null)
            {
                throw new ArgumentNullException(nameof(construction));
            }

            var result = new List<FluxSurface>();
            foreach (var surface in surfaces)
            {
                if (surface != null)
                {
                    result.Add(Create(surface, construction));
                }
            }

            return result;
        }

        /// <summary>
        /// Square millimeters per square meter, used to convert Revit/Dynamo geometry (modeled in mm) to m²
        /// </summary>
        private const double SqMmPerSqM = 1_000_000.0;

        /// <summary>
        /// Calculates the surface area in square meters, converting from the mm-based geometry
        /// </summary>
        private double CalculateArea()
        {
            if (SurfaceGeometry != null)
            {
                return SurfaceGeometry.Area / SqMmPerSqM;
            }
            return 0.0;
        }

        /// <summary>
        /// Calculates the conduction heat gain through the surface (W)
        /// Q_conduction = U × A × ΔT
        /// Based on BCA ETTV formula component
        /// </summary>
        /// <param name="temperatureDifference">Temperature difference between exterior and interior (K or °C)</param>
        /// <returns>Heat gain in Watts</returns>
        public double CalculateConductionHeatGain(double temperatureDifference = 7.0)
        {
            // Default ΔT = 7°C as per BCA ETTV calculation
            return Construction.Uvalue * Area * temperatureDifference;
        }

        /// <summary>
        /// Calculates the solar heat gain through the surface (W)
        /// Q_solar = A × SF × SC
        /// Where SF = Solar Factor based on orientation
        ///       SC = Shading Coefficient
        /// Based on BCA ETTV formula component
        /// </summary>
        /// <param name="shadingCoefficient">Shading coefficient (typically 0.0-1.0)</param>
        /// <returns>Solar heat gain in Watts</returns>
        public double CalculateSolarHeatGain(double shadingCoefficient = 1.0)
        {
            if (Orientation == null || Construction == null)
                return 0.0;

            // CorrectionFactor is NaN for Opaque surfaces — solar heat gain does not apply
            if (double.IsNaN(CorrectionFactor))
                return 0.0;

            double solarFactor = Orientation.GetSolarHeatGainFactor();
            return Area * solarFactor * shadingCoefficient * CorrectionFactor;
        }

        /// <summary>
        /// Calculates the total ETTV contribution of this surface (W/m²)
        /// ETTV = [U × ΔT] + [SF × SC]
        /// Based on Singapore BCA ETTV standard
        /// </summary>
        /// <param name="temperatureDifference">Temperature difference (default 7°C)</param>
        /// <param name="shadingCoefficient">Shading coefficient (default 1.0)</param>
        /// <returns>ETTV value in W/m²</returns>
        public double CalculateETTV(double temperatureDifference = 7.0, double shadingCoefficient = 1.0)
        {
            double conductionComponent = Construction.Uvalue * temperatureDifference;

            // CorrectionFactor is NaN for Opaque surfaces — radiation component does not apply
            if (Orientation != null && !double.IsNaN(CorrectionFactor))
            {
                double solarComponent = Orientation.GetSolarHeatGainFactor()
                                        * shadingCoefficient
                                        * CorrectionFactor;
                return conductionComponent + solarComponent;
            }

            return conductionComponent;
        }

        /// <summary>
        /// Updates the surface area calculation
        /// </summary>
        public void UpdateArea()
        {
            Area = CalculateArea();
        }
    }
}
