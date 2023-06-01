using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager
{
    Chunk[] chunks;

    public Chunk this[int x, int y, int z]
    {
        get { return chunks[(x + map_size / 2) + map_size * (y + (z + map_size / 2) * map_height)]; }
        set { chunks[(x + map_size / 2) + map_size * (y + (z + map_size / 2) * map_height)] = value; }
    }

    public Chunk this[int i]
    {
        get { return chunks[i]; }
        set { chunks[i] = value; }
    }

    public int map_size;
    public int map_height;
    public int threads;
    public TerrainSettings terrainSettings = new TerrainSettings();

    public Rendering renderer = new Rendering();
    public Terrain terrain = new Terrain();

    List<Vector3Int> positionsToUse;
    List<Vector3Int> positions2DToUse;
    List<Chunk> chunksToTransform;
    List<Chunk> chunksToTransform3D;
    float[] biomes;
    bool startedGenerating;
    bool passedBiomes;
    bool generatedBiomes;
    public bool finishedGeneraingTerrain;
    public bool finishedMarchingCubes;

    public void Start()
    {
        chunks = new Chunk[map_height * map_size * map_size];
    }

    public void Update()
    {
        if(!passedBiomes && startedGenerating && generatedBiomes){
            passedBiomes = true;

            terrain.chunksToTransform = chunksToTransform;
            terrain.chunksToTransform3D = chunksToTransform3D;
            terrain.D2ToGenerate = positions2DToUse;
            terrain.D3ToGenerate = positionsToUse;
            terrain.biomes = biomes;

            terrain.generate2D();
        }

        if(terrain.finished){
            finishedGeneraingTerrain = true;
        }

        terrain.Update();
    }

    private void generateBiomes(){
        FastNoiseLite moistureNoise = new FastNoiseLite(terrainSettings.biome_seed * 2);
        moistureNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
        moistureNoise.SetFrequency(1f/terrainSettings.biome_frequency);

        FastNoiseLite temperatureNoise = new FastNoiseLite(terrainSettings.biome_seed / 2);
        temperatureNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
        temperatureNoise.SetFrequency(1.5f/terrainSettings.biome_frequency);

        chunksToTransform = new List<Chunk>();
        chunksToTransform3D = new List<Chunk>();

        for(int i = 0; i < positions2DToUse.Count; i++){
            double moisture = moistureNoise.GetNoise(positions2DToUse[i].x, positions2DToUse[i].z);
            double temperature = temperatureNoise.GetNoise(positions2DToUse[i].x, positions2DToUse[i].z);

            biomes[i * 2] = (float) moisture;
            biomes[i * 2 + 1] = (float) temperature;

            for(int y = 0; y < map_height; y++){
                Chunk chunk = new Chunk();
                chunk.position = new Vector3Int(positions2DToUse[i].x, y, positions2DToUse[i].z);
                chunk.mesh.__init();
                chunk.moisture = (float) moisture;
                chunk.temperature = (float) temperature;

                this[positions2DToUse[i].x, y, positions2DToUse[i].z] = chunk;

                if(y == 0){
                    chunksToTransform.Add(chunk);
                }

                chunksToTransform3D.Add(chunk);
            }
        }

        generatedBiomes = true;
    }

    public void GenerateChunks(List<Vector3Int> positions){
        positionsToUse = positions;
        positions2DToUse = positions.Where(pos => pos.y == 0).ToList();
        biomes = new float[positions2DToUse.Count * 2];
        startedGenerating = true;

        Thread biomeThread = new Thread(generateBiomes);
        biomeThread.IsBackground = true;
        biomeThread.Start();
    }

    public void RenderChunks(List<Vector3Int> positions){
        /*positionsToUse = positions;
        positions2DToUse = positions.Where(pos => pos.y == 0).ToList();
        biomes = new float[positions2DToUse.Count * 2];
        startedGenerating = true;

        Thread biomeThread = new Thread(generateBiomes);
        biomeThread.IsBackground = true;
        biomeThread.Start();*/
    }
}
