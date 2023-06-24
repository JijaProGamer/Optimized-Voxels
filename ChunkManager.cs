using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class ChunkManager
{
    // Internal

    public ChunkList chunks = new ChunkList();
    public ChunkSliceList chunkSlices = new ChunkSliceList();
    public MeshPool meshPool = new MeshPool();
    public Renderer renderer = new Renderer();
    public TerrainGenerator terrainGenerator = new TerrainGenerator();
    public TerrainSettings terrainSettings;

    // Public

    public int map_size;
    public int map_height;

    public bool generatingChunks;
    public bool finishedGeneratingChunks;

    public bool renderingChunks;
    public bool finishedRenderingChunks;

    public void Start()
    {
        chunks.Start(map_height, map_size);
        chunkSlices.Start(map_size);
    }

    public void Update()
    {
        if (generatingChunks && terrainGenerator.finished)
        {
            generatingChunks = true;
            finishedGeneratingChunks = true;
        }

        if (renderingChunks && renderer.finished)
        {
            renderingChunks = false;
            for (int i = 0; i < renderer.chunksToTransform.Count; i++)
            {
                Chunk chunk = renderer.chunksToTransform[i];
                if(chunk.mesh.vertices.Count > 0){
                    if(chunk.bareMesh == null){
                        meshPool.GetMesh(chunk, false);
                    } else {
                        chunk.mesh.SetData();
                    }
                }

                if(chunk.bareMesh != null && chunk.mesh.vertices.Count == 0){
                    meshPool.AddMesh(chunk.bareMesh);
                    chunk.bareMesh = null;
                }
            }

            finishedRenderingChunks = true;
        }

        meshPool.Update();
        renderer.Update();
        terrainGenerator.Update();
    }

    public void GenerateChunks(List<Vector2Int> positions)
    {
        if (generatingChunks)
        {
            throw new Exception("Already generating chunks.");
        }

        int minX = map_size;
        int maxX = -map_size;
        int minZ = map_size;
        int maxZ = -map_size;

        for (int i = 0; i < positions.Count; i++)
        {
            Vector2Int position = positions[i];

            if (position.x < minX)
                minX = position.x;
            if (position.x > maxX)
                maxX = position.x;
            if (position.y < minZ)
                minZ = position.y;
            if (position.y > maxZ)
                maxZ = position.y;
        }

        int width = (int)MathF.Abs(minX - maxX) + 2; // TODO: Might use 1
        int length = (int)MathF.Abs(minZ - maxZ) + 2;

        List<Vector2Int> inputPositionsBiomes = new List<Vector2Int>();
        List<Vector2Int> inputPositionsSimple = new List<Vector2Int>();
        List<Vector2Int> inputPositions = new List<Vector2Int>();
        List<int> usedPositions = new List<int>();

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                inputPositionsSimple.Add(new Vector2Int(x, z));
            }
        }

        for (int x = minX; x <= maxX; x++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                int inputPositionIndex = positions.IndexOf(new Vector2Int(x, z));
                inputPositions.Add(new Vector2Int(x, z));

                if (inputPositionIndex >= 0)
                {
                    usedPositions.Add(inputPositionIndex);
                }
            }
        }

        for (int x = minX - 1; x <= maxX + 1; x++)
        {
            for (int z = minZ - 1; z <= maxZ + 1; z++)
            {
                /*
                if not file exists
                */
                inputPositionsBiomes.Add(new Vector2Int(x, z));
            }
        }

        terrainGenerator.generate(
            inputPositionsBiomes,
            inputPositionsSimple,
            inputPositions,
            usedPositions,
            width
        );
        generatingChunks = true;
    }

    public void RenderChunks(List<Vector3Int> positions)
    {
        if (renderingChunks)
        {
            throw new Exception("Already rendering chunks.");
        }

        int minX = map_size;
        int maxX = -map_size;
        int minY = map_height;
        int maxY = -map_height;
        int minZ = map_size;
        int maxZ = -map_size;

        for (int i = 0; i < positions.Count; i++)
        {
            Vector3Int position = positions[i];

            if (position.x < minX)
                minX = position.x;
            if (position.x > maxX)
                maxX = position.x;
            if (position.y < minY)
                minY = position.y;
            if (position.y > maxY)
                maxY = position.y;
            if (position.z < minZ)
                minZ = position.z;
            if (position.z > maxZ)
                maxZ = position.z;
        }

        int width = (int)MathF.Abs(minX - maxX) + 1; // TODO: Might use 1
        int height = (int)MathF.Abs(minY - maxY) + 1;
        int length = (int)MathF.Abs(minZ - maxZ) + 1;

        float[] densities = new float[(width * height * length) * 512];
        List<Chunk> usedChunks = new List<Chunk>();
        List<int> usedPositions = new List<int>();

        Vector3Int[] inputPositionsSimple = new Vector3Int[width * height * length];

        int simpleX = -1;
        int simpleY = -1;
        int simpleZ = -1;

        for (int x = minX; x <= maxX; x++)
        {
            simpleX += 1;
            simpleY = -1;
            for (int y = minY; y <= maxY; y++)
            {
                simpleY += 1;
                simpleZ = -1;
                for (int z = minZ; z <= maxZ; z++)
                {
                    simpleZ += 1;

                    int index = simpleX + width * (simpleY + height * simpleZ);
                    int start = index * 512;
                    Chunk chunk = chunks[x, y, z];

                    for (int i = 0; i < 512; i++)
                    {
                        densities[i + start] = chunk.voxels[i].density;
                    }

                    inputPositionsSimple[index] = new Vector3Int(simpleX, simpleY, simpleZ);
                    int inputPositionIndex = positions.IndexOf(new Vector3Int(x, y, z));

                    if (inputPositionIndex >= 0)
                    {
                        usedChunks.Add(chunk);
                        usedPositions.Add(index);
                    }
                }
            }
        }

        renderer.generate(densities, width, height, usedChunks, inputPositionsSimple, usedPositions);

        renderingChunks = true;
    }
}
