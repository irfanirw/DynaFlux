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
        /// </summary>
        public double Uvalue { get; private set; }

        /// <summary>
        /// Creates a new FluxConstruction
        /// </summary>
        /// <param name="id">Unique identifier</param>
        /// <param name="name">Construction name</param>
        /// <param name="materials">List of materials from exterior to interior</param>
        public FluxConstruction(string id, string name, List<FluxMaterial> materials)
        {
            Id = id;
            Name = name;
            Materials = materials ?? new List<FluxMaterial>();
            Uvalue = ComputeUvalue(Materials);
        }

        /// <summary>
        /// Computes the U-value (thermal transmittance) based on materials.
        /// U = 1 / (Rsi + Σ(R_materials) + Rse)
        /// Based on Singapore BCA ETTV calculation methodology
        /// </summary>
        /// <param name="materials">List of materials in the construction</param>
        /// <returns>U-value in W/(m²·K)</returns>
        public double ComputeUvalue(List<FluxMaterial> materials)
        {
            if (materials == null || materials.Count == 0)
            {
                return 0.0;
            }

            // Surface resistances based on BCA guidelines
            // Internal surface resistance (Rsi) for vertical surfaces
            double Rsi = 0.13; // m²·K/W

            // External surface resistance (Rse) for vertical surfaces
            double Rse = 0.04; // m²·K/W

            // Sum of thermal resistances of all material layers
            double totalMaterialResistance = materials.Sum(m => m.GetThermalResistance());

            // Total thermal resistance
            double totalResistance = Rsi + totalMaterialResistance + Rse;

            // U-value is the inverse of total thermal resistance
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
