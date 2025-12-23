using System.Collections.Generic;
using FluxConstructionCore = DynaFluxCore.FluxConstruction;
using FluxMaterialCore = DynaFluxCore.FluxMaterial;

namespace DynaFlux.Build.Query;

public static class FluxConstruction
{
    /// <summary>
    /// Query node: returns the construction identifier.
    /// </summary>
    /// <param name="construction">Construction instance.</param>
    /// <returns>Id or empty string when null.</returns>
    public static string Id(FluxConstructionCore construction)
    {
        return construction?.Id ?? string.Empty;
    }

    /// <summary>
    /// Query node: returns the construction name.
    /// </summary>
    /// <param name="construction">Construction instance.</param>
    /// <returns>Name or empty string when null.</returns>
    public static string Name(FluxConstructionCore construction)
    {
        return construction?.Name ?? string.Empty;
    }

    /// <summary>
    /// Query node: returns the construction layer materials.
    /// </summary>
    /// <param name="construction">Construction instance.</param>
    /// <returns>List of materials (empty list when null).</returns>
    public static List<FluxMaterialCore> FluxMaterials(FluxConstructionCore construction)
    {
        return construction?.FluxMaterials ?? new List<FluxMaterialCore>();
    }

    /// <summary>
    /// Query node: returns the construction overall U-value.
    /// </summary>
    /// <param name="construction">Construction instance.</param>
    /// <returns>U-value or 0 when null.</returns>
    public static double Uvalue(FluxConstructionCore construction)
    {
        return construction?.Uvalue ?? 0.0;
    }
}