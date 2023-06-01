using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    TerrainSettings terrainSettings = new TerrainSettings();
    ChunkManager chunkManager = new ChunkManager();

    public ComputeShader terrainShader;
    public ComputeShader renderingShader;

    int render_distance = 5;
    int map_height = 5;

    List<Vector3Int> toGenerate = new List<Vector3Int>();

    bool startedMarchingCubes = false;

    void Start()
    {
        int threads = SystemInfo.processorCount;


        chunkManager.terrainSettings = terrainSettings;
        chunkManager.terrain.terrainSettings = terrainSettings;
        chunkManager.renderer.threads = threads;
        chunkManager.terrain.threads = threads;
        chunkManager.threads = threads;

        chunkManager.terrain.shader = terrainShader;
        chunkManager.renderer.shader = renderingShader;

        terrainSettings.terrain_amplitude = 100;
        terrainSettings.terrain_frequency = 100;  
        terrainSettings.terrain_warp_octaves = 4;

        terrainSettings.biome_frequency = 100;

        terrainSettings.cave_octaves = 5;
        terrainSettings.cave_warp_octaves = 4;

        terrainSettings.biome_seed = 6969420;
        terrainSettings.cave_seed = 42090;
        terrainSettings.terrain_seed = 12345;

        chunkManager.map_height = map_height;
        chunkManager.map_size = 500;

        for(int x = -render_distance; x <= render_distance; x++){
            for(int y = 0; y < map_height; y++){
                for(int z = -render_distance; z <= render_distance; z++){
                    toGenerate.Add(new Vector3Int(x, y, z));
                }
            }
        }

        chunkManager.Start();
        chunkManager.GenerateChunks(toGenerate);
    }

    void Update()
    {
        chunkManager.Update();

        if(chunkManager.finishedGeneraingTerrain && !startedMarchingCubes){
            startedMarchingCubes = true;

            Debug.Log(true);
            chunkManager.RenderChunks(toGenerate);
        }
    }
}
