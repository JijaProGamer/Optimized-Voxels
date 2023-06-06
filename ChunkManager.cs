using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class ChunkManager
{
    // Internal

    Chunk[] chunks;
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

    public Chunk this[int x, int y, int z]
    {
        get
        {
            return chunks[(x + map_size / 2) + map_size * (y + (z + map_size / 2) * map_height)];
        }
        set
        {
            chunks[(x + map_size / 2) + map_size * (y + (z + map_size / 2) * map_height)] = value;
        }
    }

    public Chunk this[int i]
    {
        get { return chunks[i]; }
        set { chunks[i] = value; }
    }

    public void Start()
    {
        chunks = new Chunk[map_height * map_size * map_size];
    }

    public void Update()
    {
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

        List<Vector2Int> inputPositionsSimple = new List<Vector2Int>();
        List<Vector2Int> inputPositions = new List<Vector2Int>();
        List<int> usedPositions = new List<int>();

        for(int x = 0; x < width; x++){
            for(int z = 0; z < length; z++){
                inputPositionsSimple.Add(new Vector2Int(x, z));
            }
        }

        for(int x = minX; x < maxX; x++){
            for(int z = minZ; z < maxZ; z++){
                int inputPositionIndex = positions.IndexOf(new Vector2Int(x, z));
                inputPositions.Add(new Vector2Int(x, z));

                if(inputPositionIndex >= 0){
                    usedPositions.Add(inputPositionIndex);
                }
            }
        };

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

        int width = (int)MathF.Abs(minX - maxX) + 1;
        int height = (int)MathF.Abs(minY - maxY) + 1;
        int length = (int)MathF.Abs(minZ - maxZ) + 1;

        renderingChunks = true;
    }
}
