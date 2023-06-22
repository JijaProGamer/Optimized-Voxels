using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

using Unity.Mathematics;

struct voxelResult
{
    public float density;
    public int color_r,
        color_g,
        color_b;
}

public class TerrainGenerator
{
    public int threads;
    public int map_height;
    public ComputeShader shader;
    public TerrainSettings terrainSettings;
    public ChunkManager parent;

    List<Vector2Int> inputPositionsBiomes;
    List<Vector2Int> inputPositionsSimple;
    List<Vector2Int> inputPositions;
    List<int> usedPositions;

    public List<Chunk> chunksToTransform;
    public List<Chunk> chunksToTransform3D;

    public float[] biomes;
    float[] result2D;
    voxelResult[] result3D;

    CustomThreading threading = new CustomThreading();

    ComputeBuffer positionBuffer;
    ComputeBuffer positionSimpleBuffer;
    ComputeBuffer positionUsedBuffer;
    ComputeBuffer output2DBuffer;
    ComputeBuffer output3DBuffer;
    AsyncGPUReadbackRequest request2D;
    AsyncGPUReadbackRequest request3D;

    bool started2D;
    bool started3D;
    public bool finished;
    bool finished_biomes;
    bool finished_requests;

    int width_used;

    void transformChunk(int i)
    {
        Vector2Int basePosition = inputPositions[usedPositions[i]];
        int baseIndex = i * 64;

        for (int subchunkIndex = 0; subchunkIndex < map_height; subchunkIndex++)
        {
            Chunk subchunk = new Chunk();
            subchunk.position = new Vector3Int(basePosition.x, subchunkIndex, basePosition.y);

            for (int x = 0; x < 8; x++)
            {
                for (int z = 0; z < 8; z++)
                {
                    float height = result2D[baseIndex + (x + 8 * z)];
                    
                    for (int y = 0; y < 8; y++)
                    {
                        int real_y = y + subchunkIndex * 8;
                        int index3D = (x + width_used * (real_y + map_height * z));

                        voxelResult value = result3D[index3D + (x + 8 * (y + 8 * z))];
                        Voxel voxel = new Voxel();

                        voxel.color = new Color32(
                            (byte)value.color_r,
                            (byte)value.color_g,
                            (byte)value.color_b,
                            0
                        );

                        if (real_y < height)
                        {
                            voxel.density = 0.8f;
                            //voxel.density = value.density;
                        }
                        else
                        {
                            voxel.density = 0;
                        }

                        subchunk.voxels[x, y, z] = voxel;
                    }
                }
            }

            parent.chunks[basePosition.x, subchunkIndex, basePosition.y] = subchunk;
        }
    }

    void generateSlice(int i)
    {
       Vector2Int basePosition = inputPositionsBiomes[i];

        ChunkSlice chunkSlice = new ChunkSlice();
        chunkSlice.position = basePosition;

        Debug.Log(basePosition);

        parent.chunkSlices[basePosition.x, basePosition.y] = chunkSlice;
    }

    public void Update()
    {        
        if (finished_biomes && !started2D)
        {
            started2D = true;

            generate2D();
        }

        if (started2D && request2D.done && !request2D.hasError && !started3D)
        {
            result2D = request2D.GetData<float>().ToArray();
            output2DBuffer.Release();

            generate3D();
        }

        if (started3D && request3D.done && !request3D.hasError && !finished_requests)
        {
            finished_requests = true;
            result3D = request3D.GetData<voxelResult>().ToArray();
            output3DBuffer.Release();

            Debug.Log("Start 3D");

            threading.finished = () =>
            {
                finished = true;
            };
            threading.func = transformChunk;
            threading.SetData(threads, 25, usedPositions.Count);
        }

        threading.Update();
    }

    public void generate(
        List<Vector2Int> _inputPositionsBiomes,
        List<Vector2Int> _inputPositionsSimple,
        List<Vector2Int> _inputPositions,
        List<int> _usedPositions,
        int width
    )
    {
        output2DBuffer = new ComputeBuffer(_inputPositions.Count * 64, sizeof(float));
        output3DBuffer = new ComputeBuffer(
            _inputPositions.Count * map_height * 512,
            sizeof(float) + sizeof(int) * 3
        );

        positionBuffer = new ComputeBuffer(_inputPositions.Count, sizeof(int) * 2);

        positionSimpleBuffer = new ComputeBuffer(_inputPositionsSimple.Count, sizeof(int) * 2); // 2D only
        positionUsedBuffer = new ComputeBuffer(_usedPositions.Count, sizeof(int));

        positionBuffer.SetData(_inputPositions);

        positionSimpleBuffer.SetData(_inputPositions);
        positionUsedBuffer.SetData(_usedPositions);

        threading.finished = () =>
        {
            finished_biomes = true;
        };
        threading.func = generateSlice;
        threading.SetData(threads, 100, _inputPositionsBiomes.Count);

        inputPositions = _inputPositions;
        usedPositions = _usedPositions;
        inputPositionsBiomes = _inputPositionsBiomes;
        inputPositionsSimple = _inputPositionsSimple;
        width_used = width;
    }

    public void generate2D()
    {
        started2D = true;

        shader.SetFloat("frequency", terrainSettings.terrain_frequency);
        shader.SetInt("octaves", terrainSettings.terrain_octaves);
        shader.SetInt("amplitude", terrainSettings.terrain_amplitude);
        shader.SetInt("seed", terrainSettings.terrain_seed);

        shader.SetBuffer(0, "positions", positionBuffer);
        shader.SetBuffer(0, "Result2D", output2DBuffer);

        shader.Dispatch(0, usedPositions.Count, 1, 1);
        request2D = AsyncGPUReadback.Request(output2DBuffer);
    }

    public void generate3D()
    {
        started3D = true;

        shader.SetFloat("frequency", terrainSettings.cave_frequency);
        shader.SetInt("octaves", terrainSettings.cave_octaves);
        shader.SetInt("seed", terrainSettings.cave_seed);

        shader.SetBuffer(1, "positions", positionBuffer);
        shader.SetBuffer(1, "Result3D", output3DBuffer);

        shader.Dispatch(1, usedPositions.Count, map_height, 1);
        request3D = AsyncGPUReadback.Request(output3DBuffer);
        positionBuffer.Release();
    }
};
