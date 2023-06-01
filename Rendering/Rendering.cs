using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

struct Triangle {
    Vector3 a, b, c;
    uint chunk;
}

public class Rendering {
    public int threads;
    public ComputeShader shader;

    public List<Chunk> chunksToTransform;
    Triangle[] result;

    CustomThreading threading = new CustomThreading();

    ComputeBuffer outputBuffer;
    AsyncGPUReadbackRequest request;

    bool started;
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
                float height = result2D[baseIndex + (x + 8 * z)];

                for(int subchunkIndex = 0; subchunkIndex < subchunks.Count; subchunkIndex++){
                    Chunk subchunk = subchunks[subchunkIndex];
                    int subchunkMaxIndex = chunksToTransform3D.FindIndex(chunk => chunk.position == subchunk.position);
                    int maxIndex = subchunkMaxIndex * 512;

                    for(int y = 0; y < 8; y++){
                        int real_y = y + subchunkIndex * 8;
                        voxelResult value = result3D[maxIndex + (x + 8 * (y + 8 * z))]; 
                        Voxel voxel = new Voxel();

                        voxel.color = new Color32((byte) value.color_r, (byte) value.color_g, (byte) value.color_g, 0);
                        voxel.density = value.density;

                        subchunk[x, y, z] = voxel;
                    }
                }
            }
        }
    }

    public void Update(){
       if(started2D && request2D.done && !request2D.hasError && !started3D){
            result = request.GetData<Triangle>().ToArray();

            
       }

       if(started3D && request3D.done && !request3D.hasError && !finished_requests){
            finished_requests = true;
            result3D = request3D.GetData<voxelResult>().ToArray();
            
            threading.finished = () => {finished = true;};
            threading.func = transformChunk;
            threading.setData(threads, 25, chunksToTransform.Count);
       }

       threading.Update();
    }

    public void generate(float[] densities, List<Chunk> chunks, List<Vector3Int> positions){
        outputBuffer = new ComputeBuffer(positions.Count * 512, sizeof(float) * 3 + sizeof(uint));
        ComputeBuffer positionBuffer = new ComputeBuffer(positions.Count, sizeof(int) * 3);
        positionBuffer.SetData(positions);

        setShaderVariables();

        shader.SetBuffer(0, "positions", position2DBuffer);
        shader.SetBuffer(0, "Result2D", output2DBuffer);

        shader.Dispatch(0, D2ToGenerate.Count, 1, 1);
        request = AsyncGPUReadback.Request(outputBuffer);
        positionBuffer.Release();
    }
};