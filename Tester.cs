using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    TerrainSettings terrainSettings = new TerrainSettings();
    ChunkManager chunkManager = new ChunkManager();

    public ComputeShader terrainShader;
    public ComputeShader renderingShader;

    public int render_distance = 3;
    public int map_height = 3;

    List<Vector3Int> toGenerate = new List<Vector3Int>();

    void Start()
    {
        int threads = SystemInfo.processorCount - 1;


        chunkManager.terrainSettings = terrainSettings;
        chunkManager.terrain.terrainSettings = terrainSettings;
        chunkManager.terrain.threads = threads;
        chunkManager.threads = threads;

        chunkManager.terrain.shader = terrainShader;
        chunkManager.renderer.shader = renderingShader;

        terrainSettings.terrain_amplitude = 100;
        terrainSettings.terrain_scale = 100;  

        terrainSettings.biome_frequency = 100;

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
    }
}
