using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DynaFlux.Build;
using DynaFlux.Result;

namespace DynaFlux.Report
{
    /// <summary>
    /// Provides HTML report generation for ETTV analysis results.
    /// </summary>
    public static class EttvReport
    {
        private static readonly Dictionary<string, string> OrientationAbbreviations =
            new Dictionary<string, string>
            {
                { "North",     "N"  },
                { "NorthEast", "NE" },
                { "East",      "E"  },
                { "SouthEast", "SE" },
                { "South",     "S"  },
                { "SouthWest", "SW" },
                { "West",      "W"  },
                { "NorthWest", "NW" }
            };

        /// <summary>
        /// Generates an HTML ETTV report and saves it to the same directory as the active
        /// Dynamo script (.dyn file). Falls back to the system temp folder when the workspace
        /// has not yet been saved.
        /// </summary>
        /// <param name="result">The ETTV model result containing per-orientation data</param>
        /// <param name="run">Set to true to execute the report generation</param>
        /// <returns>The full path of the generated HTML file, or a status message when run is false</returns>
        public static string GenerateReport(FluxModelEttvResult result, bool run)
        {
            if (!run)
                return "Report not generated (Run = false)";

            if (result == null)
                throw new ArgumentNullException(nameof(result));

            string outputDir = TryGetWorkspaceDirectory()
                ?? throw new InvalidOperationException(
                    "Could not determine the .dyn file location. " +
                    "Please save the Dynamo script before generating the report.");

            string projectName = result.Model?.ProjectName ?? "Unnamed Project";
            string html = BuildHtml(result, projectName);
            string fileName = Path.Combine(outputDir, $"{SanitizeFileName(projectName)}_EttvReport.html");

            File.WriteAllText(fileName, html, Encoding.UTF8);
            return fileName;
        }

        // -------------------------------------------------------------------------
        // HTML builder
        // -------------------------------------------------------------------------

