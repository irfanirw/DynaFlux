using Autodesk.DesignScript.Geometry;


namespace DynaFluxCore
{
    /// <summary>
    /// Represents a single building surface with geometry, construction, orientation, and computed heat gain metadata.
    /// </summary>
    public class FluxSurface
    {
        private FluxConstruction _construction;

        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; private set; }
        public Mesh Geometry { get; set; }
        public double Area { get; private set; }
        public double HeatGain { get; set; }
        public FluxOrientation Orientation { get; set; }

        public FluxConstruction Construction
        {
            get => _construction;
            set
            {
                _construction = value;
                // Auto-set Type based on construction type
                if (value is FluxOpaqueConstruction)
                    Type = "Wall";
                else if (value is FluxFenestrationConstruction)
                    Type = "Fenestration";
                else
                    Type = "Unknown";
            }
        }

        public FluxSurface()
        {
            Name = string.Empty;
            Type = "Unknown";
        }

        public void ComputeHeatGain()
        {
            if (_construction == null || Area <= 0)
            {
                HeatGain = 0d;
                return;
            }

            var cf = Orientation?.Cf > 0 ? Orientation.Cf : 1d;

            if (_construction is FluxOpaqueConstruction)
            {
                HeatGain = 12d * Area * _construction.Uvalue;
            }
            else if (_construction is FluxFenestrationConstruction fen)
            {
                var scTotal = fen.ScTotal > 0 ? fen.ScTotal : 1d;
                HeatGain = (3.4d * Area * _construction.Uvalue) + (211d * Area * scTotal * cf);
            }
            else
            {
                HeatGain = 0d;
            }
        }

        /// <summary>
        /// Calculate and assign Orientation based on the average face normal of the Geometry mesh.
        /// </summary>
        public void CalculateOrientation()
        {
            if (Geometry == null || Geometry.VertexCount == 0 || Geometry.FaceCount == 0)
            {
                Orientation = new FluxOrientation
                {
                    Id = "Unknown",
                    Name = "Unknown",
                    Normal = Vector.ZAxis
                };
                return;
            }

            // Compute average face normal
            Vector avgNormal = Vector.ByCoordinates(0, 0, 0);
            int validFaces = 0;

            for (int i = 0; i < Geometry.FaceCount; i++)
            {
                var face = Geometry.GetFaceIndices(i);
                if (face == null || face.Length < 3) continue;

                var a = Geometry.VertexAt(face[0]);
                var b = Geometry.VertexAt(face[1]);
                var c = Geometry.VertexAt(face[2]);

                var v1 = Vector.ByCoordinates(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
                var v2 = Vector.ByCoordinates(c.X - a.X, c.Y - a.Y, c.Z - a.Z);
                var normal = Vector.Cross(v1, v2);

                if (normal.Length > 0)
                {
                    normal = normal.Normalize();
                    avgNormal = Vector.Add(avgNormal, normal);
                    validFaces++;
                }
            }

            if (validFaces > 0)
            {
                avgNormal = Vector.Divide(avgNormal, validFaces);
                avgNormal = avgNormal.Normalize();
            }
            else
            {
                avgNormal = Vector.ZAxis; // fallback
            }

            // Determine orientation name based on dominant direction
            string orientationName = GetOrientationName(avgNormal);

            Orientation = new FluxOrientation
            {
                Id = orientationName,
                Name = orientationName,
                Normal = avgNormal
            };
        }

        private static string GetOrientationName(Vector normal)
        {
            // Determine primary direction based on normal vector
            var absX = System.Math.Abs(normal.X);
            var absY = System.Math.Abs(normal.Y);
            var absZ = System.Math.Abs(normal.Z);

            if (absZ > absX && absZ > absY)
            {
                return normal.Z > 0 ? "Roof" : "Floor";
            }
            else
            {
                // Horizontal surface - determine cardinal direction
                double angle = System.Math.Atan2(normal.Y, normal.X) * 180.0 / System.Math.PI;
                if (angle < 0) angle += 360;

                if (angle >= 337.5 || angle < 22.5) return "East";
                if (angle >= 22.5 && angle < 67.5) return "NorthEast";
                if (angle >= 67.5 && angle < 112.5) return "North";
                if (angle >= 112.5 && angle < 157.5) return "NorthWest";
                if (angle >= 157.5 && angle < 202.5) return "West";
                if (angle >= 202.5 && angle < 247.5) return "SouthWest";
                if (angle >= 247.5 && angle < 292.5) return "South";
                return "SouthEast";
            }
        }

        public void SetArea(Mesh geometry)
        {
            if (geometry is null)
            {
                Area = 0d;
                return;
            }

            // DesignScript Mesh doesn't expose AreaMassProperties; compute by triangulating faces
            double area = 0d;
            for (int i = 0; i < geometry.FaceCount; i++)
            {
                var face = geometry.GetFaceIndices(i);
                if (face == null || face.Length < 3) continue;

                var a = geometry.VertexAt(face[0]);
                var b = geometry.VertexAt(face[1]);
                var c = geometry.VertexAt(face[2]);

                area += TriangleArea(a, b, c);

                }

            Area = area;
        }

        private static double TriangleArea(Point a, Point b, Point c)
        {
            var v1 = Vector.ByCoordinates(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
            var v2 = Vector.ByCoordinates(c.X - a.X, c.Y - a.Y, c.Z - a.Z);
            var cross = Vector.Cross(v1, v2);
            return 0.5 * cross.Length;
        }
    }
}
