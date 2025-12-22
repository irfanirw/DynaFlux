using System.Collections.Generic;
using System.Drawing;

namespace DynaFluxCore
{
    /// <summary>
    /// Container for legend metadata passed between visualization hosts.
    /// </summary>
    public class FluxLegend
    {
        /// <summary>
        /// Colors aligned with the Legend entries.
        /// </summary>
        public List<Color> Colors { get; set; } = new();

        /// <summary>
        /// Legend labels or numeric values paired with Colors. Values should be string or double.
        /// </summary>
        public List<object> Legend { get; set; } = new();

        /// <summary>
        /// Display title for the legend.
        /// </summary>
        public string LegendTitle { get; set; } = string.Empty;
    }
}
