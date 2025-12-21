using FluxMaterialCore = DynaFluxCore.FluxMaterial;

namespace DynaFlux.Nodes;

public static class FluxMaterialDeconstructor
{
    /// <summary>
    /// Deconstructs a FluxMaterial into its basic properties.
    /// </summary>
    /// <param name="material">Material to deconstruct.</param>
    /// <param name="name">Material name.</param>
    /// <param name="thermalConductivity">Thermal conductivity.</param>
    /// <param name="thickness">Material thickness.</param>
    public static void ByMaterial(
        FluxMaterialCore material,
        out string name,
        out double thermalConductivity,
        out double thickness)
    {
        if (material == null)
        {
            name = string.Empty;
            thermalConductivity = 0;
            thickness = 0;
            return;
        }

        name = material.Name;
        thermalConductivity = material.ThermalConductivity;
        thickness = material.Thickness;
    }
}
