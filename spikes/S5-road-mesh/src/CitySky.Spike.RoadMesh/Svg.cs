using System.Globalization;
using System.Numerics;
using System.Text;

namespace CitySky.Spike.RoadMesh;

/// <summary>Render top-down simples (SVG) das malhas, para conferência visual rápida sem a engine.</summary>
public static class Svg
{
    public static void WriteGrid(string path, IReadOnlyList<(string Name, RoadMesh Mesh)> items)
    {
        const int cell = 220;
        const int cols = 4;
        int rows = (items.Count + cols - 1) / cols;
        var sb = new StringBuilder();
        sb.AppendLine($"<svg xmlns='http://www.w3.org/2000/svg' width='{cols * cell}' height='{rows * cell}' style='background:#0d141c;font-family:sans-serif'>");

        for (int i = 0; i < items.Count; i++)
        {
            int cx = (i % cols) * cell;
            int cy = (i / cols) * cell;
            sb.AppendLine($"<g transform='translate({cx},{cy})'>");
            sb.AppendLine($"<rect x='2' y='2' width='{cell - 4}' height='{cell - 4}' fill='#111a24' stroke='#223142'/>");
            DrawMesh(sb, items[i].Mesh, cell);
            string label = System.Security.SecurityElement.Escape(items[i].Name);
            sb.AppendLine($"<text x='8' y='{cell - 8}' fill='#8299ad' font-size='11'>{label}</text>");
            sb.AppendLine("</g>");
        }

        sb.AppendLine("</svg>");
        File.WriteAllText(path, sb.ToString());
    }

    private static void DrawMesh(StringBuilder sb, RoadMesh mesh, int cell)
    {
        // Escala para caber a malha na célula (centrada).
        float min = float.MaxValue, max = float.MinValue;
        foreach (Vector3 v in mesh.Vertices)
        {
            min = Math.Min(min, Math.Min(v.X, v.Z));
            max = Math.Max(max, Math.Max(v.X, v.Z));
        }

        float span = Math.Max(1f, max - min);
        float scale = (cell - 40) / span;
        float off = 20 - min * scale;

        for (int t = 0; t < mesh.Triangles.Count; t += 3)
        {
            Vector3 a = mesh.Vertices[mesh.Triangles[t]];
            Vector3 b = mesh.Vertices[mesh.Triangles[t + 1]];
            Vector3 c = mesh.Vertices[mesh.Triangles[t + 2]];
            string pts = string.Join(' ',
                P(a, scale, off, cell), P(b, scale, off, cell), P(c, scale, off, cell));
            sb.AppendLine($"<polygon points='{pts}' fill='#2a4a5f' stroke='#3cc2b0' stroke-width='0.5'/>");
        }
    }

    private static string P(Vector3 v, float scale, float off, int cell)
    {
        float x = v.X * scale + off;
        float y = cell - (v.Z * scale + off); // inverte Z para ficar "para cima"
        return x.ToString("F1", CultureInfo.InvariantCulture) + "," + y.ToString("F1", CultureInfo.InvariantCulture);
    }
}
