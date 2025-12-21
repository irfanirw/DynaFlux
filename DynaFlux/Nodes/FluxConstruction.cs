using System.Collections.Generic;
using FluxConstructionCore = DynaFluxCore.FluxConstruction;
using FluxFenestrationConstructionCore = DynaFluxCore.FluxFenestrationConstruction;
using FluxMaterialCore = DynaFluxCore.FluxMaterial;

namespace DynaFlux.Nodes;

public static class FluxConstruction
{
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
    /// <param name="sc2">Shading coefficient 2.</param>
    /// <returns>Configured FluxConstruction.</returns>
    public static FluxConstructionCore FenestrationByProperties(string id, string name, double uvalue, double sc2)
    {
        var construction = new FluxFenestrationConstructionCore
        {
            Id = id,
            Name = name,
            FluxMaterials = new List<FluxMaterialCore>(),
            Uvalue = uvalue,
            Sc2 = sc2
        };
        // If uvalue not provided (<= 0) attempt to compute it. Fenestration often has no materials,
        // so this will be a no-op in most cases but keeps behavior consistent.
        if (uvalue <= 0.0)
        {
            construction.Uvalue = DynaFluxCore.FluxUvalueCalculator.ComputeUValue(construction.FluxMaterials);
        }

        construction.CalculateScTotal(construction.Sc1, sc2);
        return construction;
    }
}
