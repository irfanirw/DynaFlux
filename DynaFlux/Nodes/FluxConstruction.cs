using System.Collections.Generic;
using FluxConstructionCore = DynaFluxCore.FluxConstruction;
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
        return new FluxConstructionCore
        {
            Id = id,
            Name = name,
            FluxMaterials = fluxMaterials ?? new List<FluxMaterialCore>(),
            Uvalue = uvalue
        };
    }

    /// <summary>
    /// Creates a fenestration FluxConstruction from its properties.
    /// </summary>
    /// <param name="id">Construction identifier.</param>
    /// <param name="name">Construction name.</param>
    /// <param name="uvalue">Overall U-value.</param>
    /// <returns>Configured FluxConstruction.</returns>
    public static FluxConstructionCore FenestrationByProperties(string id, string name, double uvalue)
    {
        return new FluxConstructionCore
        {
            Id = id,
            Name = name,
            FluxMaterials = new List<FluxMaterialCore>(),
            Uvalue = uvalue
        };
    }
}
