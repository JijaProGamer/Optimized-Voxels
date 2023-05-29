using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    public ComputeShader terrainShader;
    public ComputeShader marchingShader;

    ChunkManager chunks = new ChunkManager();
    TerrainSettings settings = new TerrainSettings();

    bool finishedConstructingTerrain = false;
    bool startedMarchingCubes = false;
    bool finishedMarchingCubes = false;

    int mapSize = 500;
    int mapHeight = 40;
    int renderDistance = 16;

    void Start()
    {
        int cpu_threads = SystemInfo.processorCount;


        settings.terrain_amplitude = 150;
        settings.terrain_scale = 100;

        chunks.terrainShader = terrainShader;
        chunks.marchingShader = marchingShader;

        chunks.__init(mapSize, mapHeight, cpu_threads);
        chunks.settings = settings;

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
        if(!startedMarchingCubes && finishedConstructingTerrain){
            startedMarchingCubes = true;
            Debug.Log(true);
        }

        chunks.Update();

        if(chunks.finishedSavingChunks){
            finishedConstructingTerrain = true;
        }
    }
}
