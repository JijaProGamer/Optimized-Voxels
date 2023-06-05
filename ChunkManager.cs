using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class ChunkManager
{
    Chunk[] chunks;

    public int map_size;
    public int map_height;

    public bool generatingChunks;
    public bool finishedGeneratingChunks;

    public bool renderingChunks;
    public bool finishedRenderingChunks;

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

    public void Start(){
        chunks = new Chunk[map_height * map_size * map_size];
    }

    public void Update(){

    }

    public void GenerateChunks(List<Vector3Int> positions){
        if(generatingChunks){
            throw new Exception("Already generating chunks.");
        }

        generatingChunks = true;

    }

    public void RenderChunks(List<Vector3Int> positions){
        if(renderingChunks){
            throw new Exception("Already rendering chunks.");
        }

        renderingChunks = true;
    }
}
