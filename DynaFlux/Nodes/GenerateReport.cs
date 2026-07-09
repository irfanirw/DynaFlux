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

            // Overall summary figures — areas summed directly from surfaces
            double totalFenestrationArea = surfaces
                .Where(s => string.Equals(s?.Construction?.Type, "Fenestration", StringComparison.OrdinalIgnoreCase))
                .Sum(s => s.Area);
            double totalOpaqueArea = surfaces
                .Where(s => !string.Equals(s?.Construction?.Type, "Fenestration", StringComparison.OrdinalIgnoreCase))
                .Sum(s => s.Area);
            double totalGrossArea       = totalOpaqueArea + totalFenestrationArea;
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
            sb.AppendLine("    <script src=\"https://cdn.jsdelivr.net/npm/chart.js@4\"></script>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            // --- Page header ---
            sb.AppendLine("<h1>Envelope Thermal Transfer Value (ETTV) Report</h1>");
            sb.AppendLine("<p>Produced with DynaFlux<br/>");
            sb.AppendLine("Version: 1.0<br/>");
            sb.AppendLine($"Project Name: {Encode(projectName)}<br/>");
            sb.AppendLine($"Date: {DateTime.Now:dd MMM yyyy}</p>");
            sb.AppendLine("<hr/>");

            // --- Charts ---
            sb.Append(BuildChartsHtml(orientations));

            // --- Overall summary ---
            const double BcaLimit = 50.0;
            const double GmLimit  = 45.0;
            bool bcaCompliant = result.AverageETTV <= BcaLimit;
            bool gmCompliant  = result.AverageETTV <= GmLimit;
            string bcaStatus  = bcaCompliant ? "COMPLIANT" : "NON-COMPLIANT";
            string bcaColor   = bcaCompliant ? "green" : "red";
            string gmStatus   = gmCompliant  ? "COMPLIANT" : "NON-COMPLIANT";
            string gmColor    = gmCompliant  ? "green" : "red";

            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr><th colspan=\"2\">ETTV Calculation Summary</th></tr></thead>");
            sb.AppendLine($"<tr><th style=\"text-align:left;\">Average ETTV</th><td>{result.AverageETTV:F3} W/m&#178;</td></tr>");
            sb.AppendLine($"<tr><th style=\"text-align:left;\">BCA Prescriptive Limit</th><td>{BcaLimit:F1} W/m&#178;</td></tr>");
            sb.AppendLine($"<tr><th style=\"text-align:left;\">BCA Compliance</th>" +
                          $"<td style=\"color:{bcaColor};font-weight:bold;\">{bcaStatus}</td></tr>");
            sb.AppendLine($"<tr><th style=\"text-align:left;\">Green Mark 2021 / ES Code Baseline</th><td>{GmLimit:F1} W/m&#178;</td></tr>");
            sb.AppendLine($"<tr><th style=\"text-align:left;\">Green Mark / ES Code Compliance</th>" +
                          $"<td style=\"color:{gmColor};font-weight:bold;\">{gmStatus}</td></tr>");
            sb.AppendLine($"<tr><th style=\"text-align:left;\">WWR</th><td>{overallWwr:F2} %</td></tr>");
            sb.AppendLine($"<tr><th style=\"text-align:left;\">Window Area</th><td>{totalFenestrationArea:F2} m&#178;</td></tr>");
            sb.AppendLine($"<tr><th style=\"text-align:left;\">Wall Area</th><td>{totalOpaqueArea:F2} m&#178;</td></tr>");
            sb.AppendLine($"<tr><th style=\"text-align:left;\">Gross Area</th><td>{totalGrossArea:F2} m&#178;</td></tr>");
            sb.AppendLine($"<tr><th style=\"text-align:left;\">Total Gross Heat Gain</th><td>{totalGrossHeatGain:F3} W</td></tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("<p style=\"font-size:0.85rem;color:#555;margin-top:0.5rem;\">");
            sb.AppendLine("<strong>Note:</strong> For Singapore Green Mark 2021 / Energy Services (ES) Code compliance, ");
            sb.AppendLine("the recommended ETTV baseline target is <strong>45 W/m&#178;</strong>. ");
            sb.AppendLine("The BCA prescriptive limit remains 50 W/m&#178;.");
            sb.AppendLine("</p>");
            sb.AppendLine("<br/>");

            // --- Facade Construction Summary ---
            var allOpaqueConstructions = surfaces
                .Where(s => !string.Equals(s?.Construction?.Type, "Fenestration", StringComparison.OrdinalIgnoreCase)
                            && s?.Construction != null)
                .GroupBy(s => s.Construction.Id)
                .Select(g => g.First().Construction)
                .OrderBy(c => c.Id, StringComparer.Ordinal)
                .ToList();

            var allFenestrationConstructions = surfaces
                .Where(s => string.Equals(s?.Construction?.Type, "Fenestration", StringComparison.OrdinalIgnoreCase)
                            && s?.Construction != null)
                .GroupBy(s => s.Construction.Id)
                .Select(g => g.First().Construction)
                .OrderBy(c => c.Id, StringComparer.Ordinal)
                .ToList();

            sb.AppendLine("<table>");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr><th colspan=\"4\">Facade Construction Summary</th></tr>");
            sb.AppendLine("<tr><th>ID</th><th>Description</th><th>U-Value (W/m&#178;K)</th><th>SC Value</th></tr>");
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");
            if (allOpaqueConstructions.Count > 0)
            {
                sb.AppendLine("<tr><th colspan=\"4\" style=\"text-align:left;\">Opaque</th></tr>");
                foreach (var c in allOpaqueConstructions)
                {
                    sb.AppendLine($"<tr><td>{Encode(c.Id ?? "")}</td>" +
                                  $"<td>{Encode(c.Name ?? "")}</td>" +
                                  $"<td>{c.Uvalue:F3}</td>" +
                                  $"<td>N/A</td></tr>");
                }
            }
            if (allFenestrationConstructions.Count > 0)
            {
                sb.AppendLine("<tr><th colspan=\"4\" style=\"text-align:left;\">Fenestration</th></tr>");
                foreach (var c in allFenestrationConstructions)
                {
                    sb.AppendLine($"<tr><td>{Encode(c.Id ?? "")}</td>" +
                                  $"<td>{Encode(c.Name ?? "")}</td>" +
                                  $"<td>{c.Uvalue:F3}</td>" +
                                  $"<td>{c.ScTot:F2}</td></tr>");
                }
            }
            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine("<br/>");

            // --- Per-orientation breakdown ---
            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr><th colspan=\"2\">Breakdown by Orientation</th></tr></thead>");
            sb.AppendLine("<tbody>");

            foreach (var or in orientations)
            {
                string name   = or.Name ?? "Unknown";
                string abbrev = OrientationAbbreviations.TryGetValue(name, out var ab) ? ab : name;
                double grossHeatGain = or.OrientationGrossHeatGain * or.GrossArea;

                sb.AppendLine($"<tr><th colspan=\"2\">Orientation: {Encode(name)} ({Encode(abbrev)})</th></tr>");
                sb.AppendLine($"<tr><th style=\"text-align:left;\">Average ETTV</th><td>{or.OrientationGrossHeatGain:F3} W/m&#178;</td></tr>");
                sb.AppendLine($"<tr><th style=\"text-align:left;\">WWR</th><td>{or.Wwr * 100.0:F2} %</td></tr>");
                sb.AppendLine($"<tr><th style=\"text-align:left;\">Window Area</th><td>{or.FenestrationArea:F2} m&#178;</td></tr>");
                sb.AppendLine($"<tr><th style=\"text-align:left;\">Wall Area</th><td>{or.OpaqueArea:F2} m&#178;</td></tr>");
                sb.AppendLine($"<tr><th style=\"text-align:left;\">Gross Area</th><td>{or.GrossArea:F2} m&#178;</td></tr>");
                sb.AppendLine($"<tr><th style=\"text-align:left;\">Total Gross Heat Gain</th><td>{grossHeatGain:F3} W</td></tr>");

                if (or.CorrectionFactor.HasValue)
                    sb.AppendLine($"<tr><th style=\"text-align:left;\">Correction Factor (CF)</th><td>{or.CorrectionFactor.Value:F2}</td></tr>");

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
        // Chart builder
        // -------------------------------------------------------------------------

        private static string BuildChartsHtml(List<FluxOrientationEttvResult> orientations)
        {
            var active = orientations.Where(o => o.GrossArea > 0).ToList();
            if (active.Count == 0) return string.Empty;

            var inv = System.Globalization.CultureInfo.InvariantCulture;
            string[] palette = { "#4e79a7", "#f28e2b", "#e15759", "#76b7b2", "#59a14f", "#edc948", "#b07aa1", "#ff9da7" };

            var shortLabels = new List<string>();
            var ettvContrib = new List<string>();  // total W per orientation
            var ettvPerSqm  = new List<string>();  // W/m² per orientation
            var wwrValues   = new List<string>();  // WWR % per orientation
            var colors      = new List<string>();

            for (int i = 0; i < active.Count; i++)
            {
                var o = active[i];
                string name   = o.Name ?? "Unknown";
                string abbrev = OrientationAbbreviations.TryGetValue(name, out var ab) ? ab : name;
                shortLabels.Add($"\"{abbrev}\"");
                ettvContrib.Add((o.OrientationGrossHeatGain * o.GrossArea).ToString("F2", inv));
                ettvPerSqm.Add(o.OrientationGrossHeatGain.ToString("F3", inv));
                wwrValues.Add((o.Wwr * 100.0).ToString("F2", inv));
                colors.Add($"\"{palette[i % palette.Length]}\"");
            }

            string labelsJs = "[" + string.Join(",", shortLabels) + "]";
            string ettvCJs  = "[" + string.Join(",", ettvContrib)  + "]";
            string ettvPJs  = "[" + string.Join(",", ettvPerSqm)   + "]";
            string wwrJs    = "[" + string.Join(",", wwrValues)    + "]";
            string colorsJs = "[" + string.Join(",", colors)       + "]";

            var sb = new StringBuilder();
            sb.AppendLine("<h2 style=\"margin-top:1.5rem;\">ETTV Overview</h2>");
            sb.AppendLine("<div style=\"display:flex;gap:2rem;align-items:flex-start;flex-wrap:wrap;margin-bottom:1.5rem;\">");
            sb.AppendLine("  <div style=\"flex:1;min-width:260px;max-width:440px;\"><canvas id=\"dynaflux-ettv-chart\"></canvas></div>");
            sb.AppendLine("  <div style=\"flex:1;min-width:260px;max-width:440px;\"><canvas id=\"dynaflux-wwr-chart\"></canvas></div>");
            sb.AppendLine("</div>");
            sb.AppendLine("<script>");
            sb.AppendLine("(function(){");
            sb.AppendLine($"  var lb={labelsJs},ec={ettvCJs},ep={ettvPJs},wr={wwrJs},cl={colorsJs};");
            sb.AppendLine("  if(typeof Chart==='undefined')return;");
            // ETTV contribution pie chart
            sb.AppendLine("  new Chart(document.getElementById('dynaflux-ettv-chart'),{type:'pie',");
            sb.AppendLine("    data:{labels:lb,datasets:[{data:ec,backgroundColor:cl}]},");
            sb.AppendLine("    options:{plugins:{");
            sb.AppendLine("      title:{display:true,text:'ETTV Contribution by Orientation'},");
            sb.AppendLine("      tooltip:{callbacks:{label:function(c){");
            sb.AppendLine("        var t=c.dataset.data.reduce(function(a,b){return a+b;},0);");
            sb.AppendLine("        return[c.label+': '+c.parsed.toFixed(0)+' W ('+(c.parsed/t*100).toFixed(1)+'%)',");
            sb.AppendLine("               'Average ETTV: '+ep[c.dataIndex]+' W/m\u00b2',");
            sb.AppendLine("               'WWR: '+wr[c.dataIndex]+'%'];}}},");
            sb.AppendLine("      legend:{position:'bottom'}}}});");
            // WWR bar chart
            sb.AppendLine("  new Chart(document.getElementById('dynaflux-wwr-chart'),{type:'bar',");
            sb.AppendLine("    data:{labels:lb,datasets:[{label:'WWR (%)',data:wr,backgroundColor:cl,borderRadius:4}]},");
            sb.AppendLine("    options:{");
            sb.AppendLine("      plugins:{");
            sb.AppendLine("        title:{display:true,text:'Window-to-Wall Ratio by Orientation'},");
            sb.AppendLine("        tooltip:{callbacks:{label:function(c){return c.parsed.y.toFixed(1)+'%';}}},");
            sb.AppendLine("        legend:{display:false}},");
            sb.AppendLine("      scales:{y:{beginAtZero:true,max:100,");
            sb.AppendLine("        title:{display:true,text:'WWR (%)'},");
            sb.AppendLine("        ticks:{callback:function(v){return v+'%';}}}}}});");
            sb.AppendLine("})();");
            sb.AppendLine("</script>");
            sb.AppendLine("<hr/>");

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
