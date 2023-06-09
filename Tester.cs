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

    List<Vector2Int> toGenerate = new List<Vector2Int>();
    List<Vector3Int> toRender = new List<Vector3Int>();
    List<Mesh> meshes = new List<Mesh>();

    bool finishedRendering = false;
    bool startedRendering = false;

    void Start()
    {
        // mesh

        GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Plane);
        material = primitive.GetComponent<MeshRenderer>().sharedMaterial;
        DestroyImmediate(primitive);

        // system config

        threads = SystemInfo.processorCount - 1;

        // terrain config

        terrainSettings.terrain_amplitude = 15;
        terrainSettings.terrain_frequency = 100;
        terrainSettings.terrain_warp_octaves = 4;

        terrainSettings.biome_frequency = 100;

        terrainSettings.cave_octaves = 5;
        terrainSettings.cave_warp_octaves = 4;

        terrainSettings.biome_seed = 6969420;
        terrainSettings.cave_seed = 42090;
        terrainSettings.terrain_seed = 12345;

        // Static settings

        chunkManager.terrainSettings = terrainSettings;
        chunkManager.terrainGenerator.terrainSettings = terrainSettings;
        chunkManager.terrainGenerator.threads = threads;
        chunkManager.renderer.threads = threads;

        chunkManager.terrainGenerator.shader = terrainShader;
        chunkManager.renderer.shader = renderingShader;

        chunkManager.terrainGenerator.parent = chunkManager;
        chunkManager.terrainGenerator.map_height = map_height;
        chunkManager.map_height = map_height;
        chunkManager.map_size = 500;

        chunkManager.meshPool.threading.func = MakeMesh;

        // Game logic

        Vector3Int playerPosition = new Vector3Int(0, 20, 0);

        for (int x = -render_distance + playerPosition.x; x <= render_distance + playerPosition.x; x++)
        {
            for (int z = -render_distance + playerPosition.z; z <= render_distance + playerPosition.z; z++)
            {
                for (int y = 0; y < map_height; y++)
                {
                    toRender.Add(new Vector3Int(x, y, z));
                }

                toGenerate.Add(new Vector2Int(x, z));
            }
        }

        chunkManager.meshPool.Init();
        chunkManager.meshPool.GenerateMeshes((int)(10f / 100f * (float)toGenerate.Count));

        StartCoroutine(startRunning());
    }

    void MakeMesh(int i)
    {
        ChunkMesh chunkMesh = new ChunkMesh();
        Mesh mesh = new Mesh();

        GameObject chunk = new GameObject("Chunk");

        MeshRenderer meshRenderer = chunk.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = chunk.AddComponent<MeshFilter>();
        Collider collider = chunk.AddComponent<MeshCollider>();

        meshRenderer.sharedMaterial = material;
        meshFilter.mesh = mesh;

        chunkMesh.mesh = mesh;
        chunkMesh.chunkObject = chunk;

        chunkManager.meshPool.AddMesh(chunkMesh);
    }

    IEnumerator startRunning()
    {
        //yield return new WaitForSeconds(3f);
        yield return new WaitForSeconds(0);
        Debug.Log("Started");

        chunkManager.Start();
        chunkManager.GenerateChunks(toGenerate);
    }

    void Update()
    {
        chunkManager.Update();

        if (
            chunkManager.generatingChunks
            && chunkManager.finishedGeneratingChunks
            && !startedRendering
        )
        {
            chunkManager.generatingChunks = false;
            chunkManager.finishedGeneratingChunks = false;
            startedRendering = true;

            Debug.Log("Started rendering");
            chunkManager.RenderChunks(toRender);
        }

        if (chunkManager.finishedRenderingChunks && !finishedRendering)
        {
            chunkManager.renderingChunks = false;
            chunkManager.finishedRenderingChunks = false;
            finishedRendering = true;

            Debug.Log("Finished rendering");
        }
    }
}
