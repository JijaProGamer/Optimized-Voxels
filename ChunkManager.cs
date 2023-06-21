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
        if(generatingChunks && terrainGenerator.finished){
            finishedGeneratingChunks = true;
        }

        if(renderingChunks && renderer.finished){
            finishedRenderingChunks = true;
        }

        meshPool.threading.Update();
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

        int width = (int)MathF.Abs(minX - maxX) + 1;
        int length = (int)MathF.Abs(minZ - maxZ) + 1;

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

        for (int x = minX - 1; x < maxX + 1; x++)
        {
            for (int z = minZ - 1; z < maxZ + 1; z++)
            {

                /*
                if not file exists
                */
                inputPositionsBiomes.Add(new Vector2Int(x, z));
            }
        }

        terrainGenerator.generate(inputPositionsBiomes, inputPositionsSimple, inputPositions, usedPositions, width);
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
        int minY = -1;
        int maxY = map_height;

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
        }

        int width = (int)MathF.Abs(minX - maxX) + 1;
        int height = (int)MathF.Abs(minY - maxY) + 1;

        renderingChunks = true;
    }
}
