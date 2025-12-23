using System.Collections.Generic;
using FluxConstructionCore = DynaFluxCore.FluxConstruction;
using FluxFenestrationConstructionCore = DynaFluxCore.FluxFenestrationConstruction;
using FluxMaterialCore = DynaFluxCore.FluxMaterial;

namespace DynaFlux.Build;

public static class FluxConstruction
{
    public static string Id { get; set; } = string.Empty;
    public static string Name { get; set; } = string.Empty;
    public static List<FluxMaterialCore> FluxMaterials { get; set; } = new List<FluxMaterialCore>();
    public static double Uvalue { get; set; } = 0.0;
    /// <summary>
    /// Creates an opaque FluxConstruction from its properties.
    /// </summary>
    /// <param name="id">Construction identifier.</param>
    /// <param name="name">Construction name.</param>
    /// <param name="fluxMaterials">Layer materials.</param>
    /// <param name="uvalue">Overall U-value.</param>
    /// <returns>Configured FluxConstruction.</returns>
    public static FluxConstructionCore OpaqueByProperties(
        string id,
        string name,
        List<FluxMaterialCore> fluxMaterials,
        double uvalue)
    {
        var construction = new FluxConstructionCore
        {
            Id = id,
            Name = name,
            FluxMaterials = fluxMaterials ?? new List<FluxMaterialCore>(),
            // set a sensible default; may be overwritten below
            Uvalue = uvalue
        };

        // If caller provided a non-positive uvalue (e.g. 0) compute it from materials
        if (uvalue <= 0.0)
        {
            construction.Uvalue = DynaFluxCore.FluxUvalueCalculator.ComputeUValue(construction.FluxMaterials);
        }

        return construction;
    }

    /// <summary>
    /// Creates a fenestration FluxConstruction from its properties.
    /// </summary>
    /// <param name="id">Construction identifier.</param>
    /// <param name="name">Construction name.</param>
    /// <param name="uvalue">Overall U-value.</param>
    /// <param name="sc1">Shading coefficient 1.</param>
    /// <param name="sc2">Shading coefficient 2.</param>
    /// <returns>Configured FluxConstruction.</returns>
}
