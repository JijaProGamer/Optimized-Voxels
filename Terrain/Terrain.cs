using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

using Unity.Mathematics;

struct voxelResult {
    public float density;
    public int color_r, color_g, color_b;
}

public class TerrainGenerator {
    public int threads;
    public ComputeShader shader;
    public TerrainSettings terrainSettings = new TerrainSettings();

    public List<Vector3Int> D3ToGenerate;
    public List<Vector3Int> D2ToGenerate;
    public List<Chunk> chunksToTransform;
    public List<Chunk> chunksToTransform3D;

    public float[] biomes;
    float[] result2D;
    voxelResult[] result3D;

    CustomThreading threading = new CustomThreading();

    ComputeBuffer position2DBuffer;
    ComputeBuffer position3DBuffer;
    ComputeBuffer output2DBuffer;
    ComputeBuffer output3DBuffer;
    AsyncGPUReadbackRequest request2D;
    AsyncGPUReadbackRequest request3D;

    bool started2D;
    bool started3D;
    public bool finished;
    public bool finished_requests;

    void transformChunk(int i){
        Chunk baseChunk = chunksToTransform[i];
        List<Chunk> subchunks = chunksToTransform3D
            .Where(chunk => chunk.position.x == baseChunk.position.x && chunk.position.z == baseChunk.position.z)
            .OrderBy(chunk => chunk.position.y)
            .ToList();

        int baseIndex = i * 64;
        for(int x = 0; x < 8; x++){
            for(int z = 0; z < 8; z++){
                //int height = (int) result2D[baseIndex + (x + 8 * z)];
                Vector3 pos = new Vector3((float) (x + baseChunk.position.x * 8) / 100, 0, (float) (z + baseChunk.position.z * 8) / 100);
                int height = (int) (((noise.snoise(pos) + 1) / 2) * terrainSettings.terrain_amplitude);

                for(int subchunkIndex = 0; subchunkIndex < subchunks.Count; subchunkIndex++){
                    Chunk subchunk = subchunks[subchunkIndex];
                    int subchunkMaxIndex = chunksToTransform3D.FindIndex(chunk => chunk.position == subchunk.position);
                    int maxIndex = subchunkMaxIndex * 512;

                    for(int y = 0; y < 8; y++){
                        int real_y = y + subchunkIndex * 8;
                        voxelResult value = result3D[maxIndex + (x + 8 * (y + 8 * z))]; 
                        Voxel voxel = new Voxel();

                        voxel.color = new Color32((byte) value.color_r, (byte) value.color_g, (byte) value.color_g, 0);
                        
                        if(real_y <= height){
                            voxel.density = 0.8f;
                            //voxel.density = value.density;
                        } else {
                            voxel.density = 0;
                        }

                        subchunk[x, y, z] = voxel;
                    }
                }
            }
        }
    }

    public void Update(){
       if(started2D && request2D.done && !request2D.hasError && !started3D){
            result2D = request2D.GetData<float>().ToArray();
            output2DBuffer.Release();

            generate3D();
       }

       if(started3D && request3D.done && !request3D.hasError && !finished_requests){
            finished_requests = true;
            result3D = request3D.GetData<voxelResult>().ToArray();
            output3DBuffer.Release();
            
            threading.finished = () => {finished = true;};
            threading.func = transformChunk;
            threading.setData(threads, 25, chunksToTransform.Count);
       }

       threading.Update();
    }

    public void generate2D(){
        started2D = true;

        output2DBuffer = new ComputeBuffer(D2ToGenerate.Count * 64, sizeof(float));
        output3DBuffer = new ComputeBuffer(D3ToGenerate.Count * 512, sizeof(float) + sizeof(int) * 3);
        position2DBuffer = new ComputeBuffer(D2ToGenerate.Count, sizeof(int) * 3);
        position3DBuffer = new ComputeBuffer(D3ToGenerate.Count, sizeof(int) * 3);

        position2DBuffer.SetData(D2ToGenerate);
        position3DBuffer.SetData(D3ToGenerate);

        shader.SetFloat("frequency", terrainSettings.terrain_frequency);
        shader.SetInt("octaves", terrainSettings.terrain_octaves);
        shader.SetInt("amplitude", terrainSettings.terrain_amplitude);
        shader.SetInt("seed", terrainSettings.terrain_seed);

        shader.SetBuffer(0, "positions", position2DBuffer);
        shader.SetBuffer(0, "Result2D", output2DBuffer);

        shader.Dispatch(0, D2ToGenerate.Count, 1, 1);
        request2D = AsyncGPUReadback.Request(output2DBuffer);
        position2DBuffer.Release();
    }

    public void generate3D(){
        started3D = true;

        shader.SetFloat("frequency", terrainSettings.cave_frequency);
        shader.SetInt("octaves", terrainSettings.cave_octaves);
        shader.SetInt("seed", terrainSettings.cave_seed);

        shader.SetBuffer(1, "positions", position3DBuffer);
        shader.SetBuffer(1, "Result3D", output3DBuffer);

        shader.Dispatch(1, D3ToGenerate.Count, 1, 1);
        request3D = AsyncGPUReadback.Request(output3DBuffer);
        position3DBuffer.Release();
    }
};