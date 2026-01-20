using System;

namespace DynaFlux.Build
{
    /// <summary>
    /// Represents a building material with thermal properties for ETTV calculations.
    /// Based on Singapore BCA ETTV standard.
    /// </summary>
    public class FluxMaterial
    {
        /// <summary>
        /// Name of the material
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Thermal conductivity in W/(m·K)
        /// </summary>
        public double ThermalConductivity { get; set; }

        /// <summary>
        /// Material thickness in millimeters
        /// </summary>
        public double Thickness { get; set; }

        /// <summary>
        /// Creates a new FluxMaterial
        /// </summary>
        /// <param name="name">Material name</param>
        /// <param name="thermalConductivity">Thermal conductivity in W/(m·K)</param>
        /// <param name="thickness">Thickness in millimeters</param>
        public FluxMaterial(string name, double thermalConductivity, double thickness)
        {
            Name = name;
            ThermalConductivity = thermalConductivity;
            Thickness = thickness;
        }

        /// <summary>
        /// Calculate thermal resistance (R-value) of the material in m²·K/W
        /// R = thickness / thermal conductivity
        /// </summary>
        public double GetThermalResistance()
        {
            // Convert thickness from mm to m
            double thicknessInMeters = Thickness / 1000.0;
            return thicknessInMeters / ThermalConductivity;
        }
    }
}
