#if !USE_DESIGNSCRIPT
using System;
using System.Collections.Generic;

namespace Autodesk.DesignScript.Geometry
{
    // Minimal Point stub used by core algorithms.
    public class Point
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public Point(double x, double y, double z) { X = x; Y = y; Z = z; }
        public static Point ByCoordinates(double x, double y, double z) => new Point(x, y, z);
    }

    // Minimal Vector stub with basic operations.
    public class Vector
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public double Length => System.Math.Sqrt(X * X + Y * Y + Z * Z);

        public Vector(double x, double y, double z) { X = x; Y = y; Z = z; }

        public static Vector ByCoordinates(double x, double y, double z) => new Vector(x, y, z);

        public Vector Normalize()
        {
            var len = Length;
            if (len == 0) return new Vector(0,0,0);
            return new Vector(X / len, Y / len, Z / len);
        }

        public static Vector Cross(Vector a, Vector b)
        {
            return new Vector(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X);
        }

        public static Vector Add(Vector a, Vector b) => new Vector(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vector Divide(Vector a, double d) => d == 0 ? new Vector(0,0,0) : new Vector(a.X / d, a.Y / d, a.Z / d);

        public static Vector ZAxis => new Vector(0, 0, 1);

        public bool IsValid => !double.IsNaN(X) && !double.IsNaN(Y) && !double.IsNaN(Z);
    }

    // Very small Mesh stub to support core operations used in this project.
    public class Mesh
    {
        public List<Point> Vertices { get; } = new List<Point>();
        public List<int[]> Faces { get; } = new List<int[]>();

        public int VertexCount => Vertices.Count;
        public int FaceCount => Faces.Count;
        public bool IsValid => VertexCount > 0 && FaceCount > 0;

        public void AddVertex(Point p) => Vertices.Add(p);
        public void AddFace(int a, int b, int c) => Faces.Add(new[] { a, b, c });
        public void AddQuadFace(int a, int b, int c, int d) => Faces.Add(new[] { a, b, c, d });

        public Point VertexAt(int i) => Vertices[i];
        public int[] GetFaceIndices(int i) => Faces[i];
    }
}
#endif
    // Minimal Point stub used by core algorithms.
    public class Point
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }













































#endif}    }        public int[] GetFaceIndices(int i) => Faces[i];
n        public Point VertexAt(int i) => Vertices[i];        public void AddFace(int a, int b, int c) => Faces.Add(new[] { a, b, c });
n        public void AddVertex(Point p) => Vertices.Add(p);        public int FaceCount => Faces.Count;
n        public int VertexCount => Vertices.Count;        public List<int[]> Faces { get; } = new List<int[]>();        public List<Point> Vertices { get; } = new List<Point>();    {    public class Mesh
n    // Very small Mesh stub to support core operations used in this project.    }
n        public static Vector ZAxis => new Vector(0, 0, 1);        public static Vector Divide(Vector a, double d) => d == 0 ? new Vector(0,0,0) : new Vector(a.X / d, a.Y / d, a.Z / d);
n        public static Vector Add(Vector a, Vector b) => new Vector(a.X + b.X, a.Y + b.Y, a.Z + b.Z);        }                a.X * b.Y - a.Y * b.X);                a.Z * b.X - a.X * b.Z,                a.Y * b.Z - a.Z * b.Y,            return new Vector(        {
n        public static Vector Cross(Vector a, Vector b)        }            return new Vector(X / len, Y / len, Z / len);            if (len == 0) return new Vector(0,0,0);            var len = Length;        {        public Vector Normalize()
n        public static Vector ByCoordinates(double x, double y, double z) => new Vector(x, y, z);
n        public Vector(double x, double y, double z) { X = x; Y = y; Z = z; }
n        public double Length => Math.Sqrt(X * X + Y * Y + Z * Z);        public double Z { get; }        public double Y { get; }        public double X { get; }    {    public class Vector    // Minimal Vector stub with basic operations.    }        public static Point ByCoordinates(double x, double y, double z) => new Point(x, y, z);n        public Point(double x, double y, double z) { X = x; Y = y; Z = z; }