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
        /// Shading coefficient for solar heat gain calculations
        /// Typical range: 0.0-1.0 (0 = fully shaded, 1 = no shading)
        /// </summary>
        public double ShadingCoefficient { get; set; }

        /// <summary>
        /// Creates a new FluxConstruction
        /// </summary>
        /// <param name="id">Unique identifier</param>
        /// <param name="name">Construction name</param>
        /// <param name="materials">List of materials from exterior to interior (optional - if provided, Uvalue will be computed)</param>
        public FluxConstruction(string id, string name, List<FluxMaterial>? materials = null, double uvalue = 0.0, double shadingCoefficient = 1.0)
        {
            Id = id;
            Name = name;
            Materials = materials ?? new List<FluxMaterial>();
            ShadingCoefficient = shadingCoefficient;
            
            bool hasMaterials = materials != null && materials.Count > 0;
            bool hasUvalue = uvalue > 0.0;
            
            if (hasMaterials && hasUvalue)
            {
            // Both supplied - Uvalue input overrides
            Uvalue = uvalue;
            }
            else if (hasMaterials && !hasUvalue)
            {
            // Materials supplied but no Uvalue - compute from materials
            Uvalue = ComputeUvalue(Materials);
            }
            else if (!hasMaterials && hasUvalue)
            {
            // Uvalue supplied but no materials - use Uvalue input
            Uvalue = uvalue;
            }
            else
            {
            // Neither supplied - set to 0 and warn user
            Uvalue = 0.0;
            Console.WriteLine($"Warning: FluxConstruction '{name}' created without materials or U-value. U-value set to 0.0.");
            }
            Id = id;
            Name = name;
            Materials = materials ?? new List<FluxMaterial>();
            ShadingCoefficient = shadingCoefficient;
            // Only compute Uvalue if materials are provided
            if (materials != null && materials.Count > 0)
            {
            // If uvalue is provided, use it directly; otherwise compute from materials
            if (uvalue > 0.0)
            {
                Uvalue = uvalue;
            }
            else
            {
                // Materials provided but no uvalue - compute from materials
                Uvalue = ComputeUvalue(Materials);
            }
            }
            else
            {
            // If no materials provided but uvalue is supplied, use the uvalue input
            Uvalue = uvalue;
            }
            Id = id;
            Name = name;
            Materials = materials ?? new List<FluxMaterial>();
            ShadingCoefficient = shadingCoefficient;
            // Only compute Uvalue if materials are provided
            if (materials != null && materials.Count > 0)
            {
                // If uvalue is provided, use it directly; otherwise compute from materials
                if (uvalue > 0.0)
                {
                    Uvalue = uvalue;
                }
                else
                {
                    Uvalue = ComputeUvalue(Materials);
                }
            }
            else
            {
                Uvalue = 0.0;
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
