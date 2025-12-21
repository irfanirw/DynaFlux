using System.Collections.Generic;
using FluxConstructionCore = DynaFluxCore.FluxConstruction;
using FluxMaterialCore = DynaFluxCore.FluxMaterial;

namespace DynaFlux.Nodes;

public static class FluxConstructionDeconstructor
{
    /// <summary>
    /// Deconstructs a FluxConstruction into its basic properties.
    /// </summary>
    /// <param name="construction">Construction to deconstruct.</param>
    /// <param name="id">Construction identifier.</param>
    /// <param name="name">Construction name.</param>
    /// <param name="fluxMaterials">Layer materials.</param>
    /// <param name="uvalue">Overall U-value.</param>
    public static void ByConstruction(
        FluxConstructionCore construction,
        out string id,
        out string name,
        out List<FluxMaterialCore> fluxMaterials,
        out double uvalue)
    {
        if (construction == null)
        {
            id = string.Empty;
            name = string.Empty;
            fluxMaterials = new List<FluxMaterialCore>();
            uvalue = 0;
            return;
        }

        id = construction.Id;
        name = construction.Name;
        fluxMaterials = construction.FluxMaterials ?? new List<FluxMaterialCore>();
        uvalue = construction.Uvalue;
    }
}
