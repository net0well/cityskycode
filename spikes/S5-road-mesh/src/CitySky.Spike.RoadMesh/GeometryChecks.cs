using System.Numerics;

namespace CitySky.Spike.RoadMesh;

public sealed record MeshChecks(
    int Vertices,
    int Triangles,
    int DegenerateTriangles,
    int DownfacingTriangles,
    int NaNs,
    double MinTriangleAreaM2)
{
    public bool Ok => DegenerateTriangles == 0 && DownfacingTriangles == 0 && NaNs == 0;
}

public static class GeometryChecks
{
    private const double MinArea = 1e-4; // m²

    public static MeshChecks Check(RoadMesh mesh)
    {
        int degenerate = 0, down = 0, nan = 0;
        double minArea = double.MaxValue;

        foreach (Vector3 v in mesh.Vertices)
        {
            if (float.IsNaN(v.X) || float.IsNaN(v.Y) || float.IsNaN(v.Z))
            {
                nan++;
            }
        }

        for (int i = 0; i < mesh.Triangles.Count; i += 3)
        {
            Vector3 a = mesh.Vertices[mesh.Triangles[i]];
            Vector3 b = mesh.Vertices[mesh.Triangles[i + 1]];
            Vector3 c = mesh.Vertices[mesh.Triangles[i + 2]];
            Vector3 normal = Vector3.Cross(b - a, c - a);
            double area = normal.Length() / 2.0;
            if (area < MinArea)
            {
                degenerate++;
            }

            minArea = Math.Min(minArea, area);
            if (normal.Y < 0f)
            {
                down++; // normal apontando para baixo = winding errado
            }
        }

        return new MeshChecks(mesh.Vertices.Count, mesh.Triangles.Count / 3, degenerate, down, nan,
            minArea == double.MaxValue ? 0 : minArea);
    }
}
