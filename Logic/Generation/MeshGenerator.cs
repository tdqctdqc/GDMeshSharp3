using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class MeshGenerator 
{
    private static void JoinLinePoints(Vector2 from, Vector2 to, List<Vector2> triPoints, float thickness)
    {
        var perpendicular = (from - to).Normalized().Rotated(Mathf.Pi / 2f);
        var fromOut = from + perpendicular * .5f * thickness;
        var fromIn = from - perpendicular * .5f * thickness;
        var toOut = to + perpendicular * .5f * thickness;
        var toIn = to - perpendicular * .5f * thickness;
        
        triPoints.Add(fromIn);
        triPoints.Add(fromOut);
        triPoints.Add(toOut);
        triPoints.Add(toIn);
        triPoints.Add(toOut);
        triPoints.Add(fromIn);
    }
    public static MeshInstance2D GetLinesMesh(List<Vector2> froms,
        List<Vector2> tos, float thickness)
    {
        var triPoints = new List<Vector2>();
        for (int i = 0; i < froms.Count; i++)
        {
            JoinLinePoints(froms[i], tos[i], triPoints, thickness);
        }
        var meshInstance = new MeshInstance2D();
        var mesh = GetArrayMesh(triPoints.ToArray());
        meshInstance.Mesh = mesh;
        return meshInstance;
    }
    public static MeshInstance2D GetLineMesh(Vector2 from, Vector2 to, float thickness)
    {
        var meshInstance = new MeshInstance2D();
        var triPoints = new List<Vector2>();
        JoinLinePoints(from, to, triPoints, thickness);
        var mesh = GetArrayMesh(triPoints.ToArray());
        meshInstance.Mesh = mesh;
        return meshInstance;
    }

    public static MeshInstance2D GetCircleMesh(Vector2 center, float radius, int resolution)
    {
        var angleIncrement = Mathf.Pi * 2f / (float) resolution;
        var triPoints = new List<Vector2>();
        for (int i = 0; i < resolution; i++)
        {
            var startAngle = angleIncrement * i;
            var startPoint = center + Vector2.Up.Rotated(startAngle) * radius;
            var endAngle = startAngle + angleIncrement;
            var endPoint = center + Vector2.Up.Rotated(endAngle) * radius;
            triPoints.Add(center);
            triPoints.Add(startPoint);
            triPoints.Add(endPoint);
        }

        var mesh = GetArrayMesh(triPoints.ToArray());
        var meshInstance = new MeshInstance2D();
        meshInstance.Mesh = mesh;
        return meshInstance;
    }

    
    public static Node2D GetGraphMesh<TNode, TEdge>(Graph<TNode, TEdge> graph,
        float thickness,
        Func<TNode, Vector2> getVertexPos,
        Color color,
        Color foreignEdgeColor)
    {
        var node = new Node2D();
        for (var i = 0; i < graph.Elements.Count; i++)
        {
            var e = graph.Elements[i];
            var vertexPos = getVertexPos(e);
            var vertex = GetCircleMesh(vertexPos, thickness * 2f, 12);
            vertex.SelfModulate = color;
            node.AddChild(vertex);
            foreach (var n in graph[e].Neighbors)
            {
                var nPos = getVertexPos(n);
                var edge = GetLineMesh(vertexPos, nPos, thickness);
                edge.SelfModulate = foreignEdgeColor;
                node.AddChild(edge);
                edge.SelfModulate = color;
            }
        }
        return node;
    }
    public static ArrayMesh GetArrayMesh(Vector2[] triPoints, Color[] triColors)
    {
        var arrayMesh = new ArrayMesh();
        var arrays = new Godot.Collections.Array();
        
        arrays.Resize((int)ArrayMesh.ArrayType.Max);

        arrays[(int)ArrayMesh.ArrayType.Vertex] = triPoints;
        if (triColors == null)
            triColors = Enumerable.Range(0, triPoints.Length / 3).Select(i => Colors.White).ToArray();
        arrays[(int)ArrayMesh.ArrayType.Color] = ConvertTriToVertexColors(triColors); 
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

        return arrayMesh; 
    }
    public static ArrayMesh GetArrayMesh(Vector2[] triPoints)
    {
        var arrayMesh = new ArrayMesh();
        var arrays = new Godot.Collections.Array();
        
        arrays.Resize((int)ArrayMesh.ArrayType.Max);

        arrays[(int)ArrayMesh.ArrayType.Vertex] = triPoints;
        var triColors = Enumerable.Range(0, triPoints.Length / 3).Select(i => Colors.White).ToArray();
        arrays[(int)ArrayMesh.ArrayType.Color] = ConvertTriToVertexColors(triColors); 
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

        return arrayMesh; 
    }

    public static Color[] ConvertTriToVertexColors(Color[] triColors)
    {
        if (triColors == null) return null;
        var vertexColors = new Color[triColors.Length * 3];
        for (int i = 0; i < triColors.Length; i++)
        {
            vertexColors[3 * i] = triColors[i];
            vertexColors[3 * i + 1] = triColors[i];
            vertexColors[3 * i + 2] = triColors[i];
        }

        return vertexColors;
    }
}