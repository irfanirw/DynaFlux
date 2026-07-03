using System;
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
        /// The geometric face representing the surface
        /// </summary>
        public Face Face { get; set; }

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
        /// Null for Opaque surfaces — set automatically in the constructor based on Construction.Type.
        /// </summary>
        public double? CorrectionFactor => Orientation?.CorrectionFactor;

        /// <summary>
        /// Creates a new FluxSurface with automatic area and orientation assignment
        /// </summary>
        /// <param name="face">Geometric face</param>
        /// <param name="construction">Construction assembly</param>
        /// <param name="orientation">Surface orientation (if null, will be automatically derived from face normal)</param>
        public FluxSurface(Face face, FluxConstruction construction, FluxOrientation orientation = null)
        {
            Face = face ?? throw new ArgumentNullException(nameof(face));
            Construction = construction ?? throw new ArgumentNullException(nameof(construction));
            
            // Auto-assign orientation from face normal if not provided
            if (orientation == null)
            {
                var surface = face.SurfaceGeometry();
                var normal = surface.NormalAtParameter(0.5, 0.5);
                Orientation = FluxOrientation.FromNormal(normal);
            }
            else
            {
                Orientation = orientation;
            }

            // Null out CorrectionFactor for opaque surfaces — it only applies to fenestration
            if (string.Equals(Construction.Type, "Opaque", StringComparison.OrdinalIgnoreCase))
            {
                Orientation.CorrectionFactor = null;
            }

            // Auto-assign area from the face
            Area = CalculateArea();
        }

        /// <summary>
        /// Creates a FluxSurface and automatically determines orientation from face normal
        /// </summary>
        /// <param name="face">Geometric face</param>
        /// <param name="construction">Construction assembly</param>
        /// <returns>FluxSurface with calculated orientation</returns>
        public static FluxSurface Create(Face face, FluxConstruction construction)
        {
            if (face == null)
            {
                throw new ArgumentNullException(nameof(face));
            }

            // Get the center point of the face using mid parameters
            var surface = face.SurfaceGeometry();
            var centerPoint = surface.PointAtParameter(0.5, 0.5);
            var normal = surface.NormalAtParameter(0.5, 0.5);

            // Create orientation from normal
            var orientation = FluxOrientation.FromNormal(normal);

            return new FluxSurface(face, construction, orientation);
        }

        /// <summary>
        /// Calculates the surface area
        /// </summary>
        private double CalculateArea()
        {
            if (Face != null)
            {
                var surface = Face.SurfaceGeometry();
                return surface.Area;
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

            // CorrectionFactor is null for Opaque surfaces — solar heat gain does not apply
            if (CorrectionFactor == null)
                return 0.0;

            double solarFactor = Orientation.GetSolarHeatGainFactor();
            return Area * solarFactor * shadingCoefficient * CorrectionFactor.Value;
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

            // CorrectionFactor is null for Opaque surfaces — radiation component does not apply
            if (Orientation != null && CorrectionFactor != null)
            {
                double solarComponent = Orientation.GetSolarHeatGainFactor()
                                        * shadingCoefficient
                                        * CorrectionFactor.Value;
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
