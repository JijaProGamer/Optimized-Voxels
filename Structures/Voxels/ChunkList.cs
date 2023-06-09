public class ChunkList
{
    Chunk[] chunks;
    int map_height;
    int map_size;

    public void Start(int _map_height, int _map_size)
    {
        map_height = _map_height;
        map_size = _map_size;

        chunks = new Chunk[map_height * map_size * map_size];
    }

    public Chunk this[int x, int y, int z]
    {
        get
        {
            return chunks[(x + map_size / 2) + map_size * (y + (z + map_size / 2) * map_height)];
        }
        set
        {
            chunks[(x + map_size / 2) + map_size * (y + (z + map_size / 2) * map_height)] = value;
        }
    }

    public Chunk this[int i]
    {
        get { return chunks[i]; }
        set { chunks[i] = value; }
    }
};
