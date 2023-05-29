using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    public ComputeShader terrainShader;
    public ComputeShader marchingShader;

    ChunkManager chunks = new ChunkManager();
    TerrainSettings settings = new TerrainSettings();

    List<Chunk> chunksToConstruct = new List<Chunk>();

    bool finishedConstructingTerrain = false;
    bool startedMarchingCubes = false;
    bool finishedMarchingCubes = false;

    int mapSize = 500;
    int mapHeight = 40;
    int renderDistance = 4;

    void Start()
    {
        int cpu_threads = SystemInfo.processorCount;


        settings.terrain_amplitude = 150;
        settings.terrain_scale = 100;

        chunks.terrainShader = terrainShader;
        chunks.marchingShader = marchingShader;

        chunks.__init(mapSize, mapHeight, cpu_threads);
        chunks.settings = settings;

        for (int x = -renderDistance; x < renderDistance; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                for (int z = -renderDistance; z < renderDistance; z++) {
                    Chunk chunk = new Chunk();
                    chunk.position = new Vector3Int(x, y, z);
                    chunk.__init();

                    Mesh rawMesh = new Mesh();

                    GameObject holder = new GameObject("Chunk");
                    holder.transform.position = chunk.position * 8; 
                    MeshRenderer renderer = holder.AddComponent<MeshRenderer>();
                    MeshFilter filter = holder.AddComponent<MeshFilter>();
                    filter.mesh = rawMesh;
                    
                    ChunkMesh mesh = new ChunkMesh();
                    mesh.mesh = rawMesh;
                    mesh.__init();

                    chunk.mesh = mesh;
                    chunksToConstruct.Add(chunk);
                    chunks.SetChunk(chunk.position, chunk);
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

            chunks.constructMeshes(chunksToConstruct);
        }

        chunks.Update();

        if(chunks.finishedSavingChunks){
            finishedConstructingTerrain = true;
        }

        if(chunks.finishedMarchingCubes){
            finishedMarchingCubes = true;
        }
    }
}
