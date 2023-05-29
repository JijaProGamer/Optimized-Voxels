using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;
using Unity.Collections;
using System.Collections;
using System.Collections.Generic;

struct Triangle {
    Vector3 a, b, c;
};

public class MarchingCubes
{
    public ComputeShader shader;
    public int cpu_threads;

    ComputeBuffer buffer;
    List<Chunk> chunksUsed;
    ChunkMesh[] outputMeshes;

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

        Vector3Int[] chunksRaw = new Vector3Int[chunks.Count];
        ComputeBuffer chunksBuffer = new ComputeBuffer(chunks.Count, sizeof(int) * 3);
        ComputeBuffer densitiesBuffer = new ComputeBuffer(chunks.Count, sizeof(int) * 3);
        buffer = new ComputeBuffer(512 * 5 * chunks.Count, sizeof(float) * 9);

        for (int i = 0; i < chunks.Count; i++)
        {
            chunksRaw[i] = chunks[i].position;
        }

        chunksBuffer.SetData(chunksRaw);
        shader.SetBuffer(0, "Chunks", chunksBuffer);
        shader.setBuffer(0, "Densities", densitiesBuffer);
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
        Chunk baseChunk = chunksUsed2D[index];
        List<Chunk> ChunkSlice = chunksUsed
            .Where(x => x.position.x == baseChunk.position.x && x.position.z == baseChunk.position.z)
            .OrderBy(x => x.position.y)
            .ToList();

        int startIndex2D = index * 64;
        int startIndex3D = index * 512;

        for (int x = 0; x < 8; x++)
        {
            for (int z = 0; z < 8; z++) {
                float height = result2D[startIndex2D + (x + 8 * z)];

                for(int y = 0; y < 8 * mapHeight; y++){
                    int subchunkIndex = y / 8;
                    Chunk subchunk = ChunkSlice[subchunkIndex];

                    Voxel voxel = new Voxel();
                    voxel.material = 1;

                    if(y > height){
                        voxel.material = 0;
                        voxel.density = 0;
                    } else {
                        float density = result3D[startIndex3D + (x + 8 * (y + 8 * z))];
                        voxel.density = density;
                    }
                    
                    subchunk.setVoxel(x, y % 8, z, voxel);
                    ChunkSlice[subchunkIndex] = subchunk;
                }
            }
        }

        for(int i = 0; i < ChunkSlice.Count; i++){
            int originalIndex = chunksUsed.FindIndex(x => x.position.x == baseChunk.position.x && x.position.y == ChunkSlice[i].position.y && x.position.z == baseChunk.position.z);
            outputChunks[originalIndex] = ChunkSlice[i];
        }
    }

    public void Update()
    {
        if (request.done && !request.hasError)
        {
            result = request.GetData<Triangle>().ToArray();
            buffer.Release();

            threading.setData(cpu_threads, 100, chunksUsed.Count);
            threading.func = transformTriangles;
            threading.finished = finishedTransforming;

            isWorking = false;
            finishedWorking = true;
        }

        threading.Update();
    }
}
