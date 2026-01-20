using System;
using System.Collections.Generic;
using System.Linq;
using DynaFlux.Build;

namespace DynaFlux.Build
{
    /// <summary>
    /// Represents a complete building model for BCA ETTV analysis.
    /// Contains surfaces with their orientations and constructions.
    /// </summary>
    public class FluxModel
    {
        private List<FluxSurface> _surfaces;

        /// <summary>
        /// Project name
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// True North angle in degrees (0-360)
        /// Used to orient the building model
        /// </summary>
        public double TrueNorth { get; set; }

        /// <summary>
        /// List of surfaces in the model
        /// When assigned, automatically extracts unique orientations and constructions
        /// </summary>
        public List<FluxSurface> Surfaces
        {
            get { return _surfaces; }
            set
            {
                _surfaces = value ?? new List<FluxSurface>();
                UpdateFacadeOrientationsAndConstructions();
            }
        }

        /// <summary>
        /// List of unique facade orientations from all surfaces
        /// Automatically populated when Surfaces are assigned
        /// </summary>
        public List<FluxOrientation> FacadeOrientations { get; private set; }

        /// <summary>
        /// List of unique constructions from all surfaces
        /// Automatically populated when Surfaces are assigned
        /// </summary>
        public List<FluxConstruction> Constructions { get; private set; }

        /// <summary>
        /// Creates a new FluxModel
        /// </summary>
        /// <param name="projectName">Project name</param>
        /// <param name="trueNorth">True North angle in degrees (0-360)</param>
        /// <param name="surfaces">Optional list of surfaces (if provided, unique orientations and constructions will be extracted)</param>
        public FluxModel(string projectName, double trueNorth = 0.0, List<FluxSurface> surfaces = null)
        {
            ProjectName = projectName;
            TrueNorth = trueNorth;
            FacadeOrientations = new List<FluxOrientation>();
            Constructions = new List<FluxConstruction>();
            
            // Use the Surfaces property setter to trigger extraction logic
            Surfaces = surfaces;
        }

        /// <summary>
        /// Extracts unique facade orientations and constructions from surfaces
        /// Called automatically when Surfaces property is assigned
        /// </summary>
        private void UpdateFacadeOrientationsAndConditions()
        {
            // Extract unique orientations
            FacadeOrientations = new List<FluxOrientation>();
            var orientationNames = new HashSet<string>();

            foreach (var surface in _surfaces)
            {
                if (surface?.Orientation != null && !orientationNames.Contains(surface.Orientation.Name))
                {
                    FacadeOrientations.Add(surface.Orientation);
                    orientationNames.Add(surface.Orientation.Name);
                }
            }

            // Extract unique constructions
            Constructions = _surfaces
                .Where(s => s?.Construction != null)
                .GroupBy(s => s.Construction.Id)
                .Select(g => g.First().Construction)
                .OrderBy(c => c.Id)
                .ToList();
            var constructionIds = new HashSet<string>();

            foreach (var surface in _surfaces)
            {
                if (surface?.Construction != null && !constructionIds.Contains(surface.Construction.Id))
                {
                    Constructions.Add(surface.Construction);
                    constructionIds.Add(surface.Construction.Id);
                }
            }
        }

        /// <summary>
        /// Extracts unique facade orientations and constructions from surfaces
        /// Called automatically when Surfaces property is assigned
        /// </summary>
        private void UpdateFacadeOrientationsAndConstructions()
        {
            // Extract unique orientations preserving order of appearance
            FacadeOrientations = new List<FluxOrientation>();
            var orientationNames = new HashSet<string>();

            foreach (var surface in _surfaces)
            {
                if (surface?.Orientation != null && !orientationNames.Contains(surface.Orientation.Name))
                {
                    FacadeOrientations.Add(surface.Orientation);
                    orientationNames.Add(surface.Orientation.Name);
                }
            }

            // Extract unique constructions based on both Id and Name, preserving order of appearance
            Constructions = new List<FluxConstruction>();
            var constructionKeys = new HashSet<string>();

            foreach (var surface in _surfaces)
            {
                if (surface?.Construction != null)
                {
                    // Create composite key from both Id and Name for uniqueness check
                    string constructionKey = $"{surface.Construction.Id}|{surface.Construction.Name}";
                    
                    if (!constructionKeys.Contains(constructionKey))
                    {
                        Constructions.Add(surface.Construction);
                        constructionKeys.Add(constructionKey);
                    }
                }
            }
        }
    }
}
