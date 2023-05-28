using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;
using Unity.Collections;
using System.Collections;
using System.Collections.Generic;

public class TerrainGenerator {
    public ComputeShader shader;

    ComputeBuffer buffer2D;
    ComputeBuffer buffer3D;
    List<Chunk> chunksUsed;
    List<Chunk> outputChunks;
    TerrainSettings settingsUsed;
    CustomThreading threading = new CustomThreading();

    AsyncGPUReadbackRequest request2D;
    AsyncGPUReadbackRequest request3D;

    float[] result2D;
    float[] result3D;

    bool isWorking = false;
    bool finishedWorking = false;

    public void setToGenerate(List<Chunk> chunks, TerrainSettings settings){
        isWorking = true;
        finishedWorking = false;
        settingsUsed = settings;
        chunksUsed = chunks;

        List<Chunk> chunks2D = chunks.Where(x => x.position.y == 0).ToList();
        Vector3Int[] chunksRaw = new Vector3Int[chunks2D.Count];
        ComputeBuffer chunksBuffer = new ComputeBuffer(chunks2D.Count, sizeof(int) * 3);
        buffer2D = new ComputeBuffer(64 * chunks2D.Count, sizeof(float));
        buffer3D = new ComputeBuffer(512 * chunks.Count, sizeof(float));

        for(int i = 0; i < chunks2D.Count; i++){
            chunksRaw[i] = chunks2D[i].position;
        }

        chunksBuffer.SetData(chunksRaw);
        shader.SetBuffer(0, "Chunks", chunksBuffer);
        shader.SetBuffer(0, "Result", buffer2D);
        shader.SetInt("terrain_amplitude", settings.terrain_amplitude);
        shader.SetInt("terrain_scale", settings.terrain_scale);

        shader.Dispatch(0, chunks2D.Count, 1, 1);

        request2D = AsyncGPUReadback.Request(buffer2D);
        chunksBuffer.Release();
    }
    
    void work3D(){
        Vector3Int[] chunksRaw = new Vector3Int[chunksUsed.Count];
        for(int i = 0; i < chunksUsed.Count; i++){
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

    public void transformTerrain(int index){

    }

    public void Update(){
        if(request3D.done && !request3D.hasError){
            result3D = request3D.GetData<float>().ToArray();
            buffer3D.Release();

            threading.setData();
            threading.func = transformTerrain;

            isWorking = false;
            finishedWorking = true;
        }

        if(request2D.done && !request2D.hasError){
            result2D = request2D.GetData<float>().ToArray();
            buffer2D.Release();

            work3D();
        }
    }
}