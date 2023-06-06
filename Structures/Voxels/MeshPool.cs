using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshPool {
    public CustomThreading threading = new CustomThreading();
    List<ChunkMesh> meshes = new List<ChunkMesh>();
    int used = 0;
    int existing = 0;

    public void Init(){
        threading.useMainThread = true;
    }

    public void ReturnMesh(ChunkMesh newMesh){
        newMesh.Reset();
        newMesh.chunkObject.SetActive(false);

        used--;
        meshes.Add(newMesh);
    }

    public void AddMesh(ChunkMesh newMesh){
        newMesh.Reset();
        newMesh.chunkObject.SetActive(false);

        existing++;
        meshes.Add(newMesh);
    }

    public ChunkMesh GetMesh(Vector3Int position){
        ChunkMesh mesh = meshes[0];
        meshes.RemoveAt(0);

        mesh.chunkObject.SetActive(true);
        mesh.chunkObject.transform.position = position * 8;

        used++;
        return mesh;
    }

   public void GenerateMeshes(int number){
    threading.setData(2, 3, number);
   }
};