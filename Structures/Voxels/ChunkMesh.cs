using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChunkMesh {
    public Mesh mesh;
    public GameObject chunkObject;
    public List<Color32> colors = new List<Color32>();
    public List<Vector3> vertices = new List<Vector3>();
    public List<Vector3> normals = new List<Vector3>();
    public List<int> triangles = new List<int>();

    public void Reset(){
        colors.Clear();
        vertices.Clear();
        normals.Clear();
        triangles.Clear();
    }

    public void SetData(){
        if(triangles.Count > 0){
            //chunkObject.SetActive(true);
            //mesh.normals.Reverse();
            mesh.vertices = vertices.ToArray();
            mesh.normals = normals.ToArray();
            mesh.colors32 = colors.ToArray();
            mesh.triangles = triangles.ToArray();

            mesh.RecalculateNormals();
        } else {
            //chunkObject.SetActive(false);
        }
    }
};