        private static string BuildHtml(FluxModelEttvResult result, string projectName)
        {
            var orientations = result.ResultPerOrientation ?? new List<FluxOrientationEttvResult>();
            var surfaces = result.Model?.Surfaces ?? new List<FluxSurface>();

            // Overall summary figures
            double totalGrossArea       = orientations.Sum(o => o.GrossArea);
            double totalFenestrationArea = orientations.Sum(o => o.FenestrationArea);
            double totalOpaqueArea      = orientations.Sum(o => o.OpaqueArea);
            double overallWwr           = totalGrossArea > 0 ? totalFenestrationArea / totalGrossArea * 100.0 : 0.0;
            double totalGrossHeatGain   = orientations.Sum(o => o.OrientationGrossHeatGain * o.GrossArea);

            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\">");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset=\"utf-8\" />");
            sb.AppendLine($"    <title>{Encode(projectName)} - ETTV Report</title>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        body { font-family: Arial, sans-serif; margin: 1.5rem; }");
            sb.AppendLine("        table { border-collapse: collapse; min-width: 320px; }");
            sb.AppendLine("        th, td { border: 1px solid #ccc; padding: 6px 10px; }");
            sb.AppendLine("        th { background: #f3f3f3; text-align: left; }");
            sb.AppendLine("        hr { border: 0; border-top: 1px solid #ddd; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            // --- Page header ---
            sb.AppendLine("<h1>Envelope Thermal Transfer Value (ETTV) Report</h1>");
            sb.AppendLine("<p>Produced with DynaFlux<br/>");
            sb.AppendLine("Version: 1.0<br/>");
            sb.AppendLine($"Project Name: {Encode(projectName)}<br/>");
            sb.AppendLine($"Date: {DateTime.Now:dd MMM yyyy}</p>");
            sb.AppendLine("<hr/>");

            // --- Overall summary ---
            sb.AppendLine("<table>");
            sb.AppendLine($"<tr><th style=\"text-align:left;\">Average heat gain</th><td>{result.AverageETTV:F3} W/m&#178;</td></tr>");
            sb.AppendLine($"<tr><th style=\"text-align:left;\">WWR</th><td>{overallWwr:F2} %</td></tr>");
            sb.AppendLine($"<tr><th style=\"text-align:left;\">Window area</th><td>{totalFenestrationArea:F2} m&#178;</td></tr>");
            sb.AppendLine($"<tr><th style=\"text-align:left;\">Wall area</th><td>{totalOpaqueArea:F2} m&#178;</td></tr>");
            sb.AppendLine($"<tr><th style=\"text-align:left;\">Gross area</th><td>{totalGrossArea:F2} m&#178;</td></tr>");
            sb.AppendLine($"<tr><th style=\"text-align:left;\">Total gross heat gain</th><td>{totalGrossHeatGain:F3} W</td></tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("<br/>");

            // --- Per-orientation breakdown ---
            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr><th colspan=\"2\">Breakdown by orientation</th></tr></thead>");
            sb.AppendLine("<tbody>");

            foreach (var or in orientations)
            {
                string name   = or.Name ?? "Unknown";
                string abbrev = OrientationAbbreviations.TryGetValue(name, out var ab) ? ab : name;
                double grossHeatGain = or.OrientationGrossHeatGain * or.GrossArea;

                sb.AppendLine($"<tr><th colspan=\"2\">Orientation: {Encode(name)} ({Encode(abbrev)})</th></tr>");
                sb.AppendLine($"<tr><th style=\"text-align:left;\">Average ETTV</th><td>{or.OrientationGrossHeatGain:F3} W/m&#178;</td></tr>");
                sb.AppendLine($"<tr><th style=\"text-align:left;\">WWR</th><td>{or.Wwr * 100.0:F2} %</td></tr>");
                sb.AppendLine($"<tr><th style=\"text-align:left;\">Window area</th><td>{or.FenestrationArea:F2} m&#178;</td></tr>");
                sb.AppendLine($"<tr><th style=\"text-align:left;\">Wall area</th><td>{or.OpaqueArea:F2} m&#178;</td></tr>");
                sb.AppendLine($"<tr><th style=\"text-align:left;\">Gross area</th><td>{or.GrossArea:F2} m&#178;</td></tr>");
                sb.AppendLine($"<tr><th style=\"text-align:left;\">Total gross heat gain</th><td>{grossHeatGain:F3} W</td></tr>");

                if (or.CorrectionFactor.HasValue)
                    sb.AppendLine($"<tr><th style=\"text-align:left;\">Correction factor (Cf)</th><td>{or.CorrectionFactor.Value:F2}</td></tr>");

                // Construction breakdown table (nested)
                sb.AppendLine("<tr><td colspan=\"2\">");
                sb.AppendLine("<table><tbody>");

                double cf = or.CorrectionFactor ?? 0.0;

                // Group surfaces for this orientation by construction
                var orientSurfaces = surfaces
                    .Where(s => s?.Orientation?.Name == name)
                    .ToList();

                var opaqueGroups = orientSurfaces
                    .Where(s => !string.Equals(s?.Construction?.Type, "Fenestration", StringComparison.OrdinalIgnoreCase))
                    .GroupBy(s => s.Construction?.Id)
                    .Select(g => (Construction: g.First().Construction, TotalArea: g.Sum(s => s.Area)))
                    .OrderBy(x => x.Construction?.Id, StringComparer.Ordinal)
                    .ToList();

                var fenGroups = orientSurfaces
                    .Where(s => string.Equals(s?.Construction?.Type, "Fenestration", StringComparison.OrdinalIgnoreCase))
                    .GroupBy(s => s.Construction?.Id)
                    .Select(g => (Construction: g.First().Construction, TotalArea: g.Sum(s => s.Area)))
                    .OrderBy(x => x.Construction?.Id, StringComparer.Ordinal)
                    .ToList();

                // Opaque section
                sb.AppendLine("<tr><th colspan=\"10\" style=\"text-align:left;\">Opaque Construction</th></tr>");
                sb.AppendLine("<tr><th>ID</th><th>Description</th><th>Area</th><th>U-Value (W/m&#178;K)</th><th>12 x Area x U-Value</th></tr>");
                foreach (var item in opaqueGroups)
                {
                    double contrib = 12.0 * item.TotalArea * (item.Construction?.Uvalue ?? 0.0);
                    sb.AppendLine($"<tr><td>{Encode(item.Construction?.Id ?? "")}</td>" +
                                  $"<td>{Encode(item.Construction?.Name ?? "")}</td>" +
                                  $"<td>{item.TotalArea:F2} m&#178;</td>" +
                                  $"<td>{item.Construction?.Uvalue:F3}</td>" +
                                  $"<td>{contrib:F3}</td></tr>");
                }

                // Fenestration section
                sb.AppendLine("<tr><th colspan=\"10\" style=\"text-align:left;\">Fenestration Construction</th></tr>");
                sb.AppendLine("<tr><th>ID</th><th>Description</th><th>Area</th><th>U-Value (W/m&#178;K)</th><th>SC</th>" +
                              "<th>3.4 x Area x U-Value</th><th>211 x Area x SC x CF</th></tr>");
                foreach (var item in fenGroups)
                {
                    double condContrib = 3.4  * item.TotalArea * (item.Construction?.Uvalue ?? 0.0);
                    double radContrib  = 211.0 * item.TotalArea * (item.Construction?.ScTot  ?? 0.0) * cf;
                    sb.AppendLine($"<tr><td>{Encode(item.Construction?.Id ?? "")}</td>" +
                                  $"<td>{Encode(item.Construction?.Name ?? "")}</td>" +
                                  $"<td>{item.TotalArea:F2} m&#178;</td>" +
                                  $"<td>{item.Construction?.Uvalue:F3}</td>" +
                                  $"<td>{item.Construction?.ScTot:F2}</td>" +
                                  $"<td>{condContrib:F3}</td>" +
                                  $"<td>{radContrib:F3}</td></tr>");
                }

                sb.AppendLine("</tbody></table>");
                sb.AppendLine("</td></tr>");
                sb.AppendLine("<tr><td colspan=\"2\"><hr/></td></tr>");
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine("<hr/>");
            sb.AppendLine("<p>");
            sb.AppendLine("All calculations in this report are based on the BCA ETTV standard.<br/>");
            sb.AppendLine("For more information on the BCA ETTV guidelines, visit: " +
                          "<a href=\"https://file.go.gov.sg/bca-envl-therm-code.pdf\">" +
                          "Microsoft Word - Envelope Code - Jan 2008 R3b.doc</a>");
            sb.AppendLine("</p>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        // -------------------------------------------------------------------------
        // Dynamo workspace path detection (via reflection on DynamoCore)
        // -------------------------------------------------------------------------

        private static string? TryGetWorkspaceDirectory()
        {
            try
            {
                // DynamoServices.ExecutionEvents.ActiveSession is set while a graph is executing.
                // IExecutionSession.CurrentWorkspacePath holds the path of the open .dyn file.
                var dynamoServicesAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "DynamoServices");
                if (dynamoServicesAssembly == null) return null;

                var executionEventsType = dynamoServicesAssembly
                    .GetType("Dynamo.Events.ExecutionEvents");
                if (executionEventsType == null) return null;

                var activeSessionProp = executionEventsType.GetProperty("ActiveSession",
                    BindingFlags.Public | BindingFlags.Static);
                if (activeSessionProp == null) return null;

                var activeSession = activeSessionProp.GetValue(null);
                if (activeSession == null) return null;

                var workspacePathProp = activeSession.GetType()
                    .GetProperty("CurrentWorkspacePath",
                        BindingFlags.Public | BindingFlags.Instance);
                if (workspacePathProp == null) return null;

                var workspacePath = workspacePathProp.GetValue(activeSession) as string;
                if (!string.IsNullOrEmpty(workspacePath))
                    return Path.GetDirectoryName(workspacePath);
            }
            catch { /* fall through */ }

            return null;
        }

        // -------------------------------------------------------------------------
        // Utilities
        // -------------------------------------------------------------------------

        private static string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c));
        }

        private static string Encode(string text) =>
            System.Net.WebUtility.HtmlEncode(text ?? string.Empty);
    }
}
