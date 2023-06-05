using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    TerrainSettings terrainSettings = new TerrainSettings();
    ChunkManager chunkManager = new ChunkManager();

    Material material;

    public ComputeShader terrainShader;
    public ComputeShader renderingShader;

    int threads;
    int render_distance = 5;
    int render_height = 20;
    int map_height = 5;

    List<Vector3Int> toGenerate = new List<Vector3Int>();
    List<Mesh> meshes = new List<Mesh>();

    bool finishedRendering = false;
    bool startedRendering = false;

    void Start()
    {
        GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Plane);
        material = primitive.GetComponent<MeshRenderer>().sharedMaterial;
        DestroyImmediate(primitive);

        threads = SystemInfo.processorCount - 1;

        chunkManager.terrainSettings = terrainSettings;
        chunkManager.terrain.terrainSettings = terrainSettings;
        chunkManager.renderer.threads = threads;
        chunkManager.terrain.threads = threads;
        chunkManager.threads = threads;

        chunkManager.terrain.shader = terrainShader;
        chunkManager.renderer.shader = renderingShader;

        terrainSettings.terrain_amplitude = 15;
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

        Vector3Int playerPosition = new Vector3Int(0, 20, 0);

        for (int x = -render_distance; x <= render_distance; x++)
        {
            for (int y = 0; y < map_height; y++)
            {
                for (int z = -render_distance; z <= render_distance; z++)
                {
                    toGenerate.Add(new Vector3Int(x, y, z));
                    MakeMesh(x, y, z);
                }
            }
        }

        StartCoroutine(startRunning());
    }

    void MakeMesh(int x, int y, int z)
    {
        Mesh mesh = new Mesh();

        GameObject chunk = new GameObject("Chunk");
        chunk.transform.position = new Vector3(x, y, z) * 8;

        MeshRenderer meshRenderer = chunk.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = chunk.AddComponent<MeshFilter>();
        Collider collider = chunk.AddComponent<MeshCollider>();

        meshes.Add(mesh);
        meshRenderer.sharedMaterial = material;
        meshFilter.mesh = mesh;
    }

    IEnumerator startRunning()
    {
        yield return new WaitForSeconds(3f);
        Debug.Log("Started");

        chunkManager.Start();
        chunkManager.GenerateChunks(toGenerate);
    }

    void Update()
    {
        chunkManager.Update();

        if(chunkManager.generatingChunks && chunkManager.finishedGeneratingChunks && !startedRendering){
            chunkManager.generatingChunks = false;
            chunkManager.finishedGeneratingChunks = false;
            startedRendering = true;

            Debug.Log("Started rendering");
            chunkManager.RenderChunks(toGenerate);
        }

        if(chunkManager.finishedRenderingChunks && !finishedRendering){
            chunkManager.renderingChunks = false;
            chunkManager.finishedRenderingChunks = false;
            finishedRendering = true;

            Debug.Log("Finished rendering");
        }
    }
}
