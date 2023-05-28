using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    public ComputeShader terrainShader;
    public ComputeShader marchingShader;

    ChunkManager chunks = new ChunkManager();
    TerrainSettings settings = new TerrainSettings();

    int mapSize = 500;
    int mapHeight = 20;
    int renderDistance = 4;

    void Start()
    {
        settings.terrain_amplitude = 150;
        settings.terrain_scale = 100;

        chunks.terrainShader = terrainShader;
        chunks.marchingShader = marchingShader;

        chunks.__init(mapSize, mapHeight);
        chunks.settings = settings;

        Chunk e = new Chunk();
        e.__init();

        List<Chunk> chunksToConstruct = new List<Chunk>();

        for (int x = -renderDistance; x < renderDistance; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                for (int z = -renderDistance; z < renderDistance; z++) {
                    Chunk chunk = new Chunk();
                    chunk.position = new Vector3Int(x, y, z);
                    chunk.__init();

                    chunksToConstruct.Add(chunk);
                    chunks.SetChunk(new Vector3Int(x, y, z), chunk);
                }
            }
        }

        chunks.constructTerrain(chunksToConstruct);
    }

    void Update()
    {
        chunks.Update();
    }
}
