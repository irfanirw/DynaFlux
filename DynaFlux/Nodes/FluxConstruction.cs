using System;
using System.Collections.Generic;
using System.Linq;

namespace DynaFlux.Build
{
    /// <summary>
    /// Represents a building construction assembly (wall, roof, floor, etc.) with multiple material layers.
    /// Based on Singapore BCA ETTV standard.
    /// </summary>
    public class FluxConstruction
    {
        /// <summary>
        /// Unique identifier for the construction
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of the construction assembly
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of materials in the construction (from exterior to interior)
        /// </summary>
        public List<FluxMaterial> Materials { get; set; }

        /// <summary>
        /// Overall U-value (thermal transmittance) in W/(m²·K)
        /// Can be computed from materials or set manually
        /// </summary>
        public double Uvalue { get; set; }

        /// <summary>
        /// Shading coefficient 1 for solar heat gain calculations
        /// </summary>
        public double Sc1 { get; set; }

        /// <summary>
        /// Shading coefficient 2 for solar heat gain calculations
        /// Default = 1.0
        /// </summary>
        public double Sc2 { get; set; }

        /// <summary>
        /// Total shading coefficient (ScTot = Sc1 * Sc2)
        /// Typical range: 0.0-1.0 (0 = fully shaded, 1 consider as no shading)
        /// </summary>
        public double ScTot { get; set; }

        /// <summary>
        /// Type of construction element: "Opaque" or "Fenestration"
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Creates a FluxConstruction with a manually specified U-value.
        /// Use this when the U-value is already known (e.g. from a datasheet).
        /// </summary>
        /// <param name="id">Unique identifier</param>
        /// <param name="name">Construction name</param>
        /// <param name="uvalue">Thermal transmittance in W/(m²·K)</param>
        /// <param name="sc1">Shading coefficient 1 (default = 1.0)</param>
        /// <param name="sc2">Shading coefficient 2 (default = 1.0)</param>
        public static FluxConstruction ByUvalue(string id, string name, double uvalue, double sc1 = 1.0, double sc2 = 1.0)
        {
            var c = new FluxConstruction();
            c.Id = id;
            c.Name = name;
            c.Materials = new List<FluxMaterial>();
            c.Uvalue = uvalue;
            c.Sc1 = sc1;
            c.Sc2 = sc2;
            c.ScTot = sc1 * sc2;
            c.Type = (c.ScTot == 1.0) ? "Opaque" : "Fenestration";
            return c;
        }

        /// <summary>
        /// Creates a FluxConstruction from material layers.
        /// U-value is automatically computed from the material assembly.
        /// </summary>
        /// <param name="id">Unique identifier</param>
        /// <param name="name">Construction name</param>
        /// <param name="materials">List of materials from exterior to interior</param>
        /// <param name="sc1">Shading coefficient 1 (default = 1.0)</param>
        /// <param name="sc2">Shading coefficient 2 (default = 1.0)</param>
        public static FluxConstruction ByMaterials(string id, string name, List<FluxMaterial> materials, double sc1 = 1.0, double sc2 = 1.0)
        {
            var c = new FluxConstruction();
            c.Id = id;
            c.Name = name;
            c.Materials = materials ?? new List<FluxMaterial>();
            c.Sc1 = sc1;
            c.Sc2 = sc2;
            c.ScTot = sc1 * sc2;
            c.Type = (c.ScTot == 1.0) ? "Opaque" : "Fenestration";
            c.Uvalue = c.ComputeUvalue(c.Materials);
            return c;
        }

        private FluxConstruction()
        {
            Id = null!;
            Name = null!;
            Materials = null!;
            Type = null!;
        }

        /// <summary>
        /// Computes the U-value (thermal transmittance) based on materials.
        /// U = 1 / (Rsi + Σ(R_materials) + Rse)
        /// Based on Singapore BCA RETV standard (ref: retv.pdf)
        /// </summary>
        /// <param name="materials">List of materials in the construction from exterior to interior</param>
        /// <returns>U-value in W/(m²·K)</returns>
        public double ComputeUvalue(List<FluxMaterial> materials)
        {
            if (materials == null || materials.Count == 0)
            {
                return 0.0;
            }

            // Surface resistances based on BCA RETV guidelines (ref: retv.pdf)
            // Internal surface resistance (Rsi) for vertical surfaces - Table 2.1
            double Rsi = 0.13; // m²·K/W

            // External surface resistance (Rse) for vertical surfaces - Table 2.1
            double Rse = 0.04; // m²·K/W

            // Calculate sum of thermal resistances for all material layers
            // Each material's thermal resistance R = thickness(m) / thermal conductivity(W/(m·K))
            // The FluxMaterial.GetThermalResistance() method handles:
            // - Converting thickness from millimeters to meters
            // - Computing R = thickness(m) / k where k is thermal conductivity
            double totalMaterialResistance = 0.0;
            foreach (var material in materials)
            {
                if (material != null && material.ThermalConductivity > 0)
                {
                    // Get thermal resistance for this layer
                    // R = thickness(m) / k(W/(m·K)) = m²·K/W
                    double resistance = material.GetThermalResistance();
                    totalMaterialResistance += resistance;
                }
            }

            // Total thermal resistance: R_total = Rsi + Σ(R_materials) + Rse
            double totalResistance = Rsi + totalMaterialResistance + Rse;

            // U-value is the inverse of total thermal resistance
            // U = 1 / R_total [W/(m²·K)]
            if (totalResistance > 0)
            {
                Uvalue = 1.0 / totalResistance;
            }
            else
            {
                Uvalue = 0.0;
            }

            return Uvalue;
        }

        /// <summary>
        /// Updates the U-value calculation
        /// </summary>
        public void UpdateUvalue()
        {
            Uvalue = ComputeUvalue(Materials);
        }
    }
}
