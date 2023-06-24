using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshPool
{
    public CustomThreading threading = new CustomThreading();
    List<BareMesh> meshes = new List<BareMesh>();
    List<Chunk> needingChunks = new List<Chunk>();
    int used = 0;
    int existing = 0;
    public int needing = 0;
    bool generatingMeshes = false;

    public void Init()
    {
        threading.useMainThread = true;
        threading.finished = ThreadingFinished;
    }

    private void ThreadingFinished()
    {
        //Debug.Log("Amogus");
        generatingMeshes = false;
    }

    public void ReturnMesh(BareMesh newMesh)
    {
        newMesh.chunkObject.SetActive(false);

        used--;
        meshes.Add(newMesh);
    }

    public void AddMesh(BareMesh newMesh)
    {
        newMesh.chunkObject.SetActive(false);

        existing++;
        meshes.Add(newMesh);
    }

    public bool GetMesh(Chunk chunk, bool wasUpdate)
    {
        if (meshes.Count > 0)
        {
            BareMesh mesh = meshes[0];

            meshes.RemoveAt(0);
            setMeshObject(chunk, mesh);
            used++;

            return true;
        }
        else
        {
            if(!wasUpdate){
                needing++;
                needingChunks.Add(chunk);
            }

            return false;
        }
    }

    public void Update()
    {
        if (needing > 0 && !generatingMeshes)
        {
            generatingMeshes = true;
            threading.SetData(2, 3, needing);
            needing = 0;
        }

        if (needingChunks.Count > 0 && meshes.Count > 0)
        {
            while (true)
            {
                if(needingChunks.Count == 0){
                    break;
                }

                bool grabbedMesh = GetMesh(needingChunks[0], true);
                if (!grabbedMesh)
                {
                    break;
                }

                needingChunks.RemoveAt(0);
            }
        }

        threading.Update();
    }

    private void setMeshObject(Chunk chunk, BareMesh mesh)
    {
        mesh.chunkObject.transform.position = chunk.position * 8;
        chunk.mesh.mesh = mesh.mesh;
        chunk.bareMesh = mesh;
        chunk.mesh.SetData();

        mesh.chunkObject.SetActive(true);
    }
};
