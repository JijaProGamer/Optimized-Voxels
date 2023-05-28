using UnityEngine;
using System.Collections;
using System.Collections.Generic; 

public class ChunkManager
{
    Chunk[] chunks;
    int middleSize;
    int middleHeight;
    int middleIndex;

    public ComputeShader terrainShader;
    public ComputeShader marchingShader;

    public TerrainSettings settings = new TerrainSettings();
    TerrainGenerator terrainGenerator = new TerrainGenerator();
    MarchingCubes marchingCubes = new MarchingCubes();

    public void __init(int mapSize, int mapHeight)
    {
        middleSize = mapSize / 2;
        middleHeight = mapHeight / 2;
        middleIndex = middleHeight * middleSize * middleSize;

        chunks = new Chunk[mapSize * mapHeight * mapSize];

        terrainGenerator.shader = terrainShader;
    }

    public Chunk GetChunk(Vector3Int position){
        return chunks[middleIndex + (position.x + middleSize * (position.y + middleHeight * position.z))];
    }

    public void SetChunk(Vector3Int position, Chunk chunk){
        chunks[middleIndex + (position.x + middleSize * (position.y + middleHeight * position.z))] = chunk;
    }

    public void constructTerrain(List<Chunk> chunksToConstruct)
    {
        for (int i = 0; i < chunksToConstruct.Count; i++)
        {
            
        }

        terrainGenerator.setToGenerate(chunksToConstruct, settings);
    }

    public void Update()
    {
        terrainGenerator.Update();
        //marchingCubes.Update();
    }
}