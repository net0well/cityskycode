using System.Numerics;

namespace CitySky.Spike.RoadMesh;

/// <summary>Grafo mínimo de ruas: nós em 2D (metros) e segmentos entre nós.</summary>
public sealed class RoadGraph
{
    public List<Vector2> Nodes { get; } = new();
    public List<(int A, int B)> Segments { get; } = new();

    public int AddNode(float x, float z)
    {
        Nodes.Add(new Vector2(x, z));
        return Nodes.Count - 1;
    }

    public void Connect(int a, int b) => Segments.Add((a, b));
}

/// <summary>Malha gerada (y = 0, plana no spike): vértices 3D, triângulos e UV.</summary>
public sealed class RoadMesh
{
    public List<Vector3> Vertices { get; } = new();
    public List<int> Triangles { get; } = new();
    public List<Vector2> Uv { get; } = new();

    public int AddVertex(Vector2 p, Vector2 uv)
    {
        Vertices.Add(new Vector3(p.X, 0f, p.Y));
        Uv.Add(uv);
        return Vertices.Count - 1;
    }

    public void Tri(int a, int b, int c)
    {
        Triangles.Add(a);
        Triangles.Add(b);
        Triangles.Add(c);
    }
}

/// <summary>
/// Gera a malha de uma rede de ruas com segmentos retos e interseções de até 4 ramos.
/// Cada ramo é aparado no ponto onde sua borda encontra a borda do ramo vizinho; a interseção
/// é um polígono triangulado em leque a partir do centro do nó. Largura inclui pista + calçadas.
/// </summary>
public sealed class RoadMeshBuilder
{
    private readonly float _halfWidth;

    public RoadMeshBuilder(float widthMeters = 10f) => _halfWidth = widthMeters / 2f;

    public RoadMesh Build(RoadGraph graph)
    {
        var mesh = new RoadMesh();

        // Direções de saída de cada nó, por segmento.
        var outgoing = new List<(int Seg, Vector2 Dir, int Other)>[graph.Nodes.Count];
        for (int i = 0; i < outgoing.Length; i++)
        {
            outgoing[i] = new();
        }

        for (int s = 0; s < graph.Segments.Count; s++)
        {
            (int a, int b) = graph.Segments[s];
            Vector2 ab = graph.Nodes[b] - graph.Nodes[a];
            if (ab.LengthSquared() < 1e-6f)
            {
                continue;
            }

            Vector2 dir = Vector2.Normalize(ab);
            outgoing[a].Add((s, dir, b));
            outgoing[b].Add((s, -dir, a));
        }

        // Aparo de cada ponta de segmento (quanto recuar do nó), por (segmento, nó).
        var trim = new Dictionary<(int Seg, int Node), float>();

        for (int n = 0; n < graph.Nodes.Count; n++)
        {
            List<(int Seg, Vector2 Dir, int Other)> branches = outgoing[n];
            if (branches.Count == 0)
            {
                continue;
            }

            branches.Sort((x, y) => MathF.Atan2(x.Dir.Y, x.Dir.X).CompareTo(MathF.Atan2(y.Dir.Y, y.Dir.X)));
            BuildNode(graph, mesh, n, branches, trim);
        }

        // Segmentos retos (um quad), aparados nas duas pontas.
        for (int s = 0; s < graph.Segments.Count; s++)
        {
            (int a, int b) = graph.Segments[s];
            Vector2 pa = graph.Nodes[a];
            Vector2 pb = graph.Nodes[b];
            Vector2 ab = pb - pa;
            float len = ab.Length();
            if (len < 1e-4f)
            {
                continue;
            }

            Vector2 dir = ab / len;
            Vector2 right = new(dir.Y, -dir.X);
            float ta = trim.GetValueOrDefault((s, a), 0f);
            float tb = trim.GetValueOrDefault((s, b), 0f);
            float usable = len - ta - tb;
            if (usable <= 0.01f)
            {
                continue; // segmento curto demais entre duas interseções
            }

            Vector2 start = pa + dir * ta;
            Vector2 end = pb - dir * tb;
            Vector2 w = right * _halfWidth;

            // UV: U atravessa a largura (0..1), V acompanha o comprimento (metros / largura) → contínua.
            float v0 = ta / (_halfWidth * 2f);
            float v1 = (ta + usable) / (_halfWidth * 2f);
            int rl = mesh.AddVertex(start - w, new Vector2(0f, v0));
            int rr = mesh.AddVertex(start + w, new Vector2(1f, v0));
            int el = mesh.AddVertex(end - w, new Vector2(0f, v1));
            int er = mesh.AddVertex(end + w, new Vector2(1f, v1));

            EmitUp(mesh, rl, el, rr);
            EmitUp(mesh, rr, el, er);
        }

        return mesh;
    }

