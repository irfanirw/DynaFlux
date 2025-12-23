using FluxMaterialCore = DynaFluxCore.FluxMaterial;

namespace DynaFlux.Build;

public static class FluxMaterial
{
    /// <summary>
    public static string Name { get; set; }
    public static double ThermalConductivity { get; set; }
    public static double Thickness { get; set; }

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

    /// <summary>
    /// Deconstructs a FluxMaterial into its basic properties (extension method).
    /// </summary>
    /// <param name="Material">Material to deconstruct.</param>
    /// <param name="Name">Material name.</param>
    /// <param name="ThermalConductivity">Thermal conductivity.</param>
    /// <param name="Thickness">Material thickness.</param>
}
