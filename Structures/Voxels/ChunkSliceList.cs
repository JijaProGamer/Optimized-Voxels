using UnityEngine;

public class ChunkSliceList
{
    ChunkSlice[] chunkSlices;
    int map_size;

    public void Start(int _map_size)
    {
        map_size = _map_size;
        chunkSlices = new ChunkSlice[map_size * map_size];
    }

    public ChunkSlice this[int x, int z]
    {
        get
        {
            return chunkSlices[(x + map_size / 2) + map_size * (z + map_size / 2)];
        }
        set
        {
            chunkSlices[(x + map_size / 2) + map_size * (z + map_size / 2)] = value;
        }
    }

    public ChunkSlice this[int i]
    {
        get { return chunkSlices[i]; }
        set { chunkSlices[i] = value; }
    }
};
