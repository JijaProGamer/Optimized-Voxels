using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkMesh {
    public Mesh mesh;
    public List<Color32> colors;
    public List<Vector3> vertices;
    public List<Vector3> normals;
    public List<int> triangles;

    public bool wasInit;

    public void __init(){
        wasInit = true;

        colors = new List<Color32>();
        vertices = new List<Vector3>();
        normals = new List<Vector3>();
        triangles = new List<int>();
    }

    public void setData(){
        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.colors32 = colors.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();
    }
};