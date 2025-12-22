using FluxMaterialCore = DynaFluxCore.FluxMaterial;

namespace DynaFlux.Build;

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

    /// <summary>
    /// Deconstructs a FluxMaterial into its basic properties (extension method).
    /// </summary>
    /// <param name="Material">Material to deconstruct.</param>
    /// <param name="Name">Material name.</param>
    /// <param name="ThermalConductivity">Thermal conductivity.</param>
    /// <param name="Thickness">Material thickness.</param>
    public static void Deconstruct(this FluxMaterialCore Material, out string Name, out double ThermalConductivity, out double Thickness)
    {
        if (Material == null)
        {
            Name = string.Empty;
            ThermalConductivity = 0;
            Thickness = 0;
            return;
        }

        Name = Material.Name;
        ThermalConductivity = Material.ThermalConductivity;
        Thickness = Material.Thickness;
    }

    /// <summary>
    /// Returns the material name.
    /// </summary>
    /// <param name="Material">Material instance.</param>
    /// <returns>Material name or empty string when null.</returns>
    public static string Name(FluxMaterialCore Material)
    {
        return Material?.Name ?? string.Empty;
    }

    /// <summary>
    /// Returns the material thermal conductivity.
    /// </summary>
    /// <param name="Material">Material instance.</param>
    /// <returns>Thermal conductivity or 0 when null.</returns>
    public static double Conductivity(FluxMaterialCore Material)
    {
        return Material?.ThermalConductivity ?? 0.0;
    }

    /// <summary>
    /// Returns the material thickness.
    /// </summary>
    /// <param name="Material">Material instance.</param>
    /// <returns>Thickness or 0 when null.</returns>
    public static double Thickness(FluxMaterialCore Material)
    {
        return Material?.Thickness ?? 0.0;
    }
}
