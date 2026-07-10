using System;
using System.Collections.Generic;
using System.Linq;
using DynaFlux.Build;

namespace DynaFlux.Result
{
    /// <summary>
    /// Represents the complete ETTV analysis result for a building model.
    /// Computes overall ETTV and per-orientation ETTV based on Singapore BCA ETTV standard (ref: retv.pdf)
    /// </summary>
    public class FluxModelEttvResult
    {
        private FluxModel _model;

        /// <summary>
        /// Average ETTV for the entire building envelope in W/m²
        /// Calculated after per-orientation computations are completed
        /// </summary>
        public double AverageETTV { get; private set; }

        /// <summary>
        /// The building model being analyzed
        /// When assigned, automatically triggers ETTV computation
        /// </summary>
        public FluxModel Model
        {
            get { return _model; }
            set
            {
                _model = value;
                if (_model != null)
                {
                    ComputeETTV();
                }
            }
        }

        /// <summary>
        /// ETTV computation results per facade orientation
        /// Automatically populated when Model is assigned
        /// </summary>
        public List<FluxOrientationEttvResult> ResultPerOrientation { get; private set; }

        /// <summary>
        /// Creates a new FluxModelResult
        /// </summary>
        /// <param name="model">Optional FluxModel to analyze (if provided, ETTV computation will begin immediately)</param>
        public FluxModelEttvResult(FluxModel model = null)
        {
            ResultPerOrientation = new List<FluxOrientationEttvResult>();
            AverageETTV = 0.0;
            
            // Use the Model property setter to trigger computation
            Model = model;
        }

        /// <summary>
        /// Computes ETTV for each facade orientation and then calculates overall average ETTV
        /// Called automatically when Model property is assigned
        /// Based on Singapore BCA ETTV standard formula (ref: retv.pdf):
        /// ETTV = [12 × Σ(Awi × Uwi) / Aw] + [3.4 × Σ(Afi × Ufi) / Ao] + [211 × Σ(Afi × SCfi) × CF / Ao]
        /// </summary>
        private void ComputeETTV()
        {
            ResultPerOrientation.Clear();

            if (_model == null || _model.Surfaces == null || _model.Surfaces.Count == 0)
            {
                AverageETTV = 0.0;
                return;
            }

            // Compute ETTV for each facade orientation
            foreach (var orientation in _model.FacadeOrientations)
            {
                var orientationResult = new FluxOrientationEttvResult(orientation);

                // Get all surfaces with this orientation
                var surfacesWithOrientation = _model.Surfaces
                    .Where(s => s?.Orientation?.Name == orientation.Name)
                    .ToList();

                if (surfacesWithOrientation.Count > 0)
                {
                    var opaqueSurfaces = surfacesWithOrientation
                        .Where(s => !string.Equals(s?.Construction?.Type, "Fenestration", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    var fenestrationSurfaces = surfacesWithOrientation
                        .Where(s => string.Equals(s?.Construction?.Type, "Fenestration", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    // Calculate total opaque area (Aw), fenestration area (Af), and total envelope area (Ao)
                    double totalOpaqueArea = opaqueSurfaces.Sum(s => s.Area);
                    double totalFenestrationArea = fenestrationSurfaces.Sum(s => s.Area);
                    double totalArea = totalOpaqueArea + totalFenestrationArea; // Ao

                    orientationResult.OpaqueArea = totalOpaqueArea;
                    orientationResult.FenestrationArea = totalFenestrationArea;
                    orientationResult.GrossArea = totalArea;

                    // Collect unique constructions sorted by Id
                    orientationResult.UniqueConstructions = surfacesWithOrientation
                        .Select(s => s.Construction)
                        .Where(c => c != null)
                        .GroupBy(c => c.Id)
                        .Select(g => g.First())
                        .OrderBy(c => c.Id, StringComparer.Ordinal)
                        .ToList();

                    // Calculate opaque conduction heat gain
                    // Formula: 12 × Σ(Awi × Uwi) / Ao
                    double opaqueAreaUvalueSum = opaqueSurfaces.Sum(s => s.Area * (s.Construction?.Uvalue ?? 0.0));
                    if (totalArea > 0)
                    {
                        orientationResult.OpaqueConductionHeatGain = 12.0 * opaqueAreaUvalueSum / totalArea;
                    }

                    // Calculate fenestration conduction heat gain
                    // Formula: 3.4 × Σ(Afi × Ufi) / Ao
                    double fenestrationAreaUvalueSum = fenestrationSurfaces.Sum(s => s.Area * (s.Construction?.Uvalue ?? 0.0));
                    if (totalArea > 0)
                    {
                        orientationResult.FenestrationConductionHeatGain = 3.4 * fenestrationAreaUvalueSum / totalArea;
                    }

                    // Calculate fenestration radiation heat gain
                    // Formula: 211 × Σ(Afi × SCfi) × CF / Ao
                    double fenestrationAreaSCSum = fenestrationSurfaces.Sum(s => s.Area * (s.Construction?.ScTot ?? 1.0));
                    if (totalArea > 0)
                    {
                        orientationResult.FenestrationRadiationHeatGain = 211.0 * fenestrationAreaSCSum * (orientation.CorrectionFactor ?? 0.0) / totalArea;
                    }
                }

                ResultPerOrientation.Add(orientationResult);
            }

            // Calculate overall average ETTV
            ComputeAverageETTV();
        }

        /// <summary>
        /// Calculates the overall average ETTV for the entire building envelope
        /// ETTV_avg = (Σ(ETTV_per_orientation × orientation_area)) / total_envelope_area
        /// </summary>
        private void ComputeAverageETTV()
        {
            if (ResultPerOrientation.Count == 0 || _model.Surfaces.Count == 0)
            {
                AverageETTV = 0.0;
                return;
            }

            double weightedETTVSum = 0.0;
            double totalArea = 0.0;

            foreach (var orientationResult in ResultPerOrientation)
            {
                // Get total area for this orientation
                var orientationArea = _model.Surfaces
                    .Where(s => s?.Orientation?.Name == orientationResult.Name)
                    .Sum(s => s.Area);

                // Add weighted ETTV contribution
                double orientationETTV = orientationResult.CalculateTotalETTV();
                weightedETTVSum += orientationETTV * orientationArea;
                totalArea += orientationArea;
            }

            // Calculate area-weighted average ETTV
            if (totalArea > 0)
            {
                AverageETTV = weightedETTVSum / totalArea;
            }
            else
            {
                AverageETTV = 0.0;
            }
        }

        /// <summary>
        /// Recalculates ETTV if model or surfaces have been modified
        /// </summary>
        public void UpdateETTVCalculation()
        {
            if (_model != null)
            {
                ComputeETTV();
            }
        }
    }
}