    private void BuildNode(
        RoadGraph graph, RoadMesh mesh, int node,
        List<(int Seg, Vector2 Dir, int Other)> branches,
        Dictionary<(int Seg, int Node), float> trim)
    {
        Vector2 center = graph.Nodes[node];
        int count = branches.Count;

        if (count == 1)
        {
            // Fim de linha: não apara; o segmento vai até o nó (sem tampa no spike).
            trim[(branches[0].Seg, node)] = 0f;
            return;
        }

        // Para cada par angularmente adjacente, o canto é onde a borda esquerda de um ramo
        // encontra a borda direita do próximo (no sentido anti-horário).
        var corner = new Vector2[count];
        for (int k = 0; k < count; k++)
        {
            var cur = branches[k];
            var nxt = branches[(k + 1) % count];
            Vector2 leftCur = new(-cur.Dir.Y, cur.Dir.X);   // esquerda do ramo atual
            Vector2 rightNxt = new(nxt.Dir.Y, -nxt.Dir.X);  // direita do próximo
            Vector2 pA = center + leftCur * _halfWidth;
            Vector2 pB = center + rightNxt * _halfWidth;

            if (LineIntersect(pA, cur.Dir, pB, nxt.Dir, out Vector2 x))
            {
                corner[k] = x;
            }
            else
            {
                // Ramos quase colineares (passagem reta): borda a meia largura, sem aparo.
                corner[k] = (pA + pB) * 0.5f;
            }
        }

        // Aparo de cada ramo = maior projeção dos seus dois cantos sobre a sua direção (≥ 0).
        for (int k = 0; k < count; k++)
        {
            var cur = branches[k];
            float t1 = Math.Max(0f, Vector2.Dot(corner[k] - center, cur.Dir));
            float t2 = Math.Max(0f, Vector2.Dot(corner[(k - 1 + count) % count] - center, cur.Dir));
            trim[(cur.Seg, node)] = Math.Max(t1, t2);
        }

        // Polígono da interseção: para cada ramo, seus dois pontos de borda no aparo, em ordem.
        var boundary = new List<Vector2>(count * 2);
        foreach (var b in branches)
        {
            float t = trim[(b.Seg, node)];
            Vector2 right = new(b.Dir.Y, -b.Dir.X);
            Vector2 at = center + b.Dir * t;
            boundary.Add(at + right * _halfWidth);  // direita
            boundary.Add(at - right * _halfWidth);  // esquerda
        }

        // Remove pontos de borda praticamente coincidentes (cantos onde a borda de um ramo
        // encontra a do vizinho), que gerariam triângulos de área ~0.
        var distinct = new List<Vector2>(boundary.Count);
        foreach (Vector2 p in boundary)
        {
            if (distinct.Count == 0 || Vector2.DistanceSquared(distinct[^1], p) > 1e-4f)
            {
                distinct.Add(p);
            }
        }

        if (distinct.Count >= 2 && Vector2.DistanceSquared(distinct[0], distinct[^1]) <= 1e-4f)
        {
            distinct.RemoveAt(distinct.Count - 1);
        }

        if (distinct.Count < 3)
        {
            return; // passagem reta: sem polígono de interseção
        }

        // Triangulação em leque a partir do centro (polígono em estrela em relação ao nó).
        float uvScale = 1f / (_halfWidth * 2f);
        int centerIdx = mesh.AddVertex(center, new Vector2(0.5f, 0.5f));
        var ring = new int[distinct.Count];
        for (int i = 0; i < distinct.Count; i++)
        {
            Vector2 rel = (distinct[i] - center) * uvScale + new Vector2(0.5f, 0.5f);
            ring[i] = mesh.AddVertex(distinct[i], rel);
        }

        for (int i = 0; i < distinct.Count; i++)
        {
            EmitUp(mesh, centerIdx, ring[i], ring[(i + 1) % distinct.Count]);
        }
    }

    /// <summary>Emite um triângulo com a normal para cima; descarta se a área for ~0.</summary>
    private static void EmitUp(RoadMesh mesh, int a, int b, int c)
    {
        Vector3 va = mesh.Vertices[a];
        Vector3 vb = mesh.Vertices[b];
        Vector3 vc = mesh.Vertices[c];
        Vector3 normal = Vector3.Cross(vb - va, vc - va);
        if (normal.LengthSquared() < 1e-8f)
        {
            return; // degenerado
        }

        if (normal.Y >= 0f)
        {
            mesh.Tri(a, b, c);
        }
        else
        {
            mesh.Tri(a, c, b);
        }
    }

    private static bool LineIntersect(Vector2 p1, Vector2 d1, Vector2 p2, Vector2 d2, out Vector2 result)
    {
        float denom = d1.X * d2.Y - d1.Y * d2.X;
        if (MathF.Abs(denom) < 1e-5f)
        {
            result = default;
            return false;
        }

        float t = ((p2.X - p1.X) * d2.Y - (p2.Y - p1.Y) * d2.X) / denom;
        result = p1 + d1 * t;
        return true;
    }

    private static float Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;
}
