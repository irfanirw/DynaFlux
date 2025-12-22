using System.Text;
using Autodesk.DesignScript.Geometry;

namespace DynaFluxCore
{
    public static class FluxSurfaceDeconstructor
    {
        // Returns a readable text summary of one FluxSurface.
        public static string ToText(FluxSurface surface)
        {
            if (surface == null) return "FluxSurface: null";

            var sb = new StringBuilder();
            sb.AppendLine("FluxSurface");
            sb.AppendLine($"Id: {surface.Id}");
            sb.AppendLine($"Name: {surface.Name}");
            sb.AppendLine($"Type: {surface.Type}");

            // Geometry summary
            if (surface.Geometry is Mesh m && m.IsValid)
            {
                sb.AppendLine($"Mesh: V={m.VertexCount}, F={m.FaceCount}");
                sb.AppendLine($"Area (approx): {ApproxMeshArea(m):0.###} m²");
            }
            else
            {
                sb.AppendLine("Mesh: (none)");
            }

            // Orientation
            if (surface.Orientation != null)
            {
                sb.AppendLine($"Orientation: {surface.Orientation.Name}");
            }
            else
            {
                sb.AppendLine("Orientation: (none)");
            }

            return sb.ToString();
        }

        // Quick area approximation (triangulate faces).
        private static double ApproxMeshArea(Mesh mesh)
        {
            if (mesh == null || !mesh.IsValid) return 0.0;

            double area = 0.0;
            for (int i = 0; i < mesh.FaceCount; i++)
            {
                var f = mesh.GetFaceIndices(i);
                if (f == null || f.Length < 3) continue;

                var a = mesh.VertexAt(f[0]);
                var b = mesh.VertexAt(f[1]);
                var c = mesh.VertexAt(f[2]);
                area += TriangleArea(a, b, c);
                if (f.Length == 4)
                {
                    var d = mesh.VertexAt(f[3]);
                    area += TriangleArea(a, c, d);
                }
            }
            return area;
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
