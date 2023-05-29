using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;
using Unity.Collections;
using System.Collections;
using System.Collections.Generic;

public class TerrainGenerator
{
    public ComputeShader shader;
    public int cpu_threads;
    public int mapHeight;

    ComputeBuffer buffer2D;
    ComputeBuffer buffer3D;
    List<Chunk> chunksUsed;
    List<Chunk> chunksUsed2D;
    Chunk[] outputChunks;

    TerrainSettings settingsUsed;
    CustomThreading threading = new CustomThreading();

    AsyncGPUReadbackRequest request2D;
    AsyncGPUReadbackRequest request3D;

    float[] result2D;
    float[] result3D;

    public bool isWorking = false;
    public bool finishedWorking = false;
    public bool savedChunks = false;

    public void setToGenerate(List<Chunk> chunks, TerrainSettings settings)
    {
        isWorking = true;
        finishedWorking = false;
        settingsUsed = settings;
        chunksUsed = chunks;
        outputChunks = new Chunk[chunks.Count];

        chunksUsed2D = chunks.Where(x => x.position.y == 0).ToList();
        Vector3Int[] chunksRaw = new Vector3Int[chunksUsed2D.Count];
        ComputeBuffer chunksBuffer = new ComputeBuffer(chunksUsed2D.Count, sizeof(int) * 3);
        buffer2D = new ComputeBuffer(64 * chunksUsed2D.Count, sizeof(float));
        buffer3D = new ComputeBuffer(512 * chunks.Count, sizeof(float));

        for (int i = 0; i < chunksUsed2D.Count; i++)
        {
            chunksRaw[i] = chunksUsed2D[i].position;
        }

        chunksBuffer.SetData(chunksRaw);
        shader.SetBuffer(0, "Chunks", chunksBuffer);
        shader.SetBuffer(0, "Result", buffer2D);
        shader.SetInt("terrain_amplitude", settings.terrain_amplitude);
        shader.SetInt("terrain_scale", settings.terrain_scale);

        shader.Dispatch(0, chunksUsed2D.Count, 1, 1);

        request2D = AsyncGPUReadback.Request(buffer2D);
        chunksBuffer.Release();
    }

    void work3D()
    {
        Vector3Int[] chunksRaw = new Vector3Int[chunksUsed.Count];
        for (int i = 0; i < chunksUsed.Count; i++)
        {
            chunksRaw[i] = chunksUsed[i].position;
        }

        ComputeBuffer chunksBuffer = new ComputeBuffer(chunksUsed.Count, sizeof(int) * 3);

        chunksBuffer.SetData(chunksRaw);
        shader.SetBuffer(1, "Chunks", chunksBuffer);
        shader.SetBuffer(1, "Result", buffer3D);
        shader.SetInt("terrain_amplitude", settingsUsed.terrain_amplitude);
        shader.SetInt("terrain_scale", settingsUsed.terrain_scale);

        shader.Dispatch(1, chunksUsed.Count, 1, 1);

        request3D = AsyncGPUReadback.Request(buffer3D);
        chunksBuffer.Release();
    }

    public void finishedTransforming(){
        savedChunks = true;
    }

    public void transformTerrain(int index)
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
        if (request3D.done && !request3D.hasError)
        {
            result3D = request3D.GetData<float>().ToArray();
            buffer3D.Release();

            threading.setData(cpu_threads, 5, chunksUsed2D.Count);
            threading.func = transformTerrain;
            threading.finished = finishedTransforming;

            isWorking = false;
            finishedWorking = true;
        }

        if (request2D.done && !request2D.hasError)
        {
            result2D = request2D.GetData<float>().ToArray();
            buffer2D.Release();

            work3D();
        }

        threading.Update();
    }
}
