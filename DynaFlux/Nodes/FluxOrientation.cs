using System;
using Autodesk.DesignScript.Geometry;

namespace DynaFlux.Build
{
    /// <summary>
    /// Represents the orientation of a building surface for ETTV calculations.
    /// Based on Singapore BCA ETTV standard which considers orientation for solar heat gain.
    /// </summary>
    public class FluxOrientation
    {
        /// <summary>
        /// Name of the orientation (e.g., "North", "South", "East", "West", "NorthEast", etc.)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Normal vector of the surface
        /// </summary>
        public Vector Normal { get; set; }

        /// <summary>
        /// Angle in degrees from North (0-360)
        /// Used to determine solar heat gain factors based on BCA ETTV tables
        /// </summary>
        public double Angle { get; set; }

        /// <summary>
        /// Correction factor based on orientation
        /// Used in BCA ETTV calculations for orientation-specific adjustments
        /// </summary>
        public double CorrectionFactor { get; private set; }

        /// <summary>
        /// Creates a new FluxOrientation
        /// </summary>
        /// <param name="name">Orientation name</param>
        /// <param name="normal">Surface normal vector</param>
        /// <param name="angle">Angle in degrees from North</param>
        public FluxOrientation(string name, Vector normal, double angle)
        {
            Name = name;
            Normal = normal;
            Angle = angle;
            CorrectionFactor = GetCorrectionFactor(name);
        }

        /// <summary>
        /// Creates a FluxOrientation from a surface normal vector
        /// Coordinate system: X(1,0,0) = East, Y(0,1,0) = North, Z(0,0,1) = Up
        /// Angle measured clockwise from North: North = 0°, East = 90°, South = 180°, West = 270°
        /// </summary>
        /// <param name="normal">Surface normal vector</param>
        /// <returns>FluxOrientation with calculated angle and name</returns>
        public static FluxOrientation FromNormal(Vector normal)
        {
            if (normal == null)
            {
                throw new ArgumentNullException(nameof(normal));
            }

            // Calculate angle from North
            // Y(0,1,0) = North = 0°
            // X(1,0,0) = East = 90°
            // -Y(0,-1,0) = South = 180°
            // -X(-1,0,0) = West = 270°
            // Angle measured clockwise from North (Y-axis)
            double angleRadians = Math.Atan2(normal.X, normal.Y);
            double angleDegrees = angleRadians * (180.0 / Math.PI);
            
            // Normalize to 0-360 range
            if (angleDegrees < 0)
            {
                angleDegrees += 360.0;
            }

            // Determine orientation name based on angle
            string orientationName = GetOrientationName(angleDegrees);

            return new FluxOrientation(orientationName, normal, angleDegrees);
        }

        /// <summary>
        /// Determines the cardinal/intercardinal orientation name based on angle
        /// </summary>
        private static string GetOrientationName(double angle)
        {
            // BCA ETTV typically uses 8 main orientations
            if (angle >= 337.5 || angle < 22.5)
                return "North";
            else if (angle >= 22.5 && angle < 67.5)
                return "NorthEast";
            else if (angle >= 67.5 && angle < 112.5)
                return "East";
            else if (angle >= 112.5 && angle < 157.5)
                return "SouthEast";
            else if (angle >= 157.5 && angle < 202.5)
                return "South";
            else if (angle >= 202.5 && angle < 247.5)
                return "SouthWest";
            else if (angle >= 247.5 && angle < 292.5)
                return "West";
            else if (angle >= 292.5 && angle < 337.5)
                return "NorthWest";
            else
                return "Unknown";
        }

        /// <summary>
        /// Gets the solar heat gain factor based on BCA ETTV standard
        /// Values are representative for Singapore (tropical climate)
        /// </summary>
        public double GetSolarHeatGainFactor()
        {
            // BCA ETTV solar factors for different orientations (W/m²)
            // These are approximate values; actual values should be from BCA tables
            switch (Name)
            {
                case "North":
                    return 84.0;
                case "NorthEast":
                    return 180.0;
                case "East":
                    return 309.0;
                case "SouthEast":
                    return 287.0;
                case "South":
                    return 142.0;
                case "SouthWest":
                    return 287.0;
                case "West":
                    return 309.0;
                case "NorthWest":
                    return 180.0;
                default:
                    return 0.0;
            }
        }

        /// <summary>
        /// Gets the correction factor based on orientation name
        /// Values from BCA ETTV standard (clockwise from North)
        /// </summary>
        private static double GetCorrectionFactor(string orientationName)
        {
            switch (orientationName)
            {
                case "North":
                    return 0.80;
                case "NorthEast":
                    return 0.97;
                case "East":
                    return 1.13;
                case "SouthEast":
                    return 0.98;
                case "South":
                    return 0.83;
                case "SouthWest":
                    return 1.06;
                case "West":
                    return 1.23;
                case "NorthWest":
                    return 1.03;
                default:
                    return 1.0;
            }
        }
    }
}
