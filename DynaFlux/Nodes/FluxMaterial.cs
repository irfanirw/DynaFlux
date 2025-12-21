using FluxMaterialCore = DynaFluxCore.FluxMaterial;

namespace DynaFlux.Nodes;

public static class FluxMaterial
{
    /// <summary>
    /// Creates a FluxMaterial from its basic properties.
    /// </summary>
    /// <param name="name">Material name.</param>
    /// <param name="thermalConductivity">Thermal conductivity.</param>
    /// <param name="thickness">Material thickness.</param>
    /// <returns>Configured FluxMaterial.</returns>
    public static FluxMaterialCore ByProperties(string name, double thermalConductivity, double thickness)
    {
        return new FluxMaterialCore
        {
            Name = name,
            ThermalConductivity = thermalConductivity,
            Thickness = thickness
        };
    }
}
