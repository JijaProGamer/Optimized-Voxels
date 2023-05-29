using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;
using Unity.Collections;
using System.Collections;
using System.Collections.Generic;

struct Triangle {
    public Vector3 a, b, c;
    public int exists;
};

public class MarchingCubes
{
    public ComputeShader shader;
    public int cpu_threads;

    ComputeBuffer buffer;
    List<Chunk> chunksUsed;
    ChunkMesh[] inputMeshes;
    public ChunkMesh[] outputMeshes;

    CustomThreading threading = new CustomThreading();

    AsyncGPUReadbackRequest request;

    Triangle[] result;

    public bool isWorking = false;
    public bool finishedWorking = false;
    public bool savedMeshes = false;

    public void setToGenerate(List<Chunk> chunks)
    {
        isWorking = true;
        finishedWorking = false;
        chunksUsed = chunks;
        outputMeshes = new ChunkMesh[chunks.Count];
        inputMeshes = new ChunkMesh[chunks.Count];

        Vector3Int[] chunksRaw = new Vector3Int[chunks.Count];
        float[] densitiesRaw = new float[chunks.Count * 512];
        ComputeBuffer chunksBuffer = new ComputeBuffer(chunks.Count, sizeof(int) * 3);
        ComputeBuffer densitiesBuffer = new ComputeBuffer(chunks.Count * 512, sizeof(float));
        buffer = new ComputeBuffer(512 * 5 * chunks.Count, sizeof(float) * 9 + sizeof(uint));

        for (int i = 0; i < chunks.Count; i++)
        {
            chunksRaw[i] = chunks[i].position;
            inputMeshes[i] = chunks[i].mesh;

            for(int j = 0; j < 512; j++){
                densitiesRaw[i * 512 + j] = chunks[i].getVoxel(j).density;
            }
        }

        chunksBuffer.SetData(chunksRaw);
        densitiesBuffer.SetData(densitiesRaw);
        shader.SetBuffer(0, "Chunks", chunksBuffer);
        shader.SetBuffer(0, "Densities", densitiesBuffer);
        shader.SetBuffer(0, "Result", buffer);
        /*shader.SetInt("terrain_amplitude", settings.terrain_amplitude);
        shader.SetInt("terrain_scale", settings.terrain_scale);*/

        shader.Dispatch(0, chunks.Count, 1, 1);

        request = AsyncGPUReadback.Request(buffer);
        chunksBuffer.Release();
    }

    public void finishedTransforming(){
        savedMeshes = true;
    }

    public void transformTriangles(int index)
    {
        ChunkMesh mesh = inputMeshes[index];

        int startIndex = index * 512 * 5;
        int endIndex = (index + 1) * 512 * 5;

        for(int i = startIndex; i < endIndex; i++){
            Triangle tri = result[i];

            if(tri.exists == 1){
                mesh.vertices.Add(tri.a);
                mesh.vertices.Add(tri.b);
                mesh.vertices.Add(tri.c);

                mesh.triangles.Add(mesh.vertices.Count - 3);
                mesh.triangles.Add(mesh.vertices.Count - 2);
                mesh.triangles.Add(mesh.vertices.Count - 1);
            }
        }

        //mesh.setData();
        outputMeshes[index] = mesh;
    }

    public void Update()
    {
        if (request.done && !request.hasError)
        {
            result = request.GetData<Triangle>().ToArray();
            buffer.Release();

            threading.setData(cpu_threads, 25, chunksUsed.Count);
            threading.func = transformTriangles;
            threading.finished = finishedTransforming;

            isWorking = false;
            finishedWorking = true;
        }

        threading.Update();
    }
}
