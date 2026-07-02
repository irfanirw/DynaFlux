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
        /// Creates a new FluxConstruction
        /// </summary>
        /// <param name="id">Unique identifier</param>
        /// <param name="name">Construction name</param>
        /// <param name="materials">List of materials from exterior to interior (optional - if provided, Uvalue will be computed)</param>
        /// <summary>
        /// Type of construction element: "Opaque" or "Fenestration"
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Creates a new FluxConstruction
        /// </summary>
        /// <param name="id">Unique identifier</param>
        /// <param name="name">Construction name</param>
        /// <param name="materials">List of materials from exterior to interior (optional - if provided, Uvalue will be computed)</param>
        /// <param name="uvalue">Manual U-value override (optional)</param>
        /// <param name="sc1">Shading coefficient 1 (optional, default = 1.0)</param>
        /// <param name="sc2">Shading coefficient 2 (optional, default = 1.0)</param>
        public FluxConstruction(string id, string name, List<FluxMaterial>? materials = null, double uvalue = 0.0, double sc1 = 1.0, double sc2 = 1.0)
        {
            Id = id;
            Name = name;
            Materials = materials ?? new List<FluxMaterial>();

            Sc1 = sc1;
            Sc2 = sc2;
            ScTot = Sc1 * Sc2;

            // Set Type based on total shading coefficient (default is 1.0)
            Type = (ScTot == 1.0) ? "Opaque" : "Fenestration";

            bool hasMaterials = materials != null && materials.Count > 0;
            bool hasUvalue = uvalue > 0.0;

            if (hasUvalue)
            {
                // U-value input takes priority; materials are not used for U-value assignment
                Uvalue = uvalue;
                if (hasMaterials)
                {
                    Console.WriteLine($"Warning: FluxConstruction '{name}' received both U-value and materials. U-value input takes priority; materials will not be used for U-value calculation.");
                    Materials = new List<FluxMaterial>();
                }
            }
            else if (hasMaterials)
            {
                // No U-value provided - compute from materials
                Uvalue = ComputeUvalue(Materials);
            }
            else
            {
                // Neither supplied - set to 0 and warn user
                Uvalue = 0.0;
                Console.WriteLine($"Warning: FluxConstruction '{name}' created without materials or U-value. U-value set to 0.0.");
            }
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
