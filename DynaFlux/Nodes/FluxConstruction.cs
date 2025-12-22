using System.Collections.Generic;
using FluxConstructionCore = DynaFluxCore.FluxConstruction;
using FluxFenestrationConstructionCore = DynaFluxCore.FluxFenestrationConstruction;
using FluxMaterialCore = DynaFluxCore.FluxMaterial;

namespace DynaFlux.Build;

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

    /// <summary>
    /// Deconstructs a FluxConstruction into its basic properties (extension method).
    /// </summary>
    /// <param name="Construction">Construction to deconstruct.</param>
    /// <param name="Id">Construction identifier.</param>
    /// <param name="Name">Construction name.</param>
    /// <param name="FluxMaterials">Layer materials.</param>
    /// <param name="Uvalue">Overall U-value.</param>
    public static void Deconstruct(this FluxConstructionCore Construction, out string Id, out string Name, out List<FluxMaterialCore> FluxMaterials, out double Uvalue)
    {
        if (Construction == null)
        {
            Id = string.Empty;
            Name = string.Empty;
            FluxMaterials = new List<FluxMaterialCore>();
            Uvalue = 0;
            return;
        }

        Id = Construction.Id;
        Name = Construction.Name;
        FluxMaterials = Construction.FluxMaterials ?? new List<FluxMaterialCore>();
        Uvalue = Construction.Uvalue;
    }

    /// <summary>
    /// Returns the construction identifier.
    /// </summary>
    /// <param name="Construction">Construction instance.</param>
    /// <returns>Id or empty string when null.</returns>
    public static string Id(FluxConstructionCore Construction)
    {
        return Construction?.Id ?? string.Empty;
    }

    /// <summary>
    /// Returns the construction name.
    /// </summary>
    /// <param name="Construction">Construction instance.</param>
    /// <returns>Name or empty string when null.</returns>
    public static string Name(FluxConstructionCore Construction)
    {
        return Construction?.Name ?? string.Empty;
    }

    /// <summary>
    /// Returns the construction layer materials.
    /// </summary>
    /// <param name="Construction">Construction instance.</param>
    /// <returns>List of materials (empty list when null).</returns>
    public static List<FluxMaterialCore> FluxMaterials(FluxConstructionCore Construction)
    {
        return Construction?.FluxMaterials ?? new List<FluxMaterialCore>();
    }

    /// <summary>
    /// Returns the construction overall U-value.
    /// </summary>
    /// <param name="Construction">Construction instance.</param>
    /// <returns>U-value or 0 when null.</returns>
    public static double Uvalue(FluxConstructionCore Construction)
    {
        return Construction?.Uvalue ?? 0.0;
    }
}
